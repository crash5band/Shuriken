﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

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

        public void Read(EndianBinaryReader reader)
        {
            SceneCount = reader.ReadUInt32();
            SceneTableOffset = reader.ReadUInt32();
            SceneIDTableOffset = reader.ReadUInt32();
            NodeCount = reader.ReadUInt32();
            NodeListOffset = reader.ReadUInt32();
            NodeDictionaryOffset = reader.ReadUInt32();

            reader.SeekBegin(reader.PeekBaseOffset() + SceneTableOffset);
            for (int i = 0; i < SceneCount; ++i)
            {
                SceneOffsets.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < SceneCount; ++i)
            {
                reader.SeekBegin(reader.PeekBaseOffset() + SceneOffsets[i]);

                Scene scene = new Scene();
                scene.Read(reader);

                Scenes.Add(scene);
            }

            reader.SeekBegin(reader.PeekBaseOffset() + SceneIDTableOffset);
            for (int i = 0; i < SceneCount; ++i)
            {
                SceneID id = new SceneID();
                id.Read(reader);

                SceneIDTable.Add(id);
            }
        }
    }
}