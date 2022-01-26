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
            set { time = value; }
        }

        private bool playing;
        public bool Playing
        {
            get => playing;
            set { playing = value; }
        }

        private float playbackSpeed;
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set { playbackSpeed = value; }
        }

        private float zoom;
        public float Zoom
        {
            get => zoom;
            set { zoom = Math.Clamp(value, MinZoom, MaxZoom); }
        }

        private RelayCommand togglePlayingCmd;
        public RelayCommand TogglePlayingCmd
        {
            get => togglePlayingCmd ?? new RelayCommand(() => Playing ^= true, null);
            set { togglePlayingCmd = value; }
        }

        private RelayCommand stopPlayingCmd;
        public RelayCommand StopPlayingCmd
        {
            get => stopPlayingCmd ?? new RelayCommand(Stop, null);
            set { stopPlayingCmd = value; }
        }

        private RelayCommand replayCmd;
        public RelayCommand ReplayCmd
        {
            get => replayCmd ?? new RelayCommand(Replay, null);
            set { replayCmd = value; }
        }

        private RelayCommand<float> seekCmd;
        public RelayCommand<float> SeekCmd
        {
            get => seekCmd ?? new RelayCommand<float>(Seek, () => !Playing);
            set { seekCmd = value; }
        }

        private RelayCommand zoomOutCmd;
        public RelayCommand ZoomOutCmd
        {
            get => zoomOutCmd ?? new RelayCommand(() => Zoom -= 0.25f, null);
            set { zoomOutCmd = value; }
        }

        private RelayCommand zoomInCmd;
        public RelayCommand ZoomInCmd
        {
            get => zoomInCmd ?? new RelayCommand(() => Zoom += 0.25f, null);
            set { zoomInCmd = value; }
        }

        private RelayCommand createSceneCmd;
        public RelayCommand CreateSceneCmd
        {
            get => createSceneCmd ?? new RelayCommand(CreateScene, null);
            set { createSceneCmd = value; }
        }

        private RelayCommand removeSceneCmd;
        public RelayCommand RemoveSceneCmd
        {
            get => removeSceneCmd ?? new RelayCommand(RemoveSelectedScene, () => SelectedScene != null);
            set { removeSceneCmd = value; }
        }

        private RelayCommand createGroupCmd;
        public RelayCommand CreateGroupCmd
        {
            get { return createGroupCmd ?? new RelayCommand(AddGroupToSelection, () => SelectedScene != null); }
            set { createGroupCmd = value; }
        }

        private RelayCommand removeGroupCmd;
        public RelayCommand RemoveGroupCmd
        {
            get { return removeGroupCmd ?? new RelayCommand(RemoveSelectedGroup, () => SelectedNode is UICastGroup); }
            set { removeGroupCmd = value; }
        }

        private RelayCommand<int> changeCastSpriteCmd;
        public RelayCommand<int> ChangeCastSpriteCmd
        {
            get { return changeCastSpriteCmd ?? new RelayCommand<int>(SelectCastSprite, null); }
            set { changeCastSpriteCmd = value; }
        }

        private RelayCommand createCastCmd;
        public RelayCommand CreateCastCmd
        {
            get { return createCastCmd ?? new RelayCommand(AddCastToSelection, () => SelectedNode != null && SelectedNode is ICastContainer); }
            set { createCastCmd = value; }
        }

        private RelayCommand removeCastCmd;
        public RelayCommand RemoveCastCmd
        {
            get { return removeCastCmd ?? new RelayCommand(RemoveSelectedCast, () => SelectedNode is UICast); }
            set { removeCastCmd = value; }
        }

        public void SelectCastSprite(object index)
        {
            SpritePickerWindow dialog = new SpritePickerWindow(MainViewModel.Project.TextureLists);
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
            if (SelectedNode is UICast)
            {
                var cast = (UICast)SelectedNode;
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

        public void RemoveSelectedScene()
        {
            Scenes.Remove(SelectedScene);
        }

        public void AddGroupToSelection()
        {
            SelectedScene.Groups.Add(new UICastGroup());
        }

        public void RemoveSelectedGroup()
        {
            if (SelectedNode is UICastGroup group)
                SelectedScene.Groups.Remove(group);
        }

        public void AddCastToSelection()
        {
            if (SelectedNode is ICastContainer container)
                container.AddCast(new UICast());
        }

        public void RemoveSelectedCast()
        {
            if (ParentNode is ICastContainer container)
                container.RemoveCast(SelectedNode as UICast);
        }

        private UIScene scene;
        public UIScene SelectedScene
        {
            get { return scene; }
            set { scene = value; }
        }

        private object parentNode;
        public object ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        private object selectedNode;
        public object SelectedNode
        {
            get { return selectedNode; }
            set { selectedNode = value; }
        }
        public ObservableCollection<UIScene> Scenes => MainViewModel.Project.Scenes;
        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";

            zoom = 0.65f;
            playbackSpeed = 1.0f;
        }
    }
}
