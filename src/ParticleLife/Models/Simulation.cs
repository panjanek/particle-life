using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace ParticleLife.Models
{
    public class Simulation
    {
        public const int MaxSpeciesCount = 10;

        public const int KeypointsCount = 6;

        public Simulation(int particleCount)
        {
            shaderConfig = new ShaderConfig();
            forces = new Vector4[MaxSpeciesCount * MaxSpeciesCount * KeypointsCount];

            shaderConfig.speciesCount = 6;
            SetForce(0, 0, 5, 0);
            SetForce(1, 1, 5, 0);

            SetForce(0, 1, -5, 0);
            SetForce(1, 0, -5, 0);

            var rnd = new Random(4);
            for(int i=0; i<shaderConfig.speciesCount; i++)
            {
                for(int j=0; j<shaderConfig.speciesCount; j++)
                {
                    float v1 = (float)(10 * (rnd.NextDouble() - 0.5));
                    float v2 = (float)(5 * (rnd.NextDouble() - 0.5));
                    SetForce(i, j, v1, v2);
                }
            }


            SetupParticles(particleCount);
            shaderConfig.particleCount = particleCount;
            
        }

        public ShaderConfig shaderConfig;

        public Particle[] particles;

        public Vector4[] forces;

        private void SetForce(int specMe, int specOther, float val1, float val2)
        {
            int offset = (specMe * MaxSpeciesCount + specOther) * KeypointsCount;
            forces[offset + 0] = new Vector4(0, -5, 0, 0);
            forces[offset + 1] = new Vector4(10, 0, 0, 0);
            forces[offset + 2] = new Vector4(20, val1, 0, 0);
            forces[offset + 3] = new Vector4(40, 0, 0, 0);
            forces[offset + 4] = new Vector4(50, val2, 0, 0);
            forces[offset + 5] = new Vector4(60, 0, 0, 0);
        }

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
