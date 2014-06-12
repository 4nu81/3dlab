using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Windows.Forms;

namespace GL3DLab
{
    class ParticleField : BaseFigure
    {
        private Object lockobj = new Object();
        private bool draw = true;
        private float DotSize = 1f;
        private bool f1 = false;
        private bool f2 = false;
        private bool f3 = false;
        private bool f4 = false;
        private bool f5 = false;
        private bool f6 = false;

        List<Point3> points = new List<Point3>();
        /// <summary>
        /// Liste enthält alle Punkte des Partikelfeldes.
        /// </summary>
        public List<Point3> m_lPointField = new List<Point3>();

        /// <summary>
        /// Zufallsgenerator
        /// </summary>
        private Random r = new Random();

        /// <summary>
        /// Höhe des höchsten Punktes (in Z-Richtung)
        /// </summary>
        private float zmax = 1;
        
        /// <summary>
        /// Höhe des tiefsten Punktes (in Z-Richtung)
        /// </summary>
        private float zmin = -1;

        /// <summary>
        /// Kantenlänge der dargestellten Würfel und Flächen
        /// </summary>
        public float delta = 0.5f;

        /// <summary>
        /// Initialisierungsmuster der Punkte
        /// </summary>
        initForms forms = initForms.flat;

        /// <summary>
        /// Bewegungsgeschwindigkeit der Partikel
        /// </summary>
        int speed = 100;

        /// <summary>
        /// Bewegungsmuster der Partikel
        /// </summary>
        ParticleForms form;

        /// <summary>
        /// Sollen Ausbreitung der Punkte verringert werden?
        /// </summary>
        bool shrink;

        /// <summary>
        /// Sollen Punkte rückwärts laufen?
        /// </summary>
        bool reverse = false;

        /// <summary>
        /// Form der Punkte
        /// </summary>
        DotStyle Dots;

