using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GL3DLab
{
    /// <summary>
    /// Eine Punktklasse.
    /// </summary>
    public class Point3
    {
        /// <summary>
        /// Die X-Komponente
        /// </summary>
        public float x = 0;
        /// <summary>
        /// Die Y-Komponente
        /// </summary>
        public float y = 0;
        /// <summary>
        /// Die Z-Komponente
        /// </summary>
        public float z = 0;

        /// <summary>
        /// Assigns Values of p1 to this by leaving adresses as they are
        /// </summary>
        /// <param name="p1">Point3 to assign values to this</param>
        public void assign(Point3 p1)
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;

            this.x += p1.x;
            this.y += p1.y;
            this.z += p1.z;
        }

        /// <summary>
        /// sum up the values of p1 and p1
        /// </summary>        
        public static Point3 operator +(Point3 p1, Point3 p2)
        {
            Point3 p3 = new Point3(0, 0, 0);
            p3.x = p1.x + p2.x;
            p3.y = p1.y + p2.y;
            p3.z = p1.z + p2.z;
            return p3;
        }

        /// <summary>
        /// subtracts values of p1 and p2
        /// </summary>        
        public static Point3 operator -(Point3 p1, Point3 p2)
        {
            Point3 p3 = new Point3(0,0,0);
            p3.x = p1.x - p2.x;
            p3.y = p1.y - p2.y;
            p3.z = p1.z - p2.z;
            return p3;
        }

        /// <summary>
        /// multiplies values of p1 with v
        /// </summary>
        public static Point3 operator *(Point3 p1, double v)
        {
            Point3 p3 = new Point3(0, 0, 0);
            p3.x = p1.x * (float)v;
            p3.y = p1.y * (float)v;
            p3.z = p1.z * (float)v;
            return p3;            
        }

        /// <summary>
        /// divides values of p1 with v
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Point3 operator /(Point3 p1, double v) 
        {
            Point3 p3 = new Point3();
            p3.x = p1.x / (float)v;
            p3.y = p1.y / (float)v;
            p3.z = p1.z / (float)v;
            return p3;
        }

        /// <summary>
        /// Entfernung zwischen 2 Punkten.
        /// </summary>
        /// <param name="p1">Punkt1</param>
        /// <param name="p2">Punkt2</param>
        /// <returns>Entfernung als Float</returns>
        public static float Distance(Point3 p1, Point3 p2) 
        {
            return (float)Math.Sqrt( Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2) + Math.Pow(p1.z - p2.z, 2));
        }

        /// <summary>
        /// Betrag des Vektors: { Ursprung, Self }
        /// </summary>
        /// <returns>Betrag</returns>
        public float Abs 
        {
            get { return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        public Point3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Ein 0-Argument Constructor für die Vererbung
        /// </summary>
        public Point3() 
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }

        /// <summary>
        /// Normalizes this Point3. Its length will be 1.
        /// </summary>
        public void Normalize()
        {
            float v = (float)(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2)));
            x /= v;
            y /= v;
            z /= v;
        }

        /// <summary>
        /// gives Values of Point3 as String
        /// </summary>
        /// <returns>Values of Point3 as string</returns>
        public override string ToString()
        {
            return x + "|" + y + "|" + z;
        }
    }

    /// <summary>
    /// the Vector between Point3 pkt1 and Point3 pkt2
    /// </summary>
    public class Vector3
    {
        /// <summary>
        /// Der Startpunkt des Vectors
        /// </summary>
        public Point3 a = new Point3(0, 0, 0);
        /// <summary>
        /// Der Endpunkt des Vectors
        /// </summary>
        public Point3 b = new Point3(0, 0, 0);

        /// <summary>
        /// Constructor
        /// </summary>        
        public Vector3(Point3 pkt1, Point3 pkt2)
        {
            a = pkt1;
            b = pkt2;
        }
    }

    /// <summary>
    /// a Square of Point3
    /// </summary>
    public class Square
    {
        /// <summary>
        /// Ecke des vierecks
        /// </summary>
        public Point3 a = new Point3(0, 0, 0);
        /// <summary>
        /// Ecke des vierecks
        /// </summary>
        public Point3 b = new Point3(0, 0, 0);
        /// <summary>
        /// Ecke des vierecks
        /// </summary>
        public Point3 c = new Point3(0, 0, 0);
        /// <summary>
        /// Ecke des vierecks
        /// </summary>
        public Point3 d = new Point3(0, 0, 0);

        /// <summary>
        /// Constructor
        /// </summary>
        public Square(Point3 pkt1, Point3 pkt2, Point3 pkt3, Point3 pkt4)
        {
            a = pkt1;
            b = pkt2;
            c = pkt3;
            d = pkt4;
        }
    }
}
