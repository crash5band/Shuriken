using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Animation
{
    public class AnimationList
    {
        public UICast Layer { get; set; }
        public ObservableCollection<AnimationTrack> Tracks { get; set; }

        public bool HasAnimation(AnimationType type)
        {
            foreach (var track in Tracks)
            {
                if (track.Type == type)
                    return true;
            }

            return false;
        }

        public AnimationTrack GetTrack(AnimationType type)
        {
            foreach (var track in Tracks)
            {
                if (track.Type == type)
                    return track;
            }

            return null;
        }

        public AnimationList(UICast lyr, List<AnimationTrack> tracks)
        {
            Layer = lyr;
            Tracks = new ObservableCollection<AnimationTrack>(tracks);
        }

        public AnimationList(UICast lyr)
        {
            Layer = lyr;
            Tracks = new ObservableCollection<AnimationTrack>();
        }
    }
}
