using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Misc;

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
            BinaryObjectReader reader = new BinaryObjectReader(filename, Endianness.Little, Encoding.UTF8);

            Signature = reader.ReadUInt32();
            if (Signature == Utilities.Make4CCLE("CPAF"))
                reader.Endianness = Endianness.Big;

            Resources[0].Read(reader);
            Resources[1].Read(reader);

            reader.Dispose();
        }

        public void Save(string filename)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(filename, Endianness.Little, Encoding.UTF8);

            writer.WriteUInt32(Signature);
            if (Signature == Utilities.Make4CCLE("CPAF"))
            {
                writer.Endianness = Endianness.Big;
            }

            Resources[0].Write(writer);
            Resources[1].Write(writer);

            writer.Dispose();
        }
    }
}
