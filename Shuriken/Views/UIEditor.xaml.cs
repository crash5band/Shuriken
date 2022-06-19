using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Shuriken.Models;
using Shuriken.Rendering;
using Shuriken.ViewModels;
using Shuriken.Models.Animation;
using Shuriken.Misc;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using Vector2 = System.Numerics.Vector2;

namespace Shuriken.Views
{
    using Vec2 = Models.Vector2;
    using Vec3 = Models.Vector3;
    

    /// <summary>
    /// Interaction logic for UIEditor.xaml
    /// </summary>
    public partial class UIEditor : UserControl
    {
        public static float ViewX = 1280;
        public static float ViewY = 720;
        Converters.ColorToBrushConverter colorConverter;
        Renderer renderer;

        public UIEditor()
        {
            InitializeComponent();

            GLWpfControlSettings glSettings = new GLWpfControlSettings
            {
                GraphicsProfile = ContextProfile.Core,
                MajorVersion = 3,
                MinorVersion = 3,
            };

            glControl.Start(glSettings);

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.FramebufferSrgb);

            colorConverter = new Converters.ColorToBrushConverter();
            renderer = new Renderer(1280, 960);
        }

        private void glControlRender(TimeSpan obj)
        {
            var sv = DataContext as ScenesViewModel;
            if (sv == null) 
                return;
            
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float deltaTime = obj.Milliseconds / 1000.0f * 60.0f;
            sv.SizeX = ViewX;
            sv.SizeY = ViewY;
            sv.Tick(deltaTime);
            renderer.SetShader(renderer.shaderDictionary["basic"]);

            UpdateSceneGroups(Project.SceneGroups, Project.Fonts, sv.Time);

            GL.Finish();
        }

        private void UpdateSceneGroups(IEnumerable<UISceneGroup> groups, IEnumerable<UIFont> fonts, float time)
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

        private void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var scene in scenes)
            {
                if (!scene.Visible)
                    continue;

                foreach (var group in scene.Groups)
                {
                    if (!group.Visible)
                        continue;

                    renderer.Start();

                    foreach (var lyr in group.Casts) 
                        UpdateCast(scene, lyr, new CastTransform(), time);

                    renderer.End();
                }
            }
        }

        private void UpdateCast(UIScene scene, UICast lyr, CastTransform transform, float time)
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

                    renderer.DrawSprite(
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

                        float width = spr.Dimensions.X / renderer.Width;
                        float height = spr.Dimensions.Y / renderer.Height;

                        var begin = (Vector2)lyr.TopLeft;
                        var end = begin + new Vector2(width, height);

                        renderer.DrawSprite(
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

        private void ScenesTreeViewSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem source = e.OriginalSource as TreeViewItem;
            TreeViewItem item = source;
            
            // Move up the tree view until we reach the TreeViewItem holding the UIScene
            while (item != null && item.DataContext != null && item.DataContext is not UIScene)
                item = Utilities.GetParentTreeViewItem(item);

            if (DataContext is ScenesViewModel vm)
            {
                vm.SelectedScene = item == null ? null : item.DataContext as UIScene;
                vm.SelectedUIObject = source.DataContext;

                TreeViewItem parent = Utilities.GetParentTreeViewItem(source);
                vm.ParentNode = parent == null ? null : parent.DataContext;
            }
        }

        private void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var tree = e.OriginalSource as TreeView;
            if (tree.Items.Count < 1)
            {
                if (DataContext is ScenesViewModel vm)
                    vm.SelectedUIObject = vm.ParentNode = vm.SelectedScene = null;
            }
        }

        private void NodesTreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (DataContext is ScenesViewModel vm)
                vm.SelectedSceneGroup = item.DataContext as UISceneGroup;
        }
    }
}
