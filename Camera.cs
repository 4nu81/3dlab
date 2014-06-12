using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Drawing;

namespace GL3DLab
{
    /// <summary>
    /// Das Kameraobjekt verwaltet den Blickwinkel und die Position des "Betrachters" 
    /// </summary>
    public class Camera
    {
        // Position
        /// <summary>
        /// Die X-Komponente
        /// </summary>
        public float X = 0;
        /// <summary>
        /// Die Y-Komponente
        /// </summary>
        public float Y = 0;
        /// <summary>
        /// Die Z-Komponente
        /// </summary>
        public float Z = 0;
        // Blickwinkel
        /// <summary>
        /// Der Blickwinkel auf der X-Y-Ebene
        /// </summary>
        public float angleZ = 0;
        /// <summary>
        /// Der Blickwinkel in die Höhe
        /// </summary>
        public float angleX = 0;
        /// <summary>
        /// Der Blickwinkel wird um mx und mz Grad verändert.
        /// </summary>
        /// <param name="mz">Blickwinkel auf X-Y-Ebene wird um mz Grad verändert.</param>
        /// <param name="mx">Blickwinkel nach "Oben" wird um mx Grad verändert.</param>
        public void SetNewCameraPosition(double mz, double mx)
        {
            angleZ += (float)mz / 2;
            if (angleZ > 360)
            {
                angleZ -= 360;
            }
            if (angleZ < 0)
            {
                angleZ += 360;
            }
            angleX -= (float)mx / 2;
            if (angleX > 180)
            {
                angleX = 180;
            }
            if (angleX < 0)
            {
                angleX = 0;
            }
        }
        /// <summary>
        /// Cameraposition wird im OpenGl positioniert
        /// </summary>
        public void SetCamera()
        {
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            // Blickrichtung        
            Gl.glRotatef(angleX, 1, 0, 0);
            Gl.glRotatef(angleZ, 0, 0, 1);
            //Position
            Gl.glTranslatef(X, Y, Z);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }
    }

    /// <summary>
    /// Die Skybox umgibt die Kamera mit einer nicht Z-Gepufferten Textur.
    /// </summary>
    class Skybox 
    {        
        /// <summary>
        /// Soll Skybox gezeichnet werden?
        /// </summary>
        public bool DoDrawSkybox { get; set; }

        /// <summary>
        /// Enthält Indizes für die Texturen
        /// </summary>
        private uint[] _skybox = new uint[6];

        /// <summary>
        /// Die einzelnen Textruren werden hier geladen.
        /// </summary>
        /// <param name="filename">Dateiname der Textur</param>
        /// <param name="nr">Index der Textur</param>
        private void InternalInitTextur(string filename, int nr)
        {
            Bitmap image = new Bitmap(filename);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            System.Drawing.Imaging.BitmapData bitmapdata;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            bitmapdata = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[nr]);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, (int)Gl.GL_RGB8, image.Width, image.Height,
                0, Gl.GL_BGR_EXT, Gl.GL_UNSIGNED_BYTE, bitmapdata.Scan0);

