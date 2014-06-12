//
// Planet.cs 
//	- Module for Mainform in 3DGLLab
//  - It animates a cube or sphere shaped planet flying around its Sun.
//  - In Cubemode the sun and planet rotate.
//
// Authors:
//	Andreas Maertens <mcmaerti@gmx.de>
//
// Copyright 2011 by Andreas Maertens

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Windows.Forms;
using System.Threading;

namespace GL3DLab
{
    /// <summary>
    /// Klasse für die Leuchtspur hinter den Körpern
    /// </summary>
    class PPoint : Point3
    {
        /// <summary>
        /// Erstellungszeit des Punktes
        /// </summary>
        private DateTime _Birthtime;

        private float Lifetime;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="X">X Position</param>
        /// <param name="Y">Y Position</param>
        /// <param name="Z">Z Position</param>
        public PPoint(float X, float Y, float Z, float lifetime)
        {
            _Birthtime = DateTime.Now;
            x = X;
            y = Y;
            z = Z;
            Lifetime = lifetime;
        }

        /// <summary>
        /// Sichtbarkeit des Punktes abhängig von der Lebenszeit des Punktes
        /// </summary>
        public float Alpha
        {
            get
            {
                //Nach 5 Sekunden ist Alpha 0 - rechnet mit Millisekunden, so ist der Fade feiner.
                TimeSpan __life = DateTime.Now - _Birthtime;
                float temp = (1f - ((float)__life.Seconds * 1000 + (float)__life.Milliseconds) / Lifetime);
                return temp > 0 ? temp : 0;
            }
        }
    }

    /// <summary>
    /// Form der Planeten
    /// </summary>
    enum PlanetShape { sphere = 0, quad = 1 }

    /// <summary>
    /// Ein Planet kreist um seine Sonne.
    /// </summary>
    public class Planet : BaseFigure
    {
        /// <summary>
        /// Spur des Planeten
        /// </summary>
        private List<PPoint> trace = new List<PPoint>();

        /// <summary>
        /// Thread für Planetenbewegung
        /// </summary>
        private static Thread PlanetMove;

        private bool enabled = false;
        /// <summary>
        /// Member für Matrixoperationen
        /// </summary>
        private MatrixMath MM = new MatrixMath();

        #region settings Planet
        /// <summary>
        /// Soll Planetenspur angezeigt werden?
        /// </summary>
        private bool showTrace = false;
        private float _Speed = 0.1f;
        private float _dAngle = 0.1f;
        private float Size = 1f;
        private PlanetShape Shape = PlanetShape.quad;

        #region Colors Planet
        private float _Red1 = 0;
        private float _Green1 = 0;
        private float _Blue1 = 1f;

        private float _Red2 = 1f;
        private float _Green2 = 0.5f;
        private float _Blue2 = 0f;
        #endregion

        #region Position Planet
        /// <summary>
        /// Richtung des Planeten
        /// </summary>
        private PPoint _Dir = new PPoint(0f, 0f, 0f,2000f);
        private PPoint _Pos = new PPoint(0f, 0f, 0f,2000f);
        private PPoint __InitPos = new PPoint(0f, 0f, 0f,2000f);

        private float _AngleX = 0;
        private float _AngleZ = 0;
        #endregion

        #endregion

        #region Settings Sun
        private float _SunAngleX = 0;
        private float _SunAngleZ = 0;
        private float SunSize = 20f;
        private bool showSun = false;
        #endregion

        /// <summary>
        /// Interne Initialisierung des Systems
        /// </summary>
        /// <param name="AngleZ">Drehwinkel um Z Achse</param>
        /// <param name="AngleX">Drehwinkel um X Achse</param>
        /// <param name="Pos">Planetenposition</param>
        /// <param name="Dir">Planetenbewegung</param>
        private void InternalInit(float AngleZ, float AngleX, Point3 Pos, Point3 Dir)
        {
            // Richtung gleich normieren
            float n = (float)Math.Sqrt(Math.Pow(Dir.x, 2) + Math.Pow(Dir.y, 2) + Math.Pow(Dir.z, 2));
            _Dir.x = Dir.x / n;
            _Dir.y = Dir.y / n;
            _Dir.z = Dir.z / n;

            _Pos.x = __InitPos.x = Pos.x;
            _Pos.y = __InitPos.y = Pos.y;
            _Pos.z = __InitPos.z = Pos.z;

            _AngleX = AngleX;
            _AngleZ = AngleZ;
        }

