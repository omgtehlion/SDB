﻿using System;
using System.Collections.Generic;
using System.Text;
using SDB.EntryTypes;

namespace SDB
{
    internal class Chunk
    {
        internal Chunk(SdbFile file, SdbFile.TagValue typeId, byte[] bytes, int baseOffset)
        {
            TypeId = typeId;
            Bytes = bytes;

            //logger.Debug($"Chunk ID: {typeId} ({typeId:X}) bytes length: 0x{Bytes.Length:X}");

            var index = 0x0;

            Children = new List<ISdbEntry>();

            //bytes contains whatever the payload for this chunk is
            while (index < bytes.Length)
            {
                if (bytes.Length - index < 2)
                {
                    break;
                }

                var id1 = (SdbFile.TagValue) BitConverter.ToUInt16(bytes, index);

                var tagTypeInt = (int) id1 & 0xF000;

                //logger.Debug($"While loop TagType {tagTypeInt:X}, Id: {id1} (0x{id1:X})");

                index += 2;

                //handle some special cases
                switch ((int) id1)
                {
                    case 0x53db:
                        index -= 1;
                        continue;
                }
                //end handle some special cases

                int size;
                byte[] buff;

                var tagType = (SdbFile.TagType) tagTypeInt;

                UpdateMetrics(file, id1);

                switch (tagType)
                {
                    case SdbFile.TagType.TAG_TYPE_NULL:
                        buff = new byte[0];

                        var n = new SdbEntryNull(id1, buff, baseOffset + index);
                        Children.Add(n);

                        break;
                    case SdbFile.TagType.TAG_TYPE_BYTE:
                        throw new Exception("Send this file to saericzimmerman@gmail.com so BYTE support can be added");
//                        buff = new byte[1];
//                        Buffer.BlockCopy(bytes, index, buff, 0, 1);
//                        var b = new SdbEntryByte(id1, buff, baseOffset + index);
//
//                        Children.Add(b);
//                        break;
                    case SdbFile.TagType.TAG_TYPE_WORD:
                        buff = new byte[2];
                        Buffer.BlockCopy(bytes, index, buff, 0, 2);
                        var w = new SdbEntryWord(id1, buff, baseOffset + index);
                        Children.Add(w);

                        index += 2;
                        break;
                    case SdbFile.TagType.TAG_TYPE_DWORD:
                        buff = new byte[4];
                        Buffer.BlockCopy(bytes, index, buff, 0, 4);
                        var d = new SdbEntryDWord(id1, buff, baseOffset + index);
                        Children.Add(d);

                        index += 4;
                        break;
                    case SdbFile.TagType.TAG_TYPE_QWORD:
                        buff = new byte[8];
                        Buffer.BlockCopy(bytes, index, buff, 0, 8);
                        var q = new SdbEntryQWord(id1, buff, baseOffset + index);
                        Children.Add(q);

                        index += 8;
                        break;
                    case SdbFile.TagType.TAG_TYPE_STRINGREF:
                        buff = new byte[4];
                        Buffer.BlockCopy(bytes, index, buff, 0, 4);
                        var sr = new SdbEntryStringRef(file, id1, buff, baseOffset + index);
                        Children.Add(sr);

                        index += 4;
                        break;
                    case SdbFile.TagType.TAG_TYPE_LIST:
                        size = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        buff = new byte[size];
                        Buffer.BlockCopy(bytes, index, buff, 0, size);

                        var c = new Chunk(file, id1, buff, baseOffset + index);

                        var l = new SdbEntryList(id1, buff, baseOffset + index);

                        foreach (var sdbEntry in c.Children)
                        {
                            if (!(sdbEntry is SdbEntryList))
                            {
                                sdbEntry.Offset += 4;
                            }

                            l.Children.Add(sdbEntry);
                        }

                        Children.Add(l);

                        index += size;

                        break;
                    case SdbFile.TagType.TAG_TYPE_STRING:
                        size = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        buff = new byte[size];
                        Buffer.BlockCopy(bytes, index, buff, 0, size);

                        //this provides a means to do easy string lookups based on relative offset
                        var sti = new StringTableEntry(index, Encoding.Unicode.GetString(buff, 0, size).Trim('\0'));
                        file.StringTableEntries.Add(sti.Offset, sti);

                        //this is the structure itself that defines the strings found in the database
                        var st = new SdbEntryStringTableItem(id1, buff, baseOffset + index - 4);
                        Children.Add(st);

                        index += size;
                        break;
                    case SdbFile.TagType.TAG_TYPE_BINARY:
                        size = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        buff = new byte[size];
                        Buffer.BlockCopy(bytes, index, buff, 0, size);

                        var bin = new SdbEntryBinary(id1, buff, baseOffset + index - 4);
                        Children.Add(bin);

                        index += size;

                        break;
                    default:
                        throw new Exception($"Unknown tag type: {tagType}, index: 0x{index:X}");
                }
            }
        }


        public List<ISdbEntry> Children { get; }

        public SdbFile.TagValue TypeId { get; }

        public byte[] Bytes { get; }

        private static void UpdateMetrics(SdbFile file, SdbFile.TagValue tagId)
        {
            if (file.Metrics.ContainsKey(tagId) == false)
            {
                file.Metrics.Add(tagId, 0);
            }

            file.Metrics[tagId] += 1;
        }
    }
}