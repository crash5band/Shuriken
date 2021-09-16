using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using Shuriken.Commands;

namespace Shuriken.ViewModels
{
    public class ScenesManagerViewModel : ViewModelBase
    {
        ScenesManager manager;

        public float MinZoom => 0.25f;
        public float MaxZoom => 2.50f;
        private bool stopping;

        public float Time
        {
            get => manager.Time;
            set
            {
                manager.Time = value;
                NotifyPropertyChanged();
            }
        }

        public bool Playing
        {
            get => manager.Playing;
            set
            {
                manager.Playing = value;
                NotifyPropertyChanged();
            }
        }

        public float PlaybackSpeed
        {
            get => manager.Speed;
            set
            {
                manager.Speed = value;
                NotifyPropertyChanged();
            }
        }

        private float zoom;
        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = Math.Clamp(value, MinZoom, MaxZoom);
                NotifyPropertyChanged();
            }
        }

        private RelayCommand togglePlayingCmd;
        public RelayCommand TogglePlayingCmd
        {
            get => togglePlayingCmd ?? new RelayCommand(TogglePlaying, null);
            set
            {
                togglePlayingCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand stopPlayingCmd;
        public RelayCommand StopPlayingCmd
        {
            get => stopPlayingCmd ?? new RelayCommand(StopPlaying, null);
            set
            {
                stopPlayingCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand zoomOutCmd;
        public RelayCommand ZoomOutCmd
        {
            get => zoomOutCmd ?? new RelayCommand(() => Zoom -= 0.25f, null);
            set
            {
                zoomOutCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand zoomInCmd;
        public RelayCommand ZoomInCmd
        {
            get => zoomInCmd ?? new RelayCommand(() => Zoom += 0.25f, null);
            set
            {
                zoomInCmd = value;
                NotifyPropertyChanged();
            }
        }

        public void TogglePlaying()
        {
            Playing ^= true;
        }

        public void StopPlaying()
        {
            Playing = false;
            Time = 0.0f;
            stopping = true;
        }

        public void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float deltaT)
        {
            Time += deltaT * PlaybackSpeed * (Playing ? 1 : 0);
            manager.UpdateScenes(scenes, fonts, deltaT);

            if (stopping)
            {
                foreach (var scene in scenes)
                {
                    foreach (var animation in scene.Animations)
                    {
                        animation.Reset();
                    }
                }

                stopping = false;
            }
        }

        public ScenesManagerViewModel()
        {
            manager = new ScenesManager();
            zoom = 0.65f;
            stopping = false;
        }
    }
}
