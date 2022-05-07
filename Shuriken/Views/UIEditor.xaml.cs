using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
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

namespace Shuriken.Views
{
    using Vec2 = Models.Vector2;
    using Vec3 = Models.Vector3;

    /// <summary>
    /// Interaction logic for UIEditor.xaml
    /// </summary>
    public partial class UIEditor : UserControl
    {
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
            renderer = new Renderer(1280, 720);
        }

        private void glControlRender(TimeSpan obj)
        {
            /*
            System.Windows.Media.Brush brush = Application.Current.TryFindResource("RegionBrush") as System.Windows.Media.Brush;
            Color clearColor = new Color();
            if (brush != null)
            {
                clearColor = (Color)colorConverter.ConvertBack(brush, typeof(Color), null, CultureInfo.InvariantCulture);
            }

            GL.ClearColor(clearColor.R / 255.0f, clearColor.G / 255.0f, clearColor.B / 255.0f, 1.0f);
            */
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float delta = obj.Milliseconds / 1000.0f * 60.0f;
            var sv = DataContext as ScenesViewModel;
            if (sv != null)
            {
                sv.Tick(delta);
                renderer.ConfigureShader(renderer.shaderDictionary["basic"]);
                UpdateSceneGroups(Project.SceneGroups, Project.Fonts, sv.Time);
            }
        }

