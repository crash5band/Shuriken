using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class CSDNode
    {
        public uint SceneCount { get; set; }
        public uint SceneTableOffset { get; set; }
        public uint SceneIDTableOffset { get; set; }
        public uint NodeCount { get; set; }
        public uint NodeListOffset { get; set; }
        public uint NodeDictionaryOffset { get; set; }
        public List<uint> SceneOffsets { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<SceneID> SceneIDTable { get; set; }
        public List<CSDNode> NextNodes { get; set; }
        public List<NodeDictionary> NodeDictionaries { get; set; }

        public CSDNode()
        {
            Scenes = new List<Scene>();
            SceneOffsets = new List<uint>();
            SceneIDTable = new List<SceneID>();
            NextNodes = new List<CSDNode>();
            NodeDictionaries = new List<NodeDictionary>();
        }

        /// <summary>
        /// TODO: handle multiple nodes.
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryObjectReader reader)
        {
            SceneCount = reader.ReadUInt32();
            SceneTableOffset = reader.ReadUInt32();
            SceneIDTableOffset = reader.ReadUInt32();
            NodeCount = reader.ReadUInt32();
            NodeListOffset = reader.ReadUInt32();
            NodeDictionaryOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + SceneTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < SceneCount; ++i)
            {
                SceneOffsets.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < SceneCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + SceneOffsets[i], SeekOrigin.Begin);

                Scene scene = new Scene();
                scene.Read(reader);

                Scenes.Add(scene);
            }

            reader.Seek(reader.GetOffsetOrigin() + SceneIDTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < SceneCount; ++i)
            {
                SceneID id = new SceneID();
                id.Read(reader);

                SceneIDTable.Add(id);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(SceneCount);
            writer.WriteUInt32(SceneTableOffset);
            writer.WriteUInt32(SceneIDTableOffset);
            writer.WriteUInt32(NodeCount);
            writer.WriteUInt32(NodeListOffset);
            writer.WriteUInt32(NodeDictionaryOffset);

            writer.Seek(writer.GetOffsetOrigin() + SceneTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < SceneCount; ++i)
            {
                writer.WriteUInt32(SceneOffsets[i]);
            }

            for (int i = 0; i < SceneCount; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + SceneOffsets[i], SeekOrigin.Begin);
                Scenes[i].Write(writer);
            }

            writer.Seek(writer.GetOffsetOrigin() + SceneIDTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < SceneCount; ++i)
            {
                SceneIDTable[i].Write(writer);
            }
        }
    }
}
