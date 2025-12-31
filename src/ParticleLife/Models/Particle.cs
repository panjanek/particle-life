using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace ParticleLife.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector2 position;   // 8 bytes
        public Vector2 velocity;   // 8 bytes
        public int species;        // 4 bytes
        private int flags;         
    }
}
