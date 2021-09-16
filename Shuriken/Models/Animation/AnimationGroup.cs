using System;
using System.Collections.Generic;
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
        public Dictionary<UILayer, List<AnimationTrack>> LayerAnimations { get; set; }

        public AnimationGroup(string name)
        {
            Name = name;
            Enabled = true;
            LayerAnimations = new Dictionary<UILayer, List<AnimationTrack>>();
        }

        public bool LayerHasAnimation(UILayer layer, AnimationType type)
        {
            if (!LayerAnimations.ContainsKey(layer))
                return false;

            List<AnimationTrack> animationList = LayerAnimations[layer];
            foreach (var animation in animationList)
            {
                if (animation.Type == type)
                    return true;
            }

            return false;
        }

        public AnimationTrack GetAnimation(UILayer layer, AnimationType type)
        {
            if (!LayerHasAnimation(layer, type))
                return null;

            List<AnimationTrack> animationList = LayerAnimations[layer];
            foreach (var animation in animationList)
            {
                if (animation.Type == type)
                    return animation;
            }

            return null;
        }

        public void AddTime(float delta)
        {
            Time += delta;
        }

        public void Reset()
        {
            Time = 0.0f;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
