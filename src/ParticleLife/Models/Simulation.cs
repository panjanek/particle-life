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

        public ShaderConfig config;

        public Particle[] particles;

        public Vector4[] forces;

        public int seed = 6;

        public Simulation()
        {
            config = new ShaderConfig();
            forces = new Vector4[MaxSpeciesCount * MaxSpeciesCount * KeypointsCount];
        }

        public void StartSimulation(int particlesCount, int speciesCount, float width, float height)
        {
            config.speciesCount = speciesCount;
            config.width = width;
            config.height = height;
            config.particleCount = particlesCount;
            InitializeParticles(particlesCount);
            InitializeRandomForces();

        }

        public static int GetForceOffset(int specMe, int specOther)
        {
            int offset = (specMe * MaxSpeciesCount + specOther) * KeypointsCount;
            return offset;

        }

        private void SetForce(int specMe, int specOther, float val1, float val2)
        {
            int offset = GetForceOffset(specMe, specOther);
            forces[offset + 0] = new Vector4(0, -5, 0, 0);
            forces[offset + 1] = new Vector4(10, 0, 0, 0);
            forces[offset + 2] = new Vector4(20, val1, 0, 0);
            forces[offset + 3] = new Vector4(40, 0, 0, 0);
            forces[offset + 4] = new Vector4(50, val2, 0, 0);
            forces[offset + 5] = new Vector4(60, 0, 0, 0);
        }

        public void InitializeRandomForces()
        {
            var rnd = new Random(seed); //4
            for (int i = 0; i < config.speciesCount; i++)
            {
                for (int j = 0; j < config.speciesCount; j++)
                {
                    float v1 = (float)(10 * (rnd.NextDouble() - 0.5));
                    float v2 = (float)(5 * (rnd.NextDouble() - 0.5));
                    SetForce(i, j, v1, v2);
                }
            }
        }

        public void InitializeParticles(int count)
        {
            if (particles == null || particles.Length != count)
                particles = new Particle[count];

            var rnd = new Random(1);
            for(int i=0; i< count; i++)
            {
                particles[i].position = new Vector2((float)(config.width * rnd.NextDouble()), (float)(config.height * rnd.NextDouble()));
                particles[i].velocity = new Vector2((float)(100*config.dt * (rnd.NextDouble()-0.5)), 100*(float)(config.dt * (rnd.NextDouble()-0.5)));
                particles[i].species = rnd.Next(config.speciesCount);
            }
        }
    }
}
