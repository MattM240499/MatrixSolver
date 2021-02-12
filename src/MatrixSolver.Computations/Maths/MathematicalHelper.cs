
using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;

namespace MatrixSolver.Computations.Maths
{
    public static class MathematicalHelper
    {
        /// <summary>
        /// Implementation of the Euclidean algorithm that finds the Greatest Common Divisor of two integers
        /// </summary>
        public static BigInteger GCD(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }

            return a | b;
        }

        /// <summary>
        /// Implementation of the Extended Euclidean algorithm.
        /// Returns (gcd(a,b), s, t) such that as + bt = gcd(a, b)
        /// </summary>
        public static (BigInteger gcd, BigInteger s, BigInteger t) ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b)
        {
            if (a < b)
            {
                // Flip the output or the values of s,t will be the wrong way round
                var values = ExtendedEuclideanAlgorithm(b, a);
                return (values.gcd, values.t, values.s);
            }

            List<BigInteger> s = new List<BigInteger> { 1, 0 };
            List<BigInteger> t = new List<BigInteger> { 0, 1 };
            while (b != 0)
            {
                BigInteger q = a / b;

                // Update r
                var remainder = a - q * b;
                a = b;
                b = remainder;

                // Update s
                s.Add(s[0] - (q * s[1]));
                s.RemoveAt(0);
                // Update t
                t.Add(t[0] - (q * t[1]));
                t.RemoveAt(0);
            }

            // If GCD found is negative, flip the resulting number. GCD should always be positive
            if (a < 0)
            {
                a = -a;
                s[0] = -s[0];
                t[0] = -t[0];
            }

            return (a, s[0], t[0]);
        }

        /// <summary>
        /// Converts a matrix in SL(2,Z) to a form using only generator matrices. Returns a list of matrices made up of only S and R that when mutliplied in order 
        /// are equivelent to the original matrix.
        /// Based on the algorithm described here: https://kconrad.math.uconn.edu/blurbs/grouptheory/SL(2,Z).pdf
        /// </summary>
        public static string ConvertMatrixToCanonicalString(ImmutableMatrix2x2 matrix)
        {
            // The process is 3 steps. First convert the matrix to use the T and S matrices.
            // Then T, T^-1 and S^-1 need to be replaced to use S,R,X
            // Finally, simplify the expression to canonical form
            var matrixProduct = ConvertMatrixToUseTAndSGeneratorMatrices(matrix)
                .ReplaceTAndInverseWithStandardGenerators()
                .SimplifyToCanonicalForm();
            return String.Join("", matrixProduct.Select(m => m.ToString()));
        }

        private static List<GeneratorMatrixIdentifier> ConvertMatrixToUseTAndSGeneratorMatrices(ImmutableMatrix2x2 matrix)
        {
            List<GeneratorMatrixIdentifier> matrixProduct = new List<GeneratorMatrixIdentifier>();
            // Setup aliases for a and c
            Func<BigInteger> a = () => matrix.UnderlyingValues[0, 0].Numerator;
            Func<BigInteger> c = () => matrix.UnderlyingValues[1, 0].Numerator;
            Action PerformSSwitchIfNeeded = () =>
            {
                if (BigInteger.Abs(a()) < BigInteger.Abs(c()))
                {
                    matrix = Constants.Matrices.S * matrix;
                    matrixProduct.Add(GeneratorMatrixIdentifier.SInverse);
                }
            };

            PerformSSwitchIfNeeded();

            // Continue until the lower left entry is 0
            while (c() != 0)
            {
                var quotient = a() / c();
                var remainder = a() - quotient * c();

                var targetMatrix = Constants.Matrices.T.Pow(-quotient);
                var targetMatrixIdentifier = GeneratorMatrixIdentifier.TInverse;
                // Update the matrix
                matrix = targetMatrix * matrix;
                // Choose either T or T^-1 depending on the matrix used
                if (quotient > 0)
                {
                    targetMatrixIdentifier = GeneratorMatrixIdentifier.T;
                }

                for (int i = 0; i < BigInteger.Abs(quotient); i++)
                {
                    matrixProduct.Add(targetMatrixIdentifier);
                }

                PerformSSwitchIfNeeded();
            }
            // The matrix should now be in the form [[+-1, m], [0, +-1]].
            // TODO: Potential optimisation - We shouldn't need to multiply the matrix further. We should be able to calculate the result
            // from here.
            // Therefore it is either T^M or -T^-M = S^2*T^-M
            var sign = matrix.UnderlyingValues[0, 0].Numerator;
            if (sign == -1)
            {
                // We need to add a minus to the front (I.e. X)
                matrix = Constants.Matrices.X * matrix;
                matrixProduct.Add(GeneratorMatrixIdentifier.X);
            }

            var b = matrix.UnderlyingValues[0, 1].Numerator;
            var targetMatrix2 = GeneratorMatrixIdentifier.T;
            if (b < 0)
            {
                targetMatrix2 = GeneratorMatrixIdentifier.TInverse;
            }
            for (int i = 0; i < BigInteger.Abs(b); i++)
            {
                matrixProduct.Add(targetMatrix2);
            }
            return matrixProduct;
        }

        public static List<GeneratorMatrixIdentifier> ReplaceTAndInverseWithStandardGenerators(this List<GeneratorMatrixIdentifier> matrixProduct)
        {
            var standardGeneratorList = new List<GeneratorMatrixIdentifier>(matrixProduct.Count);
            for (int i = 0; i < matrixProduct.Count; i++)
            {
                var element = matrixProduct[i];
                if (element == GeneratorMatrixIdentifier.T || element == GeneratorMatrixIdentifier.TInverse || element == GeneratorMatrixIdentifier.SInverse)
                {
                    standardGeneratorList.AddRange(Constants.Matrices.GeneratorMatrixIdentifierLookup[element]);
                }
                else
                {
                    standardGeneratorList.Add(element);
                }
            }
            return standardGeneratorList;
        }

        public static List<GeneratorMatrixIdentifier> SimplifyToCanonicalForm(this List<GeneratorMatrixIdentifier> matrixProduct)
        {
            // First remove all the X's. Using the fact that X = -I we can remove any instances. 
            // However, we may need to add an X for the sign at the front if an odd number of X's are removed
            // The 'negative' variable will track whether the simplified matrix is -1 or 1 times the original matrix.
            bool negative = false;
            for (int i = matrixProduct.Count - 1; i >= 0; i--)
            {
                var element = matrixProduct[i];
                if (element == GeneratorMatrixIdentifier.X)
                {
                    matrixProduct.RemoveAt(i);
                    negative = !negative;
                }
            }
            // Now simplify by removing instance of S^2 and T^3. This leverages the fact that S^2 = T^3 = X = -I
            var consecutiveCharacter = GeneratorMatrixIdentifier.X;
            bool changes = true;
            // TODO: Potential optimisation: Insert/Remove actions are expensive operations on the list. Could we use an alternative approach?
            // - LinkedList?
            // - Create a new list. (Will need to track multiple indexes (think of simplifying: RSRRRSRR = epsilon))
            while (changes)
            {
                int consecutiveCharacterCount = 0;
                changes = false;

                for (int i = matrixProduct.Count - 1; i >= 0; i--)
                {
                    bool remove = false;
                    var element = matrixProduct[i];
                    if (element != consecutiveCharacter)
                    {
                        consecutiveCharacterCount = 1;
                        consecutiveCharacter = element;
                    }
                    else
                    {
                        consecutiveCharacterCount += 1;
                    }

                    switch (element)
                    {
                        case GeneratorMatrixIdentifier.R:
                            if (consecutiveCharacterCount == 3)
                            {
                                remove = true;
                            }
                            break;
                        case GeneratorMatrixIdentifier.S:
                            if (consecutiveCharacterCount == 2)
                            {
                                remove = true;
                            }
                            break;
                        default:
                            break;
                    }

                    if (remove)
                    {
                        changes = true;
                        for (int j = 0; j < consecutiveCharacterCount; j++)
                        {
                            matrixProduct.RemoveAt(i);
                        }
                        negative = !negative;
                        consecutiveCharacterCount = 0;
                    }
                }
            }
            // Finally, if an odd number of X's were removed, then add an X to the front
            if (negative)
            {
                matrixProduct.Insert(0, GeneratorMatrixIdentifier.X);
            }
            return matrixProduct;
        }
    }
}