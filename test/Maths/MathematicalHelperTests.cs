
using System;
using System.Linq;
using MatrixSolver.DataTypes;
using MatrixSolver.Maths;
using Xunit;

namespace MatrixSolver.Tests.Maths
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
        [InlineData(2, 3, 1, -1, 1)]
        [InlineData(15, 5, 5, 0, 1)]
        [InlineData(-7, 8, 1, 1, 1)]
        [InlineData(-16, -4, 4, 0, -1)]
        [InlineData(-12, -8, 4, -1, 1)]
        [InlineData(0, 7, 7, 0, 1)]
        [InlineData(-7, 0, 7, -1, 0)]
        public void ExtendedEuclideanAlgorithm_CalculatesSolution(int number1, int number2, int expectedGcd, int expectedS, int expectedT)
        {
            var values = MathematicalHelper.ExtendedEuclideanAlgorithm(number1, number2);

            Assert.Equal((expectedGcd, expectedS, expectedT), values);
        }

        // We will pass in matrices in Canonical form. We expect the same form back
        [Theory]
        [InlineData("XSRRSRSR")]
        [InlineData("SRRSRSRSRSRSRSRRSRSRSRSRSRSRSRSRSRRSRSRSRSRRSRRSRR")]
        [InlineData("XRRSRRSRR")]
        public void ConvertMatrixToGeneratorFormAsString_ConvertsCorrectly(string word)
        {
            // Convert the letters to their respective matrix ID. This is the word.
            var expectedWord = word
                .Select(w => (GeneratorMatrixIdentifier)Enum.Parse(typeof(GeneratorMatrixIdentifier), w.ToString()));
            // Convert the word to the actual matrix product
            var matrices = expectedWord
                .Select(i => Constants.Matrices.MatrixIdentifierDictionary[i]);
            
            // Calculate the input matrix
            var inputMatrix = Constants.Matrices.I;
            foreach(var matrix in matrices)
            {
                inputMatrix *= matrix;
            }
            // Make sure our conversion steps assured the same matrix is retained
            Assert.True(MathematicalHelper.IsEqual(expectedWord.ToArray(), inputMatrix));
            // Finally, run the conversion
            var matrixProduct = MathematicalHelper.ConvertMatrixToGeneratorFormAsString(inputMatrix);
            // Ensure the same matrix product in canonical form is return
            Assert.True(MathematicalHelper.IsEqual(matrixProduct, inputMatrix));
            Assert.Equal(expectedWord, matrixProduct);
        }

        [Theory]
        [InlineData("XXXXXXXXXXSSSSSSSSSSSSRRRRRRRRRRRR", "")]
        [InlineData("RRSRSRRRSRRSR", "X")]
        [InlineData("XRRSRRSRR", "XRRSRRSRR")]
        public void SimplifyToCanonicalForm_SimplifiesCorrectly(string input, string expected)
        {
            // Convert the letters to their respective matrix ID. This is the word.
            var inputWord = input
                .Select(w => (GeneratorMatrixIdentifier)Enum.Parse(typeof(GeneratorMatrixIdentifier), w.ToString()))
                .ToList();

            var expectedWord = expected
                .Select(w => (GeneratorMatrixIdentifier)Enum.Parse(typeof(GeneratorMatrixIdentifier), w.ToString()));

            var simplifiedWord = MathematicalHelper.SimplifyToCanonicalForm(inputWord);
            Assert.Equal(expectedWord, simplifiedWord);
        }
    }
}