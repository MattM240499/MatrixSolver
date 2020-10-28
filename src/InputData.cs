
using Extreme.Mathematics;

namespace MatrixSolver
{
    /// <summary>
    /// Used to deserialise from a file the input data
    /// </summary>
    public class InputData
    {
        public BigRational[][][] Matrices { get; set; }
        public BigRational[] VectorX { get; set; }
        public BigRational[] VectorY { get; set; }
    }
}