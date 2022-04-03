using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP.Animation
{
    public class AnimationKeyframeData
    {
        public uint GroupCount { get; set; }
        public uint GroupDataOffset { get; set; }
        public List<GroupAnimationData> GroupAnimationDataList { get; set; }

        public AnimationKeyframeData()
        {
            GroupAnimationDataList = new List<GroupAnimationData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            GroupCount = reader.ReadUInt32();
            GroupDataOffset = reader.ReadUInt32();

            GroupAnimationDataList.Capacity = (int)GroupCount;

            for (int i = 0; i < GroupCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + GroupDataOffset + (8 * i), SeekOrigin.Begin);

                GroupAnimationData groupData = new GroupAnimationData();
                groupData.Read(reader);

                GroupAnimationDataList.Add(groupData);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(GroupCount);
            writer.WriteUInt32(GroupDataOffset);

            for (int i = 0; i < GroupCount; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + GroupDataOffset + (8 * i), SeekOrigin.Begin);

                GroupAnimationDataList[i].Write(writer);
            }
        }
    }

    public class AnimationData2
    {
        public uint GroupAnimationData2ListOffset { get; set; }
        public GroupAnimationData2List GroupList { get; set; }

        public AnimationData2()
        {
            GroupList = new GroupAnimationData2List();
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
