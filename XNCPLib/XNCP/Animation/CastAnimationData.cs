using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP.Animation
{
    public class CastAnimationData
    {
        public uint Flags { get; set; }
        public List<CastAnimationSubData> SubDataList { get; set; }
        private uint UnwrittenPosition { get; set; }

        public CastAnimationData()
        {
            SubDataList = new List<CastAnimationSubData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            Flags = reader.ReadUInt32();
            uint DataOffset = reader.ReadUInt32();

            if (DataOffset > 0)
            {
                reader.Seek(reader.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
                uint count = Utilities.CountSetBits(Flags);
                SubDataList.Capacity = (int)count;


                for (int i = 0; i < count; ++i)
                {
                    reader.Seek(reader.GetOffsetOrigin() + DataOffset + (12 * i), SeekOrigin.Begin);
                    CastAnimationSubData subData = new CastAnimationSubData();
                    subData.Read(reader);
                    SubDataList.Add(subData);
                }
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            UnwrittenPosition = (uint)writer.Length;
            writer.WriteUInt32(Flags);

            if (Flags != 0)
            {
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                uint count = Utilities.CountSetBits(Flags);
                writer.Seek(0, SeekOrigin.End);
                Utilities.PadZeroBytes(writer, (int)count * 0xC);
            }
            else
            {
                writer.WriteUInt32(0);
            }
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            uint count = Utilities.CountSetBits(Flags);
            for (int i = 0; i < count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0xC;

                SubDataList[i].Write_Step0(writer, offsetChunk);
                // Finished
            }
        }
    }

    public class CastAnimationSubData
    {
        public uint Field00 { get; set; }
        public List<Keyframe> Keyframes { get; set; }

        public CastAnimationSubData()
        {
            Keyframes = new List<Keyframe>();
        }

        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadUInt32();
            uint KeyframeCount = reader.ReadUInt32();
            uint DataOffset = reader.ReadUInt32();

            Keyframes.Capacity = (int)KeyframeCount;

            reader.Seek(reader.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
            for (int i = 0; i < KeyframeCount; ++i)
            {
                Keyframe key = new Keyframe();
                key.Read(reader);

                Keyframes.Add(key);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.WriteUInt32(Field00);
            writer.WriteUInt32((uint)Keyframes.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

            writer.Seek(0, SeekOrigin.End);
            for (int i = 0; i < Keyframes.Count; ++i)
            {
                Keyframes[i].Write(writer);
            }
        }
    }

    public class CastAnimationData2List
    {
        public List<CastAnimationData2> ListData { get; set; }

        public CastAnimationData2List()
        {
        }
    }

    public class CastAnimationData2
    {
        public Data5 Data { get; set; }

        public CastAnimationData2()
        {
        }
    }

    public class Data5
    {
        public List<Data6> SubData { get; set; }

        public Data5()
        {
        }
    }

    public class Data6
    {
        public Data7 Data { get; set; }

        public Data6()
        {
        }
    }

    public class Data7
    {
        public List<Data8> Data { get; set; }

        public Data7()
        {
        }
    }

    public class Data8
    {
        public bool IsUsed { get; set; }
        public Vector3 Value { get; set; }
    }
}
