using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;
using OpenTK.Graphics.OpenGL;
using Shuriken.Misc;

namespace Shuriken.Rendering
{
    class Renderer
    {
        public readonly string shadersDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");

        uint vao;
        uint vbo;
        uint ebo;
        uint[] indices;
        public Dictionary<string, ShaderProgram> shaderDictionary;

        Vertex[] buffer;
        Vector2[] uvCoords;
        Vector4[] vPos;

        Dictionary<uint, string> shaderAttribs;

        public readonly int MaxVertices = 10000;
        public int MaxQuads => MaxVertices / 4;
        public int MaxIndices => MaxQuads * 6;

        public int NumVertices { get; private set; }
        public int NumQuads { get; private set; }
        public int NumIndices { get; private set; }
        public int BufferPos { get; private set; }
        public uint TexID { get; set; }
        public bool BatchStarted { get; private set; }
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

        public Renderer(int width, int height)
        {
            shaderDictionary = new Dictionary<string, ShaderProgram>();

            ShaderProgram basicShader = new ShaderProgram("basic", Path.Combine(shadersDir, "basic.vert"), Path.Combine(shadersDir, "basic.frag"));
            shaderDictionary.Add(basicShader.Name, basicShader);

            indices = new uint[MaxIndices];
            uint offset = 0;
            for (uint index = 0; index < MaxIndices; index += 6)
            {
                indices[index + 0] = offset + 0;
                indices[index + 1] = offset + 1;
                indices[index + 2] = offset + 2;

                indices[index + 3] = offset + 2;
                indices[index + 4] = offset + 3;
                indices[index + 5] = offset + 0;

                offset += 4;
            }

            buffer = new Vertex[MaxVertices];
            Init();

            RenderWidth = width;
            RenderHeight = height;
        }
        private void Init()
        {
            const int stride = 10 * sizeof(float);

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.GenBuffers(1, out ebo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, MaxVertices, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, MaxIndices, indices, BufferUsageHint.StaticDraw);

            // position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            // color
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));

            // uv
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            uvCoords = new Vector2[4];
            uvCoords[0] = new Vector2(1.0f, 1.0f);
            uvCoords[1] = new Vector2(1.0f, 0.0f);
            uvCoords[2] = new Vector2(0.0f, 0.0f);
            uvCoords[3] = new Vector2(0.0f, 1.0f);

