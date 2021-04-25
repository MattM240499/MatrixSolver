
using System;
using Extreme.Mathematics;

namespace MatrixSolver
{
    /// <summary>
    /// Used to deserialise from a file the input data
    /// </summary>
    public class InputData
    {
        public BigRational[][][] Matrices { get; set; } = null!;
        public BigRational[] VectorX { get; set; } = null!;
        public BigRational[] VectorY { get; set; } = null!;

        public void ThrowIfNull()
        {
            if (Matrices is null) throw new ArgumentNullException(nameof(Matrices));
            if (VectorX is null) throw new ArgumentNullException(nameof(Matrices));
            if (VectorY is null) throw new ArgumentNullException(nameof(Matrices));
        }
    }
}