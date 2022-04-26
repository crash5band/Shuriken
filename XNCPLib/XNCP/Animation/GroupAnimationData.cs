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
    public class GroupAnimationData
    {
        public List<CastAnimationData> CastAnimationDataList { get; set; }
        private uint UnwrittenPosition { get; set; }

        public GroupAnimationData()
        {
            CastAnimationDataList = new List<CastAnimationData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint CastCount = reader.ReadUInt32();
            uint CastDataOffset = reader.ReadUInt32();

            CastAnimationDataList.Capacity = (int)CastCount;

            for (int i = 0; i < CastCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + CastDataOffset + (8 * i), SeekOrigin.Begin);

                CastAnimationData animationData = new CastAnimationData();
                animationData.Read(reader);

                CastAnimationDataList.Add(animationData);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.WriteUInt32((uint)CastAnimationDataList.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

            // Allocate memory for CastAnimationDataList
            UnwrittenPosition = (uint)writer.Length;
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, CastAnimationDataList.Count * 0x8);
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill CastAnimationDataList
            for (int i = 0; i < CastAnimationDataList.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                CastAnimationDataList[i].Write_Step0(writer, offsetChunk);
            }
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue CastAnimationDataList steps
            for (int i = 0; i < CastAnimationDataList.Count; ++i)
            {
                CastAnimationDataList[i].Write_Step1(writer, offsetChunk);
                // Finished
            }
        }
    }

    public class GroupAnimationData2
    {
        public CastAnimationData2List AnimationData2List { get; set; }

        public GroupAnimationData2()
        {
        }
    }

    public class GroupAnimationData2List
    {
        public uint Field00 { get; set; }
        public List<GroupAnimationData2> GroupList { get; set; }

        public GroupAnimationData2List()
        {
        }
    }
}
