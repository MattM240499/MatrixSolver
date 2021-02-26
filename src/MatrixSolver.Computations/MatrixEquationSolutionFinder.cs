using System;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;
using MatrixSolver.Computations.DataTypes.Automata;
using MatrixSolver.Computations.Maths;
using MatrixSolver.Computations.Maths.Extensions;

namespace MatrixSolver.Computations
{
    public static class MatrixEquationSolutionFinder
    {
        /// <summary>
        /// Finds a solution if it exists to the vector reachability problem.
        /// I.e. given a list of matrices 
        /// </summary>
        public static Automaton SolveVectorReachabilityProblem(ImmutableMatrix2x2[] matrices, ImmutableVector2D vectorX, ImmutableVector2D vectorY)
        {
            // Validate input data
            // TODO: Reorganise into subroutines to allow for unit testing.
            ValidateMatrixList(matrices);
            ValidateVectors(vectorX, vectorY, out var scalar);

            Console.WriteLine("Input data: ");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"M1, ..., Mn =  {String.Join(", ", matrices.Select(m => m.ToString()))}");
            Console.WriteLine($"x =  {vectorX}");
            Console.WriteLine($"y =  {vectorY}");
            Console.WriteLine("-------------------------");

            // Scale down by gcd(x1, x2) = gcd(y1, y2) = d
            var scaledVectorX = scalar * vectorX;
            var scaledVectorY = scalar * vectorY;
            // Calcualte A(x)^-1, A(y)
            var Ax = CalculateMatrixA(scaledVectorX);
            var Ay = CalculateMatrixA(scaledVectorY);
            var AxInverse = Ax.Inverse();

            Console.WriteLine($"General solution is of the form: {Ay} * {Constants.Matrices.T}^t * {AxInverse}");
            // Convert each matrix we require for the final form into a canonical string
            var AyAsCanonicalString = MathematicalHelper.ConvertMatrixToCanonicalString(Ay);
            var AxAsCanonicalString = MathematicalHelper.ConvertMatrixToCanonicalString(AxInverse);
            var TAsCanonicalString = String.Join("", Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.T]);
            var TInverseAsCanonicalString = String.Join("", Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.TInverse]);

            var matricesAsGeneratorMatrices = matrices.Select(m => MathematicalHelper.ConvertMatrixToCanonicalString(m));

            string matrixSolutionRegex = String.Format("{0}(({1})*|({2})*){3}",
                AyAsCanonicalString, TAsCanonicalString, TInverseAsCanonicalString, AxAsCanonicalString);
            string matrixProductRegex = $"({String.Join("|", matricesAsGeneratorMatrices)})*";

            Console.WriteLine($"Solutions of Mx = y as a regex are of the form: {matrixSolutionRegex}");
            Console.WriteLine($"Solution as a product of input matrices as a regex are of the form: {matrixProductRegex}");

            var matrixSolutionAsCanonicalDfa = RegexToCanonicalDfa(matrixSolutionRegex);
            var matrixProductAsCanonicalDfa = RegexToCanonicalDfa(matrixProductRegex);

            var intersectedDFA = Constants.Automaton.Canonical
                .IntersectionWithDFA(matrixSolutionAsCanonicalDfa)
                .IntersectionWithDFA(matrixProductAsCanonicalDfa)
                ;
            var minimizedDfa = intersectedDFA.MinimizeDFA();

            return minimizedDfa;
        }

        private static ImmutableMatrix2x2 CalculateMatrixA(ImmutableVector2D vector)
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

        private static void ValidateVectors(ImmutableVector2D vectorX, ImmutableVector2D vectorY, out BigRational scalar)
        {
            // Validate vectors
            ValidateVectorIsInteger(vectorX);
            ValidateVectorIsInteger(vectorY);
            var xGcd = MathematicalHelper.GCD(vectorX.UnderlyingVector[0].Numerator, vectorX.UnderlyingVector[1].Numerator);
            var yGcd = MathematicalHelper.GCD(vectorY.UnderlyingVector[0].Numerator, vectorY.UnderlyingVector[1].Numerator);
            if (xGcd != yGcd)
            {
                throw new ArgumentException($"Vector X GCD had value {xGcd} which was different to Vector Y GCD of {yGcd}. Therefore, there are no solutions in SL(2,Z)");
            }

            scalar = new BigRational(1, xGcd);
        }

        private static void ValidateVectorIsInteger(ImmutableVector2D vector)
        {
            if (vector.UnderlyingVector.Any(e => !e.IsInteger()))
            {
                throw new ArgumentException("Vector had a non integer element.");
            }
        }

        /// <summary>
        /// TODO: Unit tests
        /// </summary>
        private static Automaton RegexToCanonicalDfa(string regex)
        {
            return AutomatonUtil
                .RegexToAutomaton(regex, Constants.RegularLanguage.Symbols)
                // It isn't required to convert to DFA, but as it is an inexpensive operation for the automaton created via the thompson construction, 
                // it probably will end up saving time overall by reducing the time on the other steps.
                .ToDFA()
                .UpdateAutomatonToAcceptCanonicalWords()
                .ToDFA()
                .MinimizeDFA(false);
        }
    }
}