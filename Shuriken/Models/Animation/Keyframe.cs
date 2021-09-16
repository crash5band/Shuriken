using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Animation
{
    public class Keyframe
    {
        public int Frame { get; set; }
        public float Value { get; set; }
        public int Field08 { get; set; }
        public float Offset1 { get; set; }
        public float Offset2 { get; set; }
        public int Field14 { get; set; }

        public Keyframe()
        {
            Frame = 0;
            Value = 0.0f;
            Field08 = 0;
            Offset1 = 0.0f;
            Offset2 = 0.0f;
            Field14 = 0;
        }

        public Keyframe(XNCPLib.XNCP.Animation.Keyframe k)
        {
            Frame = (int)k.Frame;
            Value = k.Value;
            Field08 = (int)k.Field08;
            Offset1 = k.Offset1;
            Offset2 = k.Offset2;
            Field14 = (int)k.Field14;
        }
    }
}
