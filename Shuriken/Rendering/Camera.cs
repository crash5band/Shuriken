using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using OpenTK.Mathematics;
using Shuriken.Misc;

namespace Shuriken.Rendering
{
    using Vec3 = Models.Vector3;

    internal class Camera
    {
        public string Name { get; set; }
        public Vec3 Position { get; set; }
        public Vec3 Target { get; set; }

        public float Near { get; set; }
        public float Far { get; set; }

        public Matrix4 GetProjectionMatrix(float aspect, float fovy)
        {
            return Matrix4.CreatePerspectiveFieldOfView(Utilities.ToRadians(fovy), aspect, Near, Far);
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(
                new OpenTK.Mathematics.Vector3(Position.X, Position.Y, Position.Z),
                new OpenTK.Mathematics.Vector3(Target.X, Target.Y, Target.Z),
                new OpenTK.Mathematics.Vector3(0.0f, 1.0f, 0.0f)
                );
        }

        public Camera(string name, Vec3 pos, Vec3 tgt)
        {
            Name = name;
            Position = pos;
            Target = tgt;
            Near = 0.01f;
            Far = 2000.0f;
        }
    }
}
