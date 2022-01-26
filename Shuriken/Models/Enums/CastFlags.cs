using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Enums
{
    [Flags]
    internal enum CastFlags
    {
        ResetPos = 1,
        Flag2 = 2,
        Flag4 = 4,
        Flag8 = 8,
        Flag16 = 16,
        Flag32 = 32,
        Flag64 = 64,
        Flag128 = 128,
        Flag256 = 256,
        Flag512 = 512,
        FlipX = 1024,
        FlipY = 2048,
    }
}
