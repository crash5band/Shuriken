using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Shuriken.Models;
using Shuriken.ViewModels;
using XNCPLib.XNCP;

namespace Shuriken.Misc
{
    public static class Utilities
    {
        public static float ToRadians(float degrees)
        {
            return degrees * MathF.PI / 180.0f;
        }

        public static float ToDegrees(float radians)
        {
            return radians * 180.0f / MathF.PI;
        }

        public static float ConvertRange(float value, float oldLow, float oldHigh, float newLow, float newHigh)
        {
            float percent = (value - oldLow) / oldHigh;
            return (percent * (newHigh - newLow)) + newLow;
        }

        public static TreeViewItem GetParentTreeViewItem(TreeViewItem child)
        {
            if (child != null)
            {
                var parent = VisualTreeHelper.GetParent(child);
                while (parent is not TreeViewItem && parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                return parent is TreeViewItem ? parent as TreeViewItem : null;
            }

            return null;
        }

        public static int ToPixels(float v, float factor)
        {
            return (int)(v * factor);
        }

        public static int FindSpriteIDFromNCPScene(int spriteIndex, List<SubImage> spriteList, ObservableCollection<Texture> textures)
        {
            if (spriteIndex >= 0 && spriteIndex < spriteList.Count)
            {
                int textureIndex = (int)spriteList[spriteIndex].TextureIndex;
                if (textureIndex >= 0 && textureIndex < textures.Count)
                {
                    var sprites = textures[textureIndex].Sprites;
                    var target = spriteList[spriteIndex];
                    for (int s = 0; s < sprites.Count; ++s)
                    {
                        int x = ToPixels(target.TopLeft.X, textures[textureIndex].Width);
                        int y = ToPixels(target.TopLeft.Y, textures[textureIndex].Height);
                        int w = ToPixels(target.BottomRight.X - target.TopLeft.X, textures[textureIndex].Width);
                        int h = ToPixels(target.BottomRight.Y - target.TopLeft.Y, textures[textureIndex].Height);

                        var spr = Project.TryGetSprite(sprites[s]);
                        if (spr.X == x && spr.Y == y
                            && spr.Width == w
                            && spr.Height == h)
                        {
                            return textures[textureIndex].Sprites[s];
                        }
                    }
                }
            }

            return -1;
        }
    }
}