        /// <summary>
        /// Zeichenroutine für die Planetenspur
        /// </summary>
        private void DrawTrace()
        {
            Gl.glDisable(Gl.GL_LIGHTING);
            //Gl_BLEND um Transparenz via Alpha zu ermöglichen.
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glBegin(Gl.GL_LINES);
            {
                if (trace.Count > 0)
                {
                    lock (trace) // für die Threadsicherheit
                    {
                        for (int i = 1; i < trace.Count; i++)
                        {
                            //Dank Alpha wird der Punkt schwächer (transparenz), je nachdem wie alt er ist.
                            Gl.glColor4f(_Red2, _Green2, _Blue2, trace[i - 1].Alpha);
                            Gl.glVertex3f(trace[i - 1].x, trace[i - 1].y, trace[i - 1].z);
                            Gl.glColor4f(_Red2, _Green2, _Blue2, trace[i].Alpha);
                            Gl.glVertex3f(trace[i].x, trace[i].y, trace[i].z);
                        }
                        //letzter Punkt mit Planetcenter verbunden.
                        int j = trace.Count - 1;
                        Gl.glColor4f(_Red2, _Green2, _Blue2, trace[j].Alpha);
                        Gl.glVertex3f(trace[j].x, trace[j].y, trace[j].z);
                        Gl.glColor4f(_Red2, _Green2, _Blue2, 1f);
                        Gl.glVertex3f(_Pos.x, _Pos.y, _Pos.z);
                    }
                }
            }
            Gl.glEnd();
            Gl.glDisable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_LIGHTING);
        }

        /// <summary>
        /// Zeichenroutine für die Sonne
        /// </summary>
        private void DrawSun()
        {
            float X = 0;
            float Y = 0;
            float Z = 0;
            float dx = SunSize;
            float dy = dx;
            float dz = dx;

            float __red1 = 1f;
            float __green1 = 0.9f;
            float __blue1 = 0.8f;

            float __red2 = 1f;
            float __green2 = 0.8f;
            float __blue2 = 0.7f;

            Gl.glDisable(Gl.GL_LIGHTING);


            switch (Shape)
            {
                case PlanetShape.sphere:
                    {
                        Gl.glPushMatrix();

                        Gl.glTranslatef(X, Y, Z);

                        Gl.glColor3f(__red1, __green1, __blue1);
                        Glu.GLUquadric sun = Glu.gluNewQuadric();
                        Glu.gluSphere(sun, SunSize, 20, 20);

                        Gl.glPopMatrix();
                        break;
                    }
                case PlanetShape.quad:
                    {
                        Gl.glPushMatrix();

                        Gl.glRotatef(-_SunAngleZ, 0, 0, 1);
                        Gl.glRotatef(-_SunAngleX - 90, 1, 0, 0);

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glColor3f(__red1, __green1, __blue1); Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glColor3f(__red2, __green2, __blue2); Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glPopMatrix();
                        break;
                    }
            }
            Gl.glEnable(Gl.GL_LIGHTING);


        }

