using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ParticleLife.Models;

namespace ParticleLife.Gpu
{
    public class ComputeProgram
    {
        private int program;

        private int uboConfig;

        private int uboForces;

        private int maxGroupsX;

        private int pointsBufferA;

        private int pointsBufferB;

        private int pointsTorusBuffer;

        private int pointsCount;

        private int shaderPointStrideSize;

        public ComputeProgram()
        {
            uboConfig = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
            int configSizeInBytes = Marshal.SizeOf<ShaderConfig>();
            GL.BufferData(BufferTarget.UniformBuffer, configSizeInBytes, IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, uboConfig);

            uboForces = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, uboForces);
            int forceSizeInBytes = Marshal.SizeOf<Vector4>();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, forceSizeInBytes * Simulation.MaxSpeciesCount * Simulation.MaxSpeciesCount * Simulation.KeypointsCount, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, uboForces);

            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);
            shaderPointStrideSize = Marshal.SizeOf<Particle>();
            program = ShaderUtil.CompileAndLinkComputeShader("solver.comp");
        }

        public void Run(ShaderConfig config, Vector4[] forces)
        {
            PrepareBuffer(config.particleCount);

            //upload config
            GL.BindBuffer(BufferTarget.UniformBuffer, uboConfig);
            GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf<ShaderConfig>(), ref config, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, uboConfig);

            //upload forces
            int forcesSizeInBytes = Marshal.SizeOf<Vector4>() * Simulation.MaxSpeciesCount * Simulation.MaxSpeciesCount * Simulation.KeypointsCount;
            GL.BindBuffer(BufferTarget.UniformBuffer, uboForces);
            GL.BufferData(BufferTarget.UniformBuffer, forcesSizeInBytes, forces, BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 4, uboForces);

            //bind storage buffers
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, pointsBufferA);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, pointsBufferB);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, pointsTorusBuffer);

            GL.UseProgram(program);
            int dispatchGroupsX = (pointsCount + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX;
            if (dispatchGroupsX > maxGroupsX)
                dispatchGroupsX = maxGroupsX;
            GL.DispatchCompute(dispatchGroupsX, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit | MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            (pointsBufferA, pointsBufferB) = (pointsBufferB, pointsBufferA);
        }

        public void UploadData(Particle[] particles)
        {
            PrepareBuffer(particles.Length);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferA);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, particles.Length * shaderPointStrideSize, particles);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferB);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, particles.Length * shaderPointStrideSize, particles);
        }

        private void PrepareBuffer(int size)
        {
            if (pointsCount != size)
            {
                pointsCount = size;

                //buffer A
                if (pointsBufferA > 0)
                {
                    GL.DeleteBuffer(pointsBufferA);
                    pointsBufferA = 0;
                }
                GL.GenBuffers(1, out pointsBufferA);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferA);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, pointsCount * shaderPointStrideSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                
                //buffer B
                if (pointsBufferB > 0)
                {
                    GL.DeleteBuffer(pointsBufferB);
                    pointsBufferB = 0;
                }
                GL.GenBuffers(1, out pointsBufferB);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsBufferB);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, pointsCount * shaderPointStrideSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                //torus buffer
                if (pointsTorusBuffer > 0)
                {
                    GL.DeleteBuffer(pointsTorusBuffer);
                    pointsTorusBuffer = 0;
                }
                GL.GenBuffers(1, out pointsTorusBuffer);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, pointsTorusBuffer);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, 9* pointsCount * shaderPointStrideSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
        }
    }
}
