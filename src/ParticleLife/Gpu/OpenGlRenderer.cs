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

        public byte[] captureBuffer;

        private int? recFrameNr;

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

            center = new Vector2(app.simulation.config.width / 2, app.simulation.config.height / 2);

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
                    double minDistance = app.simulation.config.width * 10;
                    int closestIdx = 0;
                    var projectionMatrix = GetProjectionMatrix();
                    var mouseWorld = GpuUtil.ScreenToWorld(new Vector2(e.X, e.Y), projectionMatrix, glControl.Width, glControl.Height);

                    if (mouseWorld.X > app.simulation.config.width)
                        mouseWorld.X -= app.simulation.config.width;
                    if (mouseWorld.X < 0)
                        mouseWorld.X += app.simulation.config.width;

                    if (mouseWorld.Y > app.simulation.config.height)
                        mouseWorld.Y -= app.simulation.config.height;
                    if (mouseWorld.Y < 0)
                        mouseWorld.Y += app.simulation.config.height;

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
            app.simulation.config.trackedIdx = TrackedIdx ?? -1;
            computeProgram.Run(app.simulation.config, app.simulation.forces);
        }

        public void StopTracking()
        {
            if (TrackedIdx != null)
            {
                TrackedIdx = null;
                app.simulation.config.trackedIdx = TrackedIdx ?? -1;
                computeProgram.Run(app.simulation.config, app.simulation.forces);
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
                
                var move = delta * app.simulation.cameraFollowSpeed;

                if (Math.Abs(delta.X) > 0.75* app.simulation.config.width)
                {
                    move.X = (float)Math.Sign(delta.X) * app.simulation.config.width;
                }

                if (Math.Abs(delta.Y) > 0.75 * app.simulation.config.height)
                {
                    move.Y = (float)Math.Sign(delta.Y) * app.simulation.config.height;
                }

                center += move;
            }
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            lock (app.simulation)
            {
                FollowTrackedParticle();
                displayProgram.Run(GetProjectionMatrix(), app.simulation.config.particleCount, new Vector2(glControl.Width, glControl.Height), app.simulation.particleSize);
                glControl.SwapBuffers();
                frameCounter++;
            }

            Capture();
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
                    app.simulation.config.trackedIdx = TrackedIdx ?? -1;
                    computeProgram.Run(app.simulation.config, app.simulation.forces);
                }
            }

            glControl.Invalidate();
        }

        private void Capture()
        {
            //combine PNGs into video:
            //mp4: ffmpeg -f image2 -framerate 60 -i rec1/frame_%05d.png -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" -r 60 -vcodec libx264 -pix_fmt yuv420p out.mp4 -y
            //gif: ffmpeg -framerate 60 -ss2 -i rec1/frame_%05d.png -vf "select='not(mod(n,2))',setpts=N/FRAME_RATE/TB" -t 5 3ch-tissue.gif
            //reduce bitrate:  ffmpeg -i in.mp4 -c:v libx264 -b:v 4236000 -pass 2 -c:a aac -b:a 128k out.mp4
            var recDir = app.configWindow.recordDir?.ToString();
            if (!recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                recFrameNr = 0;
            }

            if (recFrameNr.HasValue && string.IsNullOrWhiteSpace(recDir))
                recFrameNr = null;

            if (recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                string recFilename = $"{recDir}\\frame_{recFrameNr.Value.ToString("00000")}.png";
                glControl.MakeCurrent();
                int width = glControl.Width;
                int height = glControl.Height;
                int bufferSize = width * height * 4;
                if (captureBuffer == null || bufferSize != captureBuffer.Length)
                    captureBuffer = new byte[bufferSize];
                GL.ReadPixels(
                    0, 0,
                    width, height,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    captureBuffer
                );

                TextureUtil.SaveBufferToFile(captureBuffer, width, height, recFilename);
                recFrameNr = recFrameNr.Value + 1;
            }
        }
    }
}
