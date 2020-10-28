
using System.Collections.Generic;
using Extreme.Mathematics;
using MatrixSolver.DataTypes;

public static class Constants
{
    public static class Matrices
    {
        public static Matrix2x2 T { get; }
        public static Matrix2x2 S { get; }
        public static Matrix2x2 R { get; }
        public static Matrix2x2 X { get; }
        public static Matrix2x2 I { get; }

        public static Dictionary<GeneratorMatrixIdentifier, Matrix2x2> MatrixIdentifierDictionary { get; }

        public static Dictionary<GeneratorMatrixIdentifier, GeneratorMatrixIdentifier[]> GeneratorMatrixIdentifierLookup { get; }

        static Matrices()
        {
            var tMatrix = new BigRational[2, 2];
            tMatrix[0, 0] = 1;
            tMatrix[0, 1] = 1;
            tMatrix[1, 0] = 0;
            tMatrix[1, 1] = 1;
            T = new Matrix2x2(tMatrix);

            var sMatrix = new BigRational[2, 2];
            sMatrix[0, 0] = 0;
            sMatrix[0, 1] = -1;
            sMatrix[1, 0] = 1;
            sMatrix[1, 1] = 0;
            S = new Matrix2x2(sMatrix);

            var rMatrix = new BigRational[2, 2];
            rMatrix[0, 0] = 0;
            rMatrix[0, 1] = -1;
            rMatrix[1, 0] = 1;
            rMatrix[1, 1] = 1;
            R = new Matrix2x2(rMatrix);

            var xMatrix = new BigRational[2, 2];
            xMatrix[0, 0] = -1;
            xMatrix[0, 1] = 0;
            xMatrix[1, 0] = 0;
            xMatrix[1, 1] = -1;
            X = new Matrix2x2(xMatrix);

            var iMatrix = new BigRational[2, 2];
            iMatrix[0, 0] = 1;
            iMatrix[0, 1] = 0;
            iMatrix[1, 0] = 0;
            iMatrix[1, 1] = 1;
            I = new Matrix2x2(iMatrix);

            MatrixIdentifierDictionary = new Dictionary<GeneratorMatrixIdentifier, Matrix2x2>
            {
                [GeneratorMatrixIdentifier.T] = Constants.Matrices.T,
                [GeneratorMatrixIdentifier.S] = Constants.Matrices.S,
                [GeneratorMatrixIdentifier.R] = Constants.Matrices.R,
                [GeneratorMatrixIdentifier.X] = Constants.Matrices.X,
                [GeneratorMatrixIdentifier.SInverse] = Constants.Matrices.S.Inverse(), // Or -S
                [GeneratorMatrixIdentifier.TInverse] = Constants.Matrices.T.Inverse()
            };

            GeneratorMatrixIdentifierLookup = new Dictionary<GeneratorMatrixIdentifier, GeneratorMatrixIdentifier[]>
            {
                [GeneratorMatrixIdentifier.T] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.S, GeneratorMatrixIdentifier.R },
                [GeneratorMatrixIdentifier.TInverse] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.R, GeneratorMatrixIdentifier.R, GeneratorMatrixIdentifier.S },
                [GeneratorMatrixIdentifier.SInverse] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.S }
            };
        }
    }
}