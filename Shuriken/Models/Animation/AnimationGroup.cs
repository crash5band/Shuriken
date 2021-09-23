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
    public class AnimationGroup : INotifyPropertyChanged
    {
        private float time;
        public float Time
        {
            get => time;
            set
            {
                time = value;
                NotifyPropertyChanged();
            }
        }

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                NotifyPropertyChanged();
            }
        }

        public string Name { get; set; }
        public uint Field00 { get; set; }
        public float Duration { get; set; }
        public ObservableCollection<AnimationList> LayerAnimations { get; set; }

        public AnimationGroup(string name)
        {
            Name = name;
            Enabled = true;
            LayerAnimations = new ObservableCollection<AnimationList>();
        }

        public void AddTime(float delta)
        {
            Time += delta;
        }

        public void Reset()
        {
            Time = 0.0f;
        }

        public bool LayerHasAnimation(UILayer layer, AnimationType type)
        {
            foreach (var animation in LayerAnimations)
            {
                if (animation.Layer == layer)
                {
                    foreach (var track in animation.Tracks)
                    {
                        if (track.Type == type)
                            return true;
                    }
                }
            }

            return false;
        }

        public AnimationTrack GetTrack(UILayer layer, AnimationType type)
        {
            foreach (var animation in LayerAnimations)
            {
                if (animation.Layer == layer)
                {
                    return animation.GetTrack(type);
                }
            }

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
