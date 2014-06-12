using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Windows.Forms;

namespace GL3DLab
{
    /// <summary>
    /// Statische Objekte in der 3D umgebung, die nicht simuliert werden
    /// </summary>
    class Statix : BaseFigure
    {
        /// <summary>
        /// Soll Würfelgitter gezeichnet werden?
        /// </summary>
        private bool paintCube = false;

        /// <summary>
        /// Soll solider Würfel gezeichnet werden?
        /// </summary>
        private bool paintCubeSolid = false;

        /// <summary>
        /// Soll das Koordinatensystem gezeichnet werden?
        /// </summary>
        private bool paintsystem = false;

        /// <summary>
        /// Soll die Sinuskurve gezeichnet werden?
        /// </summary>
        private bool paintSin = false;

        /// <summary>
        /// Die Gittergröße des Koordinatensystems
        /// </summary>
        private float GridSize = 1000f;

        /// <summary>
        /// Enthält die Darstellungsvektoren für das Würfelgitter
        /// </summary>
        private List<Vector3> m_lVectorsCube = new List<Vector3>();

        /// <summary>
        /// Enthält die Quadrate für den soliden Würfel
        /// </summary>
        private List<Square> m_lPointsCubeSolid = new List<Square>();

        /// <summary>
        /// Enthält die Eckpunkte für den Würfel
        /// </summary>
        private List<Point3> m_lPointsCube = new List<Point3>();

        #region Internal Statix Methodes

        /// <summary>
        /// Zeichenmethode für das Koordinatensystem
        /// </summary>
        /// <param name="GridSize">Gittergröße für das Koordinatensystem</param>
        private void DrawSystem(float GridSize)
        {
            //System zeichnen
            Point3 cpx1 = new Point3(GridSize, 0, 0);
            Point3 cpx2 = new Point3(-1 * GridSize, 0, 0);
            Point3 cpy1 = new Point3(0, GridSize, 0);
            Point3 cpy2 = new Point3(0, -1 * GridSize, 0);
            Point3 cpz1 = new Point3(0, 0, GridSize);
            Point3 cpz2 = new Point3(0, 0, -1 * GridSize);
            float fak = 1;
            float i = 0;
            float delta = 10;

            Gl.glPointSize(1f);

            Gl.glDisable(Gl.GL_LIGHTING);

            Gl.glBegin(Gl.GL_LINES);
            {
                i = 0;
                while (!(i > GridSize))
                {
                    Gl.glColor3f(0.6f + fak * 0.3f, 0.6f + fak * 0.3f, 1f);
                    Gl.glVertex3f(i, 0, 0);
                    Gl.glVertex3f(i + delta, 0, 0);

                    Gl.glColor3f(0.6f - fak * 0.3f, 0.6f - fak * 0.3f, 1f);
                    Gl.glVertex3f(-i, 0, 0);
                    Gl.glVertex3f(-(i + delta), 0, 0);

                    i += delta;
                    fak *= -1;
                }
            }
            Gl.glEnd();            

            Gl.glBegin(Gl.GL_LINES);
            {
                i = 0;
                while (!(i > GridSize))
                {
                    Gl.glColor3f(1f, 0.6f + fak * 0.3f, 0.6f + fak * 0.3f);
                    Gl.glVertex3f(0, i, 0);
                    Gl.glVertex3f(0, i + delta, 0);

                    Gl.glColor3f(1f, 0.6f - fak * 0.3f, 0.6f - fak * 0.3f);
                    Gl.glVertex3f(0, -i, 0);
                    Gl.glVertex3f(0, -(i + delta), 0);

                    i += delta;
                    fak *= -1;
                }
            }
            Gl.glEnd();

            Gl.glBegin(Gl.GL_LINES);
            {
                i = 0;
                while (!(i > GridSize))
                {
                    Gl.glColor3f(0.6f + fak * 0.3f, 1f, 0.6f + fak * 0.3f);
                    Gl.glVertex3f(0, 0, i);
                    Gl.glVertex3f(0, 0, i + delta);

                    Gl.glColor3f(0.6f - fak * 0.3f, 1f, 0.6f - fak * 0.3f);
                    Gl.glVertex3f(0, 0, -i);
                    Gl.glVertex3f(0, 0, -(i + delta));

                    i += delta;
                    fak *= -1;
                }
            }
            Gl.glEnd();

            Gl.glEnable(Gl.GL_LIGHTING);
        }

