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
        public static NinjaType Type { get; set; }
        public static Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("shift-jis");
            }
        }

        public FAPCFile()
        {
            Resources = new FAPCEmbeddedRes[] { new FAPCEmbeddedRes(), new FAPCEmbeddedRes() };
        }

        public void Load(string filename, NinjaType type)
        {
            BinaryObjectReader reader = new BinaryObjectReader(filename, Endianness.Little, Encoding);
            Type = type;

            Signature = reader.ReadUInt32();
            if (Signature == Utilities.Make4CCLE("CPAF"))
            {
                Debug.Assert(Type == NinjaType.SWA);
                reader.Endianness = Endianness.Big;
            }

            Resources[0].Read(reader);
            Resources[1].Read(reader);

            reader.Dispose();
        }

        public void Save(string filename, NinjaType type)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(filename, Endianness.Little, Encoding);
            Type = type;

            writer.WriteUInt32(Signature);
            if (Signature == Utilities.Make4CCLE("CPAF"))
            {
                Debug.Assert(Type == NinjaType.SWA);
                writer.Endianness = Endianness.Big;
            }

            Resources[0].Write(writer);
            Resources[1].Write(writer);

            writer.Dispose();
        }
    }
}
