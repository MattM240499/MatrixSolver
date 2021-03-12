
using System;
using Extreme.Mathematics;

namespace MatrixSolver.Computations.DataTypes
{
    /// <summary>
    /// A 2x2 Matrix which is Immutable.
    /// </summary>
    public class ImmutableMatrix2x2 : BaseMatrix2x2
    {
        public override IReadOnlyTwoDimensionalArray<BigRational> UnderlyingValues { get; }

        public ImmutableMatrix2x2(BigRational[,] values)
        {
            base.ValidateValues(values);
            UnderlyingValues = new TwoDimensionalArray<BigRational>(values);
        }

        /// <summary>
        /// Returns a new <see cref="ImmutableMatrix2x2" /> which is the sum of this and another <see cref="ImmutableMatrix2x2" />
        /// </summary>
        public ImmutableMatrix2x2 Add(IMatrix2x2 right)
        {
            return new ImmutableMatrix2x2(base.AddBase(right));
        }

        /// <summary>
        /// Returns a new <see cref="ImmutableMatrix2x2" /> which is the product of this and another <see cref="ImmutableMatrix2x2" />
        /// </summary>
        public ImmutableMatrix2x2 Multiply(IMatrix2x2 right)
        {
            return new ImmutableMatrix2x2(base.MultiplyBase(this, right));
        }

        /// <summary>
        /// Multiply the matrix by a given vector.
        /// </summary>
        /// <returns>A new <see cref="ImmutableVector2D"/> with the result of the computation</returns>
        public ImmutableVector2D Multiply(ImmutableVector2D right)
        {
            return new ImmutableVector2D(base.MultiplyBase(right));
        }

        /// <summary>
        /// Calculates the Inverse of the matrix. 
        /// Throws if the matrix has determinant zero.
        /// </summary>
        /// <returns>A new <see cref="ImmutableMatrix2x2"/> representing the result of the computation</returns>
        public ImmutableMatrix2x2 Inverse()
        {
            return new ImmutableMatrix2x2(base.InverseBase());
        }

        public static ImmutableMatrix2x2 operator +(ImmutableMatrix2x2 left, IMatrix2x2 right)
        {
            return left.Add(right);
        }

        public static ImmutableMatrix2x2 operator *(ImmutableMatrix2x2 left, IMatrix2x2 right)
        {
            return left.Multiply(right);
        }

        public static ImmutableVector2D operator *(ImmutableMatrix2x2 left, ImmutableVector2D right)
        {
            return left.Multiply(right);
        }
    }
}