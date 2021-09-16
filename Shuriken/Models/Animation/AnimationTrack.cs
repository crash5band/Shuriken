using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Animation
{
    public class AnimationTrack
    {
        public uint Field00 { get; set; }
        public AnimationType Type { get; }
        public List<Keyframe> Keyframes { get; set; }

        public AnimationTrack(AnimationType type)
        {
            Type = type;
            Keyframes = new List<Keyframe>();
        }

        public float GetValue(float frame)
        {
            if (Keyframes.Count < 1)
                return 0.0f;

            if (frame >= Keyframes[^1].Frame)
                return Keyframes[^1].Value;

            if (Keyframes.Count > 1)
            {
                int k1 = 0, k2 = 1;
                while (Keyframes[k2].Frame < frame)
                {
                    ++k1;
                    ++k2;
                }

                float diff = Keyframes[k2].Frame - Keyframes[k1].Frame;
                float bias = (frame - Keyframes[k1].Frame) / diff;

                return Keyframes[k1].Value + bias * (Keyframes[k2].Value - Keyframes[k1].Value);
            }

            return 0.0f;
        }
    }
}
