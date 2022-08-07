using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("shift-jis");
            }
        }

        public FAPCFile()
        {
            Resources = new[] { new FAPCEmbeddedRes(), new FAPCEmbeddedRes() };
        }

        public void Load(string filename)
        {
            BinaryObjectReader reader = new BinaryObjectReader(filename, Endianness.Little, Encoding);

            Signature = reader.ReadUInt32();
            if (Signature == Utilities.Make4CCLE("CPAF")) 
                reader.Endianness = Endianness.Big;

            Resources[0].Read(reader);
            Resources[1].Read(reader);

            reader.Dispose();
        }

        public void Save(string filename)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(filename, Endianness.Little, Encoding);

            if (filename.EndsWith("yncp"))
            {
                writer.Endianness = Endianness.Big;
            }

            Signature = Utilities.Make4CCLE("FAPC");
            writer.WriteUInt32(Signature);

            Resources[0].Write(writer);
            Resources[1].Write(writer);

            writer.Dispose();
        }
    }
}
