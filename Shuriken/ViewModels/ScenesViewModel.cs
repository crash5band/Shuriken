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
using Shuriken.Rendering;
using Shuriken.Models.Animation;
using Shuriken.Misc;

namespace Shuriken.ViewModels
{
    using Vec2 = Models.Vector2;
    using Vec3 = Models.Vector3;
    using Vector2 = System.Numerics.Vector2;

    public class ScenesViewModel : ViewModelBase
    {
        public float SizeX { get; set; } = 1280;
        public float SizeY { get; set; } = 10;
        public float MinZoom => 0.25f;
        public float MaxZoom => 2.50f;
        public float Time { get; set; }
        public bool Playing { get; set; }
        public float PlaybackSpeed { get; set; }

        public Renderer Renderer { get; private set; }

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
            if (SelectedUIObject is UICast)
            {
                var cast = (UICast)SelectedUIObject;
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
            if (SelectedUIObject is UICastGroup group)
                SelectedScene.Groups.Remove(group);
        }

        public void AddCastToSelection()
        {
            if (SelectedUIObject is ICastContainer container)
                container.AddCast(new UICast());
        }

        public void RemoveSelectedCast()
        {
            if (ParentNode is ICastContainer container)
                container.RemoveCast(SelectedUIObject as UICast);
        }

        public void CreateSceneGroup()
        {
            throw new NotImplementedException();
        }

        public void RemoveSelecteSceneGroup()
        {
            throw new NotImplementedException();
        }

        public void UpdateSceneGroups(IEnumerable<UISceneGroup> groups, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var group in groups)
            {
                if (!group.Visible)
                    continue;

                UpdateSceneGroups(group.Children, fonts, time);

                List<UIScene> sortedScenes = group.Scenes.ToList();
                sortedScenes.Sort();

                UpdateScenes(group.Scenes, fonts, time);
            }
        }

