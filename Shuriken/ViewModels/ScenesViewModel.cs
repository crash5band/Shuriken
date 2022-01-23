using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Shuriken.Models;
using Shuriken.Commands;
using Shuriken.Views;

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
            set { time = value; NotifyPropertyChanged(); }
        }

        private bool playing;
        public bool Playing
        {
            get => playing;
            set { playing = value; NotifyPropertyChanged(); }
        }

        private float playbackSpeed;
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set { playbackSpeed = value; NotifyPropertyChanged(); }
        }

        private float zoom;
        public float Zoom
        {
            get => zoom;
            set { zoom = Math.Clamp(value, MinZoom, MaxZoom); NotifyPropertyChanged(); }
        }

        private RelayCommand togglePlayingCmd;
        public RelayCommand TogglePlayingCmd
        {
            get => togglePlayingCmd ?? new RelayCommand(() => Playing ^= true, null);
            set { togglePlayingCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand stopPlayingCmd;
        public RelayCommand StopPlayingCmd
        {
            get => stopPlayingCmd ?? new RelayCommand(Stop, null);
            set { stopPlayingCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand replayCmd;
        public RelayCommand ReplayCmd
        {
            get => replayCmd ?? new RelayCommand(Replay, null);
            set { replayCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand<float> seekCmd;
        public RelayCommand<float> SeekCmd
        {
            get => seekCmd ?? new RelayCommand<float>(Seek, () => !Playing);
            set { seekCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand zoomOutCmd;
        public RelayCommand ZoomOutCmd
        {
            get => zoomOutCmd ?? new RelayCommand(() => Zoom -= 0.25f, null);
            set { zoomOutCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand zoomInCmd;
        public RelayCommand ZoomInCmd
        {
            get => zoomInCmd ?? new RelayCommand(() => Zoom += 0.25f, null);
            set { zoomInCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createSceneCmd;
        public RelayCommand CreateSceneCmd
        {
            get => createSceneCmd ?? new RelayCommand(CreateScene, null);
            set { createSceneCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeSceneCmd;
        public RelayCommand RemoveSceneCmd
        {
            get => removeSceneCmd ?? new RelayCommand(RemoveScene, () => SelectedScene != null);
            set { removeSceneCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createGroupCmd;
        public RelayCommand CreateGroupCmd
        {
            get { return createGroupCmd ?? new RelayCommand(CreateGroup, () => SelectedScene != null); }
            set { createGroupCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeGroupCmd;
        public RelayCommand RemoveGroupCmd
        {
            get { return removeGroupCmd ?? new RelayCommand(RemoveGroup, () => SelectedNode != null && SelectedNode is LayerGroup); }
            set { removeGroupCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand<int> changeCastSpriteCmd;
        public RelayCommand<int> ChangeCastSpriteCmd
        {
            get { return changeCastSpriteCmd ?? new RelayCommand<int>(SelectCastSprite, null); }
            set { changeCastSpriteCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createCastCmd;
        public RelayCommand CreateCastCmd
        {
            get { return createCastCmd ?? new RelayCommand(CreateCast, () => SelectedNode != null && SelectedNode is not UIScene); }
            set { createCastCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeCastCmd;
        public RelayCommand RemoveCastCmd
        {
            get { return removeCastCmd ?? new RelayCommand(RemoveCast, () => SelectedNode is UILayer); }
            set { removeCastCmd = value; NotifyPropertyChanged(); }
        }

        public void SelectCastSprite(object index)
        {
            SpritePickerWindow dialog = new SpritePickerWindow();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                if (dialog.SelectedSprite != null)
                {
                    var sprIndex = (int)index;
                    ChangeCastSprite(sprIndex, dialog.SelectedSprite);
                }
            }
        }

        public void ChangeCastSprite(int index, Sprite spr)
        {
            if (SelectedNode is UILayer)
            {
                var cast = (UILayer)SelectedNode;
                cast.Sprites[index] = spr;
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

        public void Tick(float d)
        {
            Time += d * playbackSpeed * (playing ? 1 : 0);
        }

        public void CreateScene()
        {
            Scenes.Add(new UIScene("scene"));
        }

        public void RemoveScene()
        {
            if (SelectedScene != null)
                Scenes.Remove(SelectedScene);
        }

        public void CreateGroup()
        {
            if (SelectedScene != null)
                SelectedScene.Groups.Add(new LayerGroup());
        }

        public void RemoveGroup()
        {
            if (SelectedNode is LayerGroup)
            {
                var group = SelectedNode as LayerGroup;
                SelectedScene.Groups.Remove(group);
            }
        }

        public void CreateCast()
        {
            if (SelectedNode is LayerGroup)
            {
                (SelectedNode as LayerGroup).Layers.Add(new UILayer());
            }
            else if (SelectedNode is UILayer)
            {
                (SelectedNode as UILayer).Children.Add(new UILayer());
            }
        }

        public void RemoveCast()
        {
            if (SelectedNode != null && ParentNode != null)
            {
                if (ParentNode is LayerGroup)
                {
                    var parent = ParentNode as LayerGroup;
                    var cast = SelectedNode as UILayer;
                    if (cast != null)
                        parent.Layers.Remove(cast);
                }
                else if (ParentNode is UILayer)
                {
                    var parent = ParentNode as UILayer;
                    var cast = SelectedNode as UILayer;
                    if (cast != null)
                        parent.Children.Remove(cast);
                }
            }
        }

        public void RemoveCast(LayerGroup group, UILayer cast)
        {
            group.Layers.Remove(cast);
        }

        public void RemoveCast(UILayer parent, UILayer cast)
        {
            parent.Children.Remove(cast);
        }


        private UIScene scene;
        public UIScene SelectedScene
        {
            get { return scene; }
            set { scene = value; NotifyPropertyChanged(); }
        }

        private object parentNode;
        public object ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; NotifyPropertyChanged(); }
        }

        private object selectedNode;
        public object SelectedNode
        {
            get { return selectedNode; }
            set { selectedNode = value; NotifyPropertyChanged(); }
        }
        public ObservableCollection<UIScene> Scenes => Project.Scenes;
        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";

            zoom = 0.65f;
            playbackSpeed = 1.0f;
        }
    }
}
