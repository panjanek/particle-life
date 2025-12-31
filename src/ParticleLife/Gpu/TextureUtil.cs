using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace ParticleLife.Gpu
{
    public static class TextureUtil
    {
        public static int CreateRgba32fTexture(int width, int height)
        {
            int plotTex;
            GL.GenTextures(1, out plotTex);
            GL.BindTexture(TextureTarget.Texture2D, plotTex);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba32f, // accumulation-safe  //R32ui //Rgba32f
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            return plotTex;
        }

        public static int CreateIntegerTexture(int width, int height)
        {
            int plotTex;
            GL.GenTextures(1, out plotTex);
            GL.BindTexture(TextureTarget.Texture2D, plotTex);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.R32ui,
                width,
                height,
                0,
                PixelFormat.RedInteger,   // IMPORTANT
                PixelType.UnsignedInt,
                IntPtr.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            return plotTex;
        }
    }
}
