using Extreme.Mathematics;

namespace MatrixSolver.Computations.Maths
{
    /// <summary>
    /// Provides extensions for the <see cref="Extreme.Mathematics" /> Assembly
    /// </summary>
    public static class ExtremeMathematicsExtensions
    {
        /// <summary>
        /// Checks whether a <see cref="BigRational" /> number is an integer.
        /// </summary>
        public static bool IsInteger(this BigRational value)
        {
            return value.Denominator == 1;
        }
    }
}