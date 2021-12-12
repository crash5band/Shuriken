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
                UpdateScenes(Project.Scenes, Project.Fonts, sv.Time);
            }
        }

        private void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float time)
        {
            renderer.BeginBatch();
            renderer.ConfigureShader(renderer.shaderDictionary["basic"]);
            foreach (var scene in scenes)
            {
                if (!scene.Visible)
                    continue;

                foreach (var group in scene.Groups)
                {
                    if (!group.Visible)
                        continue;

                    foreach (var lyr in group.Layers)
                    {
                        UpdateCast(scene, lyr, new CastTransform(), time);
                    }
                }
            }

            renderer.EndBatch();
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
                        if (i > 0 && i < lyr.Sprites.Length)
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

        private void DrawCastFont(UILayer lyr, Vec2 pos, Vec2 pivot, float rot, Vec2 sz)
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
                    if (spr.Texture.ID != renderer.TexID)
                    {
                        renderer.EndBatch();
                        renderer.BeginBatch();
                        spr.Texture.Use();
                        renderer.TexID = spr.Texture.ID;
                    }

                    xOffset += spr.Width / 2.1f * sz.X;
                    Vec2 sprPos = new Vec2(pos.X + xOffset - (lyr.Width / 2.0f * sz.X), pos.Y);

                    DrawSprite(sprPos, pivot, rot, new Vec2(sz.X * spr.Width, sz.Y * spr.Height), spr, lyr.Flags, lyr.Color, lyr.GradientTopLeft, lyr.GradientTopRight, lyr.GradientBottomRight, lyr.GradientBottomLeft);
                    xOffset += spr.Width / 2.25f * sz.X;
                }
            }
        }

        private void DrawSprite(Vec2 pos, Vec2 pivot, float rot, Vec2 sz, Sprite spr, uint flags, Color col, Color tl, Color tr, Color br, Color bl)
        {
            bool mirrorX = (flags & 1024) != 0;
            bool mirrorY = (flags & 2048) != 0;
            Matrix4x4 mat = renderer.CreateModelMatrix(new System.Numerics.Vector2(pos.X, pos.Y), new System.Numerics.Vector2(pivot.X, pivot.Y), rot, new System.Numerics.Vector2(sz.X, sz.Y));
            renderer.PushQuadBuffer(mat, renderer.GetUVCoords(new System.Numerics.Vector2(spr.Start.X, spr.Start.Y), new System.Numerics.Vector2(spr.Dimensions.X, spr.Dimensions.Y), spr.Texture.Width, spr.Texture.Height, mirrorX, mirrorY), col.ToFloats(), tr.ToFloats(), br.ToFloats(), bl.ToFloats(), tl.ToFloats());
        }

        private void UpdateCast(UIScene scene, UILayer lyr, CastTransform transform, float time)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            Sprite spr = GetSprite(scene, lyr, time);
            if (lyr.Type == DrawType.Sprite)
            {
                if (spr != null)
                {
                    if (spr.Texture.ID != renderer.TexID)
                    {
                        renderer.EndBatch();
                        renderer.BeginBatch();
                        spr.Texture.Use();
                        renderer.TexID = spr.Texture.ID;
                    }
                }
            }

            Vec2 position = GetPosition(scene, lyr, time);
            position.X *= renderer.RenderWidth;
            position.Y *= -renderer.RenderHeight;
            float rotation = GetRotation(scene, lyr, time);
            Vec2 scale = GetScale(scene, lyr, time);

            Color color = GetColor(scene, lyr, time);

            position += transform.Position;
            rotation += transform.Rotation;

            if ((lyr.Field34 & (1 << 10)) != 0)
                scale.X *= transform.Scale.X;
                scale.Y *= transform.Scale.Y;

            // pivot
            Vec2 pivot = new Vec2();
            float diff = Math.Abs(lyr.TopRight.X) - Math.Abs(lyr.TopLeft.X);
            float right = diff * renderer.RenderWidth * scale.X;
            pivot.X += right / 2.0f;

            diff = Math.Abs(lyr.BottomRight.Y) - Math.Abs(lyr.TopRight.Y);
            float up = diff * renderer.RenderHeight * scale.Y;
            pivot.Y -= up / 2.0f;

            Vector4 cF = Vector4.Multiply(color.ToFloats(), transform.Color.ToFloats());
            color = new Color(cF.X, cF.Y, cF.Z, cF.W);

            if (lyr.Visible && lyr.IsEnabled)
            {
                if (lyr.Type == DrawType.Sprite && spr != null)
                {
                    DrawSprite(position, pivot, rotation, new Vec2(spr.Dimensions.X * scale.X, spr.Dimensions.Y * scale.Y), spr, lyr.Flags, color, lyr.GradientTopLeft, lyr.GradientTopRight, lyr.GradientBottomRight, lyr.GradientBottomLeft);
                }
                else if (lyr.Type == DrawType.Font)
                {
                    DrawCastFont(lyr, position, pivot, rotation, scale);
                }

                foreach (var child in lyr.Children)
                {
                    Vec2 posAdjust = new Vec2();
                    if ((child.Flags & 1) == 0)
                    {
                        float xAdjust = Math.Abs(lyr.TopRight.X) - Math.Abs(lyr.TopLeft.X);
                        float yAdjust = Math.Abs(lyr.BottomRight.Y) - Math.Abs(lyr.TopRight.Y);

                        posAdjust.X = ((xAdjust * scale.X) - xAdjust) * renderer.RenderWidth;
                        posAdjust.Y = ((yAdjust * scale.Y) - yAdjust) * renderer.RenderHeight;
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
            if (vm != null && item != null)
            {
                vm.SelectedScene = item.DataContext as UIScene;
            }
        }
    }
}
