using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife.Models
{
    public class Simulation
    {
        public Simulation(int particleCount)
        {
            shaderConfig = new ShaderConfig();
            shaderConfig.particleCount = particleCount;
            particles = new Particle[particleCount];
        }

        public ShaderConfig shaderConfig;

        public Particle[] particles;

        private void SetupParticle(Particle[] buffer, int idx)
        {
            var rnd = new Random(1);
            uint seed = (uint)(idx << 2);
            float r = shaderConfig.initR * (SimpleRand(seed) - 0.5f);
            float angle = SimpleRand(seed + 1) * 2 * (float)Math.PI;
            buffer[idx].position.X = shaderConfig.initPos.X + r * (float)Math.Sin(angle);
            buffer[idx].position.Y = shaderConfig.initPos.Y + r * (float)Math.Cos(angle);

            r = shaderConfig.initVR * (SimpleRand(seed + 2) - 0.5f);
            angle = SimpleRand(seed + 3) * 2 * (float)Math.PI;
            buffer[idx].velocity.X = shaderConfig.initVel.X + r * (float)Math.Sin(angle);
            buffer[idx].velocity.Y = shaderConfig.initVel.Y + r * (float)Math.Cos(angle);
        }
    }
}
