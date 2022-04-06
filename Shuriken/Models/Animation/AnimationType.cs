using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models.Animation
{
    public enum AnimationType : uint
    {
        None        = 0,
        Unknown     = 1,
        XPosition   = 2,
        YPosition   = 4,
        Rotation    = 8,
        XScale      = 16,
        YScale      = 32,
        SubImage    = 64,
        Color       = 128,
        GradientTL  = 256,
        GradientBL  = 512,
        GradientTR  = 1024,
        GradientBR  = 2048
    }

    public static class AnimationTypeMethods
    {
        public static bool IsColor(this AnimationType type)
        {
            return new AnimationType[] { 
                AnimationType.Color,
                AnimationType.GradientTL,
                AnimationType.GradientBL,
                AnimationType.GradientTR,
                AnimationType.GradientBR
            }.Contains(type);
        }
    }
}
