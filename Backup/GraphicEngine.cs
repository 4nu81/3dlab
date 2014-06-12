using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;

namespace GL3DLab
{
    /// <summary>
    /// Die OpenGL Grafikengine
    /// </summary>
    public static class GraphicEngine
    {

        #region InitGl
        /// <summary>
        /// Erstinizialisierung
        /// GLControl.InitialiseContext muss auserhalb inizialisiert werden
        /// </summary>
        public static void InitGl()
        {
            // Gl.glShadeModel(Gl.GL_LINE_SMOOTH);//Antialiasing
            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);//Clearcolor

            Gl.glClearDepth(1.0f);// Depth Buffer Setup
            Gl.glEnable(Gl.GL_DEPTH_TEST);// Enables Depth Testing
            Gl.glDepthFunc(Gl.GL_LEQUAL);// The Type Of Depth Test To Do
            
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);// Really Nice Perspective Calculations
        }
        #endregion

        #region ReInitGl
        /// <summary>
        /// GL reinizialisieren (zb nach resize)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ReInitGl(int width, int height)
        {
            Gl.glViewport(0, 0, width, height);
            // Hier wird der Mittelpunkt auf den die Perspektive zuläuft 
            // zurückgesetzt.

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // Hier wird die Projektionsmatrix festgelegt

            Gl.glLoadIdentity();
            // und angepasst

            Glu.gluPerspective(45.0f, width / height, 0.1f, 1000000000.0f);
            // Hier wird die das Verhältnis der Höhe zur Breite übergeben
            // und der Verzerrungswinkel von 45 Grad übergeben

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            // Hier wird die sogenannte modelview-Matrix festgelegt

            Gl.glLoadIdentity();
            // und angepasst.
        }
        #endregion

        #region DrawDice
        /// <summary>
        /// Wuerfel zeichnen
        /// </summary>
        public static void DrawDice(float r , float g , float b)
        {
            DrawPlate(r, g, b);//Vorne
            Gl.glTranslatef(0, 0, 1);
            DrawPlate(r, g, b);//Hinten

            Gl.glRotatef(90, 0, 1, 0);

            DrawPlate(r, g, b);//Links
            Gl.glTranslatef(0, 0, 1);
            DrawPlate(r, g, b);//Rechts

            Gl.glRotatef(-90, 1, 0, 0);

            DrawPlate(r, g, b);//Oben
            Gl.glTranslatef(0, 0, 1);
            DrawPlate(r, g, b);//Unten

        }
        #endregion

        #region DrawPlate
        /// <summary>
        /// Platte zeichnen
        /// </summary>
        public static void DrawPlate(float r , float g , float b)
        {
            //Platte zeichnen
            Gl.glColor3f(r, g, b);
            Gl.glBegin(Gl.GL_POLYGON);
            {
                Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex3f(1.0f, 0.0f, 0.0f);
                Gl.glVertex3f(1.0f, 1.0f, 0.0f);
                Gl.glVertex3f(0.0f, 1.0f, 0.0f);
            }
            Gl.glEnd();

            //Aussenlinie zeichnen
            Gl.glColor3f(0, 0, 0);
            Gl.glBegin(Gl.GL_LINE_LOOP);
            {
                Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex3f(1.0f, 0.0f, 0.0f);
                Gl.glVertex3f(1.0f, 1.0f, 0.0f);
                Gl.glVertex3f(0.0f, 1.0f, 0.0f);
            }
            Gl.glEnd();
        }
        #endregion

        #region BeginPaint
        /// <summary>
        /// Wird am anfang der Paintphase aufgerufen
        /// </summary>
        public static void BeginPaint()
        {
            // Clear The Screen And The Depth Buffer
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            // Reset The View
            Gl.glLoadIdentity();
        }
        #endregion

        #region GL_ShortCut
        /// <summary>
        /// Gl.glPushMatrix
        /// </summary>
        private static void Push()
        {
            Gl.glPushMatrix();
        }

        /// <summary>
        /// Gl.glPopMatrix
        /// </summary>
        private static void Pop()
        {
            Gl.glPopMatrix();
        }
        #endregion
    }
}