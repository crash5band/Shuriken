using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Shuriken.Models
{
    public static class Project
    {
        public static ObservableCollection<UIScene> Scenes { get; set; } = new ObservableCollection<UIScene>();
        public static ObservableCollection<TextureList> TextureLists { get; set; } = new ObservableCollection<TextureList>();
        public static ObservableCollection<UIFont> Fonts { get; set; } = new ObservableCollection<UIFont>();
        public static Dictionary<int, Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();

        private static int NextSpriteID = 1;

        public static Sprite TryGetSprite(int id)
        {
            Sprites.TryGetValue(id, out Sprite sprite);
            return sprite;
        }

        public static int AppendSprite(Sprite spr)
        {
            Sprites.Add(NextSpriteID, spr);
            return NextSpriteID++;
        }

        public static int CreateSprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Sprite spr = new Sprite(NextSpriteID, tex, top, left, bottom, right);
            return AppendSprite(spr);
        }

        public static void RemoveSprite(int id)
        {
            Sprites.Remove(id);
        }

        public static void Clear()
        {
            Scenes.Clear();
            Fonts.Clear();
            Sprites.Clear();

            foreach (var texlist in TextureLists)
            {
                foreach (var tex in texlist.Textures)
                    tex.GlTex.Dispose();
            }

            TextureLists.Clear();
            NextSpriteID = 1;
        }
    }
}
