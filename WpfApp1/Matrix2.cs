using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace tplan
{
    public class Matrix2
    {
        public double[,] els = new double[3, 3];
        public Matrix2(double a11, double a12, double a13,
                         double a21, double a22, double a23,
                         double a31, double a32, double a33)
        {
            els[0, 0] = a11;
            els[1, 0] = a12;
            els[2, 0] = a13;

            els[0, 1] = a21;
            els[1, 1] = a22;
            els[2, 1] = a23;

            els[0, 2] = a31;
            els[1, 2] = a32;
            els[2, 2] = a33;

        }

        public void Mul(Matrix2 matr)
        {
            double[,] res = new double[3, 3];
            for (int c = 0; c < 3; c++)
            {

                for (int r = 0; r < 3; r++)
                {
                    res[c, r] = 0;
                    for (int rr = 0; rr < 3; rr++)
                    {
                        res[c, r] = res[c, r] + els[rr, r] * matr.els[c, rr];
                    }
                }
            }
            els = res;
        }

        public void Invert()
        {
            double d = els[0, 0] * els[1, 1] * els[2, 2] + els[2, 0] * els[0, 1] * els[1, 2] + els[0, 2] * els[1, 0] * els[2, 1] -
       els[0, 2] * els[1, 1] * els[2, 0] - els[0, 0] * els[2, 1] * els[1, 2] - els[2, 2] * els[0, 1] * els[1, 0];
            double[,] res = new double[3, 3];
            for (int c = 0; c < 3; c++)
                for (int r = 0; r < 3; r++)
                {
                    res[r, c] = Minor(c, r) * (Math.Pow((-1), (r + c))) / d;
                }
            els = res;
        }

        private double Minor(int c, int r)
        {
            int r1, c1, r2, c2;
            if (r == 0)
            {
                r1 = 1;
                r2 = 2;
            }
            else
                if (r == 1)
                {
                    r1 = 0;
                    r2 = 2;
                }
                else
                {
                    r1 = 0;
                    r2 = 1;
                }
            if (c == 0)
            {
                c1 = 1;
                c2 = 2;
            }
            else
                if (c == 1)
                {
                    c1 = 0;
                    c2 = 2;
                }
                else
                {
                    c1 = 0;
                    c2 = 1;
                }
            return els[c1, r1] * els[c2, r2] - els[c1, r2] * els[c2, r1];

        }
        public void ConvertPoint(double[] s, double[] r)
        {
            r[0] = els[0, 0] * s[0] + els[0, 1] * s[1] + els[0, 2];
            r[1] = els[1, 0] * s[0] + els[1, 1] * s[1] + els[1, 2];
        }
        public void ConvertPoint(ref PointF point)
        {
            float x = (float) ( els[0, 0] * point.X + els[0, 1] * point.Y + els[0, 2]);
            float y = (float) ( els[1, 0] * point.X + els[1, 1] * point.Y + els[1, 2]);
            point.X = x;
            point.Y = y;
        }


        public void ConvertPoint(double x, double y, out double wx, out double wy)
        {
            wx = els[0, 0] * x + els[0, 1] * y + els[0, 2];
            wy = els[1, 0] * x + els[1, 1] * y + els[1, 2];
            
        }
    }
}
