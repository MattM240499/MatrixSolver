
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

        private static LinkedList<GeneratorMatrixIdentifier> ConvertMatrixToUseTAndSGeneratorMatrices(ImmutableMatrix2x2 toConvertMatrix)
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
                var remainder = a() - quotient * c();
                
                var targetMatrix = Matrix2x2.Copy(Constants.Matrices.T).Pow(-quotient);
                var targetMatrixIdentifier = GeneratorMatrixIdentifier.TInverse;
                // Update the matrix
                matrix.MultiplyLeft(targetMatrix);
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
            // The matrix should now be in the form [[+-1, m], [0, +-1]].
            // TODO: Potential optimisation - We shouldn't need to multiply the matrix further. We should be able to calculate the result
            // from here.
            // Therefore it is either T^M or -T^-M = S^2*T^-M
            var sign = matrix.UnderlyingValues[0, 0].Numerator;
            if (sign == -1)
            {
                // We need to add a minus to the front (I.e. X)
                matrix.MultiplyLeft(Constants.Matrices.X);
                matrixProduct.AddLast(GeneratorMatrixIdentifier.X);
            }

            var b = matrix.UnderlyingValues[0, 1].Numerator;
            var targetMatrix2 = GeneratorMatrixIdentifier.T;
            if (b < 0)
            {
                targetMatrix2 = GeneratorMatrixIdentifier.TInverse;
            }
            for (int i = 0; i < BigInteger.Abs(b); i++)
            {
                matrixProduct.AddLast(targetMatrix2);
            }
            return matrixProduct;
        }

        public static LinkedList<GeneratorMatrixIdentifier> ReplaceTAndInverseWithStandardGenerators(this LinkedList<GeneratorMatrixIdentifier> matrixProduct)
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

        private static Dictionary<char, int> XEqualityLookup = new Dictionary<char, int>
        {
            ['R'] = 3,
            ['S'] = 2
        };

        public static LinkedList<GeneratorMatrixIdentifier> SimplifyToCanonicalForm(this LinkedList<GeneratorMatrixIdentifier> matrixProduct)
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
            // Go backwards, 
            while (currentNode != null)
            {
                bool remove = false;
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
                        throw new InvalidOperationException($"Unexpected element {element}");
                }

                if (remove)
                {
                    // Remove all the consecutive characters
                    for (int j = 0; j < consecutiveCharacterCount; j++)
                    {
                        var previous = currentNode.Previous!;
                        matrixProduct.Remove(currentNode);
                        currentNode = previous;
                    }
                    // If we removed elements up to the first element, the currentNode will be null here.
                    // Therefore, we can simply use the element at the start of the sequence
                    if(currentNode == null)
                    {
                        currentNode = matrixProduct.First;
                    }
                    else
                    {
                        // Reset the count
                        switch(consecutiveCharacter)
                        {
                            case GeneratorMatrixIdentifier.R:
                                // Preceeding R must have been a single S, or there would have been cancellations previously
                                consecutiveCharacter = GeneratorMatrixIdentifier.S;
                                consecutiveCharacterCount = 1;
                                break;
                            case GeneratorMatrixIdentifier.S:
                                // We know, that preceeding S must have been an R, but it could have been preceeded by an R or an S (SR, RR),
                                // So we must check from that character onwards. 
                                consecutiveCharacter = GeneratorMatrixIdentifier.None;
                                consecutiveCharacterCount = 0;
                                // If we are at the end of the string once again.
                                if(currentNode.Previous != null)
                                {
                                    currentNode = currentNode.Previous;
                                }
                                break;
                            default:
                                throw new InvalidOperationException($"Unexpected element {element}");
                        }
                    }
                    
                    // These consecutive characters are cancelled because they are = X = -I, so removal changes the sign of the final expression
                    negative = !negative;
                    consecutiveCharacterCount = 0;
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