using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using XNCPLib.XNCP.Animation;
using XNCPLib.Extensions;
using Amicitia.IO.Binary;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class SceneID
    {
        public string Name { get; set; }
        public uint Index { get; set; }
        public SceneID()
        {
        }

        public void Read(BinaryObjectReader reader)
        {
            uint nameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);
            Index = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer, uint nameOffset)
        {
            writer.WriteUInt32(nameOffset);
            writer.WriteStringOffset(nameOffset, Name);
            writer.WriteUInt32(Index);
        }
    }

    public class Scene
    {
        public uint Field00 { get; set; }
        public float ZIndex { get; set; }
        public float AnimationFramerate { get; set; }
        public uint Field0C { get; set; }
        public float Field10 { get; set; }
        public float AspectRatio { get; set; }
        public List<Vector2> Data1 { get; set; }
        public List<SubImage> SubImages { get; set; }
        public List<CastGroup> UICastGroups { get; set; }
        public List<CastDictionary> CastDictionaries { get; set; }
        public List<AnimationKeyframeData> AnimationKeyframeDataList { get; set; }
        public List<AnimationDictionary> AnimationDictionaries { get; set; }
        public List<AnimationFrameData> AnimationFrameDataList { get; set; }
        public List<AnimationData2> AnimationData2List { get; set; }
        private uint UnwrittenPosition { get; set; }

        public Scene()
        {
            Data1 = new List<Vector2>();
            SubImages = new List<SubImage>();
            UICastGroups = new List<CastGroup>();
            CastDictionaries = new List<CastDictionary>();
            AnimationKeyframeDataList = new List<AnimationKeyframeData>();
            AnimationDictionaries = new List<AnimationDictionary>();
            AnimationFrameDataList = new List<AnimationFrameData>();
            AnimationData2List = new List<AnimationData2>();
        }

        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadUInt32();
            ZIndex = reader.ReadSingle();
            AnimationFramerate = reader.ReadSingle();
            Field0C = reader.ReadUInt32();
            Field10 = reader.ReadSingle();
            uint Data1Count = reader.ReadUInt32();
            uint Data1Offset = reader.ReadUInt32();
            uint SubImagesCount = reader.ReadUInt32();
            uint SubImagesOffset = reader.ReadUInt32();
            uint GroupCount = reader.ReadUInt32();
            uint CastGroupTableOffset = reader.ReadUInt32();
            uint CastCount = reader.ReadUInt32();
            uint CastDictionaryOffset = reader.ReadUInt32();
            uint AnimationCount = reader.ReadUInt32();
            uint AnimationKeyframeDataListOffset = reader.ReadUInt32();
            uint AnimationDictionaryOffset = reader.ReadUInt32();
            AspectRatio = reader.ReadSingle();
            uint AnimationFrameDataListOffset = reader.ReadUInt32();
            uint AnimationCastTableOffset = reader.ReadUInt32();

            long baseOffset = reader.GetOffsetOrigin();

            reader.Seek(baseOffset + Data1Offset, SeekOrigin.Begin);
            for (int i = 0; i < Data1Count; ++i)
            {
                Data1.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
            }

            reader.Seek(baseOffset + SubImagesOffset, SeekOrigin.Begin);
            for (int i = 0; i < SubImagesCount; ++i)
            {
                SubImage subImage = new SubImage();
                subImage.Read(reader);

                SubImages.Add(subImage);
            }

            for (int i = 0; i < GroupCount; ++i)
            {
                reader.Seek(baseOffset + CastGroupTableOffset + (16 * i), SeekOrigin.Begin);
                CastGroup group = new CastGroup();
                group.Read(reader);

                UICastGroups.Add(group);
            }

            reader.Seek(baseOffset + CastDictionaryOffset, SeekOrigin.Begin);
            for (int i = 0; i < CastCount; ++i)
            {
                CastDictionary dictionary = new CastDictionary();
                dictionary.Read(reader);

                CastDictionaries.Add(dictionary);
            }

            for (int i = 0; i < AnimationCount; ++i)
            {
                reader.Seek(baseOffset + AnimationKeyframeDataListOffset + (8 * i), SeekOrigin.Begin);

                AnimationKeyframeData keyframeData = new AnimationKeyframeData();
                keyframeData.Read(reader);

                AnimationKeyframeDataList.Add(keyframeData);
            }

            reader.Seek(baseOffset + AnimationDictionaryOffset, SeekOrigin.Begin);
            for (int i = 0; i < AnimationCount; ++i)
            {
                AnimationDictionary dictionary = new AnimationDictionary();
                dictionary.Read(reader);

                AnimationDictionaries.Add(dictionary);
            }

            reader.Seek(baseOffset + AnimationFrameDataListOffset, SeekOrigin.Begin);
            for (int i = 0; i < AnimationCount; ++i)
            {
                AnimationFrameData frameData = new AnimationFrameData();
                frameData.Read(reader);

                AnimationFrameDataList.Add(frameData);
            }

            // This is commented out for now so we can load 06 xncps
            for (int a = 0; a < AnimationCount; ++a)
            {
                reader.Seek(baseOffset + AnimationCastTableOffset + (a * 4), SeekOrigin.Begin);

                AnimationData2 data2 = new AnimationData2();
                data2.GroupAnimationData2ListOffset = reader.ReadUInt32();

                if (data2.GroupAnimationData2ListOffset > 0)
                {
                    reader.SeekBegin(baseOffset + data2.GroupAnimationData2ListOffset);

                    GroupAnimationData2List groupData2List = new GroupAnimationData2List();
                    groupData2List.Field00 = reader.ReadUInt32();
                    uint groupData2ListDataOffset = reader.ReadUInt32();

                    if (groupData2ListDataOffset > 0)
                    {
                        groupData2List.GroupList = new List<GroupAnimationData2>();
                        for (int g = 0; g < GroupCount; ++g)
                        {
                            reader.SeekBegin(baseOffset + groupData2ListDataOffset + (4 * g));

                            GroupAnimationData2 groupData2 = new GroupAnimationData2();
                            uint groupData2DataOffset = reader.ReadUInt32();

                            if (groupData2DataOffset > 0)
                            {
                                reader.SeekBegin(baseOffset + groupData2DataOffset);

                                CastAnimationData2List castAnimData2List = new CastAnimationData2List();
                                uint castAnimData2ListDataOffset = reader.ReadUInt32();

                                if (castAnimData2ListDataOffset > 0)
                                {
                                    castAnimData2List.ListData = new List<CastAnimationData2>();
                                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                                    {
                                        reader.SeekBegin(baseOffset + castAnimData2ListDataOffset + (4 * c));

                                        CastAnimationData2 castAnimData2 = new CastAnimationData2();
                                        uint castAnimData2DataOffset = reader.ReadUInt32();

                                        if (castAnimData2DataOffset > 0)
                                        {
                                            castAnimData2.Data = new Data5();
                                            reader.SeekBegin(baseOffset + castAnimData2DataOffset);

                                            // Data5
                                            uint castAnimData2DataDataOffset = reader.ReadUInt32();
                                            if (castAnimData2DataDataOffset > 0)
                                            {
                                                castAnimData2.Data.SubData = new List<Data6>();
                                                uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                                                uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);

                                                for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                                                {
                                                    reader.SeekBegin(baseOffset + castAnimData2DataDataOffset + (4 * subData1Index));

                                                    Data6 subData = new Data6();
                                                    uint subDataDataOffset = reader.ReadUInt32();

                                                    if (subDataDataOffset > 0)
                                                    {
                                                        reader.SeekBegin(baseOffset + subDataDataOffset);

                                                        Data7 data = new Data7();
                                                        uint data7DataOffset = reader.ReadUInt32();

                                                        if (data7DataOffset > 0)
                                                        {
                                                            data.Data = new List<Data8>();
                                                            uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                                                            data.Data.Capacity = (int)data8Count;

                                                            for (int v = 0; v < data8Count; ++v)
                                                            {
                                                                reader.SeekBegin(baseOffset + data7DataOffset + (4 * v));

                                                                Data8 value = new Data8();
                                                                uint valueValueOffset = reader.ReadUInt32();

                                                                if (valueValueOffset > 0)
                                                                {
                                                                    value.IsUsed = true;
                                                                    reader.SeekBegin(baseOffset + valueValueOffset);

                                                                    value.Value = reader.ReadVector3();
                                                                }

                                                                data.Data.Add(value);
                                                            }
                                                        }

                                                        subData.Data = data;
                                                    }

                                                    castAnimData2.Data.SubData.Add(subData);
                                                }
                                            }
                                        }

                                        castAnimData2List.ListData.Add(castAnimData2);
                                    }
                                }

                                groupData2.AnimationData2List = castAnimData2List;
                            }

                            groupData2List.GroupList.Add(groupData2);
                        }
                    }

                    data2.GroupList = groupData2List;
                }

                AnimationData2List.Add(data2);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            UnwrittenPosition = (uint)writer.Position;

            writer.WriteUInt32(Field00);
            writer.WriteSingle(ZIndex);
            writer.WriteSingle(AnimationFramerate);
            writer.WriteUInt32(Field0C);
            writer.WriteSingle(Field10);
            writer.WriteUInt32((uint)Data1.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x1C;

            // Fill Data1 data
            writer.Seek(0, SeekOrigin.End);
            for (int i = 0; i < Data1.Count; ++i)
            {
                writer.WriteSingle(Data1[i].X);
                writer.WriteSingle(Data1[i].Y);
            }

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32((uint)SubImages.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x8;

            // Fill SubImage data
            writer.Seek(0, SeekOrigin.End);
            for (int i = 0; i < SubImages.Count; ++i)
            {
                SubImages[i].Write(writer);
            }

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32((uint)UICastGroups.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x8;

            // Allocate memory for CastGroup data
            uint newUnwrittenPosition = (uint)writer.Length;
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, UICastGroups.Count * 0x10);

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32((uint)CastDictionaries.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x8;

            // Allocate memory for CastIDOffsets data
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, CastDictionaries.Count * 0xC);

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32((uint)AnimationDictionaries.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length + AnimationDictionaries.Count * 0x8 - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0xC;

            // Allocate memory for AnimationKeyFrame and AnimationIDOffsets data
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, AnimationDictionaries.Count * 0x10);

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteSingle(AspectRatio);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length + AnimationDictionaries.Count * 0x8 - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0xC;

            // Allocate memory for AnimationFrame and Data2List data
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, AnimationDictionaries.Count * 0xC);

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill CastGroup data
            for (int i = 0; i < UICastGroups.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x10;

                UICastGroups[i].Write_Step0(writer, offsetChunk);
            }

            // Fill CastIDOffsets data
            for (int i = 0; i < CastDictionaries.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0xC;

                offsetChunk.Add(writer);
                uint nameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                CastDictionaries[i].Write(writer, nameOffset);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }

            // Fill AnimationKeyFrame data
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                AnimationKeyframeDataList[i].Write_Step0(writer, offsetChunk);
            }

            // Fill AnimationIDOffsets data
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                offsetChunk.Add(writer);
                uint nameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                AnimationDictionaries[i].Write_REPLACE(writer, nameOffset);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }

            // Allocate memory for AnimationFrame data
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                AnimationFrameDataList[i].Write(writer);
            }

            // AnimationData2
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x4;

                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList != null)
                {
                    offsetChunk.Add(writer);
                    writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                    // Allocate memory for GroupAnimationData2List data
                    writer.Seek(0, SeekOrigin.End);
                    Utilities.PadZeroBytes(writer, 0x8);
                }
                else
                {
                    writer.WriteUInt32(0);
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue UICastGroups steps
            for (int i = 0; i < UICastGroups.Count; ++i)
            {
                UICastGroups[i].Write_Step1(writer, offsetChunk);
            }

            // Continue AnimationKeyFrame steps
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                AnimationKeyframeDataList[i].Write_Step1(writer, offsetChunk);
            }

            // Fill GroupAnimationData2List data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                GroupAnimationData2List groupData2List = data2.GroupList;
                writer.WriteUInt32(groupData2List.Field00);
                if (groupData2List.GroupList != null)
                {
                    offsetChunk.Add(writer);
                    writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                    for (int g = 0; g < UICastGroups.Count; ++g)
                    {
                        // Allocate memory for GroupAnimationData2 data
                        writer.Seek(0, SeekOrigin.End);
                        Utilities.PadZeroBytes(writer, 0x4);
                    }
                }
                else
                {
                    writer.WriteUInt32(0);
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step3(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue UICastGroups steps
            for (int i = 0; i < UICastGroups.Count; ++i)
            {
                UICastGroups[i].Write_Step2(writer, offsetChunk);
                // Finished
            }

            // Continue AnimationKeyFrame steps
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                AnimationKeyframeDataList[i].Write_Step2(writer, offsetChunk);
            }

            // Fill GroupAnimationData2 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                    UnwrittenPosition += 0x4;

                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List != null)
                    {
                        offsetChunk.Add(writer);
                        writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                        // Allocate memory for CastAnimationData2List data
                        writer.Seek(0, SeekOrigin.End);
                        Utilities.PadZeroBytes(writer, 0x4);
                    }
                    else
                    {
                        writer.WriteUInt32(0);
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step4(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue AnimationKeyFrame steps
            for (int i = 0; i < AnimationDictionaries.Count; ++i)
            {
                AnimationKeyframeDataList[i].Write_Step3(writer, offsetChunk);
                // Finished
            }

            // Fill CastAnimationData2List data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                    UnwrittenPosition += 0x4;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData != null)
                    {
                        offsetChunk.Add(writer);
                        writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                        for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                        {
                            // Allocate memory for CastAnimationData2 data
                            writer.Seek(0, SeekOrigin.End);
                            Utilities.PadZeroBytes(writer, 0x4);
                        }
                    }
                    else
                    {
                        writer.WriteUInt32(0);
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step5(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill CastAnimationData2 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                        UnwrittenPosition += 0x4;

                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data != null)
                        {
                            offsetChunk.Add(writer);
                            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                            // Allocate memory for Data5 data
                            writer.Seek(0, SeekOrigin.End);
                            Utilities.PadZeroBytes(writer, 0x4);
                        }
                        else
                        {
                            writer.WriteUInt32(0);
                        }
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step6(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill Data5 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data == null) continue;

                        writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                        UnwrittenPosition += 0x4;

                        Data5 data5 = castAnimData2.Data;
                        if (data5.SubData != null)
                        {
                            offsetChunk.Add(writer);
                            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                            uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                            uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                            for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                            {
                                // Allocate memory for Data6 data
                                writer.Seek(0, SeekOrigin.End);
                                Utilities.PadZeroBytes(writer, 0x4);
                            }
                        }
                        else
                        {
                            writer.WriteUInt32(0);
                        }
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step7(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill Data6 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data == null) continue;

                        Data5 data5 = castAnimData2.Data;
                        if (data5.SubData == null) continue;

                        uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                        uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                        for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                        {
                            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                            UnwrittenPosition += 0x4;

                            Data6 subData = data5.SubData[subData1Index];
                            if (subData.Data != null)
                            {
                                offsetChunk.Add(writer);
                                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                                // Allocate memory for Data7 data
                                writer.Seek(0, SeekOrigin.End);
                                Utilities.PadZeroBytes(writer, 0x4);
                            }
                            else
                            {
                                writer.WriteUInt32(0);
                            }
                        }
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step8(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill Data7 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data == null) continue;

                        Data5 data5 = castAnimData2.Data;
                        if (data5.SubData == null) continue;

                        uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                        uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                        for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                        {
                            Data6 subData = castAnimData2.Data.SubData[subData1Index];
                            if (subData.Data == null) continue;

                            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                            UnwrittenPosition += 0x4;

                            Data7 data7 = subData.Data;
                            if (data7.Data != null)
                            {
                                offsetChunk.Add(writer);
                                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                                uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                                for (int v = 0; v < data8Count; ++v)
                                {
                                    // Allocate memory for Data8 data
                                    writer.Seek(0, SeekOrigin.End);
                                    Utilities.PadZeroBytes(writer, 0x4);
                                }
                            }
                            else
                            {
                                writer.WriteUInt32(0);
                            }
                        }
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }
        
        public void Write_Step9(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill Data8 data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data == null) continue;

                        Data5 data5 = castAnimData2.Data;
                        if (data5.SubData == null) continue;

                        uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                        uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                        for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                        {
                            Data6 subData = castAnimData2.Data.SubData[subData1Index];
                            if (subData.Data == null) continue;

                            Data7 data7 = subData.Data;
                            if (data7.Data == null) continue;

                            uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                            for (int v = 0; v < data8Count; ++v)
                            {
                                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                                UnwrittenPosition += 0x4;

                                Data8 data8 = data7.Data[v];
                                if (data8.IsUsed)
                                {
                                    offsetChunk.Add(writer);
                                    writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                                    // Allocate memory for final Vector3 data
                                    writer.Seek(0, SeekOrigin.End);
                                    Utilities.PadZeroBytes(writer, 0xC);
                                }
                                else
                                {
                                    writer.WriteUInt32(0);
                                }
                            }
                        }
                    }
                }
            }

            UnwrittenPosition = newUnwrittenPosition;
        }
        
        public void Write_Step10(BinaryObjectWriter writer)
        {
            // Fill final Vector3 data
            for (int a = 0; a < AnimationDictionaries.Count; ++a)
            {
                AnimationData2 data2 = AnimationData2List[a];
                if (data2.GroupList == null) continue;

                GroupAnimationData2List groupData2List = data2.GroupList;
                if (groupData2List.GroupList == null) continue;

                for (int g = 0; g < UICastGroups.Count; ++g)
                {
                    GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                    if (groupData2.AnimationData2List == null) continue;

                    CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                    if (castAnimData2List.ListData == null) continue;

                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                    {
                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                        if (castAnimData2.Data == null) continue;

                        Data5 data5 = castAnimData2.Data;
                        if (data5.SubData == null) continue;

                        uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                        uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                        for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                        {
                            Data6 subData = castAnimData2.Data.SubData[subData1Index];
                            if (subData.Data == null) continue;

                            Data7 data7 = subData.Data;
                            if (data7.Data == null) continue;

                            uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                            for (int v = 0; v < data8Count; ++v)
                            {
                                Data8 data8 = data7.Data[v];
                                if (!data8.IsUsed) continue;

                                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                                UnwrittenPosition += 0xC;

                                writer.WriteSingle(data8.Value.X);
                                writer.WriteSingle(data8.Value.Y);
                                writer.WriteSingle(data8.Value.Z);
                            }
                        }
                    }
                }
            }
        }
    }
}
