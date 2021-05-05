using System;
using System.Diagnostics;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;
using MatrixSolver.Computations.DataTypes.Automata;
using MatrixSolver.Computations.DataTypes.Automata.Canonicalisation;
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
            ValidateMatrixList(matrices);
            string matrixProductRegex = TimedFunction(() => GetMatrixSemigroupRegex(matrices), "Calculate Lsemigr regex");
            return SolveGeneralisedVectorReachabilityProblem(matrixProductRegex, "Lsemigr", vectorX, vectorY);
        }

        public static Automaton SolveGeneralisedVectorReachabilityProblem(string languageRegex, string languageName, ImmutableVector2D vectorX, ImmutableVector2D vectorY)
        {
            string matrixSolutionRegex = TimedFunction(() => GetVectorReachabilityProblemRegex(vectorX, vectorY), "Calculate Lvrp regex");
            Console.WriteLine($"Solutions of Mx = y as a regex are of the form:  Lvrp = {matrixSolutionRegex}");
            Console.WriteLine($"Solution intersected with language: {languageName} = {languageRegex}");

            var matrixSolutionAsCanonicalDfa = RegularLanguageRegexToCanonicalWordAcceptingDfa(matrixSolutionRegex, "Lvrp");
            var languageAsCanonicalDfa = RegularLanguageRegexToCanonicalWordAcceptingDfa(languageRegex, "Lsemigr");

            var intersectedDFA = TimedFunction(() => Constants.Automaton.Canonical
                .IntersectionWithDFA(matrixSolutionAsCanonicalDfa)
                .IntersectionWithDFA(languageAsCanonicalDfa), $"Intersection of Lcan, Lvrp, {languageName}")
                ;
            var minimizedDfa = TimedFunction(() => intersectedDFA.MinimizeDFA(), "DFA minimization");

            return minimizedDfa;
        }

        internal static string GetMatrixSemigroupRegex(ImmutableMatrix2x2[] matrices)
        {
            var matricesAsGeneratorMatrices = matrices.Select(m => MathematicalHelper.ConvertMatrixToCanonicalString(m));
            var matrixProductRegex = String.Format("({0})*", String.Join("|", matricesAsGeneratorMatrices));
            return matrixProductRegex;
        }

        internal static string GetVectorReachabilityProblemRegex(ImmutableVector2D vectorX, ImmutableVector2D vectorY)
        {
            // Validate input data
            ValidateVRPVectors(vectorX, vectorY, out BigRational scalar);

            // Scale down by gcd(x1, x2) = gcd(y1, y2) = d
            var scaledVectorX = scalar * vectorX;
            var scaledVectorY = scalar * vectorY;
            // Calcualte A(x)^-1, A(y)
            var AxInverse = CalculateMatrixA(scaledVectorX).Inverse();
            var Ay = CalculateMatrixA(scaledVectorY);

            // Convert each matrix we require for the final form into a canonical string
            var AyAsCanonicalString = MathematicalHelper.ConvertMatrixToCanonicalString(Ay);
            var AxAsCanonicalString = MathematicalHelper.ConvertMatrixToCanonicalString(AxInverse);
            var TAsCanonicalString = String.Join("", Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.T]);
            var TInverseAsCanonicalString = String.Join("", Constants.Matrices.GeneratorMatrixIdentifierLookup[GeneratorMatrixIdentifier.TInverse]);

            string matrixSolutionRegex = String.Format("{0}(({1})*|({2})*){3}",
                AyAsCanonicalString, TAsCanonicalString, TInverseAsCanonicalString, AxAsCanonicalString);
            return matrixSolutionRegex;
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

        private static void ValidateVRPVectors(ImmutableVector2D vectorX, ImmutableVector2D vectorY, out BigRational scalar)
        {
            // Validate vectors
            ValidateVectorIsNonZeroAndInteger(vectorX, "x");
            ValidateVectorIsNonZeroAndInteger(vectorY, "y");
            var xGcd = MathematicalHelper.GCD(vectorX.UnderlyingVector[0].Numerator, vectorX.UnderlyingVector[1].Numerator);
            var yGcd = MathematicalHelper.GCD(vectorY.UnderlyingVector[0].Numerator, vectorY.UnderlyingVector[1].Numerator);
            if (xGcd != yGcd)
            {
                throw new ArgumentException($"Vector X GCD had value {xGcd} which was different to Vector Y GCD of {yGcd}. Therefore, there are no solutions in SL(2,Z)");
            }

            scalar = new BigRational(1, xGcd);
        }

        private static void ValidateVectorIsNonZeroAndInteger(ImmutableVector2D vector, string vectorName)
        {
            if (vector.UnderlyingVector.Any(e => !e.IsInteger()))
            {
                throw new ArgumentException($"Vector {vectorName} had a non integer element.");
            }
            if (vector.UnderlyingVector[0] == 0 && vector.UnderlyingVector[1] == 0)
            {
                throw new ArgumentException($"Vector {vectorName} was equal to zero");
            }
        }

        internal static Automaton RegularLanguageRegexToCanonicalWordAcceptingDfa(string regex, string regexName = "")
        {
            // It isn't required to convert to DFA, but as it is an inexpensive operation for the automaton created via the thompson construction, 
            // it probably will end up saving time overall by reducing the time on the other steps.

            var sw = Stopwatch.StartNew();
            var automaton = TimedFunction(() => AutomatonUtil.RegexToAutomaton(regex, Constants.RegularLanguage.Symbols),
                $"Regex to Thompson NFA for regex {regexName}");
            automaton = TimedFunction(() => automaton.ToDFA(),
                $"Thompson NFA to DFA for regex {regexName}");
            TimedFunction(() => automaton.UpdateAutomatonToAcceptCanonicalWords(),
                $"Canonicalisation of DFA for regex {regexName}");
            automaton = TimedFunction(() => automaton.ToDFA(),
                $"Canonicalised NFA to DFA for regex {regexName}");
            return automaton;
        }

        private static T TimedFunction<T>(Func<T> func, string functionDescription)
        {
            var sw = Stopwatch.StartNew();
            var t = func();
            sw.Stop();
            Console.WriteLine($"{functionDescription} completed in {sw.ElapsedMilliseconds}ms");
            return t;
        }
    }
}