﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNCPLib.Misc
{
    public class Utilities
    {
        public static uint Make4CCLE(string str)
        {
            return (uint)str[0] | (uint)str[1] << 8 | (uint)str[2] << 16 | (uint)str[3] << 24;
        }

        public static uint CountSetBits(uint i)
        {
            uint count = 0;
            for (int b = 0; b < 32; ++b)
            {
                if ((i & (1 << b)) != 0)
                    count += 1;
            }

            return count;
        }
    }
}