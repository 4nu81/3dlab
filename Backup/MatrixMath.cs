//
// MatrixMath.cs 
//  - provides methodes for translational and rotational matrizes to calc movement in 3D
//
// Authors:
//	Andreas Maertens <mcmaerti@gmx.de>
//
// Copyright 2011 by Andreas Maertens

using System;
using System.Collections.Generic;
using System.Text;

namespace GL3DLab
{
    class Matrix
    {
        #region Matrixdefinition
        /// <summary>
        /// Die Matrix selbst
        /// </summary>
        public float[,] Value = new float[3, 3];

        /// <summary>
        /// eine Translationsmatrix
        /// </summary>
        /// <param name="x">Translation in x Richtung</param>
        /// <param name="y">Translation in y Richtung</param>
        /// <param name="z">Translation in z Richtung</param>
        public void TransMatrix(float x, float y, float z)
        {
            Value[0, 0] = 1;
            Value[0, 1] = 0;
            Value[0, 2] = 0;
            Value[0, 3] = x;

            Value[1, 0] = 0;
            Value[1, 1] = 1;
            Value[1, 2] = 0;
            Value[1, 3] = y;

            Value[2, 0] = 0;
            Value[2, 1] = 0;
            Value[2, 2] = 1;
            Value[2, 3] = z;
        }

        /// <summary>
        /// Rotationsmatrix
        /// </summary>
        /// <param name="angle">Drehwinkel</param>
        /// <param name="rotachs">Achse um die gedreht werden soll</param>
        public void RotMatrix(float angle, Point3 rotachs)
        {
            double fangle = (angle * Math.PI) / 180;
            float fsin = (float)Math.Sin(fangle);
            float fcos = (float)Math.Cos(fangle);
            float x = rotachs.x;
            float y = rotachs.y;
            float z = rotachs.z;

            Value[0, 0] = fcos + (float)Math.Pow(x, 2) * (1f - fcos);
            Value[1, 0] = x * y * (1f - fcos) - z * fsin;
            Value[2, 0] = x * z * (1f - fcos) + y * fsin;

            Value[0, 1] = y * x * (1f - fcos) + z * fsin;
            Value[1, 1] = fcos + (float)Math.Pow(y, 2) * (1f - fcos);
            Value[2, 1] = y * z * (1f - fcos) * x * fsin;

            Value[0, 2] = z * x * (1f - fcos) - y * fsin;
            Value[1, 2] = z * y * (1f - fcos) + x * fsin;
            Value[2, 2] = fcos + (float)Math.Pow(z, 2) * (1f - fcos);

        }
        #endregion
    }

    class MatrixMath
    {
        #region Matrix Operationen;

        /// <summary>
        /// Eine Spalten - Zeilenoperation aus der Matrixmultiplikation
        /// </summary>
        /// <param name="Mat1">Matrix 1</param>
        /// <param name="Mat2">Matrix 2</param>
        /// <param name="row">Bezugszeile in Matrix 1</param>
        /// <param name="column">Bezugsspalte in Matrix 2</param>
        /// <returns>Produkt aus Spalte und Zeile</returns>
        private float solveItem(Matrix Mat1, Matrix Mat2, int row, int column)
        {
            float result = 0;

            for (int x = 0; x <= 2; x++)
            {
                result = result + Mat1.Value[row, x] * Mat2.Value[x, column];
            }
            return (result);
        }

        /// <summary>
        /// Matrix o Vector
        /// </summary>
        /// <param name="Mat1">Matrix</param>
        /// <param name="P1">Vektor</param>
        /// <returns>Punkt der mittels der Matrix transformiert wurde</returns>
        public Point3 MatDotPoint(Matrix Mat1, Point3 P1)
        {
            Point3 result = new Point3(0, 0, 0);

            result.x = Mat1.Value[0, 0] * P1.x + Mat1.Value[0, 1] * P1.y + Mat1.Value[0, 2] * P1.z;
            result.y = Mat1.Value[1, 0] * P1.x + Mat1.Value[1, 1] * P1.y + Mat1.Value[1, 2] * P1.z;
            result.z = Mat1.Value[2, 0] * P1.x + Mat1.Value[2, 1] * P1.y + Mat1.Value[2, 2] * P1.z;

            return result;
        }

        /// <summary>
        /// Matrix o Matrix
        /// </summary>
        /// <param name="Mat1">Matrix 1</param>
        /// <param name="Mat2">Matrix 2</param>
        /// <returns>Translations- oder Rotationsmatrizen werden hier verknüpft zu einer Transformationsmatrix</returns>
        public Matrix MatDotMat(Matrix Mat1, Matrix Mat2)
        {
            Matrix result = new Matrix();

            for (int i = 0; i <= Math.Sqrt(result.Value.Length) - 1; i++)
            {
                for (int j = 0; j <= Math.Sqrt(result.Value.Length) - 1; j++)
                {
                    result.Value[i, j] = solveItem(Mat1, Mat2, i, j);
                }
            }
            return (result);
        }

        #endregion

    }
}