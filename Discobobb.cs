//
// Planetadvanced.cs 
//	- Module for Mainform in 3DGLLab
//  - it simulates a single solarsystem where randomly added planets flying around, flying away from or crashing into their sun.
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
    abstract class oDBObject
    {
        public Point3 Position = new Point3();
        public Point3 Direction = new Point3();
        public float Size = 1f;
        private MatrixMath MM = new MatrixMath();
        protected float _Red = 0;
        protected float _Green = 0;
        protected float _Blue = 1f;
        protected List<PPoint> trace = new List<PPoint>();
        public float Mass = 10f;

        public void DrawTrace()
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
                            Gl.glColor4f(_Red, _Green, _Blue, trace[i - 1].Alpha);
                            Gl.glVertex3f(trace[i - 1].x, trace[i - 1].y, trace[i - 1].z);
                            Gl.glColor4f(_Red, _Green, _Blue, trace[i].Alpha);
                            Gl.glVertex3f(trace[i].x, trace[i].y, trace[i].z);
                        }
                        //letzter Punkt mit Planetcenter verbunden.
                        int j = trace.Count - 1;
                        Gl.glColor4f(_Red, _Green, _Blue, trace[j].Alpha);
                        Gl.glVertex3f(trace[j].x, trace[j].y, trace[j].z);
                        Gl.glColor4f(_Red, _Green, _Blue, 1f);
                        Gl.glVertex3f(Position.x, Position.y, Position.z);
                    }
                }
            }
            Gl.glEnd();
            Gl.glDisable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_LIGHTING);
        }

        public abstract void DrawObject();
        public abstract void MoveObject();
    }

    class oDBPlanet : oDBObject
    {
        private oDBSun Center = null;
        private float dt = 0.04f;
        private Point3 a()
        {
            float distance = Point3.Distance(this.Position, Center.Position);

            Point3 x = new Point3();
            x.assign(Position - Center.Position);

            float fak = (float)(-Center.G * Center.Mass / Math.Pow(distance, 3));

            return x * fak;
        }
        private Point3 v()
        {
            Point3 _a = new Point3();
            _a = a();
            return Direction = Direction + (_a * dt);
        }
        private Point3 p()
        {
            Point3 _v = v();
            return Position = Position + (_v * dt);
        }

        public oDBPlanet(Point3 Pos, Point3 Dir, oDBSun Central, float mass, float Red, float Green, float Blue)
        {
            if (Central == null)
            {
                throw new Exception("Kein Zentralmassiv angegeben");
            }
            Position.assign(Pos);
            Direction.assign(Dir);
            Center = Central;
            _Red = Red;
            _Green = Green;
            _Blue = Blue;
            Mass = mass;
            Size = (float)((Math.Pow(Mass, (double)(1f / 3f)) * 3f) / (4f * Math.PI));
        }

        public override void DrawObject()
        {
            float X = Position.x;
            float Y = Position.y;
            float Z = Position.z;
            float DX = Direction.x;
            float DY = Direction.y;
            float DZ = Direction.z;
            float dx = Size;
            float dy = Size;
            float dz = Size;

            Gl.glPushMatrix();

            Gl.glTranslatef(X, Y, Z);

            Gl.glColor3f(_Red, _Green, _Blue);

            Glu.GLUquadric earth = Glu.gluNewQuadric();
            Glu.gluSphere(earth, Size, 20, 20);

            Gl.glPopMatrix();
        }

        private float TraceTime = 800f;

        private void Dotrace()
        {
            lock (trace)
            {
                try
                {
                    PPoint newPoint = new PPoint(0, 0, 0, TraceTime);

                    if (trace.Count > 0)
                    {
                        newPoint.assign((Point3)Position - (Point3)trace.Last<PPoint>());

                        while (trace[0].Alpha == 0)
                        {
                            trace.RemoveAt(0);
                        }
                    }
                    else
                    {
                        newPoint.assign(Position);
                    }

                    float n = (float)Math.Sqrt(Math.Pow(newPoint.x, 2) + Math.Pow(newPoint.y, 2) + Math.Pow(newPoint.z, 2));

                    if (n > 0.1)
                    {
                        trace.Add(new PPoint(Position.x, Position.y, Position.z, TraceTime));
                    }
                }
                catch (Exception)
                {                 
                }
            }
        }

        public override void MoveObject()
        {
            Position = p();
            Dotrace();
        }
    }

    class oDBSun : oDBObject
    {        
        public float mass
        {
            get 
            {
                return Mass;
            }
            set 
            {
                Mass = value;
                Size = (float)((Math.Pow(value, (double)(1f / 3f)) * 3f) / (4f * Math.PI));                
            }
        }
        public float G = -10f;

        public oDBSun(Point3 Pos, Point3 Dir, float size, float mass, float g)
        {
            Position = Pos;
            Direction = Dir;
            _Blue = 0.05f;
            _Green = 0.05f;
            _Red = 0.05f;
            Size = size;
            this.mass = mass;
            G = g;
        }

        public override void DrawObject()
        {
            float X = 0;
            float Y = 0;
            float Z = 0;
            float dx = Size;
            float dy = dx;
            float dz = dx;
            
            Gl.glDisable(Gl.GL_LIGHTING);


            Gl.glPushMatrix();

            Gl.glTranslatef(X, Y, Z);

            Gl.glColor3f(_Red, _Green, _Blue);
            Glu.GLUquadric sun = Glu.gluNewQuadric();
            Glu.gluSphere(sun, Size, 20, 20);

            Gl.glPopMatrix();

            Gl.glEnable(Gl.GL_LIGHTING);
        }

        public override void MoveObject()
        {
        }
    }

    class DiscoBobb : BaseFigure
    {
        private static Thread PlanetMove;
        private bool enabled = false;
        private MatrixMath MM = new MatrixMath();

        private List<oDBObject> Objects = new List<oDBObject>();
        private Random rnd = new Random(Convert.ToInt32(DateTime.Now.Millisecond));

        protected override void InternalDraw()
        {
            lock (Objects)
            {
                foreach (oDBObject o in Objects.ToArray())
                {
                    o.DrawObject();
                    o.DrawTrace();
                }
            }
        }

        private void ThreadTick()
        {
            float distance;
            float ri;
            float rj;
            oDBObject oi;
            oDBObject oj;

            lock (Objects)
            {
                foreach (oDBObject o in Objects)
                {
                    o.MoveObject();
                }

                for (int i = 0; i < Objects.Count - 1; i++)
                {
                    for (int j = i + 1; j < Objects.Count; j++)
                    {
                        oi = Objects[i];
                        oj = Objects[j];
                        distance = Point3.Distance(oi.Position, oj.Position);
                        ri = oi.Size;
                        rj = oj.Size;
                        if (ri + rj > distance)
                        {
                            if (Objects[i] == Sun)
                            {
                                Sun.Mass += oj.Mass;
                                Objects.Remove(oj);
                                break;
                            }
                            else
                            {
                                if (Objects[j] == Sun)
                                {
                                    Sun.Mass += oi.Size;
;
                                    Objects.Remove(oi);
                                    break;
                                }
                                else
                                {
                                    Objects.Remove(oi);
                                    Objects.Remove(oj);
                                    break;
                                }
                            }
                        }
                    }
                }

                try // Wegen Threading kommt es gern zu Fehlern beim Entfernen von Objekten. Das wird hier abgefangen.
                {   // ist unsauber und langsamer, ich weiß.
                    #region zu weit von der Hauptsonne entfernte Objekte entfernen
                    for (int i = Objects.Count - 1; i >= 0; i--)
                    {
                        oDBObject o = Objects[i];
                        distance = Point3.Distance(Sun.Position, o.Position);
                        if (distance > 300000)
                        {
                            Objects.Remove(o);
                        }
                    }
                    #endregion
                }
                catch (Exception)
                { }
            }
        }

        private void PlanetMove_Start()
        {
            while (true)// infinity loop
            {
                if (Enabled)
                {
                    ThreadTick();
                    System.Threading.Thread.Sleep(1);
                    if (rnd.Next(0, 4) == 1) 
                    {
                        AddPlanet();
                    }
                }
            }
        }


        private float[] C = { 2f, 1f, 0f };
        private oDBSun Sun;

        /// <summary>
        /// gibt zufällig -1 oder +1 zurück
        /// </summary>
        /// <returns>float -1 oder +1</returns>
        private float fak()
        {
            return ((rnd.NextDouble() * 2) - 1 < 0) ? -1 : 1;
        }

        /// <summary>
        /// Hier wird die neue Position in Abhängigkeit von der Masse des Zentralkörpers bestimmt.
        /// </summary>        
        /// <returns>zufällig generierten Wert abhängig von Masse und Art des Zentralkörpers</returns>
        private float CalcDust()
        {
            float ss = (float)((Math.Pow(PlanetAdvanced3.initSunMass, (double)(1f / 3f)) * 3f) / (4f * Math.PI));
            return ss / 2 + (float)rnd.NextDouble() * fak() * 5 * ss;
        }

        /// <summary>
        /// Zufällige Position bezogen auf ein Zentralobjekt
        /// </summary>        
        /// <returns>beliebiger Vektor zu einer Position in der Nähe des Zentralobjektes</returns>
        private Point3 RndDustPosToSun()
        {
            Point3 result = new Point3();
            result.x = CalcDust();
            result.y = CalcDust();
            result.z = CalcDust();            
            return result;
        }

        private void AddPlanet()
        {
            lock (Objects)
            {
                float Mass = 1f;
                int ps = rnd.Next(0, 10);
                Mass = (float)rnd.NextDouble() * (float)Math.Pow(1, ps) * PlanetAdvanced2.UniFak; // Größe Abhängig von Basisgröße der Planeten

                Point3 P1Pos = RndDustPosToSun();               
                Point3 P1Dir = new Point3(0f, 0f, 0f);
                Objects.Add(new oDBPlanet(P1Pos, P1Dir, Sun, Mass, (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));
            }
        }

        public override void Init()
        {
            Point3 SunPos = new Point3();
            Point3 SunDir = new Point3();
            Sun = new oDBSun(SunPos, SunDir, 20f, PlanetAdvanced2.initSunMass, -9.81f);
            Objects.Add(Sun);
            Cam.Z = 50000f;
        }

        /// <summary>
        /// Wird aufgerufen sobald eine Taste gedrückt wurde.
        /// </summary>        
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.U)
            {
                Enabled = !Enabled;
                Visible = !Visible;
            }
            if (e.KeyCode == Keys.I)
            {
                AddPlanet();
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
        public DiscoBobb()
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
