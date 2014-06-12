//
// Mainform.cs 
//	- Shows Modules programed with IFigure and BaseFigure and tells them users input to alter.
//
// Authors:
//	Andreas Maertens <mcmaerti@gmx.de>
//  Tobias Schmidt
//  Tobias Klinkenberg
//
// Copyright 2011 by Andreas Maertens, Tobias Schmidt & Tobias Klinkenberg

using System;
using System.Drawing;
using System.Windows.Forms;
using Tao.OpenGl;
using System.Threading;
using System.Collections.Generic;



namespace GL3DLab
{
    public enum LightSource { none = 0, bluelight = 1 }

    /// <summary>
    /// Hauptformular der Anwendung. Es enthält die Zeichenoberfläche für das OpenGL
    /// </summary>
    public partial class Mainform : Form
    {
        /// <summary>
        /// Settings
        /// </summary> 
        private bool Freelook = true;

        /// <summary>
        /// Welche Lichtquellenroutine soll verwendet werden
        /// </summary>
        private LightSource light = LightSource.none;

        /// <summary>
        /// HilfsMember für Camera- und Maussteuerung
        /// </summary>
        private int MouseX = 0;
        private int MouseY = 0;
        private float Movement = 0.5f;

        /// <summary>
        /// Matrixoperationen Framework
        /// </summary>
        private MatrixMath MMath = new MatrixMath();

        /// <summary>
        /// Simulationsthread
        /// </summary>
        private static Thread thrOpenGL;

        /// <summary>
        /// Skyboxobjekt
        /// </summary>
        Skybox sky = new Skybox();

        /// <summary>
        /// Kameraobjekt
        /// </summary>
        Camera cam = new Camera();

        /// <summary>
        /// Die Figurenlisten
        /// </summary>
        List<IFigures> Figures = new List<IFigures>();

        /// <summary>
        /// Die Figuren, die eine Maussteuerung benötigen
        /// </summary>
        List<IMouseControlled> MouseFigures = new List<IMouseControlled>();

        int ModuleIndex = 0;

        private void lastActiveModule() 
        {
          Figures[ModuleIndex].Enabled = false;
          Figures[ModuleIndex].Visible = false;
          ModuleIndex -= 1;
          if (ModuleIndex < 0)
          {
            ModuleIndex = Figures.Count - 1;
          }
          Figures[ModuleIndex].Enabled = true;
          Figures[ModuleIndex].Visible = true;
          Figures[ModuleIndex].Init();
          light = Figures[ModuleIndex].getDefaultLight();
        }
        private void nextActiveModule()
        {
          Figures[ModuleIndex].Enabled = false;
          Figures[ModuleIndex].Visible = false;
          ModuleIndex += 1;
          ModuleIndex = ModuleIndex % Figures.Count;
          Figures[ModuleIndex].Enabled = true;
          Figures[ModuleIndex].Visible = true;
          Figures[ModuleIndex].Init();
          light = Figures[ModuleIndex].getDefaultLight();
        }

