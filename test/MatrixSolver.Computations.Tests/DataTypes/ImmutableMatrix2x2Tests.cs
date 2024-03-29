
using Xunit;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;
using System;

namespace MatrixSolver.Computations.Tests.DataTypes
{
    public class ImmutableMatrix2x2Tests
    {

        [Theory]
        [InlineData(1, 2, 3, 4)]
        [InlineData(0, -4324, 25478234, 4324)]
        [InlineData(-342678, -23473, -3274328, 3312312)]
        [InlineData(0, 0, 0, 0)]
        public void Constructor_Initialises_IfValidValues(int value1, int value2, int value3, int value4)
        {
            var values = new BigRational[2, 2]
            {
                {value1, value2},
                {value3, value4}
            };

            // No exception thrown is a pass.
            var matrix = new ImmutableMatrix2x2(values);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Assert.Equal(values[i, j], matrix.UnderlyingValues[i, j]);
                }
            }
        }

        [Theory]
        [InlineData(2, 3)]
        [InlineData(3, 2)]
        [InlineData(4, 1)]
        [InlineData(1, 4)]
        public void Constructor_Throws_IfWrongSizeMatrixSupplied(int sizeX, int sizeY)
        {
            var values = new BigRational[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    values[i, j] = 0;
                }
            }

            Assert.Throws<ArgumentException>(() => new ImmutableMatrix2x2(values));
        }

        [Fact]
        public void Multiply_MultipliesTwoMatricesTogether()
        {
            var values1 = new BigRational[2, 2]
            {
                {2, 5},
                {1, 4}
            };
            var values2 = new BigRational[2, 2]
            {
                {3, 1},
                {2, 0}
            };

            var matrix1 = new ImmutableMatrix2x2(values1);
            var matrix2 = new ImmutableMatrix2x2(values2);

            var resultingMatrix = matrix1 * matrix2;
            var resultingMatrixInverse = matrix2 * matrix1;

            Assert.Equal(16, resultingMatrix.UnderlyingValues[0, 0]);
            Assert.Equal(2, resultingMatrix.UnderlyingValues[0, 1]);
            Assert.Equal(11, resultingMatrix.UnderlyingValues[1, 0]);
            Assert.Equal(1, resultingMatrix.UnderlyingValues[1, 1]);

            Assert.Equal(7, resultingMatrixInverse.UnderlyingValues[0, 0]);
            Assert.Equal(19, resultingMatrixInverse.UnderlyingValues[0, 1]);
            Assert.Equal(4, resultingMatrixInverse.UnderlyingValues[1, 0]);
            Assert.Equal(10, resultingMatrixInverse.UnderlyingValues[1, 1]);
        }

        [Fact]
        public void Multiply_MultipliesAMatrixAndVector()
        {
            var matrixValues = new BigRational[2, 2]
            {
                {3, 4},
                {5, 6}
            };
            var vectorValues = new BigRational[2]
            {
                1,
                5
            };

            var matrix = new ImmutableMatrix2x2(matrixValues);
            var vector = new ImmutableVector2D(vectorValues);

            var resultingVector = matrix * vector;

            Assert.Equal(23, resultingVector.UnderlyingVector[0]);
            Assert.Equal(35, resultingVector.UnderlyingVector[1]);
        }

        [Fact]
        public void Determinant_FindsDeterminant()
        {
            var values = new BigRational[2, 2]
            {
                {2, 5},
                {1, 4}
            };
            var matrix = new ImmutableMatrix2x2(values);

            var determinant = matrix.Determinant();

            Assert.Equal(3, determinant);
        }

        [Fact]
        public void Inverse_InvertsMatrix()
        {
            var values = new BigRational[2, 2]
            {
                {2, 5},
                {1, 4}
            };
            var matrix = new ImmutableMatrix2x2(values);
            var inverse = matrix.Inverse();

            Assert.Equal(new BigRational(4, 3), inverse.UnderlyingValues[0, 0]);
            Assert.Equal(new BigRational(-5, 3), inverse.UnderlyingValues[0, 1]);
            Assert.Equal(new BigRational(-1, 3), inverse.UnderlyingValues[1, 0]);
            Assert.Equal(new BigRational(2, 3), inverse.UnderlyingValues[1, 1]);

            Assert.Equal(Constants.Matrices.I, matrix * inverse);
        }

        [Fact]
        public void Inverse_Throws_IfDeterminantZero()
        {
            var values = new BigRational[2, 2]
            {
                {2, 8},
                {1, 4}
            };
            var matrix = new ImmutableMatrix2x2(values);

            Assert.Throws<InvalidOperationException>(() => matrix.Inverse());
        }
    }
}