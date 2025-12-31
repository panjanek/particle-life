using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenTK.Mathematics;
using ParticleLife.Models;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ParticleLife.Gui
{
    public class ForceMatrix : Canvas
    {
        private Rectangle[,] rectangles;

        public int SelectedX { get; set; }

        public int SelectedY { get; set; }

        public ForceMatrix()
            :base()
        {
            rectangles = new Rectangle[Simulation.MaxSpeciesCount, Simulation.MaxSpeciesCount];
            SelectedX = 0;
            SelectedY = 0;
            Loaded += ForceMatrix_Loaded;
        }

        private void ForceMatrix_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Background = Brushes.White;
            var rectSize = Width / Simulation.MaxSpeciesCount;
            var a = ActualHeight;
            for (int x = 0; x < Simulation.MaxSpeciesCount; x++)
            {
                for (int y = 0; y < Simulation.MaxSpeciesCount; y++)
                {
                    var rect = new Rectangle();
                    rect.SetValue(Canvas.LeftProperty, x * rectSize);
                    rect.SetValue(Canvas.TopProperty, y * rectSize);
                    rect.Stroke = Brushes.Black;
                    rect.StrokeThickness = 1;
                    rect.Fill = Brushes.White;
                    rect.Width = rectSize;
                    rect.Height = rectSize;
                    rect.Visibility = System.Windows.Visibility.Visible;
                    rect.Tag = $"{x},{y}";
                    rect.MouseDown += (s, e) =>
                    {
                        var tag = WpfUtil.GetTagAsString(s);
                        var split = tag.Split(',');
                        SelectedX = int.Parse(split[0]);
                        SelectedY = int.Parse(split[1]);
                        UpdateSelection();
                    };

                    Children.Add(rect);
                    rectangles[x, y] = rect;
                }
            }

            UpdateSelection();
        }

        public void UpdateSelection()
        {
            for (int x = 0; x < Simulation.MaxSpeciesCount; x++)
            {
                for (int y = 0; y < Simulation.MaxSpeciesCount; y++)
                {
                    var rect = rectangles[x, y];
                    if (x == SelectedX && y == SelectedY)
                    {
                        rect.Stroke = Brushes.LightGreen;
                        rect.StrokeThickness = 2;
                    }
                    else
                    {
                        rect.Stroke = Brushes.Black;
                        rect.StrokeThickness = 1;
                    }
                }
            }
        }
    
        public void UpdateCells(Vector4[] forces, int speciesCount)
        {
            var inactive = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 32, 32, 32));
            for (int x = 0; x < Simulation.MaxSpeciesCount; x++)
            {
                for (int y = 0; y < Simulation.MaxSpeciesCount; y++)
                {
                    if (x < speciesCount && y < speciesCount)
                    {
                        var offset = Simulation.GetForceOffset(x, y);
                        double val = 0;
                        for (int i = 1; i < Simulation.KeypointsCount; i++)
                        {
                            val += forces[offset + i].Y;
                        }

                        var r = (val > 0) ? val / 10 : 0;
                        var b = (val < 0) ? -val / 10 : 0;
                        var rect = rectangles[x, y];
                        rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, ClampColor(r), 0, ClampColor(b)));
                    }
                    else
                    {
                        rectangles[x, y].Fill = inactive;
                    }
                 }
            }
        }

        private byte ClampColor(double x)
        {
            var c = (int)Math.Round(255 * x);
            if (c < 0)
                c = 0;
            if (c > 255)
                c = 255;
            return (byte)c;
        }
    }
}
