using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace ParticleLife.Models
{
    public class Simulation
    {
        public Simulation(int particleCount)
        {
            shaderConfig = new ShaderConfig();
            SetupParticles(particleCount);
            shaderConfig.particleCount = particleCount;
            
        }

        public ShaderConfig shaderConfig;

        public Particle[] particles;

        private void SetupParticles(int count)
        {
            if (particles == null || particles.Length != count)
                particles = new Particle[count];

            var rnd = new Random(1);
            for(int i=0; i< count; i++)
            {
                particles[i].position = new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble()));
                particles[i].velocity = new Vector2((float)(100*shaderConfig.dt * (rnd.NextDouble()-0.5)), 100*(float)(shaderConfig.dt * (rnd.NextDouble()-0.5)));
                particles[i].species = rnd.Next(shaderConfig.speciesCount);
            }
        }
    }
}
