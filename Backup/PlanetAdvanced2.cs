//
// Planetadvanced2.cs 
//	- Module for Mainform in 3DGLLab
//  - It simulate multiple starsystems with planets and moons in their orbits around their stars.
//  - These moons, planets and suns are randomly sized and positioned in their universe.
//  - Every sun up to a count of 7 has its one lightsource and lighten the planets.
//  - The objects react on gravity of all other objects in the system due to their mass and distance.
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
    public class o2Object
    {
        /// <summary>
        /// Größe des Objekts abhängig von der Masse
        /// </summary>
        /// <returns></returns>
        public float Size;

        /// <summary>
        /// Masse des Objektes
        /// </summary>
        public float mass = 0f;
        public float Mass 
        {
            get { return mass; }
            set 
            { 
                mass = value;
                Size = (float)((Math.Pow(Mass, (double)(1f / 3f)) * 3f) / (4f * Math.PI));
            }
        }

        /// <summary>
        /// Farbarray für Darstellung
        /// </summary>
        public float[] Farbe = { 1f, 1f, 1f };

        /// <summary>
        /// aktuelle Position
        /// </summary>
        public Point3 Position = new Point3(0f, 0f, 0f);

        /// <summary>
        /// Bewegungsrichtung
        /// </summary>
        public Point3 Direction = new Point3(1f, 0f, 0f);

        /// <summary>
        /// Liste der Tracepunkte
        /// </summary>
        private List<PPoint> trace = new List<PPoint>();

        /// <summary>
        /// falls sich Bewegung nur auf Parent beziehen soll
        /// </summary>
        private o2Object Parent = null;

        /// <summary>
        /// Zähler für die Leuchtspurerstellung - Bessere Performance
        /// </summary>
        private int TraceCnt = 1;
        
        /// <summary>
        /// Berechnung der Leuchspur
        /// </summary>
        public void Trace()
        {
            // Der TraceCnt verzögert die Erstellung von Tracepunkten und erhöht somit die Leistung
            if (TraceCnt == 3)
            {
                TraceCnt = 0;
                try
                {
                    lock (trace)
                    {
                        PPoint newPoint = new PPoint(0, 0, 0,2000f);

                        if (trace.Count > 0)
                        {
                            newPoint.assign((Point3)Position - (Point3)trace.Last<PPoint>());
                        }
                        else
                        {
                            newPoint.assign(Position);
                        }

                        float n = (float)Math.Sqrt(Math.Pow(newPoint.x, 2) + Math.Pow(newPoint.y, 2) + Math.Pow(newPoint.z, 2));

                        if (n > 0.1)
                        {
                            trace.Add(new PPoint(Position.x, Position.y, Position.z,2000f));
                        }

                        while (trace[0].Alpha == 0)
                        {
                            trace.RemoveAt(0);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            TraceCnt++;
        }

        /// <summary>
        /// Null Parameter Constructor für Vererbung
        /// </summary>
        public o2Object()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Pos">Position</param>
        /// <param name="Dir">Richtung</param>
        /// <param name="mass">Masse</param>
        /// <param name="Color">Farbarray</param>
        /// <param name="_Parent">Bewegung kann sich auch nur auf den Parent beziehen</param>
        public o2Object(Point3 Pos, Point3 Dir, float mass, float[] Color, o2Object _Parent)
        {
            Position = Pos;
            Direction = Dir;
            Mass = mass;
            Farbe = Color;
            Parent = _Parent;
        }

        /// <summary>
        /// Zeichenroutine für Leuchtspur
        /// </summary>
        public void DrawTrace()
        {
            //Gl_BLEND um Transparenz via Alpha zu ermöglichen.
            Gl.glDisable(Gl.GL_LIGHTING);
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
                            Gl.glColor4f(Farbe[0], Farbe[1], Farbe[2], trace[i - 1].Alpha);
                            Gl.glVertex3f(trace[i - 1].x, trace[i - 1].y, trace[i - 1].z);
                            Gl.glColor4f(Farbe[0], Farbe[1], Farbe[2], trace[i].Alpha);
                            Gl.glVertex3f(trace[i].x, trace[i].y, trace[i].z);
                        }
                        //letzter Punkt mit Planetcenter verbunden.
                        int j = trace.Count - 1;
                        Gl.glColor4f(Farbe[0], Farbe[1], Farbe[2], trace[j].Alpha);
                        Gl.glVertex3f(trace[j].x, trace[j].y, trace[j].z);
                        Gl.glColor4f(Farbe[0], Farbe[1], Farbe[2], 1f);
                        Gl.glVertex3f(Position.x, Position.y, Position.z);
                    }
                }
            }
            Gl.glEnd();
            Gl.glDisable(Gl.GL_BLEND);
            if (StaticVars.Light)
            {
                Gl.glEnable(Gl.GL_LIGHTING);
            }
        }        
        
        /// <summary>
        /// Beschleunigung Richtung Bezugsobjekt
        /// </summary>
        /// <param name="o">Bezugsobjekt</param>
        /// <returns></returns>
        private Point3 a(o2Object o)
        {
            float distance = Point3.Distance(this.Position, o.Position);

            Point3 x = new Point3();
            x.assign(Position - o.Position);

            float fak = (float)(-PlanetAdvanced2.G * o.Mass / Math.Pow(distance, 3));

            return x * fak;
        }

        /// <summary>
        /// Bewegungsrichtungsänderung durch Beschleunigung zum Objekt
        /// </summary>
        /// <param name="o">Bezugsobjekt</param>
        /// <returns>Richtungsvektor</returns>
        public Point3 _v(o2Object o)
        {
            Point3 _a = new Point3();
            _a = a(o);
            return Direction = Direction + (_a * PlanetAdvanced2.dt);
        }

        /// <summary>
        /// allgemeine Bewegungsroutine, kann überschrieben werden (für Sonnen)
        /// </summary>
        /// <param name="Objects"></param>
        public virtual void MoveObject(List<o2Object> Objects)
        {
            Point3 v = new Point3(0f, 0f, 0f);

            if (Parent == null)
            {

                foreach (o2Object o in Objects)
                {
                    if (o != this)
                    {
                        _v(o);
                    }
                }
            }
            else
            {
                _v(Parent);
            }
            Position = Position + (Direction * PlanetAdvanced2.dt);
        }

        /// <summary>
        /// allgemeine Zeichenroutine, kann überschrieben werden (für Sonnen)
        /// </summary>
        public virtual void DrawObject()
        {
            float X = Position.x;
            float Y = Position.y;
            float Z = Position.z;
            float r = Size;

            Gl.glPushMatrix();

            Gl.glColor3f(Farbe[0], Farbe[1], Farbe[2]);
            for (int i = 0; i < PlanetAdvanced2._Light.Length; i++)
            {
                Gl.glLightfv(PlanetAdvanced2._Light[i], Gl.GL_DIFFUSE, Farbe);
            }
            Gl.glTranslatef(X, Y, Z);

            Glu.GLUquadric obj = Glu.gluNewQuadric();
            Glu.gluSphere(obj, r, 20, 20);

            Gl.glPopMatrix();
        }
    }

    public class o2Sun : o2Object
    {
        /// <summary>
        /// Lichtquellenindex
        /// </summary>
        public int _light = Gl.GL_LIGHT0;

        /// <summary>
        /// Bewegungsroutine für Sonne
        /// </summary>
        /// <param name="Objects"></param>
        public override void MoveObject(List<o2Object> Objects)
        {
            Point3 v = new Point3(0f, 0f, 0f);

            foreach (o2Object o in Objects)
            {
                if (o != this)
                {
                    _v(o);
                }
            }

            //if (!lockSun)
            //{
            //    // Dämpfung, damit die Sonnen nicht abhauen. :)
            //    Point3 Center = new Point3();
            //    Direction -= (Position * 0.00001f);
            //    Direction -= (Direction - Center) * 0.001f;
            //}
            Position = Position + (Direction * PlanetAdvanced2.dt);
        }

        /// <summary>
        /// NullParameter Constructor für evtl. Vererbung.
        /// </summary>
        public o2Sun()
        {
            Mass = PlanetAdvanced2.initSunMass;
            Position.x = 0f;
            Position.y = 0f;
            Position.z = 0f;
            Direction.x = 0f;
            Direction.y = 0f;
            Direction.z = 0f;            
            Farbe[0] = 1f;
            Farbe[1] = 0.7f;
            Farbe[2] = 0.6f;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Pos">Position</param>
        /// <param name="Dir">Bewegungsrichtung</param>
        /// <param name="mass">Masse</param>
        /// <param name="Color">Farbarray</param>
        /// <param name="light">Lichtquellenindex</param>
        public o2Sun(Point3 Pos, Point3 Dir, float mass, float[] Color, int light)
        {
            Mass = mass;
            Position.assign(Pos);
            Direction.assign(Dir);            
            Farbe = Color;
            _light = light;
        }

        /// <summary>
        /// Zeichenroutine für o2Object wird überschieben, da Sonne eine Lichtquelle sind
        /// </summary>
        public override void DrawObject()
        {
            float X = Position.x;
            float Y = Position.y;
            float Z = Position.z;
            float r = Size;
            float[] Color = { Farbe[0], Farbe[1], Farbe[2], Mass / PlanetAdvanced2.initSunMass };

            Gl.glDisable(Gl.GL_LIGHTING); // Beleuchtung Ausmachen, da die Sonne ja die LichtQuelle ist.
            Gl.glPushMatrix();
            Gl.glTranslatef(X, Y, Z);
            float[] light0Pos = { 0f, 0f, 0f, 1.0f };
            Gl.glLightfv(_light, Gl.GL_POSITION, light0Pos); // Lichtquelle0 in der Sonne            
            Gl.glEnable(_light);

            Gl.glColor3f(Farbe[0], Farbe[1], Farbe[2]);
            Glu.GLUquadric obj = Glu.gluNewQuadric();
            Glu.gluSphere(obj, r, 20, 20);
            Gl.glPopMatrix();

            if (StaticVars.Light)
            {
                Gl.glEnable(Gl.GL_LIGHTING);
            }
        }

    }

    /// <summary>
    /// Planen kreisen um ihre Sonne.
    /// </summary>
    public class PlanetAdvanced2 : BaseFigure, IMouseControlled
    {
        /// <summary>
        /// Faktor, der die Potenzen des System relativiert.
        /// </summary>
        public static float UniFak = 1f * (float)Math.Pow(10, -20);
        /// <summary>
        /// Schrittweite pro Threaddurchlauf
        /// </summary>
        public static float dt = 0.1f;
        /// <summary>
        /// Globale Gravitationskonstante.
        /// </summary>
        public static float G = 6.67384f * (float)Math.Pow(10, -11) * 50000000;
        /// <summary>
        /// Alle OpenGl Lichtquellen für den Zugriff durch die Sonnen.
        /// </summary>
        public static int[] _Light = { Gl.GL_LIGHT0, Gl.GL_LIGHT1, Gl.GL_LIGHT2, Gl.GL_LIGHT3, Gl.GL_LIGHT4, Gl.GL_LIGHT5, Gl.GL_LIGHT6, Gl.GL_LIGHT7 };
        /// <summary>
        /// Die Größe der ersten Sonne. Dient auch als Basis-Faktor für neue Sonnen.
        /// </summary>
        public static float initSunMass = 2f * (float)Math.Pow(10, 31) * UniFak;

        /// <summary>
        /// Anzahl der bisher existierenden Sonnen (auch gelöschte)
        /// </summary>
        private int suncount = 1;

        /// <summary>
        /// Soll Planetspur gezeigt werden?
        /// </summary>
        private bool showTrace = true;

        /// <summary>
        /// Soll Bewegung simuliert werden?
        /// </summary>
        private bool simulate = true;

        /// <summary>
        /// Gibt an ob die Hauptsonne immer Zentriert sein soll.
        /// </summary>
        private bool centered = false;

        /// <summary>
        /// Soll die Hauptsonne statisch sein? (keine Bewegung)
        /// </summary>
        private bool lockSun = false;

        /// <summary>
        /// Member für Matrixoperationen
        /// </summary>
        private MatrixMath MM = new MatrixMath();

        /// <summary>
        /// Liste von Körpern im System
        /// </summary>
        private List<o2Object> Objects = new List<o2Object>();

        /// <summary>
        /// Hauptsonne
        /// </summary>
        private o2Sun Sun;

        /// <summary>
        /// Zufallsgenerator
        /// </summary>
        private Random rnd = new Random(Convert.ToInt32(DateTime.Now.Millisecond));

        /// <summary>
        /// Interne Drawfunktion
        /// </summary>
        protected override void InternalDraw()
        {
            try
            {
                for (int i = 0; i < _Light.Length; i++)
                {
                    Gl.glDisable(_Light[i]);
                }

                foreach (o2Object o in Objects)
                {
                    if (o is o2Sun)
                    {
                        o.DrawObject();
                        if (showTrace)
                        {
                            o.DrawTrace();
                        }
                    }
                }
                foreach (o2Object o in Objects)
                {
                    if (!(o is o2Sun))
                    {
                        o.DrawObject();
                        if (showTrace)
                        {
                            o.DrawTrace();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// eine Simulationsperiode
        /// </summary>
        private void ThreadTick()
        {            
            float distance;
            float ri;
            float rj;
            o2Object oi;
            o2Object oj;
            try
            {
                #region Bewegung
                foreach (o2Object o in Objects)
                {
                    if (!(o == Sun && lockSun))
                    {
                        o.MoveObject(Objects);
                        if (showTrace)
                        {
                            o.Trace();
                        }
                    }
                }
                #endregion
            }
            catch
            { }

            try // Wegen Threading kommt es gern zu Fehlern beim Entfernen von Objekten. Das wird hier abgefangen.
            {   // ist unsauber und langsamer, ich weiß.
                #region Kollision
                // Kollisionsabfrage Schwereres Objekt bekommt Masse von leichterem Objekt
                // und der Impuls des kleineren wird auf den größeren gerechnet.
                // Sonne wird nicht zerstört, da sie Lichtquelle ist.
                for (int i = 0; i < Objects.Count - 1; i++)
                {
                    for (int j = i + 1; j < Objects.Count; j++)
                    {
                        oi = Objects[i];
                        oj = Objects[j];
                        distance = Point3.Distance(oi.Position, oj.Position);
                        Point3 impulse = new Point3();
                        ri = oi.Size;
                        rj = oj.Size;
                        if (ri + rj > distance) // Objekte kollidieren ( Summe der Radien > Abstand )
                        {
                            impulse = (oi.Direction * oi.Mass) + (oj.Direction * oj.Mass);
                            if (Objects[i] == Sun) // erstes Objekt ist die HauptSonne - Hauptsonne soll bleiben.
                            {
                                Sun.Mass += oj.Mass;
                                Sun.Direction = impulse / Sun.Mass;
                                Objects.Remove(oj);
                                break;
                            }
                            else
                            {
                                if (Objects[j] == Sun) // zweites Objekt ist die Hauptsonne - Hauptsonne soll bleiben
                                {
                                    Sun.Mass += oi.Mass;
                                    Sun.Direction = impulse / Sun.Mass;
                                    Objects.Remove(oi);
                                    break;
                                }
                                else // Kein Objekt ist die Hauptsonne.
                                {
                                    if (oi.Mass > oj.Mass)
                                    {
                                        oi.Mass += oj.Mass;
                                        oi.Direction = impulse / oi.Mass;
                                        Objects.Remove(oj);
                                        break;
                                    }
                                    else
                                    {
                                        oj.Mass += oi.Mass;
                                        oj.Direction = impulse / oj.Mass;
                                        Objects.Remove(oi);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch
            { }

            try // Wegen Threading kommt es gern zu Fehlern beim Entfernen von Objekten. Das wird hier abgefangen.
            {   // ist unsauber und langsamer, ich weiß.
                #region zu weit von der Hauptsonne entfernte Objekte entfernen
                for (int i = Objects.Count - 1; i >= 0; i--)
                {
                    o2Object o = Objects[i];
                    distance = Point3.Distance(Sun.Position, o.Position);
                    if (distance > 1000000)
                    {
                        Objects.Remove(o);
                    }
                }
                #endregion
            }
            catch (Exception)
            { }
            
            // Zum Test
            if (centered)
            {
                ResetSun();
            }
   
        }

        #region Private ObjectAddMethoden

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
        /// <param name="_Sun">Zentralkörper</param>
        /// <param name="isSun">ist Zentralkörper eine Sonne?</param>
        /// <param name="SunIsParent">ist Zentralkörper die Hauptsonne?</param>
        /// <returns>zufällig generierten Wert abhängig von Masse und Art des Zentralkörpers</returns>
        private float Calc(o2Object _Sun, bool isSun, bool SunIsParent)
        {
            // return _Sun.Size() / 2 + (float)rnd.NextDouble() * fak * Sun.Mass / initSunSize * 10000;

            if (!isSun)
            {
                if (!SunIsParent)
                {
                    return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 5;
                }
                else
                {
                    return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 20;
                }
            }
            else
            {
                return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 35;
            }
        }

        /// <summary>
        /// eine 90° Rotationsmatrix um die z-Achse
        /// </summary>
        /// <returns></returns>
        private Matrix RotMatrix90z()
        {
            Point3 ZAxis = new Point3(0f, 0f, 1f);
            Matrix rot = new Matrix();
            rot.RotMatrix(90, ZAxis);
            return rot;
        }

        /// <summary>
        /// Zufällige Position bezogen auf ein Zentralobjekt
        /// </summary>
        /// <param name="_Sun">Zentralobjekt</param>
        /// <param name="isSun">Ist neues Objekt eine Sonne?</param>
        /// <param name="SunIsParent">Ist die Hauptsonne das Zentralobjekt?</param>
        /// <returns>beliebiger Vektor zu einer Position in der Nähe des Zentralobjektes</returns>
        private Point3 RndPosToSun(o2Object _Sun, bool isSun, bool SunIsParent)
        {
            Point3 result = new Point3();
            result.x = _Sun.Position.x + Calc(_Sun, isSun, SunIsParent);
            result.y = _Sun.Position.y + Calc(_Sun, isSun, SunIsParent);
            result.z = _Sun.Position.z + fak() * (float)rnd.NextDouble() * 100f; ;
            return result;
        }

        /// <summary>
        /// Eine um die Hauptsonne kreisende Sonne wird erstellt. Diese kann auch größer sein, was sie aber nicht zur Hauptsonne macht.
        /// </summary>
        private void AddSun()
        {
            if (suncount < _Light.Length - 1)
            {
                float Massfak = (float)rnd.Next(1, 3) * 5 * PlanetAdvanced2.initSunMass;
                float Mass = (float)rnd.NextDouble() * Massfak;
                Point3 Pos = RndPosToSun(Sun, true,false); // Zufällige Position bezgl der Sonne
                
                Point3 Dir = MM.MatDotPoint(RotMatrix90z(), Pos - Sun.Position); // Richtung 90Grad zum SonnenVektor
                
                Dir.Normalize(); // BewegungsRichtung normalisieren.
                //Dir *= (float)((rnd.NextDouble() * 3) + 2f) * 40f; // Speed zurechnen. Kosmische Gesch. schlug hier immer fehl.

                float distance = Point3.Distance(Sun.Position, Pos);

                Dir *= (float)Math.Sqrt(G * Sun.Mass / distance);// *fak(); // 1. kosmische Geschwindigkeit (Rotation auf Kreisbahn).
                Dir += Sun.Direction; // Bewegung der Bezugssonne draufrechnen, damit Sich Planet mitbewegt.

                float[] Color = { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() };
                suncount++; // Sonnenzähler erhöhen. (wichtig für Lichtquellen);
                Objects.Add(new o2Sun(Pos, Dir, Mass, Color, _Light[suncount]));
            }
        }

        /// <summary>
        /// Fügt einem Zentralobjekt untergeordnete es umkreisende Objekte hinzu
        /// </summary>
        /// <param name="_ZentralObjekt">Dies ist das Zentralobjekt um welches die hinzugefügten kreisen sollen</param>
        /// <param name="cnt">Anzahl der das Zentralobjekt umkreisenden Objekte</param>
        private void AddPlanet(o2Object _ZentralObjekt, int cnt)
        {
            bool SunIsParent = _ZentralObjekt is o2Sun; // ist das Zentralobjekt eine Sonne?

            for (int i = 0; i < cnt; i++)
            {
                float Mass = 1f;
                if (SunIsParent) // Planet
                {
                    int ps = rnd.Next(28, 30);
                    Mass = (float)rnd.NextDouble() * (float)Math.Pow(10, ps) * UniFak; // Größe Abhängig von Basisgröße der Planeten
                }
                else // Mond.
                {                    
                    float f = rnd.Next(80,1000); // Faktor um wie viel der Mond kleiner sein soll, als der Körper um den er kreist.
                    Mass = _ZentralObjekt.Mass / f; // Erd-Mond ist ca. 1/81 der Erdmasse. Da er der Größte im Sonnensystem ist, nehm ich das Verhältnis als max. an.
                }

                Point3 Pos = RndPosToSun(_ZentralObjekt, false, SunIsParent); // Position soll relativ zum Zentralkörper sein
                float distance = Point3.Distance(_ZentralObjekt.Position, Pos);

                Point3 Dir = MM.MatDotPoint(RotMatrix90z(), Pos - _ZentralObjekt.Position); // Richtung 90Grad zum Sonnenwinkel
                Dir.Normalize();
                Dir *= (float)Math.Sqrt(G * _ZentralObjekt.Mass / distance);// *fak(); // 1. kosmische Geschwindigkeit (Rotation auf Kreisbahn).
                Dir += Dir * ((float)rnd.NextDouble() / 10f); //* fak()); // ein wenig Varianz, um es interessannt zu machen.
                Dir += _ZentralObjekt.Direction; // Bewegung der Bezugssonne draufrechnen, damit Sich Planet mitbewegt.
                float[] Color = { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() }; // Farbe ist zufällig

                o2Object p = new o2Object(Pos, Dir, Mass, Color,null);
                
                Objects.Add(p);

                if (SunIsParent) 
                {
                    int c = rnd.Next(0,4);
                    AddPlanet(p, c);
                }
            }
        }

        /// <summary>
        /// Setzt die Position und Bewegungsrichtung der Hauptsonne zurück
        /// </summary>
        private void ResetSun()
        {
            Point3 _dir = Sun.Direction;
            Point3 _pos = Sun.Position;

            foreach (o2Object o in Objects)
            {
                o.Direction -= _dir;
                o.Position -= _pos;
            }
        }

        /// <summary>
        /// eine von aussen auf die Hauptsonne zufliegende Sonne mit Planeten wird erstellt
        /// </summary>
        private void AddComingSun()
        {
            if (suncount < _Light.Length - 1)
            {
                float Massfak = (float)rnd.Next(1, 3) * 5 * PlanetAdvanced2.initSunMass;
                float Mass = (float)rnd.NextDouble() * Massfak;

                Point3 Pos = RndPosToSun(Sun, true,true);
                Pos = (Pos - Sun.Position) * (float)(rnd.Next(10, 20)); // Position weiter von der Sonne entfernen.

                Point3 Dir = new Point3();
                Dir.assign(Sun.Position - Pos); // Bewegung auf die Sonne zu.

                Dir.x += (float)rnd.NextDouble(); // ein wenig Varianz in der Bewegung.
                Dir.y += (float)rnd.NextDouble(); // ein wenig Varianz in der Bewegung.
                Dir.z += (float)rnd.NextDouble(); // ein wenig Varianz in der Bewegung.

                Dir.Normalize(); // BewegungsRichtung normalisieren.

                Dir *= (float)((rnd.NextDouble() * 3) + 2f) * 10f; // Speed zurechnen;

                float[] Color = { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() };
                suncount++;
                o2Sun __Sun = new o2Sun(Pos, Dir, Mass, Color, _Light[suncount]);
                Objects.Add(__Sun);
                AddPlanet(__Sun, rnd.Next(1, 9)); // 1 bis 8 Planeten auf die Umlaufbahn der neuen Sonne bringen.
            }
        }

        #endregion

        /// <summary>
        /// Initialisiert die Sonne und Ihre Position und Laufbahn
        /// </summary>
        public override void Init()
        {
            Sun = new o2Sun();
            suncount = 0;
            Objects.Add(Sun);
        }

        /// <summary>
        /// Wird aufgerufen sobald eine Taste gedrückt wurde.
        /// </summary>        
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.O)
            {
                simulate = !simulate; // Simulation anhalten / starten
            }

            if (e.KeyCode == Keys.P && !e.Control)
            {
                Enabled = !Enabled; // Planetenprogramm darstellen / ausblenden
                Visible = Enabled;

                if (!Enabled)
                {
                    Objects.Clear(); // bei ausblenden, werden alle Objekte zerstört und System wird neu initialisiert.
                    Init();
                }
            }

            if (e.KeyCode == Keys.D5)
            {
                AddPlanet(Sun, 10); // 5 um die Sonne kreisende Planeten erzeugen
            }

            if (e.KeyCode == Keys.D4)
            {
                AddComingSun(); // Eine weitere auf die Sonne zufliegende Sonne sammt Planeten erzeugen
            }

            if (e.KeyCode == Keys.P && e.Control)
            {
                showTrace = !showTrace; // Planetenspur anzeigen
            }

            if (e.KeyCode == Keys.Add && !e.Control)
            {
                dt += 0.01f; // Simulationsschrittweite
            }

            if (e.KeyCode == Keys.Subtract && !e.Control)
            {                
                dt -= 0.01f; // Simulationsschrittweite
            }

            if (e.KeyCode == Keys.Add && e.Control)
            {
                dt += 0.001f; // Simulationsschrittweite
            }

            if (e.KeyCode == Keys.Subtract && e.Control)
            {
                dt -= 0.001f; // Simulationsschrittweite                
            }

            if (e.KeyCode == Keys.Delete)
            {
                ResetSun(); // Sonne auf Startpunkt zurücksetzen und Richtung reset
            }

            if (e.KeyCode == Keys.L)
            {
                lockSun = !lockSun; // Sonne wird im Ursprung gelockt und Richtung reset
                Sun.Direction = new Point3();
            }

            if (e.KeyCode == Keys.F7)
            {
                Sun.Mass *= 1.5f; // Masse der Sonne um hälfte erhöhen
            }
            if (e.KeyCode == Keys.F8)
            {
                Sun.Mass *= 0.66f; // Masse der Sonne um ein drittel verringern
            }
            if (e.KeyCode == Keys.Z) 
            {
                centered = !centered;
            }
        }

        /// <summary>
        /// Da in Thread ausgelagert ist, ist hier nichts nötig.
        /// </summary>
        protected override void InternalTick()
        {
            if (simulate)
            {
                ThreadTick();
            }
        }

        /// <summary>
        /// Hier nicht gebraucht
        /// </summary>
        public override void OnShutdown(){}

        /// <summary>
        /// Bewegungsdifferenz der X-Achse der Maus.
        /// </summary>
        public double mx { get; set; }

        /// <summary>
        /// Bewegungsdifferenz der Y-Achse der Maus.
        /// </summary>
        public double my { get; set; }

        /// <summary>
        /// Mousebuttonevent. Wird durchgereicht vom Mainform
        /// </summary>
        public void MouseButton(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Enabled)
            {
                AddPlanet(Sun,1);
            }
            if (e.Button == MouseButtons.Right && Enabled)
            {
                AddSun();
            }
        }

        /// <summary>
        /// Falls nach Mausaktion ein Update des Objektes nötig ist.
        /// </summary>
        public void updateObject(){}
    }
}