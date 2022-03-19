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
        public float Time { get; set; }
        public bool Playing { get; set; }
        public float PlaybackSpeed { get; set; }

        private float zoom;
        public float Zoom
        {
            get => zoom;
            set { zoom = Math.Clamp(value, MinZoom, MaxZoom); }
        }

        public RelayCommand TogglePlayingCmd { get; }
        public RelayCommand StopPlayingCmd { get; }
        public RelayCommand ReplayCmd { get; }
        public RelayCommand<float> SeekCmd { get; }
        public RelayCommand ZoomOutCmd { get; }
        public RelayCommand ZoomInCmd { get; }

        public RelayCommand CreateSceneCmd { get; }
        public RelayCommand RemoveSceneCmd { get; }
        public RelayCommand CreateGroupCmd { get; }
        public RelayCommand RemoveGroupCmd { get; }
        public RelayCommand<int> ChangeCastSpriteCmd { get; }
        public RelayCommand CreateCastCmd { get; }
        public RelayCommand RemoveCastCmd { get; }

        public void SelectCastSprite(object index)
        {
            SpritePickerWindow dialog = new SpritePickerWindow(Project.TextureLists);
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                if (dialog.SelectedSpriteID != -1)
                {
                    var sprIndex = (int)index;
                    ChangeCastSprite(sprIndex, dialog.SelectedSpriteID);
                }
            }
        }

        public void ChangeCastSprite(int index, int sprID)
        {
            if (SelectedNode is UICast)
            {
                var cast = (UICast)SelectedNode;
                cast.Sprites[index] = sprID;
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
            Time += d * PlaybackSpeed * (Playing ? 1 : 0);
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

        public UIScene SelectedScene { get; set; }
        public object ParentNode { get; set; }
        public object SelectedNode { get; set; }
        public ObservableCollection<UIScene> Scenes => Project.Scenes;

        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";

            zoom = 0.65f;
            PlaybackSpeed = 1.0f;

            TogglePlayingCmd    = new RelayCommand(() => Playing ^= true, null);
            StopPlayingCmd      = new RelayCommand(Stop, null);
            ReplayCmd           = new RelayCommand(Replay, null);
            SeekCmd             = new RelayCommand<float>(Seek, () => !Playing);
            ZoomOutCmd          = new RelayCommand(() => Zoom -= 0.25f, null);
            ZoomInCmd           = new RelayCommand(() => Zoom += 0.25f, null);

            CreateSceneCmd      = new RelayCommand(CreateScene, null);
            RemoveSceneCmd      = new RelayCommand(RemoveSelectedScene, () => SelectedScene != null);
            CreateGroupCmd      = new RelayCommand(AddGroupToSelection, () => SelectedScene != null);
            RemoveGroupCmd      = new RelayCommand(RemoveSelectedGroup, () => SelectedNode is UICastGroup);
            CreateCastCmd       = new RelayCommand(AddCastToSelection, () => SelectedNode is ICastContainer);
            RemoveCastCmd       = new RelayCommand(RemoveSelectedCast, () => SelectedNode is UICast);
            ChangeCastSpriteCmd = new RelayCommand<int>(SelectCastSprite, null);
        }
    }
}
