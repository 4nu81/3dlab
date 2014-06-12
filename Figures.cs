//
// Planetadvanced3.cs 
//	- Module for Mainform in 3DGLLab
//  - Kneul is an animation of dots randomly moving forward with their tail of dots
//  - Sphere are dots put on a sphere shaped surface
//
// Authors:
//	Andreas Maertens <mcmaerti@gmx.de>
//  Tobias Schmidt
//
// Copyright 2011 by Andreas Maertens

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Timers;
using System.Windows.Forms;

namespace GL3DLab
{
    /// <summary>
    /// Ein Punktkneul welches sich "Snake"-Ähnlich durch den Raum bewegt
    /// </summary>
    class Kneul : BaseFigure
    {
        /// <summary>
        /// Anzahl an neu berechneten Punkten je durchlauf
        /// </summary>
        private int kneulspeed = 50;

        /// <summary>
        /// Farbumkehrung ( -1 oder +1 um mit Werten zu multiplizieren)
        /// </summary>
        private int invert = 1;

        /// <summary>
        /// Zufallsgenerator
        /// </summary>
        private Random rndkn = new Random(Convert.ToInt32(DateTime.Now.Millisecond));

        #region Kneul
        /// <summary>
        /// Punkte des Kneulobjektes
        /// </summary>
        private List<Point3> m_lPointsKneul = new List<Point3>() { new Point3(0, 0, 0) };

        /// <summary>
        /// Punkte des Zauberobjektes
        /// </summary>
        private List<Point3> m_lPointsZauber = new List<Point3>() { new Point3(0, 0, 0) };

        /// <summary>
        /// Zeichenmethode für das Zauberobjekt
        /// </summary>
        private void DrawMagix(int kneulspeed, float DotSize, float brightness, int invertColor)
        {
            float zcount = (float)kneulspeed * 100f;
            Gl.glPointSize(DotSize);
            Gl.glBegin(Gl.GL_POINTS);
            {
                for (int j = 0; j < m_lPointsZauber.Count; j++)
                {
                    Point3 p = m_lPointsZauber.ElementAt(j);
                    float zcolor = 1 - ((zcount - j) / zcount);
                    //Gl.glColor3f((50 + p.x) / 100f * zcolor, (50 + p.y) / 100f * zcolor, (50 + p.z) / 100f * zcolor);
                    Gl.glColor3f(brightness + invertColor * (50 + p.x) / 100f, brightness + invertColor * (50 + p.y) / 100f, brightness + invertColor * (50 + p.z) / 100f);
                    Gl.glVertex3f(p.x, p.y, p.z);
                }
            }
            Gl.glEnd();
        }

        /// <summary>
        /// Zeichenmethode für das Kneulobjekt
        /// </summary>
        private void DrawKneul(float Linewidth, int kneulspeed, float brightness, int invertColor)
        {
            Gl.glLineWidth(Linewidth);
            Gl.glBegin(Gl.GL_POINTS);
            {
                //foreach (Point3 p in m_lPointsKneul)
                for (int i = 0; i < m_lPointsKneul.Count; i++)
                {
                    Point3 p = m_lPointsKneul.ElementAt(i);
                    float div = i / kneulspeed;
                    //Gl.glColor3f(invertColor * (50 + p.x) / 100f, invertColor * (50 + p.y) / 100f, invertColor * (50 + p.z) / 100f);
                    //Gl.glColor3f(brightness + (invertColor * (50 + p.x) / 100f) * div, brightness + (invertColor * (50 + p.y) / 100f) * div, brightness + (invertColor * (50 + p.z) / 100f) * div);
                    Gl.glColor3f(brightness + invertColor * (50 + p.x) / 100f, brightness + invertColor * (50 + p.y) / 100f, brightness + invertColor * (50 + p.z) / 100f);
                    Gl.glVertex3f(p.x, p.y, p.z);
                }
            }
            Gl.glEnd();
        }

        /// <summary>
        /// Neue Punkte auf dem Kneul
        /// </summary>
        /// <param name="rndkn">Zufallsgenerator</param>
        /// <param name="kneulspeed">Anzahl neuer Punkte</param>
        private void NewPointOnKneul(Random rndkn, int kneulspeed)
        {
            for (int i = 0; i < kneulspeed; i++)
            {
                NewPointKneul(rndkn);
            }
        }

