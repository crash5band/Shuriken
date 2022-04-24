using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class CSDNode
    {
        public uint NodeCount { get; set; }
        public uint NodeListOffset { get; set; }
        public uint NodeDictionaryOffset { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<SceneID> SceneIDTable { get; set; }
        public List<CSDNode> NextNodes { get; set; }
        public List<NodeDictionary> NodeDictionaries { get; set; }

        public CSDNode()
        {
            Scenes = new List<Scene>();
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
            uint sceneCount = reader.ReadUInt32();
            uint sceneTableOffset = reader.ReadUInt32();
            uint sceneIDTableOffset = reader.ReadUInt32();

            // TODO: Sub nodes
            NodeCount = reader.ReadUInt32();
            NodeListOffset = reader.ReadUInt32();
            NodeDictionaryOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + sceneTableOffset, SeekOrigin.Begin);
            List<uint> sceneOffsets = new();
            for (int i = 0; i < sceneCount; ++i)
            {
                sceneOffsets.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < sceneCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + sceneOffsets[i], SeekOrigin.Begin);

                Scene scene = new Scene();
                scene.Read(reader);

                Scenes.Add(scene);
            }

            reader.Seek(reader.GetOffsetOrigin() + sceneIDTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < sceneCount; ++i)
            {
                SceneID id = new SceneID();
                id.Read(reader);

                SceneIDTable.Add(id);
            }
        }

        public void Write(BinaryObjectWriter writer, uint sceneListOffset, uint sceneDataOffset)
        {
            Debug.Assert(Scenes.Count == SceneIDTable.Count);

            writer.WriteUInt32((uint)Scenes.Count);

            uint sceneIDListOffset = 0;
            if (Scenes.Count == 0)
            {
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
            }
            else
            {
                writer.WriteUInt32(sceneListOffset);
                sceneIDListOffset = sceneListOffset + (uint)Scenes.Count * 0x4;
                writer.WriteUInt32(sceneIDListOffset);
            }

            // TODO: Sub nodes
            writer.WriteUInt32(NodeCount);
            writer.WriteUInt32(NodeListOffset);
            writer.WriteUInt32(NodeDictionaryOffset);

            List<uint> sceneOffsets = new();
            for (int i = 0; i < Scenes.Count; ++i)
            {
                sceneOffsets.Add(sceneDataOffset);
                sceneDataOffset += 0x4C;
            }

            writer.Seek(writer.GetOffsetOrigin() + sceneListOffset, SeekOrigin.Begin);
            for (int i = 0; i < Scenes.Count; ++i)
            {
                writer.WriteUInt32(sceneOffsets[i]);
            }

            for (int i = 0; i < Scenes.Count; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + sceneOffsets[i], SeekOrigin.Begin);
                Scenes[i].Write(writer);
            }

            writer.Seek(writer.GetOffsetOrigin() + sceneIDListOffset, SeekOrigin.Begin);
            for (int i = 0; i < Scenes.Count; ++i)
            {
                SceneIDTable[i].Write(writer, sceneDataOffset);

                // Get the next name offset
                int nameLength = SceneIDTable[i].Name.Length + 1;
                int unalignedBytes = nameLength % 0x4;
                if (unalignedBytes != 0)
                {
                    nameLength += 0x4 - unalignedBytes;
                }
                sceneDataOffset += (uint)nameLength;
            }
        }
    }
}
