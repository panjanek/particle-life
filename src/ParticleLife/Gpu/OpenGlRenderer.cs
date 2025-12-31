using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using ParticleLife.Models;
using ParticleLife.Utils;
using AppContext = ParticleLife.Models.AppContext;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ParticleLife.Gpu
{
    public class OpenGlRenderer
    {
        public const double ZoomingSpeed = 0.0005;
        public int FrameCounter => frameCounter;

        public bool Paused { get; set; }

        public int? TrackedIdx { get; set; }

        private Panel placeholder;

        private System.Windows.Forms.Integration.WindowsFormsHost host;

        private GLControl glControl;

        private int frameCounter;

        private ComputeProgram computeProgram;

        private DisplayProgram displayProgram;

        private float zoom = 0.5f;

        private Vector2 center;

        private AppContext app;

        public OpenGlRenderer(Panel placeholder, AppContext app)
        {
            this.placeholder = placeholder;
            this.app = app;
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Visibility = Visibility.Visible;
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
            glControl = new GLControl(new GLControlSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3), // OpenGL 3.3
                Profile = ContextProfile.Compatability,
                Flags = ContextFlags.Default,
                IsEventDriven = false
            });
            glControl.Dock = DockStyle.Fill;
            host.Child = glControl;
            placeholder.Children.Add(host);
            glControl.Paint += GlControl_Paint;
            glControl.SizeChanged += GlControl_SizeChanged;

            //setup required features
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);

            computeProgram = new ComputeProgram();
            displayProgram = new DisplayProgram();
            UploadParticleData();

            center = new Vector2(app.simulation.shaderConfig.width / 2, app.simulation.shaderConfig.height / 2);

            var dragging = new DraggingHandler(glControl, (mousePos, isLeft) => isLeft, (prev, curr) =>
            {
                StopTracking();
                var delta = (curr - prev) / zoom;
                delta.Y = -delta.Y;
                center -= delta;

            }, () => { });

            glControl.MouseWheel += (s, e) =>
            {
                var pos = new Vector2(e.X, e.Y);
                float zoomRatio = (float)(1.0 + ZoomingSpeed * e.Delta);

                var projectionMatrix = GetProjectionMatrix();
                var topLeft1 = GpuUtil.ScreenToWorld(new Vector2(0, 0), projectionMatrix, glControl.Width, glControl.Height);
                var bottomRight1 = GpuUtil.ScreenToWorld(new Vector2(glControl.Width, glControl.Height), projectionMatrix, glControl.Width, glControl.Height);
                var zoomCenter = GpuUtil.ScreenToWorld(pos, projectionMatrix, glControl.Width, glControl.Height);

                var currentSize = bottomRight1 - topLeft1;
                var newSize = currentSize / (float)zoomRatio;

                var c = zoomCenter - topLeft1;
                var b = c / (float)zoomRatio;

                var topLeft2 = zoomCenter - b;
                var bottomRight2 = topLeft2 + newSize;

                center = (bottomRight2 + topLeft2) / 2;
                zoom = zoom * zoomRatio;
            };

            glControl.MouseDown += GlControl_MouseDown;
        }

        private void GlControl_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lock(app.simulation)
                {
                    computeProgram.DownloadData(app.simulation.particles);
                    double minDistance = app.simulation.shaderConfig.width * 10;
                    int closestIdx = 0;
                    var projectionMatrix = GetProjectionMatrix();
                    var mouseWorld = GpuUtil.ScreenToWorld(new Vector2(e.X, e.Y), projectionMatrix, glControl.Width, glControl.Height);

                    if (mouseWorld.X > app.simulation.shaderConfig.width)
                        mouseWorld.X -= app.simulation.shaderConfig.width;
                    if (mouseWorld.X < 0)
                        mouseWorld.X += app.simulation.shaderConfig.width;

                    if (mouseWorld.Y > app.simulation.shaderConfig.height)
                        mouseWorld.Y -= app.simulation.shaderConfig.height;
                    if (mouseWorld.Y < 0)
                        mouseWorld.Y += app.simulation.shaderConfig.height;

                    for (int idx = 0; idx< app.simulation.particles.Length; idx++)
                    {
                        var particlePosition = app.simulation.particles[idx].position;
                        var distance = Math.Sqrt((particlePosition.X - mouseWorld.X) * (particlePosition.X - mouseWorld.X) + (particlePosition.Y - mouseWorld.Y) * (particlePosition.Y - mouseWorld.Y));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestIdx = idx;
                        }
                    }

                    if (minDistance < 10)
                    {
                        if (TrackedIdx == closestIdx)
                            StopTracking();
                        else
                            StartTracking(closestIdx);
                    }
                }
            }
        }

        public void UploadParticleData()
        {
            computeProgram.UploadData(app.simulation.particles);
        }

        public void StartTracking(int idx)
        {
            TrackedIdx = idx;
            app.simulation.shaderConfig.trackedIdx = TrackedIdx ?? -1;
            computeProgram.Run(app.simulation.shaderConfig, app.simulation.forces);
        }

        public void StopTracking()
        {
            if (TrackedIdx != null)
            {
                TrackedIdx = null;
                app.simulation.shaderConfig.trackedIdx = TrackedIdx ?? -1;
                computeProgram.Run(app.simulation.shaderConfig, app.simulation.forces);
            }
        }

        private void GlControl_SizeChanged(object? sender, EventArgs e)
        {
            if (glControl.Width <= 0 || glControl.Height <= 0)
                return;

            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private Matrix4 GetProjectionMatrix()
        {
            // rescale by windows display scale setting to match WPF coordinates
            var w = (float)((glControl.Width / 1) / zoom) / 2;
            var h = (float)((glControl.Height / 1) / zoom) / 2;
            var translate = Matrix4.CreateTranslation(-center.X, -center.Y, 0.0f);
            var ortho = Matrix4.CreateOrthographicOffCenter(-w, w, -h, h, -1f, 1f);
            var matrix = translate * ortho;
            return matrix;
        }

        private void FollowTrackedParticle()
        {
            if (TrackedIdx.HasValue)
            {
                var projectionMatrix = GetProjectionMatrix();
                var tracked = computeProgram.GetTrackedParticle();
                var trackedScreenPosition = tracked.position;
                var delta = trackedScreenPosition - center;
                
                var move = delta * 0.05f;

                if (Math.Abs(delta.X) > 0.75* app.simulation.shaderConfig.width)
                {
                    move.X = (float)Math.Sign(delta.X) * app.simulation.shaderConfig.width;
                }

                if (Math.Abs(delta.Y) > 0.75 * app.simulation.shaderConfig.height)
                {
                    move.Y = (float)Math.Sign(delta.Y) * app.simulation.shaderConfig.height;
                }

                center += move;
            }
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            FollowTrackedParticle();
            displayProgram.Run(GetProjectionMatrix(), app.simulation.shaderConfig.particleCount);
            glControl.SwapBuffers();
            frameCounter++;
        }

        public void Step()
        {
            if (Application.Current.MainWindow == null || Application.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                return;

            //compute
            if (!Paused)
            {
                lock (app.simulation)
                {
                    app.simulation.shaderConfig.trackedIdx = TrackedIdx ?? -1;
                    computeProgram.Run(app.simulation.shaderConfig, app.simulation.forces);
                }
            }

            glControl.Invalidate();
        }
    }
}
