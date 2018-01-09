using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace openTK_tut {
    class Game {
        GameWindow win;
        float FOV = 45.0f;
        float ASP_RATIO;
        double REFRESH_RATE = 1 / 60.0;
        double theta = 0;
        double beta = 0;
        double scaleFactor = 1;
        float color = 0;
        bool colorFlag = false;
        bool betaFlag = false;
        Single[] fftBuff;
        List<float> fftData;
        float fftRange = 0;
        float fftMax;
        static float gain = 6000;

        SoundCapture sc;

        public Game(GameWindow win) {
            this.win = win;
            sc = new SoundCapture();
            fftData = new List<float>((int)sc.FFT_RES);
        }

        public void Start() {
            win.Load += Loaded;
            win.Resize += Resize;
            win.RenderFrame += RenderF;
            win.UpdateFrame += UpdateF;
            win.Run(REFRESH_RATE);
        }

        private void Loaded(object sender, EventArgs e) {
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);

            float[] lightPos = { 20, 20, 50 };
            float[] lightDiff = { 1.0f, 1.0f, 1.0f };
            float[] lightAmb = { 0.3f, 0.3f, 0.3f };
            GL.Light(LightName.Light0, LightParameter.Position, lightPos);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lightDiff);
            GL.Light(LightName.Light0, LightParameter.Ambient, lightAmb);
            GL.Enable(EnableCap.Light0);
        }

        private void Resize(object sender, EventArgs e) {
            GL.Viewport(0, 0, win.Width, win.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            ASP_RATIO = 16f / 9.0f;
            Matrix4 perspectiveMatrix = Matrix4.Perspective(FOV, ASP_RATIO, 1.0f, 1000.0f);      //Depriciated
            GL.LoadMatrix(ref perspectiveMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void UpdateF(object sender, FrameEventArgs e) {
            theta += 1.0;
            scaleFactor += 0.001;
            updateColor();
            UpdateBeta();         

            bool nanFound = false;
            fftBuff = sc.fftBuff;
            foreach(var point in fftBuff) {
                if (double.IsNaN(point)) {
                    nanFound = true;
                    break;
                }
            }
            if (!nanFound) {
                fftData.Clear();

                for (int i = 0; i < fftBuff.Length; ++i) {
                    fftData.Add(fftBuff[i]);
                }

                //LogarizeData(fftData);
                //NormalizeData(fftData);

                fftMax = fftData.Max();
                fftRange = (fftMax - fftData.Min()) * 10;
            }
        }

        private void UpdateBeta() {
            double updateAngle = 1f;
            double maxAngle = 20f;

            if (beta >= maxAngle) {
                betaFlag = false;
            } else if (beta <= -maxAngle) {
                betaFlag = true;
            }

            if (betaFlag) {
                beta += updateAngle;
            } else {
                beta -= updateAngle;
            }
        }

        private void updateColor() {
            if (colorFlag) {
                color -= .001f;
            } else {
                color += .001f;
            }

            if (color >= 1) {
                colorFlag = true;
            } else if (color <= 0) {
                colorFlag = false;
            }
        }

        private void AudioVisual() {
            GL.Translate(0, 0, -350);
            if (fftData != null) {
                int bars = fftData.Count; //(int)sc.FFT_RES;
                int offset = win.Width / 2;
                float w = 5.3f;
                for (int i = 0; i < bars; ++i) {    // excluding lowest bar
                    bar(new Vector3((i * w) - (bars * w) / 2, -80, 0), w, (gain * fftData[i]), new Vector3(0.8f - (i * 0.01f), color, i * 0.01f));
                    bar(new Vector3((i * w * 3.5f) - (bars * w * 3.5f) / 2, -220, -200), w * 3.5f, (gain * fftData[i]), new Vector3(0.2f, 0, .01f));
                }
            }

            GL.Translate(0, 0, 300);
            GL.Rotate(theta, 0.1, 0.2, 0.3);
            SolidCube(2, new Vector3(fftRange, fftRange * .5f, 1 - (.5f * fftRange)));
        }

        static void LogarizeData(List<float> list) {
            for(int i = 0; i < list.Count; i++) {
                list[i] = (float)Math.Log(list[i], 10);
            }
        }

        void NormalizeData(List<float> list) {
            for (int i = 0; i < list.Count; i++) {
                float min = 0;                      // float min = list.Min();
                float max = list.Max();
                list[i] = Normalize(list, list[i], min, max);
            }
        }

        static float Normalize(List<float> list, float currentValue, float min, float max) {

            int endOfScale = 0;
            float topOfScale = 1; // * gain;
            //float min = list.Min();
            //float max = list.Max();

            var normalized = endOfScale + (currentValue - min) * (topOfScale - endOfScale) / (max - min);

            return normalized;
        }

        private void RenderF(object sender, FrameEventArgs e) {
            GL.LoadIdentity();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);   // Clear frame and depth buffer

            AudioVisual();
            GL.LoadIdentity();

            GL.Translate(0, 10, -500);
            GL.Rotate(-theta, 1.0, 0.1, 0.3);

            for (int j = 0; j < 7; j++) {
                for (int i = 0; i < 17; i++) {
                    GL.LoadIdentity();
                    GL.Translate(-400, -150, -420);
                    GL.Translate(50 * i, 50 * j, 0);
                    GL.Rotate(theta, 0.0, 0.1, 0.0);
                    GL.Rotate(beta, 1.0, 0.0, 0.0);
                    GL.Begin(BeginMode.Quads);
                    GL.Color3(0.0, 0.0, 0.0); GL.Vertex3(-20, -20, -1);
                    GL.Color3(0.0, 0.0, 0.0); GL.Vertex3(-20, 20, -1);
                    GL.Color3(0.0, 0.0, 0.0); GL.Vertex3(20, 20, -1);
                    GL.Color3(1.0, 1.0, 1.0); GL.Vertex3(20, -20, -1);
                    GL.End();
                }
            }

            GL.LoadIdentity();
            GL.Translate(50, 200, -450);
            GL.Rotate(theta, 0.02, 0.5, 1.0);

            GL.LoadIdentity();
            GL.Translate(0, 0, -50);
            GL.Rotate(theta, 0.1, 0.2, 0.3);

            CrazyCube(1, 5, 3, new Vector3(0f, 0f, -60f), new Vector3(0.1f, 0.2f, 0.3f));
            CrazyCube(1, 5, 3, new Vector3(0f, 0f, -70f), new Vector3(0.1f, 0.2f, 0.3f));
            CrazyCube(1, 5, 3, new Vector3(0f, 0f, -80f), new Vector3(0.1f, 0.2f, 0.3f));
            CrazyCube(1, 5, 3, new Vector3(0f, 0f, -90f), new Vector3(0.1f, 0.2f, 0.3f));


            CrazyCube(1, 50, 8, new Vector3(0f, 0f, -800f), new Vector3(1.5f, 4.4f, 6.3f));
            CrazyCube(1, 50, 8, new Vector3(400f, 400f, -800f), new Vector3(1.5f, 4.4f, 6.3f));
            CrazyCube(1, 50, 8, new Vector3(-400f, -400f, -800f), new Vector3(1.5f, 4.4f, 6.3f));
            CrazyCube(1, 50, 8, new Vector3(400f, -400f, -800f), new Vector3(1.5f, 4.4f, 6.3f));
            CrazyCube(1, 50, 8, new Vector3(-400f, 400f, -800f), new Vector3(1.5f, 4.4f, 6.3f));

            win.SwapBuffers();
        }

        void bar(Vector3 origin, float W, float H, Vector3 color) {
            GL.Begin(BeginMode.Quads);
            GL.Color3(color);
            GL.Vertex3(origin);
            origin.X += W;
            GL.Vertex3(origin);
            origin.Y += H;
            GL.Vertex3(origin);
            origin.X -= W;
            GL.Vertex3(origin);
            GL.End();
        }
        
        void Cube(float width, Vector3 color) {
            GL.Begin(BeginMode.Quads);      //Depriciated
            GL.Normal3(-1.0, 0.0, 0.0);
            GL.Color3(0.0, 0.0, 0.0); GL.Vertex3(-width, width, width);
            GL.Color3(0.0, 0.0, 1.0); GL.Vertex3(-width, width, -width);
            GL.Color3(0.0, 1.0, 0.0); GL.Vertex3(-width, -width, -width);
            GL.Color3(0.0, 1.0, 1.0); GL.Vertex3(-width, -width, width);

            GL.Normal3(1.0, 0.0, 0.0);
            GL.Color3(1.0, 0.0, 0.0); GL.Vertex3(width, width, width);
            GL.Color3(1.0, 0.0, 1.0); GL.Vertex3(width, width, -width);
            GL.Color3(1.0, 1.0, 0.0); GL.Vertex3(width, -width, -width);
            GL.Color3(1.0, 1.0, 1.0); GL.Vertex3(width, -width, width);

            GL.Normal3(0.0, -1.0, 0.0);
            //GL.Color3(0.1, 0.2, 0.3);
            GL.Color3(color);
            GL.Vertex3(width, -width, width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, -width, width);

            GL.Normal3(0.0, 1.0, 0.0);
            GL.Color3(1.0, 1.0, 0.0);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(-width, width, -width);
            GL.Vertex3(-width, width, width);

            GL.Normal3(0.0, 0.0, -1.0);
            //GL.Color3(0.0, 1.0, 1.0);
            GL.Color3(color);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, width, -width);

            GL.Normal3(0.0, 0.0, 1.0);
            //GL.Color3(1.0, 0.0, 1.0);
            GL.Color3(color);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, -width, width);
            GL.Vertex3(-width, -width, width);
            GL.Vertex3(-width, width, width);
            GL.End();
        }

        void Cube(float width) {
            Cube(width, new Vector3(0.1f, 0.2f, 0.3f));
        }

        void SolidCube(float width, Vector3 color) {
            GL.Begin(BeginMode.Quads);      //Depriciated
            GL.Normal3(-1.0, 0.0, 0.0);
            GL.Color3(color);
            GL.Vertex3(-width, width, width);
            GL.Vertex3(-width, width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, -width, width);

            GL.Normal3(1.0, 0.0, 0.0);
            GL.Color3(color);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(width, -width, width);

            GL.Normal3(0.0, -1.0, 0.0);
            GL.Color3(color);
            GL.Vertex3(width, -width, width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, -width, width);

            GL.Normal3(0.0, 1.0, 0.0);
            GL.Color3(color);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(-width, width, -width);
            GL.Vertex3(-width, width, width);

            GL.Normal3(0.0, 0.0, -1.0);
            GL.Color3(color);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, width, -width);

            GL.Normal3(0.0, 0.0, 1.0);
            GL.Color3(color);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, -width, width);
            GL.Vertex3(-width, -width, width);
            GL.Vertex3(-width, width, width);
            GL.End();
        }

        void CrazyCube(float size, float dist, int chainLength, Vector3 origin, Vector3 rotVec) {
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(0, 0, -dist);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(0, 0, dist);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(0, -dist, 0);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(0, dist, 0);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(-dist, 0, 0);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
            GL.LoadIdentity();
            GL.Translate(origin);
            GL.Rotate((float)theta, rotVec);
            for (int j = 0; j < chainLength; ++j) {
                GL.Translate(dist, 0, 0);
                GL.Rotate(-theta, 0.3, 0.2, 0.1);
                Cube(size);
            }
        }

    }
}
