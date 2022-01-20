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
    using Vec2 = Shuriken.Models.Vector2;
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
                    foreach (var lyr in group.Layers)
                    {
                        UpdateCast(scene, lyr, new CastTransform(), time);
                    }
                    renderer.End();
                }
            }
        }

        private Vec2 GetPosition(UIScene scn, UILayer lyr, float time)
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
                        result.X = xPos.GetValue(time);
                        hasX = true;
                    }

                    var yPos = anim.GetTrack(lyr, AnimationType.YPosition);
                    if (yPos != null)
                    {
                        result.Y = yPos.GetValue(time);
                        hasY = true;
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

        private float GetRotation(UIScene scn, UILayer lyr, float time)
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
                        result = rotAnim.GetValue(time);
                        hasRot = true;
                    }
                }
            }

            return hasRot ? result : lyr.Rotation;
        }

        private Vec2 GetScale(UIScene scn, UILayer lyr, float time)
        {
            bool hasSx = false;
            bool hasSy = false;
            var result = new Vec2(1, 1);

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var sX = anim.GetTrack(lyr, AnimationType.XScale);
                    if (sX != null)
                    {
                        result.X *= sX.GetValue(time);
                        hasSx = true;
                    }

                    var sy = anim.GetTrack(lyr, AnimationType.YScale);
                    if (sy != null)
                    {
                        result.Y *= sy.GetValue(time);
                        hasSy = true;
                    }
                }
            }

            if (!hasSx)
                result.X = lyr.Scale.X;

            if (!hasSy)
                result.Y = lyr.Scale.Y;

            return result;
        }

        private Sprite GetSprite(UIScene scn, UILayer lyr, float time)
        {
            int sprIndex = 0;
            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var sprAnim = anim.GetTrack(lyr, AnimationType.SubImage);
                    if (sprAnim != null)
                    {
                        int i = (int)sprAnim.GetValue(time);
                        if (i > 0 && i < lyr.Sprites.Count)
                            sprIndex = i;
                    }
                }
            }

            return lyr.Sprites[sprIndex];
        }

        private Color GetColor(UIScene scn, UILayer lyr, float time)
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
                        result = new Color(clrAnim.GetValue(time));
                        hasClr = true;
                    }
                }
            }

            return hasClr ? result : lyr.Color;
        }

        private Color[] GetGradients(UIScene scn, UILayer lyr, float time)
        {
            bool hasTL = false;
            bool hasBL = false;
            bool hasTR = false;
            bool hasBR = false;
            Color[] results = new Color[4];

            foreach (var anim in scn.Animations)
            {
                if (anim.Enabled)
                {
                    var clrAnim = anim.GetTrack(lyr, AnimationType.GradientTL);
                    if (clrAnim != null)
                    {
                        results[0] = new Color(clrAnim.GetValue(time));
                        hasTL = true;
                    }

                    clrAnim = anim.GetTrack(lyr, AnimationType.GradientBL);
                    if (clrAnim != null)
                    {
                        results[1] = new Color(clrAnim.GetValue(time));
                        hasBL = true;
                    }

                    clrAnim = anim.GetTrack(lyr, AnimationType.GradientTR);
                    if (clrAnim != null)
                    {
                        results[2] = new Color(clrAnim.GetValue(time));
                        hasTR = true;
                    }

                    clrAnim = anim.GetTrack(lyr, AnimationType.GradientBR);
                    if (clrAnim != null)
                    {
                        results[3] = new Color(clrAnim.GetValue(time));
                        hasBR = true;
                    }
                }
            }

            if (!hasTL)
                results[0] = lyr.GradientTopLeft;

            if (!hasBL)
                results[1] = lyr.GradientBottomLeft;

            if (!hasTR)
                results[2] = lyr.GradientTopRight;

            if (!hasBR)
                results[3] = lyr.GradientBottomRight;

            return results;
        }

        private void DrawCastFont(UILayer lyr, Vec2 pos, Vec2 pivot, float rot, Vec2 sz, Color[] gradients)
        {
            float xOffset = 0.0f;
            foreach (var c in lyr.FontCharacters)
            {
                Sprite spr = null;
                foreach (var mapping in lyr.Font.Mappings)
                {
                    if (mapping.Character == c)
                    {
                        spr = mapping.Sprite;
                        break;
                    }
                }

                if (spr != null)
                {
                    xOffset += spr.Width / 2.1f * sz.X;
                    Vec2 sprPos = new Vec2(pos.X + xOffset - (lyr.Width / 2.0f * sz.X), pos.Y);

                    renderer.DrawSprite(new System.Numerics.Vector2(sprPos.X, sprPos.Y), new System.Numerics.Vector2(pivot.X, pivot.Y), rot, new System.Numerics.Vector2(sz.X * spr.Width, sz.Y * spr.Height), spr, lyr.Flags, lyr.Color.ToFloats(),
                        gradients[0].ToFloats(), gradients[2].ToFloats(), gradients[3].ToFloats(), gradients[1].ToFloats(), lyr.ZIndex);

                    xOffset += spr.Width / 2.25f * sz.X;
                }
            }
        }

        private Vec2 GetPivot(UILayer lyr, Vec2 scale, int width, int height)
        {
            float diff = Math.Abs(lyr.TopRight.X) - Math.Abs(lyr.TopLeft.X);
            float right = diff * renderer.RenderWidth * scale.X;

            diff = Math.Abs(lyr.BottomRight.Y) - Math.Abs(lyr.TopRight.Y);
            float up = diff * renderer.RenderHeight * scale.Y;

            return new Vec2(right / 2.0f, -up / 2.0f);
        }

        private void UpdateCast(UIScene scene, UILayer lyr, CastTransform transform, float time)
        {
            Sprite spr = GetSprite(scene, lyr, time);

            Vec2 position = GetPosition(scene, lyr, time);
            position.X *= renderer.RenderWidth;
            position.Y *= -renderer.RenderHeight;
            float rotation = GetRotation(scene, lyr, time);
            Vec2 scale = GetScale(scene, lyr, time);

            Color color = GetColor(scene, lyr, time);

            // 0: top-left, 1: bottom-left, 2: top-right, 3: bottom-right
            Color[] gradients = GetGradients(scene, lyr, time);

            position += transform.Position;
            rotation += transform.Rotation;

            if ((lyr.Field34 & (1 << 10)) != 0)
                scale.X *= transform.Scale.X;

            if ((lyr.Field34 & (1 << 11)) != 0)
                scale.Y *= transform.Scale.Y;

            Vec2 pivot = GetPivot(lyr, scale, renderer.RenderWidth, renderer.RenderHeight);

            // inherit color
            if ((lyr.Field34 & 8) != 0)
            {
                Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
                color = new Color(cF.X, cF.Y, cF.Z, cF.W);
            }

            if (lyr.Visible && lyr.IsEnabled)
            {
                if (lyr.Type == DrawType.Sprite && spr != null)
                {
                    renderer.DrawSprite(new System.Numerics.Vector2(position.X, position.Y), new System.Numerics.Vector2(pivot.X, pivot.Y), rotation, new System.Numerics.Vector2(spr.Dimensions.X * scale.X, spr.Dimensions.Y * scale.Y), spr,
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
                        float xAdjust = Math.Abs(lyr.TopRight.X) - Math.Abs(lyr.TopLeft.X);
                        float yAdjust = Math.Abs(lyr.BottomRight.Y) - Math.Abs(lyr.TopRight.Y);

                        posAdjust.X = ((xAdjust * scale.X) - xAdjust) * renderer.RenderWidth;
                        posAdjust.Y = ((yAdjust * scale.Y) - yAdjust) * renderer.RenderHeight;
                        //posAdjust.X += lyr.Field68 * scale.X;
                        //posAdjust.Y += lyr.Field6C * scale.Y;
                    }

                    CastTransform childTransform = new CastTransform(position + posAdjust, rotation, scale, color);
                    UpdateCast(scene, child, childTransform, time);
                }
            }
        }

        private void ScenesTreeViewSelected(object sender, RoutedEventArgs e)
        {
            // Move up the tree view until we reach the TreeViewItem holding the UIScene
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            
            while (item != null && item.DataContext != null && item.DataContext is not UIScene)
                item = Utilities.GetParentTreeViewItem(item);

            var vm = DataContext as ScenesViewModel;
            if (vm != null)
            {
                if (item != null)
                    vm.SelectedScene = item.DataContext as UIScene;

                vm.SelectedNode = (e.OriginalSource as TreeViewItem).DataContext;
            }
        }
    }
}
