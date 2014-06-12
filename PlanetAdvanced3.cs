//
// Planetadvanced3.cs 
//	- Module for Mainform in 3DGLLab
//  - It simulate Materia in a closed System placed randomly at random size.
//  - With a "critical" mass of a Object it becomes a Sun lightning the system and all Materia in it.
//  - The Materia reacts on gravity of all other objects in the system due to their mass and distance.
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
    /// Planeten kreisen um ihre Sonne.
    /// </summary>
    public class PlanetAdvanced3 : BaseFigure, IMouseControlled
    {
        /// <summary>
        /// Die Größe der ersten Sonne. Dient auch als Basis-Faktor für neue Sonnen.
        /// </summary>
        public static float initSunMass = 2f * (float)Math.Pow(10, 31) * PlanetAdvanced2.UniFak;

        /// <summary>
        /// Anzahl der bisher existierenden Sonnen (auch gelöschte)
        /// </summary>
        private int suncount = 1;

        /// <summary>
        /// Soll Planetspur gezeigt werden?
        /// </summary>
        private bool showTrace = false;

        /// <summary>
        /// Soll Bewegung simuliert werden?
        /// </summary>
        private bool simulate = true;

        /// <summary>
        /// gibt an, ob die Sonne im Ursprung gelockt werden soll
        /// </summary>
        private bool centered = false;

        /// <summary>
        /// Sonne, falls entsteht.
        /// </summary>
        private o2Sun Sun;

        /// <summary>
        /// Member für Matrixoperationen
        /// </summary>
        private MatrixMath MM = new MatrixMath();

        /// <summary>
        /// Liste von Körpern im System
        /// </summary>
        private List<o2Object> Objects = new List<o2Object>();

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
                for (int i = 0; i < PlanetAdvanced2._Light.Length; i++)
                {
                    Gl.glDisable(PlanetAdvanced2._Light[i]);
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
                    o.DrawObject();
                    if (showTrace)
                    {
                        o.DrawTrace();
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
                    o.MoveObject(Objects);
                    if (showTrace)
                    {
                        o.Trace();
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
                                ConvertPlanet2Sun(oi);
                                break;
                            }
                            else
                            {
                                if (Objects[j] == Sun) // zweites Objekt ist die Hauptsonne - Hauptsonne soll bleiben
                                {
                                    Sun.Mass += oi.Mass;
                                    Sun.Direction = impulse / Sun.Mass;
                                    Objects.Remove(oi);
                                    ConvertPlanet2Sun(oj);
                                    break;
                                }
                                else // Kein Objekt ist die Hauptsonne.
                                {
                                    if (oi.Mass > oj.Mass)
                                    {
                                        oi.Mass += oj.Mass;
                                        oi.Direction = impulse / oi.Mass;
                                        Objects.Remove(oj);
                                        ConvertPlanet2Sun(oi);
                                        break;
                                    }
                                    else
                                    {
                                        oj.Mass += oi.Mass;
                                        oj.Direction = impulse / oj.Mass;
                                        Objects.Remove(oi);
                                        ConvertPlanet2Sun(oj);
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
                #region zu weit vom Ursprung entfernte Objekte entfernen
                for (int i = Objects.Count - 1; i >= 0; i--)
                {
                    o2Object o = Objects[i];
                    distance = Point3.Distance(new Point3(), o.Position);
                    if (distance > 1000000)
                    {
                        if (o == Sun) 
                        {
                            Sun.Direction = new Point3();
                            Sun.Position = new Point3();
                        }
                        else
                        {
                            Objects.Remove(o);
                        }
                    }
                }
                #endregion
            }
            catch (Exception)
            { }

            if (Sun != null && centered) 
            {
                resetSun();
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
        /// <returns>zufällig generierten Wert abhängig von Masse und Art des Zentralkörpers</returns>
        private float CalcDust()
        {
            float ss = (float)((Math.Pow(PlanetAdvanced3.initSunMass, (double)(1f / 3f)) * 3f) / (4f * Math.PI));
            return  ss / 2 + (float)rnd.NextDouble() * fak() * 55 * ss;
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
                    return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 10;
                }
                else
                {
                    return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 30;
                }
            }
            else
            {
                return _Sun.Size / 2 + (float)rnd.NextDouble() * fak() * _Sun.Size * 55;
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
        /// <returns>beliebiger Vektor zu einer Position in der Nähe des Zentralobjektes</returns>
        private Point3 RndDustPosToSun()
        {
            Point3 result = new Point3();
            result.x = CalcDust();
            result.y = CalcDust();
            result.z = 0f;
            // result.z = fak() * (float)rnd.NextDouble() * 100f;
            return result;
        }

        /// <summary>
        /// Zufällige Position bezogen auf ein Zentralobjekt
        /// </summary>
        /// <param name="_Sun">Zentralobjekt</param>
        /// <param name="isSun">Ist neues Objekt eine Sonne?</param>
        /// <param name="SunIsParent">Ist die Hauptsonne das Zentralobjekt?</param>
        /// <returns>beliebiger Vektor zu einer Position in der Nähe des Zentralobjektes</returns>
        private Point3 RndPosToSun(o2Object _Sun, bool isSun , bool SunIsParent)
        {
            Point3 result = new Point3();
            result.x = _Sun.Position.x + Calc(_Sun, isSun, SunIsParent);
            result.y = _Sun.Position.y + Calc(_Sun, isSun, SunIsParent);
            result.z = _Sun.Position.z + fak() * (float)rnd.NextDouble() * 100f;
            return result;
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
                    Mass = (float)rnd.NextDouble() * (float)Math.Pow(10, ps) * PlanetAdvanced2.UniFak; // Größe Abhängig von Basisgröße der Planeten
                }
                else // Mond.
                {
                    float f = rnd.Next(80, 1000); // Faktor um wie viel der Mond kleiner sein soll, als der Körper um den er kreist.
                    Mass = _ZentralObjekt.Mass / f; // Erd-Mond ist ca. 1/81 der Erdmasse. Da er der Größte im Sonnensystem ist, nehm ich das Verhältnis als max. an.
                }

                Point3 Pos = RndPosToSun(_ZentralObjekt, false, SunIsParent); // Position soll relativ zum Zentralkörper sein
                float distance = Point3.Distance(_ZentralObjekt.Position, Pos);

                Point3 Dir = MM.MatDotPoint(RotMatrix90z(), Pos - _ZentralObjekt.Position); // Richtung 90Grad zum Sonnenwinkel
                Dir.Normalize();
                Dir *= (float)Math.Sqrt(PlanetAdvanced2.G * _ZentralObjekt.Mass / distance);// *fak(); // 1. kosmische Geschwindigkeit (Rotation auf Kreisbahn).
                Dir += Dir * ((float)rnd.NextDouble() / 10f); //* fak()); // ein wenig Varianz, um es interessannt zu machen.
                Dir += _ZentralObjekt.Direction; // Bewegung der Bezugssonne draufrechnen, damit Sich Planet mitbewegt.
                float[] Color = { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() }; // Farbe ist zufällig

                o2Object p = new o2Object(Pos, Dir, Mass, Color,null);

                Objects.Add(p);

                if (SunIsParent)
                {
                    int c = rnd.Next(0, 4);
                    AddPlanet(p, c);
                }
            }
        }

        /// <summary>
        /// Fügt einem Zentralobjekt untergeordnete es umkreisende Objekte hinzu
        /// </summary>        
        /// <param name="cnt">Anzahl der das Zentralobjekt umkreisenden Objekte</param>
        private void AddDust(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                float Mass = 1f;                
                int ps = rnd.Next(28, 30);
                Mass = (float)rnd.NextDouble() * (float)Math.Pow(10, ps) * PlanetAdvanced2.UniFak; // Größe Abhängig von Basisgröße der Planeten
                Point3 Pos = RndDustPosToSun(); // Position soll relativ zum Ursprung sein
                Point3 Dir = new Point3();
                float[] Color = { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() }; // Farbe ist zufällig
                o2Object p = new o2Object(Pos, Dir, Mass, Color,null);                
                Objects.Add(p);                
            }
        }
        
        /// <summary>
        /// Ein Planet soll zu einer Sonne werden ab einer bestimmten Masse
        /// </summary>
        /// <param name="Planet2Convert">zu verändernder Planet</param>        
        private void ConvertPlanet2Sun(o2Object Planet2Convert)
        {
            if (
                    (suncount < PlanetAdvanced2._Light.Length - 1)
                    &&
                    (Planet2Convert.Mass > PlanetAdvanced3.initSunMass * 0.5))
            {                
                suncount++; // Sonnenzähler erhöhen. (wichtig wegen Lichtquellen);
                Sun = new o2Sun(Planet2Convert.Position, Planet2Convert.Direction, Planet2Convert.Mass, Planet2Convert.Farbe, PlanetAdvanced2._Light[suncount]);
                Objects.Add(Sun);
                Objects.Remove(Planet2Convert); // Convertierten Planeten löschen
            }            
        }

        #endregion

        /// <summary>
        /// Initialisiert die Sonne und Ihre Position und Laufbahn
        /// </summary>
        public override void Init()
        {
            PlanetAdvanced2.dt = 1f;
            Sun = null;
            Objects.Clear(); // bei ausblenden, werden alle Objekte zerstört und System wird neu initialisiert.
        }

        /// <summary>
        /// Setzt die Sonne in den Ursprung und versetzt die Planeten relativ dazu.
        /// </summary>
        private void resetSun() 
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
                    Init();
                }
            }

            if (e.KeyCode == Keys.D5)
            {
                AddDust(50); // 5 um die Sonne kreisende Planeten erzeugen
            }

            if (e.KeyCode == Keys.D1)
            {
                AddDust(100); // 100 Staubpatikel
            }

            if (e.KeyCode == Keys.D2)
            {
                AddDust(500); // 500 Staubpartikel
            }

            if (e.KeyCode == Keys.P && e.Control)
            {
                showTrace = !showTrace; // Planetenspur anzeigen
            }

            if (e.KeyCode == Keys.Add && !e.Control)
            {
                PlanetAdvanced2.dt += 0.01f; // Simulationsschrittweite
            }

            if (e.KeyCode == Keys.Subtract && !e.Control)
            {
                if (PlanetAdvanced2.dt > 0)
                {
                    PlanetAdvanced2.dt -= 0.01f; // Simulationsschrittweite
                }
            }

            if (e.KeyCode == Keys.Add && e.Control)
            {
                PlanetAdvanced2.dt += 0.001f; // Simulationsschrittweite
            }

            if (e.KeyCode == Keys.Subtract && e.Control)
            {
                if (PlanetAdvanced2.dt > 0)
                {
                    PlanetAdvanced2.dt -= 0.001f; // Simulationsschrittweite
                }
            }

            if (e.KeyCode == Keys.Back && !e.Control)
            {
                if (Sun != null)
                {
                    resetSun();
                }
            }
            if (e.KeyCode == Keys.Back && e.Control)
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
                if (Sun == null)
                {
                    AddDust(100);
                }
                else
                {                    
                    AddPlanet(Sun,1);
                }
            }
            if (e.Button == MouseButtons.Right && Enabled)
            {
                simulate = !simulate;
            }
            if (e.Button == MouseButtons.Middle && Enabled) 
            {
                if (Sun == null)
                {
                    AddDust(50);
                }
                else
                {
                    AddPlanet(Sun, 50);
                }
            }
        }

        /// <summary>
        /// Falls nach Mausaktion ein Update des Objektes nötig ist.
        /// </summary>
        public void updateObject(){}
    }
}