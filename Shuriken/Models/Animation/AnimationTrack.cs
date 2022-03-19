using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public float GetValue(float frame)
        {
            if (Keyframes.Count < 1)
                return 0.0f;

            if (frame >= Keyframes[^1].Frame)
                return Keyframes[^1].KValue;

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

                return Keyframes[k1].KValue + bias * (Keyframes[k2].KValue - Keyframes[k1].KValue);
            }

            return 0.0f;
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
