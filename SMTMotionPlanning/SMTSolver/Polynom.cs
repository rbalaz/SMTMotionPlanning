using System;

namespace SMTMotionPlanning
{
    class Polynom
    {
        private double[] coeficients;
        private bool functionByY;

        public Polynom(double[] coeficients, bool functionByY)
        {
            this.coeficients = coeficients;
            this.functionByY = functionByY;
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