        /// <summary>
        /// Zeichenroutine für den Planeten
        /// </summary>
        private void DrawPlanet()
        {
            float X = _Pos.x;
            float Y = _Pos.y;
            float Z = _Pos.z;
            float DX = _Dir.x;
            float DY = _Dir.y;
            float DZ = _Dir.z;
            float dx = Size;
            float dy = Size;
            float dz = Size;

            switch (Shape)
            {
                case PlanetShape.sphere:
                    {
                        Gl.glPushMatrix();

                        Gl.glTranslatef(X, Y, Z);

                        Gl.glColor3f(_Red1, _Green1, _Blue1);

                        Glu.GLUquadric earth = Glu.gluNewQuadric();
                        Glu.gluSphere(earth, Size, 20, 20);

                        Gl.glPopMatrix();
                        break;
                    }
                case PlanetShape.quad:
                    {
                        Gl.glPushMatrix();

                        Gl.glTranslatef(X, Y, Z);

                        Gl.glRotatef(-_AngleZ, 0, 0, 1);
                        Gl.glRotatef(-_AngleX - 90, 1, 0, 0);

                        X = 0;
                        Y = 0;
                        Z = 0;
                        Gl.glEnable(Gl.GL_CULL_FACE_MODE);

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(0f, 0f, -1f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(0f, -1f, 0f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(-1f, 0f, 0f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X - dx, Y - dy, Z - dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(1f, 0f, 0f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glVertex3f(X + dx, Y - dy, Z - dz);
                        Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(0f, 1f, 0f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X + dx, Y + dy, Z - dz);
                        Gl.glVertex3f(X - dx, Y + dy, Z - dz);
                        Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glEnd();

                        Gl.glBegin(Gl.GL_POLYGON);

                        Gl.glNormal3f(0f, 0f, 1f);

                        Gl.glColor3f(_Red2, _Green2, _Blue2);
                        Gl.glVertex3f(X + dx, Y + dy, Z + dz);
                        Gl.glColor3f(_Red1, _Green1, _Blue1);
                        Gl.glVertex3f(X - dx, Y + dy, Z + dz);
                        Gl.glVertex3f(X - dx, Y - dy, Z + dz);
                        Gl.glVertex3f(X + dx, Y - dy, Z + dz);
                        Gl.glEnd();

                        Gl.glPopMatrix();
                        break;
                    }
            }


        }

        /// <summary>
        /// Interne Drawfunktion
        /// </summary>
        protected override void InternalDraw()
        {
            DrawPlanet();
            if (showTrace)
            {
                DrawTrace();
            }
            if (showSun)
            {
                DrawSun();
            }
        }

        /// <summary>
        /// ein Tick im Thread
        /// </summary>
        private void ThreadTick()
        {
            // Bewegung
            _Pos.x += _Dir.x * _Speed;
            _Pos.y += _Dir.y * _Speed;
            _Pos.z += _Dir.z * _Speed;

            // Drehung um  _dAngle Grad um Z Axe.
            Point3 ZAxis = new Point3(0, 0, 1);
            Matrix rot = new Matrix();
            rot.RotMatrix(_dAngle, ZAxis);
            _Dir.assign(MM.MatDotPoint(rot, (Point3)_Dir));

            // Rotation
            _AngleZ = (_AngleZ - 1f) % 360;
            _SunAngleZ = (_SunAngleZ + 0.01f) % 360;


            // Trace
            if (showTrace)
            {
                lock (trace)
                {
                    PPoint newPoint = new PPoint(0, 0, 0,2000f);

                    if (trace.Count > 0)
                    {
                        newPoint.assign((Point3)_Pos - (Point3)trace.Last<PPoint>());

                        while (trace[0].Alpha == 0)
                        {
                            trace.RemoveAt(0);
                        }
                    }
                    else
                    {
                        newPoint.assign(_Pos);
                    }

                    float n = (float)Math.Sqrt(Math.Pow(newPoint.x, 2) + Math.Pow(newPoint.y, 2) + Math.Pow(newPoint.z, 2));

                    if (n > 0.1)
                    {
                        trace.Add(new PPoint(_Pos.x, _Pos.y, _Pos.z,2000f));
                    }
                }
            }
            else
            {
                trace.Clear();
            }

        }

        /// <summary>
        /// Eigener Thread für Bewegung des Planeten
        /// </summary>
        private void PlanetMove_Start()
        {
            while (true)// infinity loop
            {
                if (Enabled)
                {
                    ThreadTick();
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Initialisiert den Planeten und seine Position und Laufbahn
        /// </summary>
        public override void Init()
        {
            InternalInit(0, 0, new Point3(-50f, 0, 0), new Point3(0, 1f, 0));
        }

        /// <summary>
        /// Wird aufgerufen sobald eine Taste gedrückt wurde.
        /// </summary>        
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L && !e.Control)
            {
                Enabled = !Enabled;
                Visible = !Visible;
            }
            if (e.KeyCode == Keys.K && !e.Control)
            {
                showTrace = !showTrace;
            }
            if (e.KeyCode == Keys.K && e.Control)
            {
                showSun = !showSun;
            }
            if (e.KeyCode == Keys.L && e.Control)
            {
                Shape = (PlanetShape)(((int)Shape + 1) % 2);
            }
        }

        /// <summary>
        /// Da in Thread ausgelagert ist hier nichts nötig.
        /// </summary>
        protected override void InternalTick()
        {
            // nothing to do. Ist in Thread ausgelagert.
        }

        /// <summary>
        /// Constructor erzeugt auch Threadobjekt.
        /// </summary>
        public Planet()
        {
            PlanetMove = new Thread(new ThreadStart(PlanetMove_Start));
        }

        /// <summary>
        /// musste überschrieben werden, da es zum starten des Thread dienen soll.
        /// </summary>
        new public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (enabled && !PlanetMove.IsAlive)
                {
                    PlanetMove.Start();
                }
            }
        }

        /// <summary>
        /// PlanetenBewegungsThread muss angehalten werden beim beenden.
        /// </summary>
        public override void OnShutdown()
        {
            PlanetMove.Abort();
        }
    }
}
