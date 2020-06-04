using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Exports.Builder
{
    public class OrdinalNamePointerTableBuffer : SegmentBase
    {
        private readonly NameTableBuffer _nameTableBuffer;
        private readonly List<ushort> _ordinals = new List<ushort>();
        private readonly List<ExportedSymbol> _namedEntries = new List<ExportedSymbol>();

        /// <summary>
        /// Creates a new empty ordinal and name-pointer table buffer.
        /// </summary>
        /// <param name="nameTableBuffer">The table containing the names of all exports.</param>
        public OrdinalNamePointerTableBuffer(NameTableBuffer nameTableBuffer)
        {
            _nameTableBuffer = nameTableBuffer ?? throw new ArgumentNullException(nameof(nameTableBuffer));
        }

        /// <summary>
        /// Gets the relative virtual address (RVA) to the ordinal table.
        /// </summary>
        public uint OrdinalTableRva => Rva;

        /// <summary>
        /// Gets the raw size in bytes of the ordinal table.
        /// </summary>
        public uint OrdinalTableSize => (uint) (_ordinals.Count * sizeof(ushort));

        /// <summary>
        /// Gets the relative virtual address (RVA) to the name pointer table.
        /// </summary>
        public uint NamePointerTableRva => Rva + OrdinalTableSize; 
        
        /// <summary>
        /// Gets the raw size in bytes of the name pointer.
        /// </summary>
        public uint NamePointerTableSize => (uint) (_namedEntries.Count * sizeof(uint));
        
        /// <summary>
        /// When the symbol is exported by name, adds the ordinal and name pointer pair to the buffer.
        /// </summary>
        /// <param name="symbol">The symbol to register.</param>
        public void AddSymbol(ExportedSymbol symbol)
        {
            if (symbol.IsByName)
            {
                _namedEntries.Add(symbol);
                _ordinals.Add((ushort) (symbol.Ordinal - symbol.ParentDirectory.BaseOrdinal));
            }
        } 
        
        /// <inheritdoc />
        public override uint GetPhysicalSize() => OrdinalTableSize + NamePointerTableSize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            WriteOrdinalTable(writer);
            WriteNamePointerTable(writer);
        }

        private void WriteNamePointerTable(IBinaryStreamWriter writer)
        {
            foreach (var entry in _namedEntries)
                writer.WriteUInt32(_nameTableBuffer.GetNameRva(entry.Name));
        }

        private void WriteOrdinalTable(IBinaryStreamWriter writer)
        {
            foreach (ushort ordinal in _ordinals)
                writer.WriteUInt16(ordinal);
        }
    }
}