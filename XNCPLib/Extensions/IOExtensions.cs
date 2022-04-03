using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using System.Numerics;

namespace XNCPLib.Extensions
{
    public static class IOExtensions
    {
        public static long GetOffsetOrigin(this BinaryObjectReader reader)
        {
            return reader.OffsetHandler.OffsetOrigin;
        }

        public static long GetOffsetOrigin(this BinaryObjectWriter writer)
        {
            return writer.OffsetHandler.OffsetOrigin;
        }
        public static string ReadStringOffset(this BinaryObjectReader reader, long offset)
        {
            if (offset == 0)
                return "";

            long savedPosition = reader.Position;
            reader.Seek(reader.GetOffsetOrigin() + offset, SeekOrigin.Begin);

            string result = reader.ReadString(StringBinaryFormat.NullTerminated);
            reader.Seek(savedPosition, SeekOrigin.Begin);
            
            return result;
        }

        public static void WriteStringOffset(this BinaryObjectWriter writer, long offset, string value)
        {
            if (offset == 0)
                return;

            long savedPosition = writer.Position;
            writer.Seek(writer.GetOffsetOrigin() + offset, SeekOrigin.Begin);

            writer.WriteString(StringBinaryFormat.NullTerminated, value);
            writer.Seek(savedPosition, SeekOrigin.Begin);
        }

        public static void SeekBegin(this BinaryObjectReader reader, long offset)
        {
            reader.Seek(offset, SeekOrigin.Begin);
        }

        public static Vector3 ReadVector3(this BinaryObjectReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
