using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace openTK_tut {
    class textureCube {
        GameWindow win;
        float FOV = 45.0f;
        float ASP_RATIO;    // = win.Height / win.Width;
        double REFRESH_RATE = 1 / 60.0;
        double theta = 0;
        double scaleFactor = 1;
        float color = 0;
        int texture;

        public textureCube(GameWindow win) {
            this.win = win;
            Start();
        }

        void Start() {
            win.Load += Loaded;     // TODO learn what this is
            win.Resize += Resize;
            win.RenderFrame += RenderF;
            win.UpdateFrame += UpdateF;
            win.Run(REFRESH_RATE);
        }

        private void Loaded(object sender, EventArgs e) {
            //GL.ClearColor(0.43f, 0.15f, 0.97f, 0.0f);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Texture2D);

            float[] lightPos = { 20, 20, 50 };
            float[] lightDiff = { 1.0f, 1.0f, 1.0f };
            float[] lightAmb = { 0.3f, 0.3f, 0.3f };
            GL.Light(LightName.Light0, LightParameter.Position, lightPos);
            GL.Light(LightName.Light0, LightParameter.Diffuse, lightDiff);
            GL.Light(LightName.Light0, LightParameter.Ambient, lightAmb);
            GL.Enable(EnableCap.Light0);

            GL.GenTextures(1,out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            System.Drawing.Imaging.BitmapData textureData = loadImage(@"D:\test_texture.bmp");
            GL.TexImage2D(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgb,textureData.Width,
                textureData.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgr,PixelType.UnsignedByte,textureData.Scan0);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private void Resize(object sender, EventArgs e) {
            GL.Viewport(0, 0, win.Width, win.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            ASP_RATIO = 16f / 9.0f;
            Matrix4 perspectiveMatrix = Matrix4.Perspective(FOV, ASP_RATIO, 1.0f, 1000.0f);
            GL.LoadMatrix(ref perspectiveMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void UpdateF(object sender, FrameEventArgs e) {
            theta += 1.0;
            scaleFactor += 0.001;
            color += .005f;
        }

        private void RenderF(object sender, FrameEventArgs e) {
            GL.LoadIdentity();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);   // Clear frame and depth buffer

            GL.Translate(0, 10, -70);
            GL.Rotate(-theta, 1.0, 0.1, 0.3);

            Cube(2);

            win.SwapBuffers();
        }

        void Cube(float width) {
            GL.Begin(BeginMode.Quads);      //Depriciated

            GL.Normal3(-1.0, 0.0, 0.0);
            GL.TexCoord2(0,0);
            GL.Vertex3(-width, width, width);
            GL.TexCoord2(1,0);
            GL.Vertex3(-width, width, -width);
            GL.TexCoord2(1, 1);
            GL.Vertex3(-width, -width, -width);
            GL.TexCoord2(0, 1);
            GL.Vertex3(-width, -width, width);

            GL.Normal3(1.0, 0.0, 0.0);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(width, -width, width);

            GL.Normal3(0.0, -1.0, 0.0);
            GL.Color3(0.1, 0.2, 0.3);
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
            GL.Color3(0.0, 1.0, 1.0);
            GL.Vertex3(width, width, -width);
            GL.Vertex3(width, -width, -width);
            GL.Vertex3(-width, -width, -width);
            GL.Vertex3(-width, width, -width);

            GL.Normal3(0.0, 0.0, 1.0);
            GL.Color3(1.0, 0.0, 1.0);
            GL.Vertex3(width, width, width);
            GL.Vertex3(width, -width, width);
            GL.Vertex3(-width, -width, width);
            GL.Vertex3(-width, width, width);
            GL.End();
        }

        System.Drawing.Imaging.BitmapData loadImage(string filePath) {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filePath);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmp.UnlockBits(data);
            return data;
        }
    }
}
