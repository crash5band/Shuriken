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
        public uint DataOffset { get; set; }
        public List<CastAnimationSubData> SubDataList { get; set; }

        public CastAnimationData()
        {
            SubDataList = new List<CastAnimationSubData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            Flags = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();

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

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Flags);
            writer.WriteUInt32(DataOffset);

            if (DataOffset > 0)
            {
                writer.Seek(writer.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
                uint count = Utilities.CountSetBits(Flags);

                for (int i = 0; i < count; ++i)
                {
                    writer.Seek(writer.GetOffsetOrigin() + DataOffset + (12 * i), SeekOrigin.Begin);
                    SubDataList[i].Write(writer);
                }
            }
        }
    }

    public class CastAnimationSubData
    {
        public uint Field00 { get; set; }
        public uint KeyframeCount { get; set; }
        public uint DataOffset { get; set; }
        public List<Keyframe> Keyframes { get; set; }

        public CastAnimationSubData()
        {
            Keyframes = new List<Keyframe>();
        }

        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadUInt32();
            KeyframeCount = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();

            Keyframes.Capacity = (int)KeyframeCount;

            reader.Seek(reader.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
            for (int i = 0; i < KeyframeCount; ++i)
            {
                Keyframe key = new Keyframe();
                key.Read(reader);

                Keyframes.Add(key);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Field00);
            writer.WriteUInt32(KeyframeCount);
            writer.WriteUInt32(DataOffset);

            writer.Seek(writer.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
            for (int i = 0; i < KeyframeCount; ++i)
            {
                Keyframes[i].Write(writer);
            }
        }
    }

    public class CastAnimationData2List
    {
        public uint DataOffset { get; set; }
        public List<CastAnimationData2> ListData { get; set; }

        public CastAnimationData2List()
        {
            ListData = new List<CastAnimationData2>();
        }
    }

    public class CastAnimationData2
    {
        public uint DataOffset { get; set; }
        public Data5 Data { get; set; }

        public CastAnimationData2()
        {
            Data = new Data5();
        }
    }

    public class Data5
    {
        public uint DataOffset { get; set; }
        public List<Data6> SubData { get; set; }

        public Data5()
        {
            SubData = new List<Data6>();
        }
    }

    public class Data6
    {
        public uint DataOffset { get; set; }
        public Data7 Data { get; set; }

        public Data6()
        {
            Data = new Data7();
        }
    }

    public class Data7
    {
        public uint DataOffset { get; set; }
        public List<Data8> Data { get; set; }

        public Data7()
        {
            Data = new List<Data8>();
        }
    }

    public class Data8
    {
        public uint ValueOffset { get; set; }
        public Vector3 Value { get; set; }
    }
}
