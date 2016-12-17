using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTMotionPlanning
{
    class Polynom
    {
        private double[] coeficients;

        public Polynom(double[] coeficients)
        {
            this.coeficients = coeficients;
        }

        public double getPolynomValue(double x)
        {
            double value = 0;
            for (int i = 0; i < coeficients.Length; i++)
            {
                value += coeficients[i] * Math.Pow(x, coeficients.Length - 1 - i);
            }

            return value;
        }
    }
}
