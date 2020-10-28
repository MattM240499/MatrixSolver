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
        public static void TrySolveVectorReachabilityProblem(Matrix2x2[] matrices, Vector2D vectorX, Vector2D vectorY)
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
            // Validate vectors
            if (vectorX.UnderlyingVector.Any(e => !e.IsInteger()) || vectorY.UnderlyingVector.Any(e => !e.IsInteger()))
            {
                throw new ArgumentException("One or more vectors had a non integer element.");
            }
            var xGcd = MathematicalHelper.GCD(vectorX.UnderlyingVector[0].Numerator, vectorX.UnderlyingVector[1].Numerator);
            var yGcd = MathematicalHelper.GCD(vectorY.UnderlyingVector[0].Numerator, vectorY.UnderlyingVector[1].Numerator);
            if (xGcd != yGcd)
            {
                throw new ArgumentException($"Vector X GCD had value {xGcd} which was different to Vector Y GCD of {yGcd}. ");
            }
            Console.WriteLine("Input data: ");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"M1, ..., Mn =  {String.Join(", ", matrices.Select(m => m.ToString()))}");
            Console.WriteLine($"x =  {vectorX}");
            Console.WriteLine($"y =  {vectorY}");
            Console.WriteLine("-------------------------");

            var scaledVectorX = vectorX / xGcd;
            var scaledVectorY = vectorY / xGcd;

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
                var calculatedY = sol*vectorX;
                if(!(calculatedY).Equals(vectorY))
                {
                    throw new ApplicationException($"Solution {sol} is not a solution. Y calculated to have value {calculatedY} but expected {vectorY}");
                }
            }


            var AyAsGeneratorMatrices = MathematicalHelper.ConvertMatrixToGeneratorFormAsString(Ay);
            var AxInverseAsGeneratorMatrices = MathematicalHelper.ConvertMatrixToGeneratorFormAsString(AxInverse);
        }

        private static Matrix2x2 CalculateAx(Vector2D vector)
        {
            var euclideanAlgorithmValues = MathematicalHelper.ExtendedEuclideanAlgorithm(vector.UnderlyingVector[0].Numerator, vector.UnderlyingVector[1].Numerator);
            var matrix = new BigRational[2, 2];
            matrix[0, 0] = vector.UnderlyingVector[0];
            matrix[1, 0] = vector.UnderlyingVector[1];
            matrix[0, 1] = -euclideanAlgorithmValues.t;
            matrix[1, 1] = euclideanAlgorithmValues.s;
            return new Matrix2x2(matrix);
        }
    }
}