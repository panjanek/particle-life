using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
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

        [FieldOffset(20)] public float width;

        [FieldOffset(24)] public float height;
    }
}
