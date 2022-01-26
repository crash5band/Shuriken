using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Enums
{
    [Flags]
    internal enum InheritFlags
    {
        Flag1 = 1,
        Flag2 = 2,
        Flag4 = 4,
        InheritColor = 8,
        Flag16 = 16,
        Flag32 = 32,
        Flag64 = 64,
        Flag128 = 128,
        Flag256 = 256,
        Flag512 = 512,
        InheritScaleX = 1024,
        InheritScaleY = 2048,
        Flag4096 = 4096,
    }
}
