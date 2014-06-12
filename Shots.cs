using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using GL3DLab;
using System.Windows.Forms;

namespace GL3DLab
{
    /// <summary>
    /// Schüsse die sich linear durch den Raum bewegen
    /// </summary>
    class Shot
    {
        /// <summary>
        /// Richtung des Schusses
        /// </summary>
        private Point3 _Dir = new Point3(0f, 0f, 0f);

        /// <summary>
        /// Rotanteil in der Farbe
        /// </summary>
        private float _Red = 0;

        /// <summary>
        /// Grünanteil in der Farbe
        /// </summary>
        private float _Green = 0;

        /// <summary>
        /// Blauanteil in der Farbe
        /// </summary>
        private float _Blue = 0;

        /// <summary>
        /// Geschwindigkeit des Schusses
        /// </summary>
        private float _Speed = 0;

        /// <summary>
        /// Erstellungszeit für den Schuss
        /// </summary>
        private DateTime _Birthtime;

        /// <summary>
        /// Position des Schusses im Raum
        /// </summary>
        public Point3 _Pos = new Point3(0f, 0f, 0f);

        /// <summary>
        /// Ausgangsposition des Schusses im Raum
        /// </summary>
        public Point3 __InitPos = new Point3(0f, 0f, 0f);

        /// <summary>
        /// Hat schuss etwas getroffen wird er automatisch entfernt.
        /// </summary>
        public bool _hit = false;

        /// <summary>
        /// Ausrichtung um X-Achse
        /// </summary>
        private float _AngleX = 0;

        /// <summary>
        /// Ausrichtung um Z-Achse
        /// </summary>
        private float _AngleZ = 0;

        /// <summary>
        /// Matrizen Modul
        /// </summary>
        MatrixMath MM = new MatrixMath();

        /// <summary>
        /// Größe des Schusses
        /// </summary>
        public float Size = 0.01f;

        /// <summary>
        /// Constructor für Schuss
        /// </summary>
        /// <param name="cam">Kameraobjekt dient zur ermittlung der Position und Ausrichtung</param>
        /// <param name="Dir">Richtung in die der Schuss fliegen soll (wird intern normiert)</param>
        /// <param name="Red">Rotanteil in der Farbe</param>
        /// <param name="Green">Grünanteil in der Farbe</param>
        /// <param name="Blue">Blauanteil in der Farbe</param>
        /// <param name="Speed">Bewegungsgeschwindigkeit</param>
        public Shot(Camera cam, Point3 Dir, float Red, float Green, float Blue, float Speed)
        {
            // Richtung gleich normieren
            float n = (float)Math.Sqrt(Math.Pow(Dir.x, 2) + Math.Pow(Dir.y, 2) + Math.Pow(Dir.z, 2));
            _Dir.x = Dir.x / n;
            _Dir.y = Dir.y / n;
            _Dir.z = Dir.z / n;

            _Pos.x = __InitPos.x = -cam.X;
            _Pos.y = __InitPos.y = -cam.Y;
            _Pos.z = __InitPos.z = -cam.Z;

            _AngleX = cam.angleX;
            _AngleZ = cam.angleZ;

            _Red = Red;
            _Green = Green;
            _Blue = Blue;
            _Speed = Speed;
            _Birthtime = DateTime.Now;
        }

