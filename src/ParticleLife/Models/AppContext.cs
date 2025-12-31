using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParticleLife.Gpu;
using ParticleLife.Gui;

namespace ParticleLife.Models
{
    public class AppContext
    {
        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;
    }
}
