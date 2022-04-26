using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP.Animation
{
    public class AnimationKeyframeData
    {
        public List<GroupAnimationData> GroupAnimationDataList { get; set; }
        private uint UnwrittenPosition { get; set; }

        public AnimationKeyframeData()
        {
            GroupAnimationDataList = new List<GroupAnimationData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint GroupCount = reader.ReadUInt32();
            uint groupDataOffset = reader.ReadUInt32();

            GroupAnimationDataList.Capacity = (int)GroupCount;

            for (int i = 0; i < GroupCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + groupDataOffset + (8 * i), SeekOrigin.Begin);

                GroupAnimationData groupData = new GroupAnimationData();
                groupData.Read(reader);

                GroupAnimationDataList.Add(groupData);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.WriteUInt32((uint)GroupAnimationDataList.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

            // Allocate memory for GroupAnimationDataList
            UnwrittenPosition = (uint)writer.Length;
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, GroupAnimationDataList.Count * 0x8);
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            for (int i = 0; i < GroupAnimationDataList.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                GroupAnimationDataList[i].Write_Step0(writer, offsetChunk);
            }
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue GroupAnimationDataList steps
            for (int i = 0; i < GroupAnimationDataList.Count; ++i)
            {
                GroupAnimationDataList[i].Write_Step1(writer, offsetChunk);
            }
        }

        public void Write_Step3(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue GroupAnimationDataList steps
            for (int i = 0; i < GroupAnimationDataList.Count; ++i)
            {
                GroupAnimationDataList[i].Write_Step2(writer, offsetChunk);
                // Finished
            }
        }
    }

    public class AnimationData2
    {
        public GroupAnimationData2List GroupList { get; set; }

        public AnimationData2()
        {
        }
    }

    public class AnimationFrameData
    {
        public uint Field00 { get; set; }
        public float FrameCount { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadUInt32();
            FrameCount = reader.ReadSingle();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Field00);
            writer.WriteSingle(FrameCount);
        }
    }
}