        /// <summary>
        /// Zeichenmethode für das Würfelgitter
        /// </summary>
        /// <param name="Linewidth">Linienstärke der Gitterlinien</param>
        private void DrawCube(float Linewidth)
        {
            //Würfel zeichnen

            Gl.glLineWidth(Linewidth);
            foreach (Vector3 p in m_lVectorsCube)
            {
                Gl.glBegin(Gl.GL_LINES);
                {
                    Gl.glColor3f(1f, 1f, 1f);
                    Gl.glVertex3f(p.a.x, p.a.y, p.a.z);
                    Gl.glVertex3f(p.b.x, p.b.y, p.b.z);
                }
            }
            Gl.glEnd();

        }

        /// <summary>
        /// Zeichenmethode für den soliden Würfel
        /// </summary>
        private void DrawCubeSolid()
        {
            Gl.glMatrixMode(Gl.GL_TEXTURE_MATRIX);
            for (int i = 0; i < m_lPointsCubeSolid.Count; i++)
            {
                Square p = m_lPointsCubeSolid.ElementAt(i);
                Gl.glColor3f(0.3f, 0.2f, 0.6f);
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                Gl.glMateriali(Gl.GL_FRONT_AND_BACK, Gl.GL_SHININESS, 60);
                Gl.glBegin(Gl.GL_QUADS);
                {
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.a.x, p.a.y, p.a.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.b.x, p.b.y, p.b.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.c.x, p.c.y, p.c.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.d.x, p.d.y, p.d.z);
                }
                Gl.glPopMatrix();
                Gl.glEnd();
            }
            Gl.glMatrixMode(Gl.GL_MODELVIEW_MATRIX);
        }

        /// <summary>
        /// Zeichenmethode für die Sinuskurve
        /// </summary>
        private void DrawSinus()
        {
            float delta = 0.2f;
            float sy = 100;
            Gl.glBegin(Gl.GL_LINES);
            {
                Gl.glColor3f(0.5f, 1f, 0.5f);
                for (float i = -100; i <= 100; i += delta)
                {
                    Gl.glVertex3f(i, sy + -delta, (float)(Math.Sin(i) * Math.Pow(i, 2)));
                    Gl.glVertex3f(i + delta, sy + -delta, (float)(Math.Sin(i + delta) * Math.Pow(i + delta, 2)));
                }
            }
            Gl.glEnd();



            Gl.glColor3f(1f, 1f, 1f);
            Gl.glBegin(Gl.GL_QUADS);
            {
                Gl.glVertex3f(0, 0, 0);
                Gl.glVertex3f(0, 1, 0);
                Gl.glVertex3f(0, 1, 1);
                Gl.glVertex3f(0, 0, 1);
            }
            Gl.glEnd();
            Gl.glBegin(Gl.GL_LINES);
            {
                Gl.glVertex3f(0, 0.5f, 1);
                Gl.glVertex3f(0, 0.5f, 3);

                Gl.glVertex3f(0, -0.5f, 2);
                Gl.glVertex3f(0, 1.5f, 2);

                Gl.glVertex3f(0, 0.5f, 3);
                Gl.glVertex3f(0, -0.5f, 5);

                Gl.glVertex3f(0, 0.5f, 3);
                Gl.glVertex3f(0, 1.5f, 5);
            }
            Gl.glEnd();
        }

