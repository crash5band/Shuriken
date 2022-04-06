using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Animation
{
    public class Keyframe : INotifyPropertyChanged
    {
        private int frame;
        public int Frame
        {
            get => frame;
            set
            {
                if (value >= 0) 
                {
                    frame = value;
                    HasNoFrame = false;
                }
                else
                {
                    HasNoFrame = true;
                }
            }
        }
        public bool HasNoFrame { get; set; }
        public float KValue { get; set; }
        public Color KValueColor
        {
            get => new Color(KValue);
            set => KValue = value.ToUint();
        }
        public int Field08 { get; set; }
        public float Offset1 { get; set; }
        public float Offset2 { get; set; }
        public int Field14 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Keyframe()
        {
            Frame = 0;
            KValue = 0.0f;
            Field08 = 0;
            Offset1 = 0.0f;
            Offset2 = 0.0f;
            Field14 = 0;
        }

        public Keyframe(XNCPLib.XNCP.Animation.Keyframe k)
        {
            Frame = (int)k.Frame;
            KValue = k.Value;
            Field08 = (int)k.Field08;
            Offset1 = k.Offset1;
            Offset2 = k.Offset2;
            Field14 = (int)k.Field14;
        }
    }
}
