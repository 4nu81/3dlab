using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Windows.Forms;

namespace GL3DLab
{
    /// <summary>
    /// Stellt eine Tangentialebene auf einer Funktion im R³ dar.
    /// </summary>
    class TangentialFunctions : BaseFigure, IMouseControlled
    {
        /// <summary>
        /// Wird Funktion gezeichnet
        /// </summary>
        private bool paintfunction = true;

        /// <summary>
        /// Wird Tangentialebene gezeichnet?
        /// </summary>
        private bool paintfunctiontangent = true;
        
        #region Function
        /// <summary>
        /// niedrigster Funktionswert
        /// </summary>
        private float fmin = -10;

        /// <summary>
        /// höchster Funktionswert
        /// </summary>
        private float fmax = 10;

        /// <summary>
        /// Schrittweite der Funktionsargumente
        /// </summary>
        private double step = 0.2f;

        /// <summary>
        /// niedrigster Funktionswert
        /// </summary>
        private float ymin = -50;
        /// <summary>
        /// höchster Funktionswert
        /// </summary>
        private float ymax = 50;

        /// <summary>
        /// x Komponente für Aufhängepunkt der Tangentialebene
        /// </summary>
        private float x0 = 0;

        /// <summary>
        /// y Komponente für Aufhängepunkt der Tangentialebene
        /// </summary>
        private float y0 = 0;

        /// <summary>
        /// z Komponente für Aufhängepunkt der Tangentialebene
        /// </summary>
        private float z0 = 0;

        /// <summary>
        /// Enthält die Punkte der Funktion
        /// </summary>
        private List<Square> m_lPointsFunction = new List<Square>();
        private List<Square> m_lPointsFunctionLines = new List<Square>();

        /// <summary>
        /// Enthält die Punkte der Tangentialfunktion
        /// </summary>
        private List<Square> m_lPointsFunctionTangent = new List<Square>();

        /// <summary>
        /// Funktionsgleichung f(x,y)
        /// </summary>
        private double solvef(double x, double y)
        {
            //ymin = -2;
            //ymax = 2;
            //return Math.Sin(y) + Math.Sin(x);

            ymin = fmin;
            ymax = fmax;
            return (x * Math.Sin(y) + y * Math.Sin(x));

            //ymin = -1;
            //ymax = 1;
            //return Math.Sin(x);
        }

        /// <summary>
        /// partielle Ableitung der Funktion nach x f'x(x,y)
        /// </summary>
        private double solvefx(double x, double y)
        {
            //return Math.Cos(x);
            return Math.Sin(y) + y * Math.Cos(x);
            //return Math.Cos(x);
        }

        /// <summary>
        /// partielle Ableitung der Funktion nach y f'x(x,y)
        /// </summary>
        private double solvefy(double x, double y)
        {
            //return Math.Cos(y);
            return Math.Sin(x) + x * Math.Cos(y);
            //return 0;
        }

        /// <summary>
        /// Gleichung für die Tangentialebene
        /// f'x(x0,y0)*(x-x0)+f'y(x0,y0)*(y-y0)+f(x0,y0)
        /// </summary>
        private double tangentPlane(double x, double y)
        {
            double f = solvef(x0, y0);
            double fx = solvefx(x0, y0);
            double fy = solvefy(x0, y0);

            return fx * (x - x0) + fy * (y - y0) + f;
        }
        #endregion

        #region Internal Function Methodes
        /// <summary>
        /// Initialisiertung Tangentialebene in P=(x0,y0,z0)
        /// </summary>
        private void initFunctionTangent()
        {
            double dif = 4f;

            m_lPointsFunctionTangent.Clear();

            double x = x0 - dif; // links unten
            double y = y0 - dif;
            double z = tangentPlane(x, y);
            Point3 lu = new Point3((float)x, (float)y, (float)z);

            x = x0 + dif; // rechts unten
            y = y0 - dif;
            z = tangentPlane(x, y);
            Point3 ru = new Point3((float)(x), (float)y, (float)z);

            x = x0 - dif; // links oben
            y = y0 + dif;
            z = tangentPlane(x, y);
            Point3 lo = new Point3((float)(x), (float)(y), (float)z);
            // rechts oben
            Point3 ro = new Point3((float)(x), (float)(y), (float)z);

            Point3 lr = new Point3(lu.x - ru.x, lu.y - ru.y, lu.z - ru.z);
            Point3 ou = new Point3(lo.x - lu.x, lo.y - lu.y, lo.z - lu.z);
            lr.Normalize();
            ou.Normalize();
            z0 = (float)solvef(x0, y0);

            Point3 X0 = new Point3(x0, y0, z0);

            lr *= dif / 2;
            ou *= dif / 2;

            lu.assign(X0);
            lu -= lr;
            lu -= ou;

            lo.assign(X0);
            lo -= lr;
            lo += ou;

            ro.assign(X0);
            ro += lr;
            ro += ou;

            ru.assign(X0);
            ru += lr;
            ru -= ou;

            m_lPointsFunctionTangent.Add(new Square(lu, lo, ro, ru));
        }

