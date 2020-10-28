
using System;
using System.Collections.Generic;
using Extreme.Mathematics;
using MatrixSolver.DataTypes;

namespace MatrixSolver.Maths
{
    public static class MathematicalHelper
    {
        /// <summary>
        /// Implementation of the Euclidean algorithm
        /// </summary>
        public static BigInteger GCD(BigInteger a, BigInteger b)
        {
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
        /// Returns gcd(a,b), s,t, such that as + bt = gcd(a, b)
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

            return (a, s[0], t[0]);
        }

        /// <summary>
        /// Converts a matrix in SL(2,Z) to a form using only generator matrices. Returns a list of matrices made up of only S and R that when mutliplied in order 
        /// are equivelent to the original matrix.
        /// Based on https://kconrad.math.uconn.edu/blurbs/grouptheory/SL(2,Z).pdf
        /// </summary>
        public static List<GeneratorMatrixIdentifier> ConvertMatrixToGeneratorFormAsString(Matrix2x2 matrix)
        {
            var originalMatrix = matrix;
            // First convert it to use T and S
            List<GeneratorMatrixIdentifier> matrices = new List<GeneratorMatrixIdentifier>();

            var a = matrix.UnderlyingValues[0, 0].Numerator;
            var c = matrix.UnderlyingValues[1, 0].Numerator;
            if (BigInteger.Abs(a) < BigInteger.Abs(c))
            {
                matrix = Constants.Matrices.S * matrix;
                matrices.Add(GeneratorMatrixIdentifier.SInverse);

                a = matrix.UnderlyingValues[0, 0].Numerator;
                c = matrix.UnderlyingValues[1, 0].Numerator;
            }

            // Continue until the lower left entry is 0
            while (c != 0)
            {
                var quotient = a / c;
                var remainder = a - quotient * c;

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
                    matrices.Add(targetMatrixIdentifier);
                }

                // Perform an S switch.
                // Divide a by c
                a = matrix.UnderlyingValues[0, 0].Numerator;
                c = matrix.UnderlyingValues[1, 0].Numerator;
                if (BigInteger.Abs(a) < BigInteger.Abs(c))
                {
                    matrix = Constants.Matrices.S * matrix;
                    matrices.Add(GeneratorMatrixIdentifier.SInverse);

                    a = matrix.UnderlyingValues[0, 0].Numerator;
                    c = matrix.UnderlyingValues[1, 0].Numerator;
                }
            }
            // The matrix should now be in the form [[+-1, m], [0, +-1]].
            // TODO: Remove unneccessary matrix multiplication after here.
            // Therefore it is either T^M or -T^-M = S^2*T^-M
            var sign = matrix.UnderlyingValues[0, 0].Numerator;
            if (sign == -1)
            {
                // We need to add a minus to the front (I.e. X)
                matrix = Constants.Matrices.X * matrix;
                matrices.Add(GeneratorMatrixIdentifier.X);
            }

            var b = matrix.UnderlyingValues[0, 1].Numerator;
            var targetMatrix2 = GeneratorMatrixIdentifier.T;
            if (b < 0)
            {
                targetMatrix2 = GeneratorMatrixIdentifier.TInverse;
            }
            for (int i = 0; i < BigInteger.Abs(b); i++)
            {
                matrices.Add(targetMatrix2);
            }

            // TODO: Remove unnecessary checks
            matrix = Constants.Matrices.T.Pow(-b) * matrix;
            
            if(matrix != Constants.Matrices.I)
            {
                throw new ApplicationException("Procedure did not convert6 correctly.");
            }
            if(!IsEqual(matrices, originalMatrix))
            {
                throw new ApplicationException("Procedure did not convert correctly.");
            }
            // Replace T and T^-1 with {S,R}*
            for (int i = matrices.Count - 1; i >= 0; i--)
            {
                var element = matrices[i];
                if (element == GeneratorMatrixIdentifier.T || element == GeneratorMatrixIdentifier.TInverse || element == GeneratorMatrixIdentifier.SInverse)
                {
                    matrices.RemoveAt(i);
                    // Insert the replacement version with S/R/X
                    matrices.InsertRange(i, Constants.Matrices.GeneratorMatrixIdentifierLookup[element]);
                }
            }
            // Simplify:
            // First remove all the X's. Using the fact that X = -I we can remove any instances. 
            // However, we may need to add an X for the sign at the front if an odd number have been removed
            // The 'negative' variable will track whether the simplified matrix is -1 or 1 times the original matrix.
            bool negative = false;
            for (int i = matrices.Count - 1; i >= 0; i--)
            {
                var element = matrices[i];
                if (element == GeneratorMatrixIdentifier.X)
                {
                    matrices.RemoveAt(i);
                    negative = !negative;
                }
            }
            // Now simplify by removing instance of S^2 and T^3. This leverages the fact that S^2 = T^3 = X
            int consecutiveCharacterCount = 0;
            var consecutiveCharacter = GeneratorMatrixIdentifier.X;
            bool changes = true;
            while (changes)
            {
                changes = false;

                for (int i = matrices.Count - 1; i >= 0; i--)
                {
                    bool remove = false;
                    var element = matrices[i];
                    if (element != consecutiveCharacter)
                    {
                        consecutiveCharacterCount = 1;
                    }
                    else
                    {
                        consecutiveCharacterCount += 1;
                    }

                    switch (element)
                    {
                        // TODO: Double check this makes sense.
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
                            matrices.RemoveAt(i);
                            negative = !negative;
                        }
                        consecutiveCharacterCount = 0;
                    }
                }
            }
            // Finally, if an odd number of X's were removed, then add an X to the front
            if (negative)
            {
                matrices.Insert(0, GeneratorMatrixIdentifier.X);
            }

            // TODO: Remove unnecessary check
            if(!IsEqual(matrices, originalMatrix))
            {
                throw new ApplicationException($"Matrix {originalMatrix} was incorrectly translated to regular language.");
            };
            return matrices;
        }

        public static bool IsEqual(IReadOnlyCollection<GeneratorMatrixIdentifier> matrices, Matrix2x2 matrix)
        {
            var workingMatrix = Constants.Matrices.I;
            foreach(var matrixIdentifier in matrices)
            {
                var nextMatrix = Constants.Matrices.MatrixIdentifierDictionary[matrixIdentifier];
                workingMatrix = workingMatrix * nextMatrix;
            }

            if(!workingMatrix.Equals(matrix))
            {
                return false;
            }
            return true;
        }
    }
}