            image.UnlockBits(bitmapdata);
            image.Dispose();
        }

        /// <summary>
        /// Texturen werden den Indizes zugeordnet und geladen
        /// </summary>
        public void InitGLTexture()
        {            
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.5f);					// black background
            Gl.glClearDepth(1.0f);										// depth buffer setup
            Gl.glDepthFunc(Gl.GL_LEQUAL);								// type of depth testing
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);	// nice perspective calculations

            Gl.glGenTextures(6, _skybox);
            InternalInitTextur("RT.jpg", 0);
            InternalInitTextur("DN.jpg", 1);
            InternalInitTextur("UP.jpg", 2);
            InternalInitTextur("FR.jpg", 3);
            InternalInitTextur("LF.jpg", 4);
            InternalInitTextur("BK.jpg", 5);

            DoDrawSkybox = true;
        }

        /// <summary>
        /// Zeichenroutine für die Skybox
        /// </summary>
        /// <param name="c">Das Kameraobjekt um Ausrichtung im Raum zu ermitteln.</param>
        public void DrawSky(Camera c)
        {
            if (DoDrawSkybox)
            {
                float dist = 0.5f;

                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();
                Gl.glPushAttrib(Gl.GL_ENABLE_BIT);

                Gl.glEnable(Gl.GL_TEXTURE_2D);
                Gl.glDisable(Gl.GL_DEPTH_TEST);
                Gl.glDisable(Gl.GL_LIGHTING);
                Gl.glDisable(Gl.GL_BLEND);

                Gl.glRotatef(c.angleX, 1.0f, 0.0f, 0.0f);			// rotate on the X-axis                        
                Gl.glRotatef(c.angleZ, 0.0f, 0.0f, 1.0f);			// rotate on the Z-axis            
                Gl.glColor4f(1f, 1f, 1f, 1f);

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[1]);
                Gl.glBegin(Gl.GL_QUADS);
                // Unten
                Gl.glTexCoord2f(1.0f, 1.0f);			// top right of texture
                Gl.glVertex3f(dist, dist, dist);		// top right of quad
                Gl.glTexCoord2f(0.0f, 1.0f);			// top left of texture
                Gl.glVertex3f(-dist, dist, dist);		// top left of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// bottom left of texture
                Gl.glVertex3f(-dist, -dist, dist);	    // bottom left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom right of texture
                Gl.glVertex3f(dist, -dist, dist);		// bottom right of quad
                Gl.glEnd();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[2]);
                Gl.glBegin(Gl.GL_QUADS);
                // Ground
                Gl.glTexCoord2f(1.0f, 1.0f);			// top right of texture
                Gl.glVertex3f(-dist, dist, -dist);	// top right of quad
                Gl.glTexCoord2f(0.0f, 1.0f);			// top left of texture
                Gl.glVertex3f(dist, dist, -dist);		// top left of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// bottom left of texture
                Gl.glVertex3f(dist, -dist, -dist);	// bottom left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom right of texture
                Gl.glVertex3f(-dist, -dist, -dist);	// bottom right of quad
                Gl.glEnd();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[3]);
                Gl.glBegin(Gl.GL_QUADS);
                // Up
                Gl.glTexCoord2f(1.0f, 1.0f);			// top right of texture
                Gl.glVertex3f(dist, dist, -dist);		// top right of quad
                Gl.glTexCoord2f(0.0f, 1.0f);			// top left of texture
                Gl.glVertex3f(-dist, dist, -dist);	// top left of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// bottom left of texture
                Gl.glVertex3f(-dist, dist, dist);		// bottom left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom right of texture
                Gl.glVertex3f(dist, dist, dist);		// bottom right of quad
                Gl.glEnd();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[4]);
                Gl.glBegin(Gl.GL_QUADS);
                // Right Face
                Gl.glTexCoord2f(1.0f, 1.0f);			// top right of texture
                Gl.glVertex3f(dist, dist, -dist);		// top right of quad
                Gl.glTexCoord2f(0.0f, 1.0f);			// top left of texture
                Gl.glVertex3f(dist, dist, dist);		// top left of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// bottom left of texture
                Gl.glVertex3f(dist, -dist, dist);		// bottom left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom right of texture
                Gl.glVertex3f(dist, -dist, -dist);	// bottom right of quad
                Gl.glEnd();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[5]);
                Gl.glBegin(Gl.GL_QUADS);
                // Left Face
                Gl.glTexCoord2f(0.0f, 1.0f);			// top right of texture            			
                Gl.glVertex3f(dist, -dist, dist);		// top right of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// top left of texture            
                Gl.glVertex3f(-dist, -dist, dist);	    // top left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom left of texture            
                Gl.glVertex3f(-dist, -dist, -dist);	    // bottom left of quad
                Gl.glTexCoord2f(1.0f, 1.0f);			// bottom right of texture            
                Gl.glVertex3f(dist, -dist, -dist);	    // bottom right of quad            
                Gl.glEnd();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, _skybox[0]);
                Gl.glBegin(Gl.GL_QUADS);
                // Left Face
                Gl.glTexCoord2f(1.0f, 1.0f);			// top right of texture
                Gl.glVertex3f(-dist, dist, dist);		// top right of quad
                Gl.glTexCoord2f(0.0f, 1.0f);			// top left of texture
                Gl.glVertex3f(-dist, dist, -dist);	// top left of quad
                Gl.glTexCoord2f(0.0f, 0.0f);			// bottom left of texture
                Gl.glVertex3f(-dist, -dist, -dist);	// bottom left of quad
                Gl.glTexCoord2f(1.0f, 0.0f);			// bottom right of texture
                Gl.glVertex3f(-dist, -dist, dist);	// bottom right of quad                        
                Gl.glEnd();

                Gl.glPopAttrib();
                Gl.glPopMatrix();
            }
        }
    }
}
