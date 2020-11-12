using System;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver.DataTypes;
using MatrixSolver.Maths;
using MatrixSolver.Maths.Extensions;

namespace MatrixSolver
{
    public static class MatrixEquationSolutionFinder
    {
        /// <summary>
        /// Finds a solution if it exists to the vector reachability problem.
        /// I.e. given a list of matrices 
        /// </summary>
        public static void TrySolveVectorReachabilityProblem(ImmutableMatrix2x2[] matrices, ImmutableVector2D vectorX, ImmutableVector2D vectorY)
        {
            // Validate input data
            ValidateMatrixList(matrices);
            BigRational scalar = GetVectorScalar(vectorX, vectorY);

            Console.WriteLine("Input data: ");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"M1, ..., Mn =  {String.Join(", ", matrices.Select(m => m.ToString()))}");
            Console.WriteLine($"x =  {vectorX}");
            Console.WriteLine($"y =  {vectorY}");
            Console.WriteLine("-------------------------");

            var scaledVectorX = vectorX * scalar;
            var scaledVectorY = vectorY * scalar;

            Console.WriteLine($"ScaledX = {scaledVectorX}");
            Console.WriteLine($"ScaledY = {scaledVectorY}");

            var Ax = CalculateAx(scaledVectorX);
            var Ay = CalculateAx(scaledVectorY);

            Console.WriteLine($"Ax = {Ax}");
            Console.WriteLine($"Ay = {Ay}");

            if (Ax.Determinant() != 1)
            {
                throw new ApplicationException("Ax is not in SL(2,Z)");
            }

            if (Ay.Determinant() != 1)
            {
                throw new ApplicationException("Ay is not in SL(2,Z)");
            }

            var AxInverse = Ax.Inverse();
            Console.WriteLine($"General solution is of the form: {Ay} * {Constants.Matrices.T}^t * {AxInverse}");

            // Output some solutions ()
            for (int i = 0; i < 10; i++)
            {
                var sol = Ay * (Constants.Matrices.T ^ i) * AxInverse;
                Console.WriteLine($"M{i} = {sol}");

                // Validate solution is correct (TODO: Remove)
                var calculatedY = sol * vectorX;
                if (!(calculatedY).Equals(vectorY))
                {
                    throw new ApplicationException($"Solution {sol} is not a solution. Y calculated to have value {calculatedY} but expected {vectorY}");
                }
            }

            var AyAsGeneratorMatrices = MathematicalHelper.ConvertMatrixToGeneratorFormAsString(Ay);
            var AxInverseAsGeneratorMatrices = MathematicalHelper.ConvertMatrixToGeneratorFormAsString(AxInverse);
            var TAsGeneratorMatrices = Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.T];
            var TInverseAsGeneratorMatrices = Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.TInverse];

            string matrixForm = String.Join("",
                AyAsGeneratorMatrices.Select(i => i.ToString())
                    .Append("(")
                    .Concat(TAsGeneratorMatrices.Select(i => i.ToString()))
                    .Append(")*")
                    .Concat(AxInverseAsGeneratorMatrices.Select(i => i.ToString()))
                    .Append(" | ")
                    .Concat(AyAsGeneratorMatrices.Select(i => i.ToString()))
                    .Append("(")
                    .Concat(TInverseAsGeneratorMatrices.Select(i => i.ToString()))
                    .Append(")*")
                    .Concat(AyAsGeneratorMatrices.Select(i => i.ToString()))
            );
            Console.WriteLine($"Solutions as a regular language as a regex are of the form: {matrixForm}");
        }

        private static ImmutableMatrix2x2 CalculateAx(ImmutableVector2D vector)
        {
            var euclideanAlgorithmValues = MathematicalHelper.ExtendedEuclideanAlgorithm(vector.UnderlyingVector[0].Numerator, vector.UnderlyingVector[1].Numerator);
            var matrix = new BigRational[2, 2];
            matrix[0, 0] = vector.UnderlyingVector[0];
            matrix[1, 0] = vector.UnderlyingVector[1];
            matrix[0, 1] = -euclideanAlgorithmValues.t;
            matrix[1, 1] = euclideanAlgorithmValues.s;
            return new ImmutableMatrix2x2(matrix);
        }

        private static void ValidateMatrixList(ImmutableMatrix2x2[] matrices)
        {
            // Validate matrices
            foreach (var matrix in matrices)
            {
                var det = matrix.Determinant();
                if (det != 1)
                {
                    throw new ArgumentException($"Determinant of matrix {matrix} was {det} but expected determinant 1.");
                }
            }
        }

        private static BigRational GetVectorScalar(ImmutableVector2D vectorX, ImmutableVector2D vectorY)
        {
            // Validate vectors
            ValidateVectorIsInteger(vectorX);
            ValidateVectorIsInteger(vectorY);
            var xGcd = MathematicalHelper.GCD(vectorX.UnderlyingVector[0].Numerator, vectorX.UnderlyingVector[1].Numerator);
            var yGcd = MathematicalHelper.GCD(vectorY.UnderlyingVector[0].Numerator, vectorY.UnderlyingVector[1].Numerator);
            if (xGcd != yGcd)
            {
                throw new ArgumentException($"Vector X GCD had value {xGcd} which was different to Vector Y GCD of {yGcd}. ");
            }

            return new BigRational(1, xGcd);
        }

        private static void ValidateVectorIsInteger(ImmutableVector2D vector)
        {
            if (vector.UnderlyingVector.Any(e => !e.IsInteger()))
            {
                throw new ArgumentException("Vector had a non integer element.");
            }
        }
    }
}