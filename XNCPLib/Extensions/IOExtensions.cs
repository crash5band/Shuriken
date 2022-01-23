using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;

namespace XNCPLib.Extensions
{
    public static class IOExtensions
    {
        public static long GetOffsetOrigin(this BinaryObjectReader reader)
        {
            return reader.OffsetHandler.OffsetOrigin;
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
    }
}