        /// <summary>
        /// Einzelner neuer Punkt auf dem Kneul
        /// </summary>
        /// <param name="rndkn">Zufallsgenerator</param>
        private void NewPointKneul(Random rndkn)
        {
            Point3 StPoint = m_lPointsKneul.Last();
            Point3 newPoint = new Point3(StPoint.x, StPoint.y, StPoint.z);
            float p = newPoint.x + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.x = p;
            }
            p = newPoint.y + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.y = p;
            }
            p = newPoint.z + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.z = p;
            }
            m_lPointsKneul.Add(newPoint);
        }

        /// <summary>
        /// Neue Punkte auf dem Zauber
        /// </summary>
        /// <param name="rndkn">Zufallsgenerator</param>
        /// <param name="kneulspeed">Anzahl neuer Punkte</param>
        private void NewPointOnMagix(Random rndkn, int kneulspeed)
        {
            for (int i = 0; i < kneulspeed; i++)
            {
                NewPointZauber(rndkn, kneulspeed);
            }
        }

        /// <summary>
        /// einzelner neuer Punkt auf dem Kneul
        /// </summary>
        /// <param name="rndkn">Zufallsgenerator</param>
        /// <param name="kneulspeed">Länge des Zaubers</param>
        private void NewPointZauber(Random rndkn, int kneulspeed)
        {
            Point3 StPoint = m_lPointsZauber.Last();
            Point3 newPoint = new Point3(StPoint.x, StPoint.y, StPoint.z);
            float p = newPoint.x + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.x = p;
            }
            p = newPoint.y + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.y = p;
            }
            p = newPoint.z + (float)rndkn.NextDouble() - 0.5f;
            if (!(p >= 50) && !(p <= -50))
            {
                newPoint.z = p;
            }
            m_lPointsZauber.Add(newPoint);
            while (m_lPointsZauber.Count > (100 * kneulspeed))
            {
                m_lPointsZauber.RemoveAt(0);
            }
        }
        #endregion

        #region BaseFigure
        /// <summary>
        /// Interne Zeichenfunktion
        /// </summary>
        protected override void InternalDraw()
        {
            DrawMagix(kneulspeed, StaticVars.Linewidth, StaticVars.Brightness, invert);
        }

        /// <summary>
        /// Interne Tickfunktion
        /// </summary>
        protected override void InternalTick()
        {
            NewPointOnMagix(rndkn, kneulspeed);
        }

        /// <summary>
        /// Initialisiertung der Kneul und Magix Objekte
        /// </summary>
        public override void Init()
        {
            m_lPointsZauber.Clear();
        }

        /// <summary>
        /// Auswertung Tastendruck
        /// </summary>
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                Visible = !Visible;
                Enabled = !Enabled;
            }

            if (e.KeyCode == Keys.I)
            {
                invert *= -1;
            }

            //1 DecSpeed
            if (e.KeyCode == Keys.D1)
            {
                if (kneulspeed > 10)
                {
                    kneulspeed -= 10;
                }
            }

            //2 IncSpeed
            if (e.KeyCode == Keys.D2)
            {
                if (kneulspeed < 5000)
                {
                    kneulspeed += 10;
                }
            }
        }

        /// <summary>
        /// Wird beim Beenden des Programmes ausgelöst.
        /// </summary>
        public override void OnShutdown()
        {
        }
        #endregion
    }

    /// <summary>
    /// Viele Punkte auf eine Kugeloberfläche
    /// </summary>
    class Sphere : BaseFigure
    {
        /// <summary>
        /// Liste der Punkte auf der Kugeloberfläche
        /// </summary>
        public List<Point3> m_lPoints = new List<Point3>() { new Point3(0, 0, 0) };

        /// <summary>
        /// Matrizen Modul
        /// </summary>
        private MatrixMath MMath;

        /// <summary>
        /// Für die erzeugung neuer Punkte nötig
        /// </summary>
        private float angle = 0;

        /// <summary>
        /// Für die erzeugung neuer Punkte nötig
        /// </summary>
        private float angle2 = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aMMath">Matrixmodul</param>
        public Sphere(MatrixMath aMMath)
        {
            MMath = aMMath;
        }

        /// <summary>
        /// erzeugt neuen Punkt auf der Kugeloberfläche
        /// </summary>
        public void NewPointOnSphere()
        {
            for (int i = 0; i < 1000; i++)
            {
                NewPoint();
            }
        }

        /// <summary>
        /// Zeichnet Kugel
        /// </summary>
        /// <param name="DotSize">Punktgröße</param>
        public void DrawSphere(float DotSize)
        {
            Gl.glPointSize(DotSize);
            foreach (Point3 p in m_lPoints)
            {
                Gl.glBegin(Gl.GL_POINTS);
                {
                    Gl.glColor3f((10 + p.x) / 20f, (10 + p.y) / 20f, (10 + p.z) / 20f);
                    Gl.glVertex3f(p.x, p.y, p.z);
                }
            }
            Gl.glEnd();
        }

        /// <summary>
        /// eigentliche Berechnung eines neuen Punktes
        /// </summary>
        public void NewPoint()
        {
            //Neuen Punkt errechnen
            Random rnd1 = new Random(m_lPoints.Count);
            Random rnd2 = new Random(rnd1.Next());

            angle = (float)rnd2.NextDouble() * 360;
            angle2 = (float)rnd1.NextDouble() * 360;


            //Neuen Punkt hinzufuegen  
            Point3 StPoint = new Point3(0, 0, 50);
            Point3 Point = new Point3(0, 0, 0);
            Matrix Rot = new Matrix();
            Matrix Rot2 = new Matrix();
            Matrix Rot3 = new Matrix();

            Rot.RotMatrix(angle, new Point3(1f, 0f, 0f));
            Rot2.RotMatrix(angle2, new Point3(0f, 0f, 1f));
            Rot3.RotMatrix(angle, new Point3(1f, 0f, 1f));

            Point = MMath.MatDotPoint(Rot, StPoint);

            Point = MMath.MatDotPoint(Rot2, Point);

            m_lPoints.Add(Point);

        }

        /// <summary>
        /// Interne Zeichenfunktion aus BaseFigure
        /// </summary>
        protected override void InternalDraw()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Interne Simulationsfunktion aus BaseFigure
        /// </summary>
        protected override void InternalTick()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initialisierung der Kugeloberfläche
        /// </summary>
        public override void Init()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event bei Tastendruck
        /// </summary>
        public override void KeyPressed(KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Wird beim Beenden des Programms aufgerufen.
        /// </summary>
        public override void OnShutdown()
        {
            throw new NotImplementedException();
        }
    }
}