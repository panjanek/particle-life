using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ParticleLife.Utils;
using AppContext = ParticleLife.Models.AppContext;

namespace ParticleLife.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppContext app;

        private bool updating;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
        }

        private void global_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fieldSize != null && particlesCount != null && speciesCount!=null && !updating)
            {
                var newParticleCountStr = WpfUtil.GetComboSelectionAsString(particlesCount);
                var newSpeciesCountStr = WpfUtil.GetComboSelectionAsString(speciesCount);
                var newSizeStr = WpfUtil.GetComboSelectionAsString(fieldSize);
                if (!string.IsNullOrWhiteSpace(newParticleCountStr) && !string.IsNullOrWhiteSpace(newSpeciesCountStr) && !string.IsNullOrWhiteSpace(newSizeStr))
                {
                    var newParticleCount = int.Parse(newParticleCountStr);
                    var newSpeciesCount = int.Parse(newSpeciesCountStr);
                    var sizeSplit = newSizeStr.Split('x');
                    var newWidth = int.Parse(sizeSplit[0]);
                    var newHeight = int.Parse(sizeSplit[1]);
                    if (newParticleCount != app.simulation.shaderConfig.particleCount ||
                        newSpeciesCount != app.simulation.shaderConfig.speciesCount ||
                        newWidth != app.simulation.shaderConfig.width ||
                        newHeight != app.simulation.shaderConfig.height)
                    {
                        app.simulation.StartSimulation(newParticleCount, newSpeciesCount, newWidth, newHeight);
                        app.renderer.UploadParticleData();
                        UpdateActiveControls();
                        UpdatePassiveControls();
                    }
                }

            }
        }

        public void UpdateActiveControls()
        {
            updating = true;
            WpfUtil.SetComboStringSelection(fieldSize, $"{app.simulation.shaderConfig.width}x{app.simulation.shaderConfig.height}");
            WpfUtil.SetComboStringSelection(particlesCount, app.simulation.shaderConfig.particleCount.ToString());
            WpfUtil.SetComboStringSelection(speciesCount, app.simulation.shaderConfig.particleCount.ToString());
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
                var tag = WpfUtil.GetTagAsString(slider);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    slider.Value = ReflectionUtil.GetObjectValue<float>(app.simulation, tag);
                }
            }
            updating = false;
        }

        public void UpdatePassiveControls()
        {

        }
    }
}
