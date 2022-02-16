using System;
using System.IO;
using System.Collections.Generic;
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
                List<UIScene> sortedScenes = Project.Scenes.ToList();
                sortedScenes.Sort();

                UpdateScenes(sortedScenes, Project.Fonts, sv.Time);
            }
        }

        private void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float time)
        {
            renderer.ConfigureShader(renderer.shaderDictionary["basic"]);
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

        private Vec2 GetPosition(UIScene scn, UICast lyr, float time)
        {
            bool hasX = false;
            bool hasY = false;
            Vec2 result = new Vec2();

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var xPos = anim.GetTrack(lyr, AnimationType.XPosition);
                    if (xPos != null)
                    {
                        if (xPos.Enabled)
                        {
                            result.X = xPos.GetValue(time);
                            hasX = true;
                        }
                    }

                    var yPos = anim.GetTrack(lyr, AnimationType.YPosition);
                    if (yPos != null)
                    {
                        if (yPos.Enabled)
                        {
                            result.Y = yPos.GetValue(time);
                            hasY = true;
                        }
                    }
                }
            }

            if (!hasX)
                result.X = lyr.Translation.X;

            if (!hasY)
                result.Y = lyr.Translation.Y;

            result += lyr.Offset;
            return result;
        }

        private float GetZPosition(UIScene scn, UICast lyr, float time)
        {
            return lyr.ZTranslation;
        }

        private float GetRotation(UIScene scn, UICast lyr, float time)
        {
            bool hasRot = false;
            float result = 0.0f;

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var rotAnim = anim.GetTrack(lyr, AnimationType.Rotation);
                    if (rotAnim != null)
                    {
                        if (rotAnim.Enabled)
                        {
                            result = rotAnim.GetValue(time);
                            hasRot = true;
                        }
                    }
                }
            }

            return hasRot ? result : lyr.Rotation;
        }

        private Vec3 GetScale(UIScene scn, UICast lyr, float time)
        {
            bool hasSx = false;
            bool hasSy = false;
            bool hasSz = false;
            var result = new Vec3(1, 1, 1);

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var sX = anim.GetTrack(lyr, AnimationType.XScale);
                    if (sX != null)
                    {
                        if (sX.Enabled)
                        {
                            result.X *= sX.GetValue(time);
                            hasSx = true;
                        }
                    }

                    var sy = anim.GetTrack(lyr, AnimationType.YScale);
                    if (sy != null)
                    {
                        if (sy.Enabled)
                        {
                            result.Y *= sy.GetValue(time);
                            hasSy = true;
                        }
                    }
                }
            }

            if (!hasSx)
                result.X = lyr.Scale.X;

            if (!hasSy)
                result.Y = lyr.Scale.Y;

            if (!hasSz)
                result.Z = lyr.Scale.Z;

            return result;
        }

        private int GetSprite(UIScene scn, UICast lyr, float time)
        {
            int sprIndex = lyr.DefaultSprite;
            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var sprAnim = anim.GetTrack(lyr, AnimationType.SubImage);
                    if (sprAnim != null)
                    {
                        if (sprAnim.Enabled)
                        {
                            int i = (int)sprAnim.GetValue(time);
                            if (i > 0 && i < lyr.Sprites.Count)
                                sprIndex = i;
                        }
                    }
                }
            }

            return lyr.Sprites[sprIndex];
        }

        private Color GetColor(UIScene scn, UICast lyr, float time)
        {
            bool hasClr = false;
            Color result = new Color(255, 255, 255, 255);

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var clrAnim = anim.GetTrack(lyr, AnimationType.Color);
                    if (clrAnim != null)
                    {
                        if (clrAnim.Enabled)
                        {
                            result = new Color(clrAnim.GetValue(time));
                            hasClr = true;
                        }
                    }
                }
            }

            return hasClr ? result : lyr.Color;
        }

        private Color[] GetGradients(UIScene scn, UICast lyr, float time)
        {
            // order: tl, bl, tr, br
            bool[] hasGradient = Enumerable.Repeat(false, 4).ToArray();
            Color[] results = new Color[4];

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    int animValue = 256;
                    for (int i = 0; i < 4; ++i)
                    {
                        var clrAnim = anim.GetTrack(lyr, (AnimationType)animValue);
                        if (clrAnim != null)
                        {
                            if (clrAnim.Enabled)
                            {
                                hasGradient[i] = true;
                                results[i] = new Color(clrAnim.GetValue(time));
                            }
                        }

                        animValue <<= 1;
                    }
                }
            }

            if (!hasGradient[0])
                results[0] = lyr.GradientTopLeft;

            if (!hasGradient[1])
                results[1] = lyr.GradientBottomLeft;

            if (!hasGradient[2])
                results[2] = lyr.GradientTopRight;

            if (!hasGradient[3])
                results[3] = lyr.GradientBottomRight;

            return results;
        }

        private void DrawCastFont(UICast lyr, Vec2 pos, Vec2 pivot, float rot, Vec3 sz, Color[] gradients)
        {
            float xOffset = 0.0f;
            foreach (var c in lyr.FontCharacters)
            {
                int sprID = -1;
                foreach (var mapping in lyr.Font.Mappings)
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
                    float sprStep = spr.Width / 2.0f * sz.X;
                    xOffset += sprStep;
                    Vec2 sprPos = new Vec2(pos.X + xOffset - (lyr.Width * 0.5f * sz.X), pos.Y);

                    renderer.DrawSprite(new Vec3(sprPos.X, sprPos.Y, lyr.ZTranslation), pivot, rot, new Vec3(sz.X * spr.Width, sz.Y * spr.Height, 1.0f), spr, lyr.Flags, lyr.Color.ToFloats(),
                        gradients[0].ToFloats(), gradients[2].ToFloats(), gradients[3].ToFloats(), gradients[1].ToFloats(), lyr.ZIndex);

                    xOffset += sprStep - lyr.FontSpacingCorrection;
                }
            }
        }

        private Vec2 GetPivot(UICast lyr, Vec3 scale, int width, int height)
        {
            float x = lyr.Anchor.X * scale.X * width * 0.5f;
            float y = lyr.Anchor.Y * scale.Y * height * 0.5f;
            return new Vec2(x, y);
        }

        private void UpdateCast(UIScene scene, UICast lyr, CastTransform transform, float time)
        {
            int sprID = GetSprite(scene, lyr, time);

            Vec2 position = GetPosition(scene, lyr, time);
            position.X *= renderer.RenderWidth;
            position.Y *= -renderer.RenderHeight;
            float zPosition = GetZPosition(scene, lyr, time);
            float rotation = GetRotation(scene, lyr, time);
            Vec3 scale = GetScale(scene, lyr, time);

            Color color = GetColor(scene, lyr, time);

            // 0: top-left, 1: bottom-left, 2: top-right, 3: bottom-right
            Color[] gradients = GetGradients(scene, lyr, time);

            // TODO: figure out position and rotation inheritance flags
            position += transform.Position;
            rotation += transform.Rotation;

            // inherit scale
            if ((lyr.Field34 & (1 << 10)) != 0)
                scale.X *= transform.Scale.X;

            if ((lyr.Field34 & (1 << 11)) != 0)
                scale.Y *= transform.Scale.Y;

            // inherit color
            if ((lyr.Field34 & 8) != 0)
            {
                Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
                color = new Color(cF.X, cF.Y, cF.Z, cF.W);
            }

            Vec2 pivot = GetPivot(lyr, scale, renderer.RenderWidth, renderer.RenderHeight);

            if (lyr.Visible && lyr.IsEnabled)
            {
                var spr = Project.TryGetSprite(sprID);
                if (lyr.Type == DrawType.Sprite && spr != null)
                {
                    renderer.DrawSprite(new Vec3(position.X, position.Y, zPosition), pivot, rotation, new Vec3(lyr.Width, lyr.Height, 1.0f) * scale, spr,
                        lyr.Flags, color.ToFloats(), gradients[0].ToFloats(), gradients[2].ToFloats(), gradients[3].ToFloats(), gradients[1].ToFloats(), lyr.ZIndex);
                }
                else if (lyr.Type == DrawType.Font)
                {
                    DrawCastFont(lyr, position, pivot, rotation, scale, gradients);
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

                    var childTransform = new CastTransform(position + posAdjust, zPosition, rotation, scale, color);
                    UpdateCast(scene, child, childTransform, time);
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
                vm.SelectedNode = source.DataContext;

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
                    vm.SelectedNode = vm.ParentNode = vm.SelectedScene = null;
            }
        }
    }
}
