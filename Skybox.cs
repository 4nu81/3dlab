using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Tao.OpenGl;

namespace GL_Depth
{
    class Skybox
    {
        int[] skybox;
        public void initBox()
        {            
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP);
        }

        public void renderBox(float angleX, float angleZ) 
        {            
            // Store the current matrix
            Gl.glPushMatrix();
            // Reset and transform the matrix.
            Gl.glLoadIdentity();

            Gl.glRotatef(angleX, 1, 0, 0);
            Gl.glRotatef(angleZ, 0, 0, 1);

            // Enable/Disable features
            Gl.glPushAttrib(Gl.GL_ENABLE_BIT);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_BLEND);
            // Just in case we set all vertices to white.
            Gl.glColor4f(1, 1, 1, 1);
            
                    // Render the front quad
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[0]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(0.5f, -0.5f, -0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(-0.5f, -0.5f, -0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(-0.5f, 0.5f, -0.5f);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(0.5f, 0.5f, -0.5f);
            Gl.glEnd(); 

            // Render the left quad
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[1]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(0.5f, -0.5f, -0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(0.5f, 0.5f, -0.5f);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(0.5f, 0.5f, 0.5f);
            Gl.glEnd(); 

            // Render the back quad

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[2]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(0.5f, 0.5f, 0.5f);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-0.5f, 0.5f, 0.5f);
            Gl.glEnd(); 

            // Render the right quad
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[3]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-0.5f, -0.5f, -0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(-0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(-0.5f, 0.5f, 0.5f);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-0.5f, 0.5f, -0.5f);
            Gl.glEnd(); 

            // Render the top quad
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[4]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-0.5f, 0.5f, -0.5f);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-0.5f, 0.5f, 0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(0.5f, 0.5f, 0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(0.5f, 0.5f, -0.5f);
            Gl.glEnd();

            // Render the bottom quad
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, skybox[5]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-0.5f, -0.5f, -0.5f);
            Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 1); Gl.glVertex3f(0.5f, -0.5f, 0.5f);
            Gl.glTexCoord2f(1, 0); Gl.glVertex3f(0.5f, -0.5f, -0.5f);
            Gl.glEnd(); 

            // Restore enable bits and matrix
            Gl.glPopAttrib();
            Gl.glPopMatrix();
        }
    }
}
