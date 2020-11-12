
using System;
using Extreme.Mathematics;
using MatrixSolver.DataTypes;
using Xunit;

namespace MatrixSolver.Tests.DataTypes
{
    public class ImmutableVector2DTests
    {
        [Theory]
        [InlineData(1,2)]
        [InlineData(-2,4324)]
        [InlineData(0,0)]
        [InlineData(0, -23)]
        public void Constructor_AllowsValidVectors(int value1, int value2)
        {
            var values = new BigRational[]
            {
                value1,
                value2,
            };

            var vector = new ImmutableVector2D(values);

            Assert.Equal(value1, vector.UnderlyingVector[0]);
            Assert.Equal(value2, vector.UnderlyingVector[1]);
        }

        [Fact]
        public void Constructor_Throws_IfTooFewValues()
        {
            var values = new BigRational[]
            {
                123,
            };

            Assert.Throws<ArgumentException>(() => new ImmutableVector2D(values));
        }

        [Fact]
        public void Constructor_Throws_IfTooManyValues()
        {
            var values = new BigRational[]
            {
                123,
                456,
                789,
            };

            Assert.Throws<ArgumentException>(() => new ImmutableVector2D(values));
        }

        [Fact]
        public void Add_AddsValues()
        {
            var values1 = new BigRational[]
            {
                5,
                7,
            };
            var values2 = new BigRational[]
            {
                1,
                9,
            };

            var vector1 = new ImmutableVector2D(values1);
            var vector2 = new ImmutableVector2D(values2);

            var resultingVector = vector1 + vector2;

            Assert.Equal(6, resultingVector.UnderlyingVector[0]);
            Assert.Equal(16, resultingVector.UnderlyingVector[1]);
        }

        [Fact]
        public void Subtract_SubtractsValues()
        {
            var values1 = new BigRational[]
            {
                5,
                7,
            };
            var values2 = new BigRational[]
            {
                1,
                9,
            };

            var vector1 = new ImmutableVector2D(values1);
            var vector2 = new ImmutableVector2D(values2);

            var resultingVector = vector1 - vector2;

            Assert.Equal(4, resultingVector.UnderlyingVector[0]);
            Assert.Equal(-2, resultingVector.UnderlyingVector[1]);
        }

        [Fact]
        public void MultiplyConstant_Multiplies()
        {
            var values = new BigRational[]
            {
                5,
                7,
            };
            var scalar = 9;

            var vector = new ImmutableVector2D(values);

            var resultingVector = vector * scalar;
            Assert.Equal(45, resultingVector.UnderlyingVector[0]);
            Assert.Equal(63, resultingVector.UnderlyingVector[1]);
        }
    }
}