            // top-right, bottom-right, top-left, bottom-left
            vPos = new Vector4[4];
            vPos[0] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
            vPos[1] = new Vector4(0.5f, -0.5f, 0.0f, 1.0f);
            vPos[2] = new Vector4(-0.5f, -0.5f, 0.0f, 1.0f);
            vPos[3] = new Vector4(-0.5f, 0.5f, 0.0f, 1.0f);


        }

        private void ResetRenderStats()
        {
            NumQuads = 0;
            NumIndices = 0;
            NumVertices = 0;
        }

        public void BeginBatch()
        {
            BufferPos = 0;
            BatchStarted = true;

            ResetRenderStats();
        }

        public void EndBatch()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, BufferPos * 4 * 10, buffer);

            Flush();
            TexID = 0;
            BatchStarted = false;
        }

        public void Flush()
        {
            GL.DrawElements(PrimitiveType.Triangles, NumIndices, DrawElementsType.UnsignedInt, 0);
        }

        public Vector2[] GetUVCoords(Vector2 sprStart, Vector2 sprSize, float texWidth, float texHeight, bool mirrorX, bool mirrorY)
        {
            // Order: top-right, bottom-right, bottom-left, top-left
            Vector2[] uvCoords = new Vector2[4];
            Vector2 start = new Vector2(sprStart.X, sprStart.Y);
            Vector2 end = start + new Vector2(sprSize.X, sprSize.Y);

            float right = end.X / texWidth;
            float left = start.X / texWidth;
            float top = 1 - (start.Y / texHeight);
            float bottom = 1 - (end.Y / texHeight);

            uvCoords[0] = new Vector2(mirrorX ? left : right, mirrorY ? bottom : top);
            uvCoords[1] = new Vector2(mirrorX ? left : right, mirrorY ? top : bottom);
            uvCoords[2] = new Vector2(mirrorX ? right : left, mirrorY ? top : bottom);
            uvCoords[3] = new Vector2(mirrorX ? right : left, mirrorY ? bottom : top);

            return uvCoords;
        }

        /*
        public void DrawLayerSprite(UIScene scene, UILayer layer, float time)
        {
            
            if (layer.SubImageIndices[0] == -1 || layer.SubImageIndices[0] > scene.Sprites.Count)
                return;

            Sprite spr = scene.Sprites[layer.SubImageIndices[0]].Sprite;
            if (TexID != spr.Texture.ID)
            {
                EndBatch();
                BeginBatch();
                TexID = spr.Texture.ID;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            spr.Texture.Use();

            UIVector2 resultTranslate = new UIVector2();
            UIVector2 resultScale = new UIVector2(1.0f, 1.0f);
            UIVector2 pivot = new UIVector2(0.0f, 0.0f);
            UIVector2 tpivot = new UIVector2(0.0f, 0.0f);
            Color resultColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            int spriteIndex = layer.SubImageIndices[0];
            float resultRotation = 0.0f;

            UILayer current = layer;
            foreach (var anim in scene.Animations)
            {
                foreach (var layerAnimList in anim.LayerAnimations)
                {
                    AnimationTrack a = layerAnimList.GetTrack(AnimationType.SubImage);
                    if (a != null && anim.Enabled)
                    {
                        int value = (int)a.GetValue(time);
                        if (value >= 0 && value < layer.SubImageIndices.Length)
                        {
                            int index = layer.SubImageIndices[value];
                            if (index >= 0 && index < scene.Sprites.Count)
                            {
                                spriteIndex = index;
                                spr = scene.Sprites[spriteIndex].Sprite;
                            }
                        }
                    }
                }
            }

            while (current != null)
            {
                bool hasTx = false;
                bool hasTy = false;
                bool hasRot = false;
                bool hasSx = false;
                bool hasSy = false;
                bool hasCol = false;
                foreach (var anim in scene.Animations)
                {
                    AnimationTrack a = anim.GetTrack(current, AnimationType.XPosition);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasTx = true;
                            resultTranslate.X += a.GetValue(time);
                        }
                    }

                    a = anim.GetTrack(current, AnimationType.YPosition);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasTy = true;
                            resultTranslate.Y += a.GetValue(time);
                        }
                    }

                    a = anim.GetTrack(current, AnimationType.Rotation);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasRot = true;
                            resultRotation += a.GetValue(time);
                        }
                    }

                    a = anim.GetTrack(current, AnimationType.XScale);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasSx = true;
                            resultScale.X *= a.GetValue(time);
                        }
                    }

                    a = anim.GetTrack(current, AnimationType.YScale);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasSy = true;
                            resultScale.Y *= a.GetValue(time);
                        }
                    }

                    a = anim.GetTrack(current, AnimationType.Color);
                    if (a != null)
                    {
                        if (anim.Enabled)
                        {
                            hasCol = true;
                            float val = a.GetValue(time);

                            Color c = new Color();
                            byte[] bytes = BitConverter.GetBytes(val);

                            c.R = bytes[3];
                            c.G = bytes[2];
                            c.B = bytes[1];
                            c.A = bytes[0];

                            Vector4 v = resultColor.ToFloats() * c.ToFloats();
                            resultColor = new Color(v.X, v.Y, v.Z, v.W);
                        }
                    }
                }

                if (!hasTx)
                    resultTranslate.X += current.Translation.X;
                if (!hasTy)
                    resultTranslate.Y += current.Translation.Y;

                if (!hasRot)
                    resultRotation += current.Rotation;

                if (!hasSx)
                    resultScale.X *= current.Scale.X;
                if (!hasSy)
                    resultScale.Y *= current.Scale.Y;

                if (!hasCol)
                {
                    Vector4 v = resultColor.ToFloats() * current.Color.ToFloats();
                    resultColor = new Color(v.X, v.Y, v.Z, v.W);
                }    

                resultTranslate += current.Offset;
                current = current.Parent;
            }

            if ((layer.Field34 & (1 << 10)) == 0)
                resultScale = layer.Scale;

            // pivot
            float diff = Math.Abs(layer.TopRight.X) - Math.Abs(layer.TopLeft.X);
            float right = diff * RenderWidth * resultScale.X;
            pivot.X += right / 2.0f;

            diff = Math.Abs(layer.BottomRight.Y) - Math.Abs(layer.TopRight.Y);
            float up = diff * RenderHeight * resultScale.Y;
            pivot.Y -= up / 2.0f;

            /*
            if (layer.Parent != null)
            {
                bool hasSx = false;
                bool hasSy = false;
                UIVector2 parentScale = new UIVector2(1.0f, 1.0f);

                foreach (var anim in scene.Animations)
                {
                    AnimationTrack a = anim.GetTrack(layer.Parent, AnimationType.XScale);
                    if (a != null && anim.Enabled)
                    {
                        hasSx = true;
                        parentScale.X *= a.GetValue(time);
                    }

                    a = anim.GetTrack(layer.Parent, AnimationType.YScale);
                    if (a != null && anim.Enabled)
                    {
                        hasSy = true;
                        parentScale.Y *= a.GetValue(time);
                    }
                }

                if (!hasSx)
                    parentScale.X *= layer.Parent.Scale.X;
                if (!hasSy)
                    parentScale.Y *= layer.Parent.Scale.Y;

                UIVector2 adjust = new UIVector2();
                float xAdjust = Math.Abs(layer.Parent.TopRight.X) - Math.Abs(layer.Parent.TopLeft.X);
                adjust.X = (xAdjust * parentScale.X) - xAdjust;

                float yAdjust = Math.Abs(layer.Parent.BottomRight.Y) - Math.Abs(layer.Parent.TopRight.Y);
                adjust.Y = (yAdjust * parentScale.Y) - yAdjust;

                resultTranslate.X += adjust.X;
                resultTranslate.Y += adjust.Y;
            }
            

            // subtract half of the render dimensions to transform the point (0, 0) from the center to the top left corner
            float x = (resultTranslate.X * RenderWidth) - (RenderWidth / 2.0f);
            float y = (RenderHeight / 2.0f) - (resultTranslate.Y * RenderHeight);

            Matrix4 model = CreateModelMatrix(new Vector2(x, y), new Vector2(pivot.X, pivot.Y), resultRotation,
                new Vector2(spr.Dimensions.X * resultScale.X, spr.Dimensions.Y * resultScale.Y));

            var uvCoords = GetUVCoords(spr, spr.Texture.Width, spr.Texture.Height, (layer.Field38 & 1024) != 0, (layer.Field38 & 2048) != 0);
            PushQuadBuffer(model, uvCoords, resultColor.ToFloats(), layer.GradientTopRight.ToFloats(), layer.GradientBottomRight.ToFloats(),
                layer.GradientBottomLeft.ToFloats(), layer.GradientTopLeft.ToFloats());
            
        }
        */

        public void PushQuadBuffer(Matrix4x4 model, Vector2[] uvCoords, Vector4 color, Vector4 colorTopRight, Vector4 colorBottomRight, Vector4 colorBottomLeft, Vector4 colorTopLeft)
        {
            int offset = 0;
            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], model);
            buffer[BufferPos + offset].Color = color * colorTopRight;
            buffer[BufferPos + offset].UV = uvCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], model);
            buffer[BufferPos + offset].Color = color * colorBottomRight;
            buffer[BufferPos + offset].UV = uvCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], model);
            buffer[BufferPos + offset].Color = color * colorBottomLeft;
            buffer[BufferPos + offset].UV = uvCoords[offset];
            ++offset;

            buffer[BufferPos + offset].Position = Vector4.Transform(vPos[offset], model);
            buffer[BufferPos + offset].Color = color * colorTopLeft;
            buffer[BufferPos + offset].UV = uvCoords[offset];

            BufferPos += 4;
            ++NumQuads;
            NumIndices += 6;
        }

        /*
        public void DrawLayerFont(UIScene scene, IEnumerable<UIFont> fonts, UILayer layer, float time)
        {
            
            if (string.IsNullOrEmpty(layer.FontName) || string.IsNullOrEmpty(layer.FontCharacters))
                return;

            UIFont targetFont = null;
            foreach (var font in fonts)
            {
                if (font.Name == layer.FontName)
                {
                    targetFont = font;
                    break;
                }
            }

            float xOffset = 0.0f;
            foreach (var character in layer.FontCharacters)
            {
                int sprIndex = -1;
                foreach (var mapping in targetFont.Mappings)
                {
                    if (character == mapping.Character)
                    {
                        sprIndex = mapping.SpriteIndex;
                        break;
                    }
                }

                if (sprIndex != -1)
                {
                    var spr = scene.Sprites[sprIndex].Sprite;
                    if (TexID != spr.Texture.ID)
                    {
                        EndBatch();
                        BeginBatch();
                        TexID = spr.Texture.ID;
                    }

                    GL.ActiveTexture(TextureUnit.Texture0);
                    spr.Texture.Use();

                    UIVector2 resultTranslate = new UIVector2();
                    UIVector2 resultScale = new UIVector2(1.0f, 1.0f);
                    UIVector2 pivot = new UIVector2(0.0f, 0.0f);
                    int spriteIndex = layer.SubImageIndices[0];
                    float resultRotation = 0.0f;

                    UILayer current = layer;
                    foreach (var anim in scene.Animations)
                    {
                        if (anim.LayerHasAnimation(current, AnimationType.SubImage) && anim.Enabled)
                        {
                            AnimationTrack a = anim.GetTrack(current, AnimationType.SubImage);
                            if (a != null)
                            {
                                int value = (int)a.GetValue(time);
                                if (value >= 0 && value < layer.SubImageIndices.Length)
                                {
                                    int index = layer.SubImageIndices[value];
                                    if (index >= 0 && index < scene.Sprites.Count)
                                    {
                                        spriteIndex = index;
                                        spr = scene.Sprites[spriteIndex].Sprite;
                                    }
                                }
                            }
                        }
                    }

                    while (current != null)
                    {
                        bool hasTx = false;
                        bool hasTy = false;
                        bool hasRot = false;
                        bool hasSx = false;
                        bool hasSy = false;
                        bool hasCol = false;
                        foreach (var anim in scene.Animations)
                        {
                            AnimationTrack a = anim.GetTrack(current, AnimationType.XPosition);
                            if (a != null)
                            {
                                if (anim.Enabled)
                                {
                                    hasTx = true;
                                    resultTranslate.X += a.GetValue(time);
                                }
                            }

                            a = anim.GetTrack(current, AnimationType.YPosition);
                            if (a != null)
                            {
                                if (anim.Enabled)
                                {
                                    hasTy = true;
                                    resultTranslate.Y += a.GetValue(time);
                                }
                            }

                            a = anim.GetTrack(current, AnimationType.Rotation);
                            if (a != null)
                            {
                                if (anim.Enabled)
                                {
                                    hasRot = true;
                                    resultRotation += a.GetValue(time);
                                }
                            }

                            a = anim.GetTrack(current, AnimationType.XScale);
                            if (a != null)
                            {
                                if (anim.Enabled)
                                {
                                    hasSx = true;
                                    resultScale.X *= a.GetValue(time);
                                }
                            }

                            a = anim.GetTrack(current, AnimationType.YScale);
                            if (a != null)
                            {
                                if (anim.Enabled)
                                {
                                    hasSy = true;
                                    resultScale.Y *= a.GetValue(time);
                                }
                            }
                        }

                        if (!hasTx)
                            resultTranslate.X += current.Translation.X;
                        if (!hasTy)
                            resultTranslate.Y += current.Translation.Y;

                        if (!hasRot)
                            resultRotation += current.Rotation;

                        if (!hasSx)
                            resultScale.X *= current.Scale.X;
                        if (!hasSy)
                            resultScale.Y *= current.Scale.Y;

                        resultTranslate += current.Offset;
                        current = current.Parent;
                    }

                    float diffX = Math.Abs(layer.TopRight.X) - Math.Abs(layer.TopLeft.X);
                    pivot = GetPivot(layer);

                    xOffset += spr.Dimensions.X / 2.0f * resultScale.X;// - (Math.Abs(diffX) * spr.Dimensions.X / 2.0f * resultScale.X / 2.0f);

                    resultTranslate.X -= layer.Width / 2.0f * resultScale.X / RenderWidth;
                    float x = (resultTranslate.X * RenderWidth) + xOffset - (RenderWidth / 2.0f);
                    float y = (RenderHeight / 2.0f) - (resultTranslate.Y * RenderHeight);

                    Matrix4 model = CreateModelMatrix(new Vector2(x, y), new Vector2(pivot.X, pivot.Y), resultRotation,
                        new Vector2(spr.Dimensions.X * resultScale.X, spr.Dimensions.Y * resultScale.Y));

                    var uvCoords = GetUVCoords(spr, spr.Texture.Width, spr.Texture.Height, false, false);
                    PushQuadBuffer(model, uvCoords, layer.Color.ToFloats(), layer.GradientTopRight.ToFloats(), layer.GradientBottomRight.ToFloats(),
                        layer.GradientBottomLeft.ToFloats(), layer.GradientTopLeft.ToFloats());

                    xOffset += (spr.Dimensions.X / 2.0f * resultScale.X) - (Math.Abs(diffX) * spr.Dimensions.X / 2.0f * resultScale.X);
                }
            }
            
        }
        */

        public void ConfigureShader(ShaderProgram shader)
        {
            shader.Use();
            shader.SetMatrix4("projection", OpenTK.Mathematics.Matrix4.CreateOrthographicOffCenter(0.0f, RenderWidth, -RenderHeight, 0.0f, -1.0f, 2.0f));
        }

        public Matrix4x4 CreateModelMatrix(Vector2 position, Vector2 pivot, float rotation, Vector2 size)
        {
            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateScale(size.X, size.Y, 1.0f));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(pivot.X, pivot.Y, 0.0f));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateRotationZ(Utilities.ToRadians(rotation)));
            model = Matrix4x4.Multiply(model, Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f));

            return model;
        }

        public void DrawBoundingBox(BoundingBox box)
        {

        }
    }
}