        public void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var scene in scenes)
            {
                if (!scene.Visible)
                    continue;

                foreach (var group in scene.Groups)
                {
                    if (!group.Visible)
                        continue;

                    Renderer.Start();

                    foreach (var lyr in group.Casts)
                        UpdateCast(scene, lyr, new CastTransform(), time);

                    Renderer.End();
                }
            }
        }

        public void UpdateCast(UIScene scene, UICast lyr, CastTransform transform, float time)
        {
            bool hideFlag = lyr.HideFlag != 0;
            var position = new Vec2(lyr.Translation.X, lyr.Translation.Y);
            float rotation = lyr.Rotation;
            var scale = new Vec2(lyr.Scale.X, lyr.Scale.Y);
            float sprID = lyr.DefaultSprite;
            var color = lyr.Color;
            var gradientTopLeft = lyr.GradientTopLeft;
            var gradientBottomLeft = lyr.GradientBottomLeft;
            var gradientTopRight = lyr.GradientTopRight;
            var gradientBottomRight = lyr.GradientBottomRight;

            foreach (var animation in scene.Animations)
            {
                if (!animation.Enabled)
                    continue;

                for (int i = 0; i < 12; i++)
                {
                    var type = (AnimationType)(1 << i);
                    var track = animation.GetTrack(lyr, type);

                    if (track == null)
                        continue;

                    switch (type)
                    {
                        case AnimationType.HideFlag:
                            hideFlag = track.GetSingle(time) != 0;
                            break;

                        case AnimationType.XPosition:
                            position.X = track.GetSingle(time);
                            break;

                        case AnimationType.YPosition:
                            position.Y = track.GetSingle(time);
                            break;

                        case AnimationType.Rotation:
                            rotation = track.GetSingle(time);
                            break;

                        case AnimationType.XScale:
                            scale.X = track.GetSingle(time);
                            break;

                        case AnimationType.YScale:
                            scale.Y = track.GetSingle(time);
                            break;

                        case AnimationType.SubImage:
                            sprID = track.GetSingle(time);
                            break;

                        case AnimationType.Color:
                            color = track.GetColor(time);
                            break;

                        case AnimationType.GradientTL:
                            gradientTopLeft = track.GetColor(time);
                            break;

                        case AnimationType.GradientBL:
                            gradientBottomLeft = track.GetColor(time);
                            break;

                        case AnimationType.GradientTR:
                            gradientTopRight = track.GetColor(time);
                            break;

                        case AnimationType.GradientBR:
                            gradientBottomRight = track.GetColor(time);
                            break;
                    }
                }
            }

            if (hideFlag)
                return;

            // Inherit position scale
            // TODO: Is this handled through flags?
            position.X *= transform.Scale.X;
            position.Y *= transform.Scale.Y;

            // Rotate through parent transform
            float angle = Utilities.ToRadians(transform.Rotation);
            float rotatedX = position.X * MathF.Cos(angle) * scene.AspectRatio + position.Y * MathF.Sin(angle);
            float rotatedY = position.Y * MathF.Cos(angle) - position.X * MathF.Sin(angle) * scene.AspectRatio;

            position.X = rotatedX / scene.AspectRatio;
            position.Y = rotatedY;

            position += lyr.Offset;

            // Inherit position
            if ((lyr.Field34 & 0x100) != 0)
                position.X += transform.Position.X;

            if ((lyr.Field34 & 0x200) != 0)
                position.Y += transform.Position.Y;

            // Inherit rotation
            if ((lyr.Field34 & 0x2) != 0)
                rotation += transform.Rotation;

            // Inherit scale
            if ((lyr.Field34 & 0x400) != 0)
                scale.X *= transform.Scale.X;

            if ((lyr.Field34 & 0x800) != 0)
                scale.Y *= transform.Scale.Y;

            // Inherit color
            if ((lyr.Field34 & 0x8) != 0)
            {
                Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
                color = new Color(cF.X, cF.Y, cF.Z, cF.W);
            }

            if (lyr.Visible && lyr.IsEnabled)
            {
                if (lyr.Type == DrawType.Sprite)
                {
                    var spr = sprID >= 0 ? Project.TryGetSprite(lyr.Sprites[Math.Min(lyr.Sprites.Count - 1, (int)sprID)]) : null;
                    var nextSpr = sprID >= 0 ? Project.TryGetSprite(lyr.Sprites[Math.Min(lyr.Sprites.Count - 1, (int)sprID + 1)]) : null;

                    spr ??= nextSpr;
                    nextSpr ??= spr;

                    Renderer.DrawSprite(
                        lyr.TopLeft, lyr.BottomLeft, lyr.TopRight, lyr.BottomRight,
                        position, Utilities.ToRadians(rotation), scale, scene.AspectRatio, spr, nextSpr, sprID % 1, color,
                        gradientTopLeft, gradientBottomLeft, gradientTopRight, gradientBottomRight,
                        lyr.ZIndex, lyr.Flags);
                }
                else if (lyr.Type == DrawType.Font)
                {
                    float xOffset = 0.0f;

                    for (var i = 0; i < lyr.FontCharacters.Length; i++)
                    {
                        var font = Project.TryGetFont(lyr.FontID);
                        if (font == null)
                            continue;

                        Sprite spr = null;

                        foreach (var mapping in font.Mappings)
                        {
                            if (mapping.Character != lyr.FontCharacters[i])
                                continue;

                            spr = Project.TryGetSprite(mapping.Sprite);
                            break;
                        }

                        if (spr == null)
                            continue;

                        float width = spr.Dimensions.X / Renderer.Width;
                        float height = spr.Dimensions.Y / Renderer.Height;

                        var begin = (Vector2)lyr.TopLeft;
                        var end = begin + new Vector2(width, height);

                        Renderer.DrawSprite(
                            new Vector2(begin.X + xOffset, begin.Y),
                            new Vector2(begin.X + xOffset, end.Y),
                            new Vector2(end.X + xOffset, begin.Y),
                            new Vector2(end.X + xOffset, end.Y),
                            position, Utilities.ToRadians(rotation), scale, scene.AspectRatio, spr, spr, 0, color,
                            gradientTopLeft, gradientBottomLeft, gradientTopRight, gradientBottomRight,
                            lyr.ZIndex, lyr.Flags
                        );

                        xOffset += width + lyr.FontSpacingAdjustment;
                    }
                }

                var childTransform = new CastTransform(position, rotation, scale, color);

                foreach (var child in lyr.Children)
                    UpdateCast(scene, child, childTransform, time);
            }
        }

        public UISceneGroup SelectedSceneGroup { get; set; }
        public UIScene SelectedScene { get; set; }
        public object ParentNode { get; set; }
        public object SelectedUIObject { get; set; }
        public ObservableCollection<UISceneGroup> SceneGroups => Project.SceneGroups;
        public ObservableCollection<UIScene> Scenes => SelectedSceneGroup?.Scenes;

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
            RemoveGroupCmd      = new RelayCommand(RemoveSelectedGroup, () => SelectedUIObject is UICastGroup);
            CreateCastCmd       = new RelayCommand(AddCastToSelection, () => SelectedUIObject is ICastContainer);
            RemoveCastCmd       = new RelayCommand(RemoveSelectedCast, () => SelectedUIObject is UICast);
            ChangeCastSpriteCmd = new RelayCommand<int>(SelectCastSprite, null);

            Renderer = new Renderer(1280, 720);
        }
    }
}
