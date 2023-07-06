
using System;
using System.Collections.Generic;
using Extreme.Mathematics;

namespace MatrixSolver.Computations.DataTypes
{
    /// <summary>
    /// A 2x2 Matrix. Operations on this matrix will change the underlying values.
    /// </summary>
    public class Matrix2x2 : BaseMatrix2x2
    {
        private TwoDimensionalArray<BigRational> _underlyingValues;
        public override IReadOnlyTwoDimensionalArray<BigRational> UnderlyingValues => _underlyingValues;

        public Matrix2x2(BigRational[,] values)
        {
            base.ValidateValues(values);
            _underlyingValues = new TwoDimensionalArray<BigRational>(values);
        }

        public static Matrix2x2 Copy(IMatrix2x2 matrix)
        {
            var newArray = new BigRational[2,2];
            for(int i = 0; i < 2; i++)
            {
                for(int j = 0; j < 2; j++)
                {
                    newArray[i,j] = matrix.UnderlyingValues[i,j];
                }
            }
            
            return new Matrix2x2(newArray);
        }

        /// <summary>
        /// Updates the current matrix with the product of this and another <see cref="Matrix2x2" /> to the left.
        /// </summary>
        public Matrix2x2 MultiplyLeft(IMatrix2x2 left)
        {
            _underlyingValues.Update(base.MultiplyBase(left, this));
            return this;
        }

        /// <summary>
        /// Updates the current matrix with the product of this and another <see cref="Matrix2x2" /> to the right.
        /// </summary>
        public Matrix2x2 MultiplyRight(IMatrix2x2 right)
        {
            _underlyingValues.Update(base.MultiplyBase(this, right));
            return this;
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
        /// Updates the current matrix with the inverse of this matrix. 
        /// Throws if the matrix has determinant zero.
        /// </summary>
        public Matrix2x2 Inverse()
        {
            _underlyingValues.Update(base.InverseBase());
            return this;
        }

        public static ImmutableVector2D operator *(Matrix2x2 left, ImmutableVector2D right)
        {
            return left.Multiply(right);
        }
    }
}