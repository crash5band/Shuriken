using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class CSDNode
    {
        public List<Scene> Scenes { get; set; }
        public List<SceneID> SceneIDTable { get; set; }
        public List<CSDNode> NextNodes { get; set; }
        public List<NodeDictionary> NodeDictionaries { get; set; }
        private uint UnwrittenPosition { get; set; }

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

            uint nodeCount = reader.ReadUInt32();
            uint nodeListOffset = reader.ReadUInt32();
            uint nodeDictionaryOffset = reader.ReadUInt32();

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

            for (int i = 0; i < nodeCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + nodeListOffset + i * 0x18, SeekOrigin.Begin);

                CSDNode node = new CSDNode();
                node.Read(reader);

                NextNodes.Add(node);
            }

            reader.Seek(reader.GetOffsetOrigin() + nodeDictionaryOffset, SeekOrigin.Begin);
            for (int i = 0; i < nodeCount; ++i)
            {
                NodeDictionary id = new NodeDictionary();
                id.Read(reader);

                NodeDictionaries.Add(id);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer)
        {
            Debug.Assert(Scenes.Count == SceneIDTable.Count);

            // CSDNode Data memory should be already allocated
            UnwrittenPosition = (uint)writer.Position;
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);

            writer.WriteUInt32((uint)Scenes.Count);
            if (Scenes.Count == 0)
            {
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
            }
            else
            {
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length + Scenes.Count * 0x4 - writer.GetOffsetOrigin()));
            }

            writer.WriteUInt32((uint)NextNodes.Count);
            if (NextNodes.Count == 0)
            {
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
            }
            else
            {
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length + Scenes.Count * 0xC - writer.GetOffsetOrigin()));
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length + Scenes.Count * 0xC + NextNodes.Count * 0x18 - writer.GetOffsetOrigin()));
            }

            writer.Seek(0, SeekOrigin.End);
            UnwrittenPosition = (uint)writer.Position;

            if (Scenes.Count > 0)
            {
                // Allocate memory for SceneOffsets and SceneIDOffsets
                Utilities.PadZeroBytes(writer, Scenes.Count * 0xC);
            }
            
            if (NextNodes.Count > 0)
            {
                // Allocate memory for NodeListOffsets
                for (int i = 0; i < NextNodes.Count; i++)
                {
                    NextNodes[i].Write_Step0(writer);
                    Utilities.PadZeroBytes(writer, 0x18);
                }

                // Allocate memory for NodeDictionaryOffsets
                Utilities.PadZeroBytes(writer, NextNodes.Count * 0x8);
            }
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill SceneOffsets data
            uint newUnwrittenPosition = (uint)writer.Length;
            for (int f = 0; f < Scenes.Count; ++f)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x4;
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                // Allocate memory for Scene data
                writer.Seek(0, SeekOrigin.End);
                Utilities.PadZeroBytes(writer, 0x4C);
            }

            // Fill SceneIDOffsets data
            for (int i = 0; i < Scenes.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                offsetChunk.Add(writer);
                uint nameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                SceneIDTable[i].Write(writer, nameOffset);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step1(writer, offsetChunk);
                UnwrittenPosition += 0x18;
            }

            // Fill NodeDictionaries
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                offsetChunk.Add(writer);
                uint nameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                NodeDictionaries[i].Write(writer, nameOffset);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step3(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            for (int i = 0; i < Scenes.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                Scenes[i].Write_Step0(writer, offsetChunk);
                UnwrittenPosition += 0x4C;
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step2(writer, offsetChunk);
            }
        }

        public void Write_Step4(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step1(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step3(writer, offsetChunk);
            }
        }

        public void Write_Step5(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step2(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step4(writer, offsetChunk);
            }
        }

        public void Write_Step6(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step3(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step5(writer, offsetChunk);
            }
        }

        public void Write_Step7(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step4(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step6(writer, offsetChunk);
            }
        }

        public void Write_Step8(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step5(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step7(writer, offsetChunk);
            }
        }

        public void Write_Step9(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step6(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step8(writer, offsetChunk);
            }
        }

        public void Write_Step10(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step7(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step9(writer, offsetChunk);
            }
        }

        public void Write_Step11(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step8(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step10(writer, offsetChunk);
            }
        }

        public void Write_Step12(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step9(writer, offsetChunk);
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step11(writer, offsetChunk);
            }
        }

        public void Write_Step13(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue Scene steps
            for (int i = 0; i < Scenes.Count; ++i)
            {
                Scenes[i].Write_Step10(writer);
                // Finished
            }

            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step12(writer, offsetChunk);
            }
        }

        public void Write_Step14(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Continue NextNode steps
            for (int i = 0; i < NextNodes.Count; ++i)
            {
                NextNodes[i].Write_Step13(writer, offsetChunk);
                // Finished
            }
        }
    }
}
