using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GL3DLab
{
    /// <summary>
    /// Hält Globale Settings für die Anwendung
    /// </summary>
    public static class StaticVars
    {
        /// <summary>
        /// Licht an/aus
        /// </summary>
        public static bool Light = false;

        /// <summary>
        /// Helligkeitskonstante.
        /// </summary>
        private static float brightness = 1f;
        /// <summary>
        /// Kann für die Helligkeit benutzt werden.
        /// </summary>
        public static float Brightness
        {
            get
            {
                return brightness;
            }
            set            
            {
                brightness = value;
                if (brightness < 0) 
                {
                    brightness = 0;
                }
                if (brightness > 1)
                {
                    brightness = 1;
                }
            }
        }
        
        private static float linewidth = 1f;
        /// <summary>
        /// Kann für die Linienstärke benutzt werden.
        /// </summary>
        public static float Linewidth
        {
            get
            {
                return linewidth;
            }
        }
        /// <summary>
        /// erhöht die Liniendicke beim zeichnen
        /// </summary>
        public static void IncLinewidth()
        {
            linewidth += 1f;
        }
        /// <summary>
        /// verringert die Liniendicke beim zeichnen
        /// </summary>
        public static void DecLinewidth()
        {
            if (linewidth >= 1f) 
            { 
                linewidth -= 1f; 
            }
        }
    }
}
