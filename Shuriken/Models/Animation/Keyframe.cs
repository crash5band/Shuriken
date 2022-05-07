using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XNCPLib.XNCP.Animation;

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

        public Color KValueColor { get; set; }

        public KeyframeType Type { get; set; }
        public float InTangent { get; set; }
        public float OutTangent { get; set; }
        public int Field14 { get; set; }
        public Vector3 Data8Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Keyframe()
        {
            Frame = 0;
            KValue = 0.0f;
            KValueColor = new Color();
            Type = 0;
            InTangent = 0.0f;
            OutTangent = 0.0f;
            Field14 = 0;
            Data8Value = new Vector3(0, 0, 0);
        }

        public Keyframe(XNCPLib.XNCP.Animation.Keyframe k, System.Numerics.Vector3 data8Value)
        {
            Frame = (int)k.Frame;
            KValue = k.Value;
            KValueColor = new Color(k.Value);
            Type = k.Type;
            InTangent = k.InTangent;
            OutTangent = k.OutTangent;
            Field14 = (int)k.Field14;
            Data8Value = new Vector3(data8Value.X, data8Value.Y, data8Value.Z);
        }

        public Keyframe(Keyframe k)
        {
            Frame = k.Frame;
            KValue = k.KValue;
            Type = k.Type;
            InTangent = k.InTangent;
            OutTangent = k.OutTangent;
            Field14 = k.Field14;
            Data8Value = k.Data8Value;
        }
    }
}