        /// <summary>
        /// Initialisierung für das Würfelgitter
        /// </summary>
        private void initCube()
        {
            float delta = 100f;
            Point3 a = new Point3(-delta, -delta, -delta);
            Point3 b = new Point3(delta, delta, -delta);
            Point3 c = new Point3(delta, -delta, -delta);
            Point3 d = new Point3(delta, -delta, delta);
            Point3 f = new Point3(-delta, -delta, delta);
            Point3 g = new Point3(-delta, delta, delta);
            Point3 h = new Point3(-delta, delta, -delta);
            Point3 k = new Point3(delta, delta, delta);

            m_lPointsCube.Add(a);
            m_lPointsCube.Add(b);
            m_lPointsCube.Add(c);
            m_lPointsCube.Add(d);
            m_lPointsCube.Add(f);
            m_lPointsCube.Add(g);
            m_lPointsCube.Add(h);
            m_lPointsCube.Add(k);

            m_lVectorsCube.Add(new Vector3(k, g));
            m_lVectorsCube.Add(new Vector3(k, d));
            m_lVectorsCube.Add(new Vector3(k, b));
            m_lVectorsCube.Add(new Vector3(a, c));
            m_lVectorsCube.Add(new Vector3(a, f));
            m_lVectorsCube.Add(new Vector3(a, h));
            m_lVectorsCube.Add(new Vector3(c, b));
            m_lVectorsCube.Add(new Vector3(c, d));
            m_lVectorsCube.Add(new Vector3(f, g));
            m_lVectorsCube.Add(new Vector3(f, d));
            m_lVectorsCube.Add(new Vector3(h, g));
            m_lVectorsCube.Add(new Vector3(c, d));
            m_lVectorsCube.Add(new Vector3(h, b));
        }

        /// <summary>
        /// Initialisierungsmethode für den soliden Würfel
        /// </summary>
        private void initCubeSolid()
        {
            Point3 a = new Point3(-50f, -50f, -50f);
            Point3 b = new Point3(50f, 50f, -50f);
            Point3 c = new Point3(50f, -50f, -50f);
            Point3 d = new Point3(50f, -50f, 50f);
            Point3 f = new Point3(-50f, -50f, 50f);
            Point3 g = new Point3(-50f, 50f, 50f);
            Point3 h = new Point3(-50f, 50f, -50f);
            Point3 k = new Point3(50f, 50f, 50f);

            m_lPointsCubeSolid.Clear();

            m_lPointsCubeSolid.Add(new Square(a, c, b, h));
            m_lPointsCubeSolid.Add(new Square(a, h, g, f));
            m_lPointsCubeSolid.Add(new Square(g, f, d, k));
            m_lPointsCubeSolid.Add(new Square(k, d, c, b));
            m_lPointsCubeSolid.Add(new Square(h, b, k, g));
            m_lPointsCubeSolid.Add(new Square(a, c, d, f));
        }
        #endregion

        #region BaseFigures & IFigures

        /// <summary>
        /// Interne Zeichenfunktion spricht die einzelnen Zeichenfunktionen der Klasse an
        /// </summary>
        protected override void InternalDraw()
        {
            if (paintCube) 
            {
                DrawCube(StaticVars.Linewidth); 
            }
            if (paintCubeSolid) 
            { 
                DrawCubeSolid(); 
            }
            if (paintSin) 
            { 
                DrawSinus(); 
            }
            if (paintsystem) 
            {
                DrawSystem(GridSize); 
            }
        }

        /// <summary>
        /// Interne Tickfunktion ist bei den statischen Sachen nicht nötig
        /// </summary>
        protected override void InternalTick()
        {
            // nicht nötig
        }

        /// <summary>
        /// Initialisierung für die Würfel
        /// </summary>
        public override void Init()
        {
            initCube();
            initCubeSolid();
        }

        /// <summary>
        /// Tastendruck auswerten
        /// </summary>
        /// <param name="e"></param>
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            //C Gitter Cube zeichnen
            if (e.KeyCode == Keys.C)
            { 
                paintCube = !paintCube; 
            }

            //G System malen
            if (e.KeyCode == Keys.G)
            { 
                paintsystem = !paintsystem; 
            }

            //0 Dec Gridsize
            if (e.KeyCode == Keys.D0)
            {
                if (GridSize > 1000)
                {
                    GridSize -= 1000f;
                }
            }

            //9 Inc Gridsize
            if ((e.KeyCode == Keys.D9))
            { 
                if (GridSize < 500000) 
                { 
                    GridSize += 1000f; 
                } 
            }

            //R Paint Sinus
            if (e.KeyCode == Keys.R)
            { 
                paintSin = !paintSin;
            }

            Visible = paintsystem || paintSin || paintCubeSolid || paintCube;
        }

        /// <summary>
        /// bei Programmende aufgerufen
        /// </summary>
        public override void OnShutdown()
        {
        }
        #endregion
    }

}
