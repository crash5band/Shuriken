using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class CastMaterialInfo
    {
        public int[] SubImageIndices;

        public CastMaterialInfo()
        {
            SubImageIndices = new int[32];
        }

        public void Read(BinaryObjectReader reader)
        {
            for (int i = 0; i < 32; ++i)
                SubImageIndices[i] = reader.ReadInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            for (int i = 0; i < 32; ++i)
            {
                writer.WriteInt32(SubImageIndices[i]);
            }
        }
    }
}
