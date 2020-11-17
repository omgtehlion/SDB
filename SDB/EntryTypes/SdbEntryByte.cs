﻿using System.Collections.Generic;

namespace SDB.EntryTypes
{
    public class SdbEntryByte : ISdbEntry
    {
        public SdbEntryByte(SdbFile.TagValue typeId, byte[] bytes, int offset)
        {
            TypeId = typeId;
            Bytes = bytes;
            Offset = offset;

            Children = new List<ISdbEntry>();
        }


        public List<ISdbEntry> Children { get; }

        public SdbFile.TagValue TypeId { get; }

        public byte[] Bytes { get; }

        public object Value => Bytes[0];

        public int Offset { get; set; }

        public override string ToString()
        {
            return $"Type: {TypeId} (0x{TypeId:X}) --> {Value} Children count: {Children.Count:N0}";
        }
    }
}