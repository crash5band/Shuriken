using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Rendering;
using Shuriken.Models.Animation;

namespace Shuriken.Models
{
    public class ScenesManager
    {
        Renderer renderer;

        public float Speed { get; set; }
        public float Time { get; set; }
        public bool Playing { get; set; }

        int sceneWidth;
        int sceneHeight;

        public void ResizeScene(int width, int height)
        {
            renderer.RenderWidth = width;
            renderer.RenderHeight = height;
        }

        public void UpdateScenes(IEnumerable<UIScene> scenes, IEnumerable<UIFont> fonts, float deltaT)
        {
            // Eventually all of the update logic for scenes should be moved here and the drawing is left to the renderer.
            if (Playing)
            {
                foreach (var scene in scenes)
                {
                    foreach (var animation in scene.Animations)
                        animation.AddTime(deltaT);
                }
            }

            renderer.DrawScenes(scenes, fonts);
        }

        public ScenesManager()
        {
            sceneWidth = 1280;
            sceneHeight = 720;
            renderer = new Renderer(sceneWidth, sceneHeight);
            
            Speed = 1.0f;
            Time = 0.0f;
            Playing = false;
        }
    }
}
