using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 36)]
    public struct ShaderConfig
    {
        public ShaderConfig()
        {

        }

        [FieldOffset(0)] public int particleCount;

        [FieldOffset(4)] public float dt = 0.1f;

        [FieldOffset(8)] public float sigma2 = 0.1f;

        [FieldOffset(12)] public float clampVel = 10000f;

        [FieldOffset(16)] public float clampAcc = 10000f;

        [FieldOffset(20)] public float width = 1920;

        [FieldOffset(24)] public float height = 1080;

        [FieldOffset(28)] public float maxDist = 200;

        [FieldOffset(32)] public int speciesCount = 2;
    }
}
