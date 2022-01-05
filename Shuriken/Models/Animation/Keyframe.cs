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
            set { frame = value; NotifyPropertyChanged(); }
        }

        private float kValue;
        public float KValue
        {
            get => kValue;
            set { kValue = value; NotifyPropertyChanged(); }
        }
        public int Field08 { get; set; }
        public float Offset1 { get; set; }
        public float Offset2 { get; set; }
        public int Field14 { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
