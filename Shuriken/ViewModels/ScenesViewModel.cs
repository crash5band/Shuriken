using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using Shuriken.Commands;

namespace Shuriken.ViewModels
{
    public class ScenesViewModel : ViewModelBase
    {
        public float MinZoom => 0.25f;
        public float MaxZoom => 2.50f;

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

        private bool playing;
        public bool Playing
        {
            get => playing;
            set
            {
                playing = value;
                NotifyPropertyChanged();
            }
        }

        private float playbackSpeed;
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set
            {
                playbackSpeed = value;
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
            get => togglePlayingCmd ?? new RelayCommand(() => Playing ^= true, null);
            set
            {
                togglePlayingCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand stopPlayingCmd;
        public RelayCommand StopPlayingCmd
        {
            get => stopPlayingCmd ?? new RelayCommand(Stop, null);
            set
            {
                stopPlayingCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand replayCmd;
        public RelayCommand ReplayCmd
        {
            get => replayCmd ?? new RelayCommand(Replay, null);
            set
            {
                replayCmd = value;
                NotifyPropertyChanged();
            }
        }

        private RelayCommand<float> seekCmd;
        public RelayCommand<float> SeekCmd
        {
            get => seekCmd ?? new RelayCommand<float>(Seek, () => !Playing);
            set
            {
                seekCmd = value;
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

        public void Stop()
        {
            Playing = false;
            Time = 0.0f;
        }

        public void Replay()
        {
            Stop();
            TogglePlaying();
        }

        public void Seek(object frames)
        {
            float f = (float)frames;
            if (Time + f >= 0.0f)
                Time += f;
        }

        public void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float deltaT)
        {
            Time += deltaT * PlaybackSpeed * (Playing ? 1 : 0);
            foreach (UIScene scene in scenes)
            {
                if (!scene.Visible)
                    continue;

                foreach (LayerGroup group in scene.Groups)
                {
                    if (!group.Visible)
                        continue;

                    foreach (UILayer layer in group.Layers)
                    {

                    }
                }
            }
        }

        public ObservableCollection<UIScene> Scenes => Project.Scenes;

        private UIScene scene;
        public UIScene SelectedScene
        {
            get { return scene; }
            set { scene = value; NotifyPropertyChanged(); }
        }

        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";

            zoom = 0.65f;
            playbackSpeed = 1.0f;
        }
    }
}
