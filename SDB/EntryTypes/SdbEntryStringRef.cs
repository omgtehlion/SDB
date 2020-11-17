using System;
using System.Collections.Generic;

namespace SDB.EntryTypes
{
    public class SdbEntryStringRef : ISdbEntry
    {
        readonly SdbFile File;

        public SdbEntryStringRef(SdbFile file, SdbFile.TagValue typeId, byte[] bytes, int offset)
        {
            TypeId = typeId;
            Bytes = bytes;
            Offset = offset;
            File = file;

            Children = new List<ISdbEntry>();
        }

        public List<ISdbEntry> Children { get; }

        public SdbFile.TagValue TypeId { get; }

        public byte[] Bytes { get; }

        public object Value => GetValue();

        public int Offset { get; set; }

        private object GetValue()
        {
            var stringOffset = BitConverter.ToInt32(Bytes, 0);
            return File.StringTableEntries[stringOffset].Value;
        }

        public override string ToString()
        {
            return $"Type: {TypeId} (0x{TypeId:X}) --> {Value} Children count: {Children.Count:N0}";
        }
    }
}