        /// <summary>
        /// Interne Zeichenfunktion
        /// </summary>
        protected override void InternalDraw()
        {
          lock(lockobj){
            if (draw)
            {

              foreach (Point3 p in points) // ToArray sorgt für Threadsicherheit des Enummerators
              {
                float red;
                float green;
                float blue;
                float alpha;

                //if (form == ParticleForms.universe)
                //{
                //  float du = (float)Math.Sqrt(Math.Pow(p.x, 2) + Math.Pow(p.y, 2));

                //  red = (float)Math.Sqrt(1f / (4f * du / 20f + 1f));
                //  green = (float)Math.Sqrt(1f / (Math.Pow(du / 20f, 3f) + 1f));
                //  blue = (float)Math.Sqrt(1f / (Math.Pow(2f * du / 20f, 2f) + 1f));
                //  alpha = 0f;
                //}
                //else
                //{

                  float cole = (zmax - p.z) / (zmax - zmin);
                  
                  red = 1 - cole;
                  green = cole;
                  blue = 0f;
                  alpha = 1f;
                //}
                switch (Dots)
                {

                  //case DotStyle.Rounds:
                  //  #region Spheres
                  //  {
                  //    float X = p.x;
                  //    float Y = p.y;
                  //    float Z = p.z;
                  //    float r = delta;

                  //    Gl.glPushMatrix();

                  //    Gl.glTranslatef(X, Y, Z);

                  //    Glu.GLUquadric obj = Glu.gluNewQuadric();
                  //    Glu.gluSphere(obj, r, 5, 5);
                  //    Gl.glColor3f(red, green, blue);
                  //    Gl.glPopMatrix();
                  //    break;
                  //  #endregion
                  //  }


                  case DotStyle.Point:
                    #region Point
                    {
                      // schon scheen
                      Gl.glBegin(Gl.GL_POINTS);
                      Gl.glVertex3f(p.x, p.y, p.z);

                      Gl.glColor3f(red, green, blue);
                      Gl.glEnd();
                      break;
                    #endregion
                    }


                  case DotStyle.Vertects:
                    #region 3Flächen
                    {
                      // scheener.         

                      Gl.glNormal3f(1f, 0f, 0f);

                      Gl.glBegin(Gl.GL_QUADS);
                      Gl.glVertex3f(p.x, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x, p.y + delta, p.z - delta);
                      Gl.glEnd();

                      Gl.glNormal3f(0f, 1f, 0f);
                      Gl.glColor4f(red + 0.01f, green + 0.01f, blue + 0.01f, alpha);
                      Gl.glBegin(Gl.GL_QUADS);
                      Gl.glVertex3f(p.x + delta, p.y, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y, p.z - delta);
                      Gl.glEnd();

                      Gl.glNormal3f(0f, 0f, 1f);
                      Gl.glColor4f(red + 0.02f, green + 0.02f, blue + 0.02f, alpha);
                      Gl.glBegin(Gl.GL_QUADS);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z);
                      Gl.glEnd();
                      break;
                    #endregion
                    }


                  case DotStyle.Cubes:
                    #region Würfel
                    {
                      // für Dolle schön und so.       
                      Gl.glColor4f(red + 0.1f, green + 0.1f, blue + 0.1f, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(0f, 0f, 1f);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z + delta);
                      Gl.glEnd();

                      Gl.glColor4f(red - 0.1f, green - 0.1f, blue - 0.1f, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(1f, 0f, 0f);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z - delta);
                      Gl.glEnd();

                      Gl.glColor4f(red + 0.1f, green + 0.1f, blue + 0.1f, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(0f, 0f, -1f);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z - delta);
                      Gl.glEnd();

                      Gl.glColor4f(red - 0.1f, green - 0.1f, blue - 0.1f, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(-1f, 0f, 0f);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z - delta);
                      Gl.glEnd();

                      Gl.glColor4f(red, green, blue, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(0f, 1f, 0f);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y + delta, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y + delta, p.z - delta);
                      Gl.glEnd();

                      Gl.glColor4f(red, green, blue, alpha);
                      Gl.glBegin(Gl.GL_POLYGON);
                      Gl.glNormal3f(0f, -1f, 0f);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z + delta);
                      Gl.glVertex3f(p.x - delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z - delta);
                      Gl.glVertex3f(p.x + delta, p.y - delta, p.z + delta);
                      Gl.glEnd();
                      break;
                    #endregion
                    }
                }
              }
            }
   
          }

     

        }
        /// <summary>
        /// Interner Threadtick für die Simulation der Punkte
        /// </summary>
        protected override void InternalTick()
        {
          lock (lockobj)
          {
            if (draw)
            {
              if (f1)
              {
                Funktion1();
              }
              if (f2)
              {
                Funktion2();
              }
              if (f3)
              {
                Funktion3();
              }
              if (f4)
              {
                Funktion4();
              }
              if (f5)
              {
                Funktion5();
              }
              if (f6)
              {
                Funktion6();
              }
            }
          }
        }

        /// <summary>
        /// Initialisierung der Punktmenge
        /// </summary>
        public override void Init()
        {   
          InitPoints();
          Cam.X = 0;
          Cam.Y = 0;
          Cam.Z = 200;
        }

        /// <summary>
        /// Wird bei Tastendruck ausgelöst
        /// </summary>
        /// <param name="e"></param>
        public override void KeyPressed(KeyEventArgs e)
        {
            //M swich Dotmode
            if (e.KeyCode == Keys.M)
            { Dots = (DotStyle)(((int)Dots + 1) % 3); } // % 4 für Kugeln aber sehr langsam

            //F7 Pointfield grow on/off
            if (e.KeyCode == Keys.F7)
            { reverse = !reverse; }

            //F8 shrink/grow Punktfeld
            if (e.KeyCode == Keys.F8)
            { shrink = !shrink; }

            //F9 Simulationsart switch
            if (e.KeyCode == Keys.F9)
            { form = (ParticleForms)(((int)form + 1) % 4); }


            if (e.KeyCode == Keys.D1)
            { f1 = !f1; }
            if (e.KeyCode == Keys.D2)
            { f2 = !f2; }
            if (e.KeyCode == Keys.D3)
            { f3 = !f3; }
            if (e.KeyCode == Keys.D4)
            { f4 = !f4; }
            if (e.KeyCode == Keys.D5)
            { f5 = !f5; }
            if (e.KeyCode == Keys.D6)
            { f6 = !f6; }

            //L Pointfield Flat
            if (e.KeyCode == Keys.L)
            {
                forms = initForms.flat;
                InitPoints();
            }

            //B Pointfield Würfel
            if (e.KeyCode == Keys.B)
            {
                forms = initForms.cube;
                InitPoints();
            }

            ////U Pointfields Universe
            //if (e.KeyCode == Keys.U)
            //{
            //    forms = initForms.universe;
            //    InitPoints();
            //}            
        }

        /// <summary>
        /// Wird beim Beenden des Programmes aufgerufen
        /// </summary>
        public override void OnShutdown()
        {
        }


        private void Funktion1()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {

            item.x = (float)(item.x + Math.Sin(item.z * 0.01) - Math.Sin(item.y * 0.01));
            item.y = (float)(item.y + Math.Sin(item.x * 0.01) - Math.Sin(item.z * 0.01));
            item.z = (float)(item.z + Math.Sin(item.y * 0.01) - Math.Sin(item.x * 0.01));

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }
        private void Funktion2()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {
            item.x = (float)(item.x + 5 * Math.Cos(0.02 * item.y));
            item.y = (float)(item.y + 5 * Math.Cos(0.02 * item.x));
            item.z = (float)(item.z + 0.2 * (Math.Cos(0.025 * item.y))); 

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }
        private void Funktion3()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {
            
            item.x = (float)(item.x + Math.Sin(item.z * 0.01) - Math.Tan(item.y * 0.01));
            item.y = (float)(item.y + Math.Sin(item.x * 0.01) - Math.Tan(item.z * 0.01));
            item.z = (float)(item.z + Math.Sin(item.y * 0.01) - Math.Tan(item.x * 0.01));

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }
        private void Funktion4()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {
            float x = item.x;
            float y = item.y;
            float z = item.z;

            item.x = (float)(x + Math.Cos(z * 0.01) - Math.Cos(y * 0.01));
            item.y = (float)(y + Math.Cos(x * 0.01) - Math.Cos(z * 0.01));
            item.z = (float)(z + Math.Cos(y * 0.01) - Math.Cos(x * 0.01));
            

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }
        private void Funktion5()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {           

            float x = item.x;
            float y = item.y;
            float z = item.z;

            item.x = (float)(x + Math.Tan(z * 0.01) - Math.Tan(y * 0.01));
            item.y = (float)(y + Math.Tan(x * 0.01) - Math.Tan(z * 0.01));
            item.z = (float)(z + Math.Tan(y * 0.01) - Math.Tan(x * 0.01));

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }
        private void Funktion6()
        {
          zmin = 1;
          zmax = 1;
          foreach (Point3 item in points)
          {
            
            item.x = (float)(item.x + Math.Tan(item.z * 0.01) - Math.Cos(item.y * 0.01));
            //item.y = (float)(item.y + Math.Sin(item.x * 0.01) - Math.Sin(item.z * 0.01));
            item.z = (float)(item.z + Math.Cos(item.y * 0.01) - Math.Tan(item.x * 0.01));

            if (item.z > zmax) { zmax = item.z; }
            if (item.z < zmin) { zmin = item.z; }

          }
        }


        private void InitPoints()
        {
          lock (lockobj)
          {
            draw = false;
            System.Threading.Thread.Sleep(10);
            points.Clear();


            if (forms == initForms.flat)
            {
              for (int x = -150; x < 150; x++)
              {
                for (int y = -150; y < 150; y++)
                {
                  points.Add(new Point3(x, y, 0));
                }
              }
            }

            if (forms == initForms.cube)
            {
              for (int x = -50; x < 50; x++)
              {

                for (int y = -50; y < 50; y++)
                {
                  for (int z = -10; z < 10; z++)
                  {
                    points.Add(new Point3(x, y, z));
                  }
                }
              }
            }

            draw = true;
          }
        }
    }
}
