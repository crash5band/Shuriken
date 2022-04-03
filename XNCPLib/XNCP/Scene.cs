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
        private uint NameOffset { get; set; }
        public uint Index { get; set; }
        public SceneID()
        {
        }

        public void Read(BinaryObjectReader reader)
        {
            NameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(NameOffset);
            Index = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(NameOffset);
            writer.WriteStringOffset(NameOffset, Name);
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
        public uint Data1Count { get; set; }
        public uint Data1Offset { get; set; }
        public uint SubImagesCount { get; set; }
        public uint SubImagesOffset { get; set; }
        public uint GroupCount { get; set; }
        public uint CastGroupTableOffset { get; set; }
        public uint CastCount { get; set; }
        public uint CastDictionaryOffset { get; set; }
        public uint AnimationCount { get; set; }
        public uint AnimationKeyframeDataListOffset { get; set; }
        public uint AnimationDictionaryOffset { get; set; }
        public float AspectRatio { get; set; }
        public uint AnimationFrameDataListOffset { get; set; }
        public uint AnimationCastTableOffset { get; set; }
        public List<Vector2> Data1 { get; set; }
        public List<SubImage> SubImages { get; set; }
        public List<CastGroup> UICastGroups { get; set; }
        public List<CastDictionary> CastDictionaries { get; set; }
        public List<AnimationKeyframeData> AnimationKeyframeDataList { get; set; }
        public List<AnimationDictionary> AnimationDictionaries { get; set; }
        public List<AnimationFrameData> AnimationFrameDataList { get; set; }
        public List<AnimationData2> AnimationData2List { get; set; }

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
            Data1Count = reader.ReadUInt32();
            Data1Offset = reader.ReadUInt32();
            SubImagesCount = reader.ReadUInt32();
            SubImagesOffset = reader.ReadUInt32();
            GroupCount = reader.ReadUInt32();
            CastGroupTableOffset = reader.ReadUInt32();
            CastCount = reader.ReadUInt32();
            CastDictionaryOffset = reader.ReadUInt32();
            AnimationCount = reader.ReadUInt32();
            AnimationKeyframeDataListOffset = reader.ReadUInt32();
            AnimationDictionaryOffset = reader.ReadUInt32();
            AspectRatio = reader.ReadSingle();
            AnimationFrameDataListOffset = reader.ReadUInt32();
            AnimationCastTableOffset = reader.ReadUInt32();

            long baseOffset = reader.GetOffsetOrigin();
            reader.Seek(baseOffset + Data1Offset, SeekOrigin.Begin);

            for (int i = 0; i < Data1Count; ++i)
            {
                Data1.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
            }

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
            long pos = reader.Position;
            for (int a = 0; a < AnimationCount; ++a)
            {
                reader.SeekBegin(pos + (a * 4));

                AnimationData2 data2 = new AnimationData2();
                data2.GroupAnimationData2ListOffset = reader.ReadUInt32();

                if (data2.GroupAnimationData2ListOffset > 0)
                {
                    reader.SeekBegin(baseOffset + data2.GroupAnimationData2ListOffset);

                    GroupAnimationData2List groupData2List = new GroupAnimationData2List();
                    groupData2List.Field00 = reader.ReadUInt32();
                    groupData2List.DataOffset = reader.ReadUInt32();

                    if (groupData2List.DataOffset > 0)
                    {
                        for (int g = 0; g < GroupCount; ++g)
                        {
                            reader.SeekBegin(baseOffset + groupData2List.DataOffset + (4 * g));

                            GroupAnimationData2 groupData2 = new GroupAnimationData2();
                            groupData2.DataOffset = reader.ReadUInt32();

                            if (groupData2.DataOffset > 0)
                            {
                                reader.SeekBegin(baseOffset + groupData2.DataOffset);

                                CastAnimationData2List castAnimData2List = new CastAnimationData2List();
                                castAnimData2List.DataOffset = reader.ReadUInt32();

                                if (castAnimData2List.DataOffset > 0)
                                {
                                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                                    {
                                        reader.SeekBegin(baseOffset + castAnimData2List.DataOffset + (4 * c));

                                        CastAnimationData2 castAnimData2 = new CastAnimationData2();
                                        castAnimData2.DataOffset = reader.ReadUInt32();

                                        if (castAnimData2.DataOffset > 0)
                                        {
                                            reader.SeekBegin(baseOffset + castAnimData2.DataOffset);

                                            // Data5
                                            castAnimData2.Data.DataOffset = reader.ReadUInt32();
                                            if (castAnimData2.Data.DataOffset > 0)
                                            {
                                                uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                                                uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);
                                                
                                                for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                                                {
                                                    reader.SeekBegin(baseOffset + castAnimData2.Data.DataOffset + (4 * subData1Index));

                                                    Data6 subData = new Data6();
                                                    subData.DataOffset = reader.ReadUInt32();

                                                    if (subData.DataOffset > 0)
                                                    {
                                                        reader.SeekBegin(baseOffset + subData.DataOffset);

                                                        Data7 data = new Data7();
                                                        data.DataOffset = reader.ReadUInt32();

                                                        if (data.DataOffset > 0)
                                                        {
                                                            uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                                                            data.Data.Capacity = (int)data8Count;

                                                            for (int v = 0; v < data8Count; ++v)
                                                            {
                                                                reader.SeekBegin(baseOffset + data.DataOffset + (4 * v));

                                                                Data8 value = new Data8();
                                                                value.ValueOffset = reader.ReadUInt32();

                                                                if (value.ValueOffset > 0)
                                                                {
                                                                    reader.SeekBegin(baseOffset + value.ValueOffset);

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

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Field00);
            writer.WriteSingle(ZIndex);
            writer.WriteSingle(AnimationFramerate);
            writer.WriteUInt32(Field0C);
            writer.WriteSingle(Field10);
            writer.WriteUInt32(Data1Count);
            writer.WriteUInt32(Data1Offset);
            writer.WriteUInt32(SubImagesCount);
            writer.WriteUInt32(SubImagesOffset);
            writer.WriteUInt32(GroupCount);
            writer.WriteUInt32(CastGroupTableOffset);
            writer.WriteUInt32(CastCount);
            writer.WriteUInt32(CastDictionaryOffset);
            writer.WriteUInt32(AnimationCount);
            writer.WriteUInt32(AnimationKeyframeDataListOffset);
            writer.WriteUInt32(AnimationDictionaryOffset);
            writer.WriteSingle(AspectRatio);
            writer.WriteUInt32(AnimationFrameDataListOffset);
            writer.WriteUInt32(AnimationCastTableOffset);

            long baseOffset = writer.GetOffsetOrigin();
            writer.Seek(baseOffset + Data1Offset, SeekOrigin.Begin);

            for (int i = 0; i < Data1Count; ++i)
            {
                writer.WriteSingle(Data1[i].X);
                writer.WriteSingle(Data1[i].Y);
            }

            for (int i = 0; i < SubImagesCount; ++i)
            {
                SubImages[i].Write(writer);
            }

            for (int i = 0; i < GroupCount; ++i)
            {
                writer.Seek(baseOffset + CastGroupTableOffset + (16 * i), SeekOrigin.Begin);
                UICastGroups[i].Write(writer);
            }

            writer.Seek(baseOffset + CastDictionaryOffset, SeekOrigin.Begin);
            for (int i = 0; i < CastCount; ++i)
            {
                CastDictionaries[i].Write(writer);
            }

            for (int i = 0; i < AnimationCount; ++i)
            {
                writer.Seek(baseOffset + AnimationKeyframeDataListOffset + (8 * i), SeekOrigin.Begin);

                AnimationKeyframeDataList[i].Write(writer);
            }

            writer.Seek(baseOffset + AnimationDictionaryOffset, SeekOrigin.Begin);
            for (int i = 0; i < AnimationCount; ++i)
            {
                AnimationDictionaries[i].Write(writer);
            }

            writer.Seek(baseOffset + AnimationFrameDataListOffset, SeekOrigin.Begin);
            for (int i = 0; i < AnimationCount; ++i)
            {
                AnimationFrameDataList[i].Write(writer);
            }

            long pos = writer.Position;
            for (int a = 0; a < AnimationCount; ++a)
            {
                writer.Seek(pos + (a * 4), SeekOrigin.Begin);

                AnimationData2 data2 = AnimationData2List[a];
                writer.WriteUInt32(data2.GroupAnimationData2ListOffset);

                if (data2.GroupAnimationData2ListOffset > 0)
                {
                    writer.Seek(baseOffset + data2.GroupAnimationData2ListOffset, SeekOrigin.Begin);

                    GroupAnimationData2List groupData2List = data2.GroupList;
                    writer.WriteUInt32(groupData2List.Field00);
                    writer.WriteUInt32(groupData2List.DataOffset);

                    if (groupData2List.DataOffset > 0)
                    {
                        for (int g = 0; g < GroupCount; ++g)
                        {
                            writer.Seek(baseOffset + groupData2List.DataOffset + (4 * g), SeekOrigin.Begin);

                            GroupAnimationData2 groupData2 = groupData2List.GroupList[g];
                            writer.WriteUInt32(groupData2.DataOffset);

                            if (groupData2.DataOffset > 0)
                            {
                                writer.Seek(baseOffset + groupData2.DataOffset, SeekOrigin.Begin);

                                CastAnimationData2List castAnimData2List = groupData2.AnimationData2List;
                                writer.WriteUInt32(castAnimData2List.DataOffset);

                                if (castAnimData2List.DataOffset > 0)
                                {
                                    for (int c = 0; c < UICastGroups[g].CastCount; ++c)
                                    {
                                        writer.Seek(baseOffset + castAnimData2List.DataOffset + (4 * c), SeekOrigin.Begin);

                                        CastAnimationData2 castAnimData2 = castAnimData2List.ListData[c];
                                        writer.WriteUInt32(castAnimData2.DataOffset);

                                        if (castAnimData2.DataOffset > 0)
                                        {
                                            writer.Seek(baseOffset + castAnimData2.DataOffset, SeekOrigin.Begin);

                                            // Data5
                                            writer.WriteUInt32(castAnimData2.Data.DataOffset);
                                            if (castAnimData2.Data.DataOffset > 0)
                                            {
                                                uint flags = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].Flags;
                                                uint castAnimationDataSubData1Count = Utilities.CountSetBits(flags);

                                                for (int subData1Index = 0; subData1Index < castAnimationDataSubData1Count; ++subData1Index)
                                                {
                                                    writer.Seek(baseOffset + castAnimData2.Data.DataOffset + (4 * subData1Index), SeekOrigin.Begin);
                                                    
                                                    Data6 subData = castAnimData2.Data.SubData[subData1Index];
                                                    writer.WriteUInt32(subData.DataOffset);

                                                    if (subData.DataOffset > 0)
                                                    {
                                                        writer.Seek(baseOffset + subData.DataOffset, SeekOrigin.Begin);

                                                        Data7 data = subData.Data;
                                                        writer.WriteUInt32(data.DataOffset);

                                                        if (data.DataOffset > 0)
                                                        {
                                                            uint data8Count = AnimationKeyframeDataList[a].GroupAnimationDataList[g].CastAnimationDataList[c].SubDataList[subData1Index].KeyframeCount;
                                                            data.Data.Capacity = (int)data8Count;

                                                            for (int v = 0; v < data8Count; ++v)
                                                            {
                                                                writer.Seek(baseOffset + data.DataOffset + (4 * v), SeekOrigin.Begin);

                                                                Data8 value = data.Data[v];
                                                                writer.WriteUInt32(value.ValueOffset);

                                                                if (value.ValueOffset > 0)
                                                                {
                                                                    writer.Seek(baseOffset + value.ValueOffset, SeekOrigin.Begin);
                                                                    writer.WriteSingle(value.Value.X);
                                                                    writer.WriteSingle(value.Value.Y);
                                                                    writer.WriteSingle(value.Value.Z);

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