        /// <summary>
        /// Schuss wird gezeichnet
        /// </summary>
        /// <param name="cam">Kameraobjekt</param>
        public void Draw(Camera cam)
        {
            float X = _Pos.x;
            float Y = _Pos.y;
            float Z = _Pos.z;
            float DX = _Dir.x;
            float DY = _Dir.y;
            float DZ = _Dir.z;
            float dx = Size;
            float dy = Size * 3;
            float dz = Size;

            Gl.glPushMatrix();

            Gl.glTranslatef(X, Y, Z);


            Gl.glRotatef(-_AngleZ, 0, 0, 1);
            Gl.glRotatef(-_AngleX - 90, 1, 0, 0);

            X = 0;
            Y = 0;
            Z = 0;
            Gl.glEnable(Gl.GL_CULL_FACE_MODE);
            Gl.glColor3f(_Red, _Green, _Blue);

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X, Y, Z);
            Gl.glVertex3f(X + dx, Y - dy, Z - dz);
            Gl.glVertex3f(X + dx, Y + dy, Z - dz);
            Gl.glVertex3f(X - dx, Y + dy, Z - dz);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X - dx, Y - dy, Z - dz);
            Gl.glVertex3f(X + dx, Y - dy, Z - dz);
            Gl.glVertex3f(X + dx, Y - dy, Z + dz);
            Gl.glVertex3f(X - dx, Y - dy, Z + dz);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X - dx, Y - dy, Z - dz);
            Gl.glVertex3f(X - dx, Y + dy, Z - dz);
            Gl.glVertex3f(X - dx, Y + dy, Z + dz);
            Gl.glVertex3f(X - dx, Y - dy, Z + dz);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X + dx, Y + dy, Z + dz);
            Gl.glVertex3f(X + dx, Y + dy, Z - dz);
            Gl.glVertex3f(X + dx, Y - dy, Z - dz);
            Gl.glVertex3f(X + dx, Y - dy, Z + dz);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X + dx, Y + dy, Z + dz);
            Gl.glVertex3f(X + dx, Y + dy, Z - dz);
            Gl.glVertex3f(X - dx, Y + dy, Z - dz);
            Gl.glVertex3f(X - dx, Y + dy, Z + dz);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glVertex3f(X + dx, Y + dy, Z + dz);
            Gl.glVertex3f(X - dx, Y + dy, Z + dz);
            Gl.glVertex3f(X - dx, Y - dy, Z + dz);
            Gl.glVertex3f(X + dx, Y - dy, Z + dz);
            Gl.glEnd();

            Gl.glPopMatrix();
        }

        /// <summary>
        /// Bewegung des Schusses, abhängig vom _Speed
        /// </summary>
        public void Move()
        {
            //TimeSpan __life = DateTime.Now - _Birthtime;
            //int __Seconds = __life.Seconds;
            //int __Millis = __life.Milliseconds;
            //float __timeFactor = (__Millis + __Seconds * 1000f) / 10f;

            //_Pos.x = __InitPos.x + _Dir.x * _Speed * __timeFactor;
            //_Pos.y = __InitPos.y + _Dir.y * _Speed * __timeFactor;
            //_Pos.z = __InitPos.z + _Dir.z * _Speed * __timeFactor;

            _Pos.x += _Dir.x * _Speed;
            _Pos.y += _Dir.y * _Speed;
            _Pos.z += _Dir.z * _Speed;
        }

        /// <summary>
        /// Prüft Abstand des Schusses zu seiner Quelle. bei > 500 wird shot als Old betrachtet.
        /// </summary>
        /// <returns></returns>
        public bool IsOld()
        {
            float n = (float)Math.Sqrt(Math.Pow(_Pos.x - __InitPos.x, 2) + Math.Pow(_Pos.y - __InitPos.y, 2) + Math.Pow(_Pos.z - __InitPos.z, 2));
            return (n - 500) > 0;

            //TimeSpan __life = DateTime.Now - _Birthtime;
            //return __life.Seconds > 5 || _hit;
        }
    }

    /// <summary>
    /// Verwaltungsklasse für die Schüsse
    /// </summary>
    class Shots : BaseFigure, IMouseControlled
    {
        /// <summary>
        /// Liste enthält alle Schüsse (to be outsourced)
        /// </summary>
        private List<Shot> _Shots = new List<Shot>();

        /// <summary>
        /// Liste enthält Schüsse die veraltet sind und zerstört werden können
        /// </summary>
        private List<Shot> _ShotsDel = new List<Shot>();

        // private List<Point3> _PointDel = new List<Point3>();

        /// <summary>
        /// Matrizenmodul
        /// </summary>
        MatrixMath MMAth;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aMMAth">Matrizenmodul</param>
        public Shots(MatrixMath aMMAth)
        {
            MMAth = aMMAth;
            Visible = true; // immer sichtbar
            Enabled = true;
        }
        
        /// <summary>
        /// interne Zeichenfunktion
        /// </summary>
        protected override void InternalDraw()
        {
            try
            {
                if (_Shots.Count > 0)
                {
                    foreach (Shot s in _Shots.ToArray()) // ToArray sorgt für Threadsicherheit des Enummerators
                    {
                        s.Draw(Cam);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// interne Simulationsmethode für die Schüsse
        /// </summary>
        protected override void InternalTick()
        {
            // Schusssimulation
            if (_Shots.Count > 0)
            {
                foreach (Shot s in _Shots)
                {
                    if (s.IsOld())
                    {
                        _ShotsDel.Add(s);
                    }
                    else
                    {
                        s.Move();
                    }
                }

                ///// Kollisionskontrolle
                //* Vergleich über Radien und Abstand der Zentren ist schön schnell.
                // * Da sehr viele Partikel existieren können, ist Geschwindigkeit hier wichtig.
                // * leider wird bei großen Partikelmengen die Berechnung ungenau, da die Position
                // * der Schüsse unregelmäßiger aktualisiert wird.*/

                //foreach (Point3 p in pfld.m_lPointField.ToArray()) // ToArray sorgt für Threadsicherheit des Enummerators
                //{
                //    foreach (Shot s in Shots.ToArray()) // ToArray sorgt für Threadsicherheit des Enummerators
                //    {
                //        // AbstandZentren = sqrt((x1-x2)^2+(y1-y1)^2+(z1-z2)^2)
                //        float CenterDistance = (float)Math.Sqrt(Math.Pow(p.x - s._Pos.x, 2)
                //            + Math.Pow(p.y - s._Pos.y, 2) + Math.Pow(p.z - s._Pos.z, 2));
                //        // rGes = 3*r1^2 + 3*r2^2
                //        float LengthRadiants = (float)Math.Sqrt(3 * Math.Pow(pfld.delta, 2)
                //            + 3 * Math.Pow(s.Size, 2));// Mal 3 da Würfel vom Zentrum bis zu Ecken                        
                //        if (CenterDistance < LengthRadiants)
                //        {
                //            //_ShotsDel.Add(s);
                //            _PointDel.Add(p);
                //        }
                //    }
                //}
                //
                //lock (pfld)
                //{
                //    foreach (Point3 p in _PointDel)
                //    {
                //        pfld.m_lPointField.Remove(p);
                //    }
                //    _PointDel.Clear();
                //}

                foreach (Shot s in _ShotsDel)
                {
                    _Shots.Remove(s);
                }
                _ShotsDel.Clear();
            }
        }
        
        /// <summary>
        /// Initialisierung für die Schüsse
        /// </summary>
        public override void Init()
        {
            // nicht nötig
        }

        /// <summary>
        /// Event bei Tastendruck
        /// </summary>
        /// <param name="e">Key Event Argument</param>
        public override void KeyPressed(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V)
            {
                Enabled = !Enabled;
            }
        }
        
        public double mx { get; set; } // nicht nötig
        public double my { get; set; } // nicht nötig

        /// <summary>
        /// Tastendruck auf Maus
        /// </summary>
        /// <param name="e">Mouse Event Argument</param>
        public void MouseButton(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point3 pos = new Point3(-Cam.X, -Cam.Y, -Cam.Z + 0.1f);
                Point3 look = new Point3(1, 0, 0);
                Point3 zAxes = new Point3(0, 0, 1);
                Point3 yAxes = new Point3(0, 1, 0);
                Matrix rot1 = new Matrix();
                Matrix rot2 = new Matrix();
                rot1.RotMatrix(Cam.angleZ + 90, zAxes);
                rot2.RotMatrix(Cam.angleX - 90, yAxes);
                Matrix rotRes = MMAth.MatDotMat(rot1, rot2);
                look = MMAth.MatDotPoint(rotRes, look);
                _Shots.Add(new Shot(Cam, look, 1f, 0.0f, 0.0f, 0.5f));
            }
        }

        /// <summary>
        /// Falls Objekt nach Mausdruck oder Tastendruck geupdatet werden muss
        /// </summary>
        public void updateObject()
        {
            // nicht nötig
        }

        /// <summary>
        /// Wird beim Beenden des Programms aufgerufen.
        /// </summary>
        public override void OnShutdown()
        {
        }
    }
}