        private void UpdateSceneGroups(IEnumerable<UISceneGroup> groups, IEnumerable<UIFont> fonts, float time)
        {
            foreach (var group in groups)
            {
                if (group.Visible)
                {
                    UpdateSceneGroups(group.Children, fonts, time);

                    List<UIScene> sortedScenes = group.Scenes.ToList();
                    sortedScenes.Sort();
                    UpdateScenes(group.Scenes, fonts, time);
                }
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
                    {
                        UpdateCast(scene, lyr, new CastTransform(), time);
                    }
                    renderer.End();
                }
            }
        }

        private void DrawCastFont(UICast lyr, Vec2 pos, Vec2 pivot, float rot, Vec3 sz, Vector4 tl, Vector4 bl, Vector4 tr, Vector4 br)
        {
            float xOffset = 0.0f;
            foreach (var c in lyr.FontCharacters)
            {
                int sprID = -1;
                var font = Project.TryGetFont(lyr.FontID);
                if (font == null)
                    continue;

                foreach (var mapping in font.Mappings)
                {
                    if (mapping.Character == c)
                    {
                        sprID = mapping.Sprite;
                        break;
                    }
                }

                var spr = Project.TryGetSprite(sprID);
                if (spr != null)
                {
                    float sprStep = spr.Width * 0.5f * sz.X;

                    xOffset += sprStep;
                    Vec2 sprPos = new Vec2(pos.X + xOffset - (lyr.Width * 0.5f * sz.X), pos.Y);

                    renderer.DrawSprite(new Vec3(sprPos.X, sprPos.Y, lyr.ZTranslation), pivot, rot,
                        new Vec3(sz.X * spr.Width, sz.Y * spr.Height, 1.0f), spr, lyr.Flags, lyr.Color.ToFloats(),
                        tl, bl, tr, br, lyr.ZIndex);

                    xOffset += sprStep + (lyr.FontSpacingAdjustment * renderer.RenderWidth);
                }
            }
        }

        private Vec2 GetPivot(UICast lyr, Vec3 scale, int width, int height)
        {
            float x = lyr.Anchor.X * scale.X * width * 0.5f;
            float y = lyr.Anchor.Y * scale.Y * height * 0.5f;
            return new Vec2(x, y);
        }

        private void UpdateCast(UIScene scene, UICast lyr, CastTransform transform, float time, Vec2 parentOrigin = null)
        {
            bool hideFlag = lyr.HideFlag != 0;
            var position = new Vec2(lyr.Translation.X, lyr.Translation.Y);
            float rotation = lyr.Rotation;
            var scale = new Vec3(lyr.Scale.X, lyr.Scale.Y, lyr.Scale.Z);
            int sprID = lyr.Sprites[Math.Max(0, Math.Min(lyr.Sprites.Count - 1, (int)lyr.DefaultSprite))];
            var color = lyr.Color;
            var tl = lyr.GradientTopLeft;
            var bl = lyr.GradientBottomLeft;
            var tr = lyr.GradientTopRight;
            var br = lyr.GradientBottomRight;

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
                            sprID = lyr.Sprites[Math.Max(0, Math.Min(lyr.Sprites.Count - 1, (int)track.GetSingle(time)))];
                            break;

                        case AnimationType.Color:
                            color = track.GetColor(time);
                            break;
                        
                        case AnimationType.GradientTL:
                            tl = track.GetColor(time);
                            break;
                        
                        case AnimationType.GradientBL:
                            bl = track.GetColor(time);
                            break;

                        case AnimationType.GradientTR:
                            tr = track.GetColor(time);
                            break;

                        case AnimationType.GradientBR:
                            br = track.GetColor(time);
                            break;
                    }
                }
            }

            if (hideFlag)
                return;

            position += lyr.Offset;
            position.X *= renderer.RenderWidth;
            position.Y *= renderer.RenderHeight;

            // Inherit position
            if ((lyr.Field34 & 0x100) != 0)
                position.X += transform.Position.X;
            
            if ((lyr.Field34 & 0x200) != 0)
                position.Y += transform.Position.Y;

            rotation += transform.Rotation;

            // inherit scale
            if ((lyr.Field34 & (1 << 10)) != 0) 
                scale.X *= transform.Scale.X;

            if ((lyr.Field34 & (1 << 11)) != 0) 
                scale.Y *= transform.Scale.Y;

            if (parentOrigin != null)
            {
                position.X = parentOrigin.X + (position.X - parentOrigin.X) * scale.X;
                position.Y = parentOrigin.Y + (position.Y - parentOrigin.Y) * scale.Y;
            }

            // inherit color
            if ((lyr.Field34 & 8) != 0)
            {
                Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
                color = new Color(cF.X, cF.Y, cF.Z, cF.W);
            }

            scale.X = Math.Max(0, scale.X);
            scale.Y = Math.Max(0, scale.Y);
            Vec2 pivot = GetPivot(lyr, scale, renderer.RenderWidth, renderer.RenderHeight);

            if (lyr.Visible && lyr.IsEnabled)
            {
                var spr = Project.TryGetSprite(sprID);
                if (lyr.Type == DrawType.Sprite && spr != null)
                {
                    renderer.DrawSprite(new Vec3(position.X, position.Y, lyr.ZTranslation), pivot, rotation, new Vec3(lyr.Width, lyr.Height, 1.0f) * scale, spr,
                        lyr.Flags, color.ToFloats(), tl.ToFloats(), bl.ToFloats(), tr.ToFloats(), br.ToFloats(), lyr.ZIndex);
                }
                else if (lyr.Type == DrawType.Font)
                {
                    DrawCastFont(lyr, position, pivot, rotation, scale, tl.ToFloats(), bl.ToFloats(), tr.ToFloats(), br.ToFloats());
                }

                foreach (var child in lyr.Children)
                {
                    Vec2 posAdjust = new Vec2();

                    // continue from area of parent cast?
                    if ((child.Flags & 1) == 0)
                    {
                        posAdjust.X = ((lyr.Anchor.X * scale.X) - lyr.Anchor.X) * renderer.RenderWidth;
                        posAdjust.Y = ((lyr.Anchor.Y * scale.Y) - lyr.Anchor.Y) * renderer.RenderHeight;
                    }

                    var childTransform = new CastTransform(position + posAdjust, lyr.ZTranslation, rotation, scale, color);

                    // Not correct, blb_gauge looks better when passing position and doesn't have field5c with value 2
                    if (false)
                    {
                        UpdateCast(scene, child, childTransform, time, position);
                    }
                    else
                    {
                        UpdateCast(scene, child, childTransform, time);
                    }
                }
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
