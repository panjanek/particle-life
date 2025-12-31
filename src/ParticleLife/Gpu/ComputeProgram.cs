using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using ParticleLife.Models;

namespace ParticleLife.Gpu
{
    public class ComputeProgram
    {
        private int program;

        private int ubo;

        private int maxGroupsX;

        private int dummyVao;

        private int pointsBuffer;

        private int pointsCount;

        private int shaderPointStrideSize;

        public ComputeProgram()
        {
            ubo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ubo);
            int configSizeInBytes = Marshal.SizeOf<ShaderConfig>();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, configSizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ubo);
            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);

            // create dummy vao
            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);

            shaderPointStrideSize = Marshal.SizeOf<Particle>();
            program = ShaderUtil.CompileAndLinkComputeShader("solver.comp");
        }

        public void Run(ShaderConfig config)
        {
            PrepareBuffer(config.particleCount);

            //upload config
            int configSizeInBytes = Marshal.SizeOf<ShaderConfig>();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ubo);
            GL.BufferSubData(
                BufferTarget.ShaderStorageBuffer,
                IntPtr.Zero,
                Marshal.SizeOf<ShaderConfig>(),
                ref config
            );

            GL.UseProgram(program);
            int dispatchGroupsX = (pointsCount + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX;
            if (dispatchGroupsX > maxGroupsX)
                dispatchGroupsX = maxGroupsX;
            GL.DispatchCompute(dispatchGroupsX, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        public void UploadData(Particle[] particles)
        {
            PrepareBuffer(particles.Length);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBuffer);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, particles.Length * shaderPointStrideSize, particles);
        }

        private void PrepareBuffer(int size)
        {
            if (pointsCount != size)
            {
                if (pointsBuffer > 0)
                {
                    GL.DeleteBuffer(pointsBuffer);
                    pointsBuffer = 0;
                }
                GL.GenBuffers(1, out pointsBuffer);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBuffer);
                pointsCount = size;
                
                GL.BufferData(BufferTarget.ShaderStorageBuffer, pointsCount * shaderPointStrideSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBuffer);
            }
        }
    }
}
