using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XNCPLib.XNCP.Animation;

namespace Shuriken.Models.Animation
{
    public class AnimationTrack : INotifyPropertyChanged
    {
        public bool Enabled { get; set; } = true;
        public uint Field00 { get; set; }
        public AnimationType Type { get; }
        public string TypeString => Type.ToString();
        public ObservableCollection<Keyframe> Keyframes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public int FindKeyframe(float frame)
        {
            int min = 0;
            int max = Keyframes.Count - 1;

            while (min <= max)
            {
                int index = (min + max) / 2;

                if (frame < Keyframes[index].Frame)
                    max = index - 1;
                else
                    min = index + 1;
            }

            return min;
        }

        public float GetSingle(float frame)
        {
            if (Keyframes.Count == 0)
                return 0.0f;

            if (frame >= Keyframes[^1].Frame)
                return Keyframes[^1].KValue;

            int index = FindKeyframe(frame);

            if (index == 0)
                return Keyframes[index].KValue;

            var keyframe = Keyframes[index - 1];
            var nextKeyframe = Keyframes[index];

            float factor;

            if (nextKeyframe.Frame - keyframe.Frame > 0)
                factor = (frame - keyframe.Frame) / (nextKeyframe.Frame - keyframe.Frame);
            else
                factor = 0.0f;

            switch (keyframe.Type)
            {
                case KeyframeType.Linear:
                    return (1.0f - factor) * keyframe.KValue + nextKeyframe.KValue * factor;

                case KeyframeType.Hermite:
                    float valueDelta = nextKeyframe.KValue - keyframe.KValue;
                    float frameDelta = nextKeyframe.Frame - keyframe.Frame;

                    float biasSquaric = factor * factor;
                    float biasCubic = biasSquaric * factor;

                    float valueCubic = (keyframe.OutTangent + keyframe.InTangent) * frameDelta - valueDelta * 2.0f;
                    float valueSquaric = valueDelta * 3.0f - (keyframe.InTangent * 2.0f + keyframe.OutTangent) * frameDelta;
                    float valueLinear = frameDelta * keyframe.InTangent;

                    return valueCubic * biasCubic + valueSquaric * biasSquaric + valueLinear * factor + keyframe.KValue;

                default:
                    return keyframe.KValue;
            }
        }

        public Color GetColor(float frame)
        {
            if (Keyframes.Count == 0)
                return new Color();

            if (frame >= Keyframes[^1].Frame)
                return Keyframes[^1].KValueColor;

            int index = FindKeyframe(frame);

            if (index == 0)
                return Keyframes[index].KValueColor;

            var keyframe = Keyframes[index - 1];
            var nextKeyframe = Keyframes[index];

            float factor;

            if (nextKeyframe.Frame - keyframe.Frame > 0)
                factor = (frame - keyframe.Frame) / (nextKeyframe.Frame - keyframe.Frame);
            else
                factor = 0.0f;

            // Color values always use linear interpolation regardless of the type.

            return new Color
            {
                R = (byte)((1.0f - factor) * keyframe.KValueColor.R + nextKeyframe.KValueColor.R * factor),
                G = (byte)((1.0f - factor) * keyframe.KValueColor.G + nextKeyframe.KValueColor.G * factor),
                B = (byte)((1.0f - factor) * keyframe.KValueColor.B + nextKeyframe.KValueColor.B * factor),
                A = (byte)((1.0f - factor) * keyframe.KValueColor.A + nextKeyframe.KValueColor.A * factor)
            };
        }

        public AnimationTrack(AnimationType type)
        {
            Type = type;
            Keyframes = new ObservableCollection<Keyframe>();
        }

        public AnimationTrack(AnimationTrack a)
        {
            Type = a.Type;
            Keyframes = new ObservableCollection<Keyframe>(a.Keyframes);
        }
    }
}
