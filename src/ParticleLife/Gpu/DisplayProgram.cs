using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ParticleLife.Gpu
{
    public class DisplayProgram
    {
        private int program;

        private int projLocation;

        private int dummyVao;

        public DisplayProgram()
        {
            program = ShaderUtil.CompileAndLinkRenderShader("display.vert", "display.frag");
            projLocation = GL.GetUniformLocation(program, "projection");
            if (projLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");

            // create dummy vao
            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);
        }

        public void Run(Matrix4 projectionMatrix, int particlesCount)
        {
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(program);
            GL.BindVertexArray(dummyVao);
            GL.UniformMatrix4(projLocation, false, ref projectionMatrix);
            GL.DrawArrays(PrimitiveType.Points, 0, particlesCount);
        }
    }
}
