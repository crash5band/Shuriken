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
            reader.SeekL(reader.GetOffsetOrigin() + offset, SeekOrigin.Begin);

            string result = reader.ReadString(StringBinaryFormat.NullTerminated);
            reader.SeekL(savedPosition, SeekOrigin.Begin);
            
            return result;
        }
        public static void SeekL(this BinaryObjectReader reader, long offset, SeekOrigin origin)
        {
            try
            {
                reader.Seek(offset, origin);
            }
            catch (Exception ex)
            {
                Log.Exception(ex.ToString(), "IOExtensions");
            }
            
        }
    }
}
