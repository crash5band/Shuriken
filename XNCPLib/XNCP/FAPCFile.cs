using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class FAPCFile
    {
        public uint Signature { get; set; }
        public FAPCEmbeddedRes[] Resources { get; set; }

        public FAPCFile()
        {
            Resources = new FAPCEmbeddedRes[] { new FAPCEmbeddedRes(), new FAPCEmbeddedRes() };
        }

        public void Load(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Open);
            EndianBinaryReader reader = new EndianBinaryReader(stream, Endianness.LittleEndian);

            Signature = reader.ReadUInt32();
            if (Signature == Utilities.Utilities.Make4CCLE("CPAF"))
                reader.Endianness = Endianness.BigEndian;

            Resources[0].Read(reader);
            Resources[1].Read(reader);

            reader.Close();
            stream.Close();

            reader.Dispose();
            stream.Dispose();
        }
    }
}