        private IFigures getActiveModule()
        {
          return Figures[ModuleIndex];      
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Mainform()
        {
            InitializeComponent();
            view.InitializeContexts();
            GraphicEngine.InitGl();
            GraphicEngine.ReInitGl(Width, Height);
            view.MouseDown += new MouseEventHandler(MouseActionDown);
            view.MouseUp += new MouseEventHandler(MouseActionUp);
            view.MouseWheel += new MouseEventHandler(MouseActionWheel);
            FullScreen();

            Cursor.Hide();
            sky.InitGLTexture();

            /* 
             * Hier werden die nötigen Module eingebunden.
             * Manche müssen auch der Maussteuerung hinzugefügt werden,
             * um etwa Mausklicks auswerten zu können.
            */

// IMouseControlled + IFigures
            TangentialFunctions _Fkt = new TangentialFunctions();
            _Fkt.Enabled = true;
            _Fkt.Visible = true;
            MouseFigures.Add(_Fkt);
            Figures.Add(_Fkt);

            PlanetAdvanced3T _P3 = new PlanetAdvanced3T();   
            MouseFigures.Add(_P3);
            Figures.Add(_P3);
// IFigures only
            Figures.Add(new ParticleField());
            foreach (IFigures f in Figures)
            {
                f.Cam = cam;
            }

            getActiveModule().Init();
            light = getActiveModule().getDefaultLight();

            thrOpenGL = new Thread(new ThreadStart(OpenGL_Start));
            thrOpenGL.Start();
        }

        /// <summary>
        /// Moves Mousecursor to center
        /// </summary>
        private void CenterMouse()
        {
            MouseX = Screen.GetWorkingArea(view).Width / 2;
            MouseY = Screen.GetWorkingArea(view).Height / 2;
            Cursor.Position = PointToClient(new Point(MouseX, MouseY));
        }

        /// <summary>
        /// If Canvas is painted
        /// </summary>        
        private void OnPaintCanvas(object sender, PaintEventArgs e)
        {
            #region MouseControl

            double mx = (MouseX - MousePosition.X);
            double my = (MouseY - MousePosition.Y);
            if (Freelook) // Freelook oder Tangentialebene bewegen
            {
                cam.SetNewCameraPosition(mx, my);
            }
            else
            {
                bool check = false;

                if (mx != 0)
                {
                    foreach (IMouseControlled m in MouseFigures)
                    {
                        m.mx = mx;
                    }
                    check = true;
                }
                if (my != 0)
                {
                    foreach (IMouseControlled m in MouseFigures)
                    {
                        m.my = my;
                    }
                    check = true;
                }
                if (check)
                {
                    foreach (IMouseControlled m in MouseFigures)
                    {
                        m.updateObject();
                    }
                }
            }
            CenterMouse();
            #endregion

            GraphicEngine.BeginPaint();

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            if (sky.DoDrawSkybox)
            {
                Gl.glDepthMask(false);
                // Tiefenpuffer aus (Skybox soll überzeichnet werden, also immer im Hintergrund sein.)
                sky.DrawSky(cam);
                // Skybox zeichnen
                Gl.glDepthMask(true);
                // Tiefenpuffer wieder an, alles andere hat eine Tiefe.
            }

            Gl.glLoadIdentity();

            cam.SetCamera();
            // Position und Richtung des Blickes einstellen.

            InitLight();
            // Lichtpositionen zeichnen.

            getActiveModule().Draw();
            // draw Objects

            Gl.glFlush();
            // in OpenGL wiedergeben
        }

        /// <summary>
        /// If Canvas is resizing
        /// </summary>        
        private void OnResizeCanvas(object sender, EventArgs e)
        {
            GraphicEngine.ReInitGl(Width, Height);
        }

        /// <summary>
        /// If Key is down on Canvas
        /// </summary>        
        private void OnKeyDownCanvas(object sender, KeyEventArgs e)
        {
            # region Movement
            float speed = 1;
            if (e.Shift) { speed = 100; }
            if (e.Control)
            { 
              speed = 1000;
              strgPressed = true;
            }
            if (e.Alt) { speed = 0.1f; }
            //Up
            if (e.KeyCode == Keys.W)
            {
                Point3 look = new Point3(1, 0, 0);
                Point3 zAxes = new Point3(0, 0, 1);
                Point3 yAxes = new Point3(0, 1, 0);
                Matrix rot1 = new Matrix();
                Matrix rot2 = new Matrix();
                rot1.RotMatrix(cam.angleZ + 90, zAxes);
                rot2.RotMatrix(cam.angleX - 90, yAxes);
                Matrix rotRes = MMath.MatDotMat(rot1, rot2);
                look = MMath.MatDotPoint(rotRes, look);
                cam.X -= look.x * Movement * speed;
                cam.Y -= look.y * Movement * speed;
                cam.Z -= look.z * Movement * speed;
            }
            if (e.KeyCode == Keys.S)
            {
                Point3 look = new Point3(1, 0, 0);
                Point3 zAxes = new Point3(0, 0, 1);
                Point3 yAxes = new Point3(0, 1, 0);
                Matrix rot1 = new Matrix();
                Matrix rot2 = new Matrix();
                rot1.RotMatrix(cam.angleZ + 90, zAxes);
                rot2.RotMatrix(cam.angleX - 90, yAxes);
                Matrix rotRes = MMath.MatDotMat(rot1, rot2);
                look = MMath.MatDotPoint(rotRes, look);
                cam.X += look.x * Movement * speed;
                cam.Y += look.y * Movement * speed;
                cam.Z += look.z * Movement * speed;
            }
            if (e.KeyCode == Keys.A)
            {
                Point3 look = new Point3(1, 0, 0);
                Point3 axes = new Point3(0, 0, 1);
                Matrix rot = new Matrix();
                rot.RotMatrix(cam.angleZ, axes);
                look = MMath.MatDotPoint(rot, look);
                cam.X += look.x * Movement * speed;
                cam.Y += look.y * Movement * speed;
            }
            if (e.KeyCode == Keys.D)
            {
                Point3 look = new Point3(1, 0, 0);
                Point3 axes = new Point3(0, 0, 1);
                Matrix rot = new Matrix();
                rot.RotMatrix(cam.angleZ, axes);
                look = MMath.MatDotPoint(rot, look);
                cam.X -= look.x * Movement * speed;
                cam.Y -= look.y * Movement * speed;
            }
            if (e.KeyCode == Keys.E)
            {
                cam.Z -= Movement * speed;
            }
            if (e.KeyCode == Keys.Q)
            {
                cam.Z += Movement * speed;
            }
            #endregion

            #region settings

            getActiveModule().KeyPressed(e);            

            //ESC Close
            if (e.KeyCode == Keys.Escape)
            { Close(); }

            //F3 light on/off
            if (e.KeyCode == Keys.F3)
            { 
                light = (LightSource)(((int)light + 1)%2);
                switch (light)
                {
                    case LightSource.none:
                        {
                            StaticVars.Light = false;
                            break;
                        }
                    case LightSource.bluelight:
                        {
                            StaticVars.Light = true;
                            break; 
                        }                    
                    default:
                        {
                            StaticVars.Light = false;
                            break; 
                        }
                }
            }

            if (e.KeyCode == Keys.F11) 
            {
              nextActiveModule();
            }
            
            if (e.KeyCode == Keys.F12)
            {
              lastActiveModule();
            }


            //J Skybox on/off
            if (e.KeyCode == Keys.J)
            { sky.DoDrawSkybox = !sky.DoDrawSkybox; }


            //F1 Inc LineWidth
            if (e.KeyCode == Keys.F1 && !e.Control)
            {
                StaticVars.IncLinewidth();
            }

            //F2 Dec LineWidth
            if (e.KeyCode == Keys.F2 && !e.Control)
            {
                StaticVars.DecLinewidth();
            }

            //Control + F1 Inc Brightness
            if (e.KeyCode == Keys.F1 && e.Control)
            {
                StaticVars.Brightness += 0.1f;
            }

            //Control + F2 Dec Brightness
            if (e.KeyCode == Keys.F2 && e.Control)
            {
                StaticVars.Brightness -= 0.1f;
            }

            #endregion
        }

        /// <summary>
        /// The Thread for Simulation starts
        /// </summary>
        private void OpenGL_Start()
        {
            while (true)// infinity loop for rendering
            {
                Tick();
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Lightszene initialization
        /// </summary>
        private void InitLight()
        {
            switch (light)
            {
                default:
                    {
                        Gl.glDisable(Gl.GL_LIGHTING);
                        break;
                    }
                case LightSource.bluelight:
                    {
                        #region blueLight
                        
                        /* Beleuchtung aktiveren */
                        Gl.glEnable(Gl.GL_LIGHTING);
                        /* Ambiente Lichtfarbe setzen */
                        float[] ambientLight = { 0f, 0f, 0f, 1.0f };
                        /* Diffuse Lichtfarbe setzen */
                        float[] diffuseLight = { 0.7f, 0.7f, 0.7f, 0.3f };
                        float[] gray = { 0.75f, 0.75f, 0.75f, 1f };
                        /* Lichtquelle positionieren */
                        float[] light0Pos = { 0.0f, 0.0f, 0.0f, 1.0f };
                        
                        Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, gray);
                        
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT4, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT5, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT5, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT5, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT6, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT6, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT6, Gl.GL_POSITION, light0Pos);

                        Gl.glLightfv(Gl.GL_LIGHT7, Gl.GL_AMBIENT, ambientLight);
                        Gl.glLightfv(Gl.GL_LIGHT7, Gl.GL_DIFFUSE, diffuseLight);
                        Gl.glLightfv(Gl.GL_LIGHT7, Gl.GL_POSITION, light0Pos);
                        break;
                        #endregion
                    }
            }            
        }

        /// <summary>
        /// Simulation of Figures
        /// </summary>
        private void Tick()
        {
            foreach (IFigures f in Figures)
            {
                f.Tick();
            }
        }

        /// <summary>
        /// inits Fullscreen
        /// </summary>
        private void FullScreen()
        {
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Location = new Point(0, 0);
            Size = Screen.PrimaryScreen.Bounds.Size;
        }

        /// <summary>
        /// RepaintTimer fires TickEvent
        /// </summary>        
        private void RepaintTimer_Tick(object sender, EventArgs e)
        {
            view.Invalidate();
        }

        bool strgPressed = false;

        /// <summary>
        /// Mousewheel was used
        /// </summary>        
        private void MouseActionWheel(object sender, MouseEventArgs e)
        {
            float fak = 10;
            if (strgPressed) { fak = 1000; }
            if (e.Delta < 0)
            {
                cam.Z -= fak;
            }
            else
            {
                cam.Z += fak;
            }
        }

        /// <summary>
        /// Mousekey pressed
        /// </summary>        
        private void MouseActionDown(object sender, MouseEventArgs e)
        {
            if (getActiveModule() is IMouseControlled)
            {
                ((IMouseControlled)getActiveModule()).MouseButton(e);
            }

            if (e.Button == MouseButtons.Right)
            {
                Freelook = false;
            }
        }

        /// <summary>
        /// Mousekey released
        /// </summary>        
        private void MouseActionUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Freelook = true;
            }
        }

        /// <summary>
        /// Trigger fires on Closing Program
        /// </summary>        
        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (IFigures f in Figures)
            {
                f.OnShutdown();
            }
        }

        private void view_KeyUp(object sender, KeyEventArgs e)
        {
          if (e.Control) { strgPressed = false; }
        }
    }
}