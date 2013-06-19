﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldReference : MemberReference
    {
        internal FieldSignature signature = null;
        internal string name;

        public FieldReference(MetaDataRow row)
            : base(row)
        {
        }

        public FieldReference(string name, TypeReference declaringType, uint signature)
            : base(new MetaDataRow(0U, 0U, signature))
        {
            this.name = name;
            this.declaringType = declaringType;
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]), out name);
                return name;
            }
        }
        public override string FullName
        {
            get
            {
                try
                {
                    if (DeclaringType is TypeReference)
                        return (Signature != null ? Signature.ReturnType.FullName + " " : "") + ((TypeReference)DeclaringType).FullName + "::" + Name;

                    return Name;
                }
                catch { return Name; }
            }
        }

        public FieldSignature Signature
        {
            get
            {
                if (signature != null)
                    return signature;
                signature = (FieldSignature)netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(metadatarow.parts[2]), this.DeclaringType);
                return signature;
            }
        }

        public override void ClearCache()
        {
            signature = null;
            declaringType = null;
            name = null;
        }

        public override void LoadCache()
        {
            signature = Signature;
            declaringType = DeclaringType;
            name = Name;
        }
    }
}
