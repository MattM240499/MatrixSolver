
using System;
using System.Collections.Generic;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;
using MatrixSolver.Computations.Maths;
using Xunit;

namespace MatrixSolver.Computations.Tests.Maths
{
    public class MathematicalHelperTests
    {
        [Theory]
        [InlineData(35, 14, 7)]
        [InlineData(23, 71, 1)]
        [InlineData(13, 16, 1)]
        [InlineData(24, 26, 2)]
        [InlineData(0, 7, 7)]
        [InlineData(-7, 0, 7)]
        [InlineData(-73, -13, 1)]
        [InlineData(-35, -28, 7)]
        public void GCD_CalculatesGCD(int number1, int number2, int expectedGcd)
        {
            Assert.Equal(expectedGcd, MathematicalHelper.GCD(number1, number2));
            Assert.Equal(expectedGcd, MathematicalHelper.GCD(number2, number1));
        }

        [Theory]
        [InlineData(35, 14, 7, 1, -2)]
        [InlineData(119, 42, 7, -1, 3)]
        [InlineData(2, 3, 1, -1, 1)]
        [InlineData(15, 5, 5, 0, 1)]
        [InlineData(-7, 8, 1, 1, 1)]
        [InlineData(7, -8, 1, -1, -1)]
        [InlineData(15, -9, 3, -1, -2)]
        [InlineData(-15, 9, 3, 1, 2)]
        [InlineData(-16, -4, 4, 0, -1)]
        [InlineData(-12, -8, 4, -1, 1)]
        [InlineData(0, 7, 7, 0, 1)]
        [InlineData(-7, 0, 7, -1, 0)]
        [InlineData(197, 757, 1, 342, -89)]
        public void ExtendedEuclideanAlgorithm_CalculatesSolution(int number1, int number2, int expectedGcd, int expectedS, int expectedT)
        {
            var values = MathematicalHelper.ExtendedEuclideanAlgorithm(number1, number2);

            Assert.Equal((expectedGcd, expectedS, expectedT), values);
        }

        [Fact]
        public void ConvertMatrixToUseTAndSGeneratorMatrices_ConvertsCorrectly()
        {
            var matrixValues = new BigRational[2, 2] { { 437, 202 }, { 543, 251 } };
            var matrix = new ImmutableMatrix2x2(matrixValues);
            var expectedIdentifierList = new[]
            {
                GeneratorMatrixIdentifier.X,
                GeneratorMatrixIdentifier.SInverse,
                // -1
                GeneratorMatrixIdentifier.TInverse,
                // S Switch
                GeneratorMatrixIdentifier.SInverse,
                // 4
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                // S Switch
                GeneratorMatrixIdentifier.SInverse,
                // -8
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                // S Switch
                GeneratorMatrixIdentifier.SInverse,
                // 6
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                GeneratorMatrixIdentifier.T,
                // S Switch
                GeneratorMatrixIdentifier.SInverse,
                // -2
                GeneratorMatrixIdentifier.TInverse,
                GeneratorMatrixIdentifier.TInverse,
                // S Switch
                GeneratorMatrixIdentifier.SInverse
            };
            var matrixIdentifierList = MathematicalHelper.ConvertMatrixToUseTAndSGeneratorMatrices(matrix);
            Assert.Equal(expectedIdentifierList, matrixIdentifierList);
        }

        [Theory]
        [InlineData(0, 1, GeneratorMatrixIdentifier.None, false)]
        [InlineData(0, -1, GeneratorMatrixIdentifier.None, true)]
        [InlineData(5, 1, GeneratorMatrixIdentifier.T, false)]
        [InlineData(-5, 1, GeneratorMatrixIdentifier.TInverse, false)]
        [InlineData(5, -1, GeneratorMatrixIdentifier.TInverse, true)]
        [InlineData(-5, -1, GeneratorMatrixIdentifier.T, true)]
        public void ConvertMatrixToUseTAndSGeneratorMatrices_ConvertsCorrectly_WithTFinish(int bValue, int cornerSign, GeneratorMatrixIdentifier expectedIdentifier, bool expectedX)
        {
            var matrixValues = new BigRational[2, 2] { { cornerSign, bValue }, { 0, cornerSign } };
            var matrix = new ImmutableMatrix2x2(matrixValues);
            var count = Math.Abs(bValue);
            var expectedIdentifierList = new List<GeneratorMatrixIdentifier>();
            if(expectedX)
            {
                expectedIdentifierList.Add(GeneratorMatrixIdentifier.X);
            }
            
            for(int i = 0;i < count; i++)
            {
                expectedIdentifierList.Add(expectedIdentifier);
            }
            var matrixIdentifierList = MathematicalHelper.ConvertMatrixToUseTAndSGeneratorMatrices(matrix);
            Assert.Equal(expectedIdentifierList, matrixIdentifierList);
        }

        // We will pass in matrices in Canonical form. We expect the same form back
        [Theory]
        [InlineData("XSRRSRSR")]
        [InlineData("SRRSRSRSRSRSRSRRSRSRSRSRSRSRSRSRSRRSRSRSRSRRSRRSRR")]
        [InlineData("XRRSRRSRR")]
        public void ConvertMatrixToCanonicalString_ConvertsCorrectly(string expectedWord)
        {
            // Convert the word to the actual matrix product
            var matrices = expectedWord
                .Select(i => RegularLanguageHelper.MatrixLookup[i]);

            // Calculate the input matrix
            var inputMatrix = Constants.Matrices.I;
            foreach (var matrix in matrices)
            {
                inputMatrix *= matrix;
            }
            // Make sure our conversion steps assured the same matrix is retained
            Assert.True(RegularLanguageHelper.IsEqual(expectedWord, inputMatrix));
            // Finally, run the conversion
            var matrixProduct = MathematicalHelper.ConvertMatrixToCanonicalString(inputMatrix);
            // Ensure the same matrix product in canonical form is return
            Assert.True(RegularLanguageHelper.IsEqual(matrixProduct, inputMatrix));
            Assert.Equal(expectedWord, matrixProduct);
        }

        [Theory]
        [InlineData("RRSSR", "")]
        [InlineData("RSSRR", "")]
        [InlineData("SRRRS", "")]
        [InlineData("XXXXXXXXXXSSSSSSSSSSSSRRRRRRRRRRRR", "")]
        [InlineData("RRSRSRRRSRRSR", "X")]
        [InlineData("XRRSRRSRR", "XRRSRRSRR")]
        [InlineData("SRRSRRRSRSRRSS", "XRR")]
        public void SimplifyToCanonicalForm_SimplifiesCorrectly(string input, string expected)
        {
            // Convert the letters to their respective matrix ID. This is the word.
            var inputWord = new LinkedList<GeneratorMatrixIdentifier>(input
                .Select(w => (GeneratorMatrixIdentifier)Enum.Parse(typeof(GeneratorMatrixIdentifier), w.ToString())));

            var expectedWord = expected
                .Select(w => (GeneratorMatrixIdentifier)Enum.Parse(typeof(GeneratorMatrixIdentifier), w.ToString()));

            var simplifiedWord = MathematicalHelper.SimplifyToCanonicalForm(inputWord);
            Assert.Equal(expectedWord, simplifiedWord);
        }
    }
}