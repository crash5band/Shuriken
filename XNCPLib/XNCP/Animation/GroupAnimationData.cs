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
    public class GroupAnimationData
    {
        public uint CastCount { get; set; }
        public uint CastDataOffset { get; set; }
        public List<CastAnimationData> CastAnimationDataList { get; set; }

        public GroupAnimationData()
        {
            CastAnimationDataList = new List<CastAnimationData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            CastCount = reader.ReadUInt32();
            CastDataOffset = reader.ReadUInt32();

            CastAnimationDataList.Capacity = (int)CastCount;

            for (int i = 0; i < CastCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + CastDataOffset + (8 * i), SeekOrigin.Begin);

                CastAnimationData animationData = new CastAnimationData();
                animationData.Read(reader);

                CastAnimationDataList.Add(animationData);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(CastCount);
            writer.WriteUInt32(CastDataOffset);

            for (int i = 0; i < CastCount; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + CastDataOffset + (8 * i), SeekOrigin.Begin);

                CastAnimationDataList[i].Write(writer);
            }
        }
    }

    public class GroupAnimationData2
    {
        public uint DataOffset { get; set; }
        public CastAnimationData2List AnimationData2List { get; set; }

        public GroupAnimationData2()
        {
            AnimationData2List = new CastAnimationData2List();
        }
    }

    public class GroupAnimationData2List
    {
        public uint Field00 { get; set; }
        public uint DataOffset { get; set; }
        public List<GroupAnimationData2> GroupList { get; set; }

        public GroupAnimationData2List()
        {
            GroupList = new List<GroupAnimationData2>();
        }
    }
}
