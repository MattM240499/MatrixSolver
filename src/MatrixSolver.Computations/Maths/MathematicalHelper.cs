
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
            while (b != 0)
            {
                var t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        /// <summary>
        /// Implementation of the Extended Euclidean algorithm.
        /// Returns (gcd(a,b), s, t) such that as + bt = gcd(a, b)
        /// </summary>
        public static (BigInteger gcd, BigInteger s, BigInteger t) ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b)
        {
            BigInteger old_r = a; BigInteger r = b;
            BigInteger old_s = 1; BigInteger s = 0;
            BigInteger old_t = 0; BigInteger t = 1;
            while (r != 0)
            {
                BigInteger quotient = old_r / r;

                (old_r, r) = (r, old_r - (quotient * r));
                (old_s, s) = (s, old_s - (quotient * s));
                (old_t, t) = (t, old_t - (quotient * t));
            }

            // If GCD found is negative, flip the resulting number. GCD should always be positive
            if (old_r < 0)
            {
                old_s = -old_s;
                old_t = -old_t;
                old_r = -old_r;
            }

            return (old_r, old_s, old_t);
        }

        /// <summary>
        /// Converts a matrix in SL(2,Z) to a form using only generator matrices. 
        /// Returns a list of matrices made up of only S and R that when mutliplied in order 
        /// are equivelent to the original matrix.
        /// Based on the algorithm described here: 
        /// https://kconrad.math.uconn.edu/blurbs/grouptheory/SL(2,Z).pdf
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

        internal static LinkedList<GeneratorMatrixIdentifier> ConvertMatrixToUseTAndSGeneratorMatrices(ImmutableMatrix2x2 toConvertMatrix)
        {
            var matrix = Matrix2x2.Copy(toConvertMatrix);
            LinkedList<GeneratorMatrixIdentifier> matrixProduct = new LinkedList<GeneratorMatrixIdentifier>();
            // Setup aliases for a and c
            Func<BigInteger> a = () => matrix.UnderlyingValues[0, 0].Numerator;
            Func<BigInteger> c = () => matrix.UnderlyingValues[1, 0].Numerator;
            Action PerformSSwitchIfNeeded = () =>
            {
                if (BigInteger.Abs(a()) < BigInteger.Abs(c()))
                {
                    matrix.MultiplyLeft(Constants.Matrices.S);
                    matrixProduct.AddLast(GeneratorMatrixIdentifier.SInverse);
                }
            };

            PerformSSwitchIfNeeded();

            // Continue until the lower left entry is 0
            while (c() != 0)
            {
                var quotient = a() / c();

                var targetMatrix = TToPower(-quotient);
                matrix.MultiplyLeft(targetMatrix);

                var targetMatrixIdentifier = GeneratorMatrixIdentifier.TInverse;
                // Choose either T or T^-1 depending on the matrix used
                if (quotient > 0)
                {
                    targetMatrixIdentifier = GeneratorMatrixIdentifier.T;
                }

                for (int i = 0; i < BigInteger.Abs(quotient); i++)
                {
                    matrixProduct.AddLast(targetMatrixIdentifier);
                }

                PerformSSwitchIfNeeded();
            }
            // The matrix should now be in the form [[+-1, m], [0, +-1]]. This is ensured by the SL(2,Z) Group Property
            // Therefore it is either T^m or -T^-m = X*T^-m. 
            var sign = a();
            if (sign == -1)
            {
                // We need to add a minus to the front (I.e. X)
                matrixProduct.AddFirst(GeneratorMatrixIdentifier.X);
            }

            var power = matrix.UnderlyingValues[0, 1].Numerator * sign;
            var TOrTInverse = GeneratorMatrixIdentifier.T;
            // If the resulting matrix has a negative power of T, use the inverse matrix instead.
            if (power < 0)
            {
                TOrTInverse = GeneratorMatrixIdentifier.TInverse;
            }
            for (int i = 0; i < BigInteger.Abs(power); i++)
            {
                matrixProduct.AddLast(TOrTInverse);
            }
            return matrixProduct;
        }

        private static Matrix2x2 TToPower(BigInteger power)
        {
            var values = new BigRational[2, 2];
            values[0, 0] = 1;
            values[0, 1] = power;
            values[1, 0] = 0;
            values[1, 1] = 1;
            return new Matrix2x2(values);
        }

        internal static LinkedList<GeneratorMatrixIdentifier> ReplaceTAndInverseWithStandardGenerators(this LinkedList<GeneratorMatrixIdentifier> matrixProduct)
        {
            var currentNode = matrixProduct.First;
            while (currentNode != null)
            {
                if (currentNode.Value == GeneratorMatrixIdentifier.T ||
                    currentNode.Value == GeneratorMatrixIdentifier.TInverse ||
                    currentNode.Value == GeneratorMatrixIdentifier.SInverse)
                {
                    var toRemoveNode = currentNode;
                    // And then replace it with it's implementation
                    foreach (var element in Constants.Matrices.GeneratorMatrixIdentifierLookup[currentNode.Value])
                    {
                        currentNode = matrixProduct.AddAfter(currentNode, element);
                    }
                    // Finally remove the initial node.
                    matrixProduct.Remove(toRemoveNode);
                }
                currentNode = currentNode.Next;
            }
            return matrixProduct;
        }

        internal static LinkedList<GeneratorMatrixIdentifier> SimplifyToCanonicalForm(this LinkedList<GeneratorMatrixIdentifier> matrixProduct)
        {
            // First remove all the X's. Using the fact that X = -I we can remove any instances. 
            // However, we may need to add an X for the sign at the front if an odd number of X's are removed
            // The 'negative' variable will track whether the simplified matrix is -1 or 1 times the original matrix.
            bool negative = false;
            var currentNode = matrixProduct.First;
            while (currentNode != null)
            {
                var next = currentNode.Next;
                if (currentNode.Value == GeneratorMatrixIdentifier.X)
                {
                    matrixProduct.Remove(currentNode);
                    negative = !negative;
                }
                currentNode = next;
            }
            // Now simplify by removing instance of S^2 and T^3. This leverages the fact that S^2 = T^3 = X = -I
            var consecutiveCharacter = GeneratorMatrixIdentifier.None;
            int consecutiveCharacterCount = 0;
            currentNode = matrixProduct.First;

            while (currentNode != null)
            {
                var element = currentNode.Value;
                if (element != consecutiveCharacter)
                {
                    consecutiveCharacterCount = 1;
                    consecutiveCharacter = element;
                }
                else
                {
                    consecutiveCharacterCount += 1;
                }

                if ((element == GeneratorMatrixIdentifier.R && consecutiveCharacterCount == 3) || (element == GeneratorMatrixIdentifier.S && consecutiveCharacterCount == 2))
                {
                    // Remove all the consecutive characters
                    for (int j = 0; j < consecutiveCharacterCount; j++)
                    {
                        var previous = currentNode.Previous!;
                        matrixProduct.Remove(currentNode);
                        currentNode = previous;
                    }
                    // 4 is the magic number of steps we want to go back for S^2 and R^3, as this takes us to the 
                    // next potential starting element of a consecutive sequence.
                    var remainingMoves = 4 - consecutiveCharacterCount;
                    if(currentNode != null)
                    {
                        for(int j = 0; j < remainingMoves; j++)
                        {
                            currentNode = currentNode.Previous;
                            if (currentNode == null)
                            {
                                currentNode = matrixProduct.First;
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentNode = matrixProduct.First;
                    }
                    consecutiveCharacter = GeneratorMatrixIdentifier.None;
                    consecutiveCharacterCount = 0;

                    // These consecutive characters are cancelled because they are = X = -I, so removal changes the sign of the final expression
                    negative = !negative;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }

            // Finally, if an odd number of X's were removed, then add an X to the front
            if (negative)
            {
                matrixProduct.AddFirst(GeneratorMatrixIdentifier.X);
            }
            return matrixProduct;
        }
    }
}