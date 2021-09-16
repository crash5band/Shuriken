using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Shuriken.Rendering
{
    internal class ShaderProgram
    {
        public int ID { get; private set; } = 0;
        public string Name { get; private set; }

        /// <summary>
        /// Compiles the shader program using the specified vertex and fragment
        /// programs.
        /// </summary>
        /// <param name="vertexPath">The path to the vertex program.</param>
        /// <param name="fragmentPath">The path to the fragment program.</param>
        public void Compile(string name, string vertexPath, string fragmentPath)
        {
            Name = name;
            string vertexSource = "";
            string fragmentSource = "";

            using (StreamReader reader = new StreamReader(vertexPath))
            {
                vertexSource = reader.ReadToEnd();
            };

            using (StreamReader reader = new StreamReader(fragmentPath))
            {
                fragmentSource = reader.ReadToEnd();
            };

            // Create shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            string vLog = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrEmpty(vLog))
                Console.WriteLine(vLog);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            string fLog = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrEmpty(fLog))
                Console.WriteLine(fLog);

            // Link shaders to program
            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertexShader);
            GL.AttachShader(ID, fragmentShader);
            GL.LinkProgram(ID);

            // Cleanup
            GL.DetachShader(ID, vertexShader);
            GL.DetachShader(ID, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void SetUniform(string attribute, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, attribute), value);
        }

        public void SetUniform(string attribute, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, attribute), value);
        }

        public void SetMatrix4(string name, Matrix4 mat)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(ID, name), true, ref mat);
        }

        public void Use()
        {
            GL.UseProgram(ID);
        }

        public ShaderProgram(string name, string vertexPath, string fragmentPath)
        {
            Compile(name, vertexPath, fragmentPath);
        }

        public ShaderProgram()
        {

        }
    }
}
