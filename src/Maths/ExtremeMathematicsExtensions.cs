
using System;
using Extreme.Mathematics;

namespace MatrixSolver.Maths.Extensions
{
    public static class ExtremeMathematicsExtensions
    {
        public static bool IsInteger(this BigRational value)
        {
            return value.Denominator == 1;
        }
    }
}