        /// <summary>
        /// Initialisierung der Funktion
        /// </summary>
        private void initFunction()
        {
            double _xmin = fmin;
            double _xmax = fmax;
            double _ymin = fmin;
            double _ymax = fmax;
            int ix = -1;
            int iy = -1;

            m_lPointsFunction.Clear();
            m_lPointsFunctionLines.Clear();

            for (double x = _xmin; x <= _xmax; x += step)
            {
                ix += 1;
                for (double y = _ymin; y <= _ymax; y += step)
                {
                    iy += 1;
                    double z = solvef(x, y);
                    Point3 a = new Point3((float)x, (float)y, (float)z);
                    z = solvef(x + step, y);
                    Point3 b = new Point3((float)(x + step), (float)y, (float)z);
                    z = solvef(x, y + step);
                    Point3 c = new Point3((float)(x), (float)(y + step), (float)z);
                    z = solvef(x + step, y + step);
                    Point3 d = new Point3((float)(x + step), (float)(y + step), (float)z);

                    m_lPointsFunction.Add(new Square(a, c, d, b));
                    if (ix % 5 == 0 && iy % 5 == 0)
                    {
                        m_lPointsFunctionLines.Add(new Square(a, c, d, b));
                    }
                }
            }
        }

        /// <summary>
        /// Zeichenroutine für die Funktion
        /// </summary>
        private void DrawFunction()
        {
            Gl.glMatrixMode(Gl.GL_TEXTURE_MATRIX);
            Gl.glPushMatrix();
            for (int i = 0; i < m_lPointsFunction.Count; i++)
            {
                Square p = m_lPointsFunction.ElementAt(i);
                float col = (ymax - p.a.z) / (ymax - ymin);
                Gl.glColor3f(1 - col, col, 0f);
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                //Gl.glMateriali(Gl.GL_FRONT_AND_BACK, Gl.GL_SHININESS, 60);
                Gl.glBegin(Gl.GL_POLYGON);
                {
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.a.x, p.a.y, p.a.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.b.x, p.b.y, p.b.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.c.x, p.c.y, p.c.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.d.x, p.d.y, p.d.z);
                }
                Gl.glPopMatrix();
                Gl.glEnd();
            }
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW_MATRIX);
        }

        /// <summary>
        /// Zeichenroutine für die Tangentialebene
        /// </summary>
        private void DrawTangente()
        {
            Gl.glMatrixMode(Gl.GL_TEXTURE_MATRIX);
            for (int i = 0; i < m_lPointsFunctionTangent.Count; i++)
            {
                Gl.glEnable(Gl.GL_BLEND);
                Square p = m_lPointsFunctionTangent.ElementAt(i);
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                Gl.glColor4f(0f, 0f, 1f, 0.2f);
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                Gl.glBegin(Gl.GL_POLYGON);
                {
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.a.x, p.a.y, p.a.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.b.x, p.b.y, p.b.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.c.x, p.c.y, p.c.z);
                    Gl.glTexCoord3f(p.a.x, p.a.y, p.a.z); Gl.glVertex3f(p.d.x, p.d.y, p.d.z);
                }
                Gl.glPopMatrix();
                Gl.glEnd();
                Gl.glDisable(Gl.GL_BLEND);
                Gl.glBegin(Gl.GL_LINES);
                {
                    Gl.glColor3f(0.5f, 0.5f, 1f);
                    Gl.glVertex3f(x0, y0, z0 + 10);
                    Gl.glVertex3f(x0, y0, z0 - 10);
                }
                Gl.glEnd();
            }
            Gl.glMatrixMode(Gl.GL_MODELVIEW_MATRIX);
        }
        #endregion

        #region BaseFigures & IFigures
        /// <summary>
        /// Interne Zeichenfunktion aus der BaseFigure
        /// </summary>
        protected override void InternalDraw()
        {
            if (paintfunction) DrawFunction();
            if (paintfunctiontangent) DrawTangente();
        }

        /// <summary>
        /// Simulationsroutine: hier überflüssig
        /// </summary>
        protected override void InternalTick()
        {
            // nothing to do
        }

        /// <summary>
        /// Initialisierung aus der BaseFigure
        /// </summary>
        public override void Init()
        {
            initFunction();
            initFunctionTangent();
            Cam.X = 0f;
            Cam.Y = 00f;
            Cam.Z = 100f;
        }

        /// <summary>
        /// Tastendruckevent
        /// </summary>
        public override void KeyPressed(System.Windows.Forms.KeyEventArgs e)
        {
            //F Function darstellen
            if (e.KeyCode == Keys.F)
            { 
                paintfunction = !paintfunction;                
                Visible = paintfunction || paintfunctiontangent;
            }

            //H FunctionTangential darstellen
            if (e.KeyCode == Keys.H)
            { 
                paintfunctiontangent = !paintfunctiontangent;
                Visible = paintfunction || paintfunctiontangent;
            }
        }

        /// <summary>
        /// Wird beim Beenden des Programms ausgelöst.
        /// </summary>
        public override void OnShutdown()
        {
        }
        #endregion

        #region IMousefigures

        /// <summary>
        /// Bewegungsregistrierung der Maus-X-Achse
        /// </summary>
        public double mx
        {
            get
            {
                return 0;
            }
            set
            {
                x0 += (float)(value / Math.Abs(value) * 0.1);
            }
        }

        /// <summary>
        /// Bewegungsregistrierung der Maus-Y-Achse
        /// </summary>
        public double my
        {
            get
            {
                return 0;
            }
            set
            {
                y0 += (float)(value / Math.Abs(value) * 0.1);
            }
        }

        /// <summary>
        /// Registrierung der Maustastenbenutzung
        /// </summary>
        public void MouseButton(MouseEventArgs e)
        {
            // nothing to do
        }

        /// <summary>
        /// Falls nach einer Maussteuerung ein Objekt aktualisiert werden muss
        /// </summary>
        public void updateObject()
        {
            initFunctionTangent();
        }

        #endregion
    }
}

