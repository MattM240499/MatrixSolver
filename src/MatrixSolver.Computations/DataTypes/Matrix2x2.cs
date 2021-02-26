
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
        /// Returns a new <see cref="Matrix2x2" /> which is the sum of this and another <see cref="Matrix2x2" />
        /// </summary>
        public Matrix2x2 Add(IMatrix2x2 right)
        {
            _underlyingValues.Update(base.AddBase(right));
            return this;
        }

        /// <summary>
        /// Returns a new <see cref="Matrix2x2" /> which is the product of this and another <see cref="Matrix2x2" /> to the left.
        /// </summary>
        public Matrix2x2 MultiplyLeft(IMatrix2x2 left)
        {
            _underlyingValues.Update(base.MultiplyBase(left, this));
            return this;
        }

        /// <summary>
        /// Returns a new <see cref="Matrix2x2" /> which is the product of this and another <see cref="Matrix2x2" /> to the right.
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
        /// Calculates the Inverse of the matrix. 
        /// Throws if the matrix has determinant zero.
        /// </summary>
        /// <returns>A new <see cref="Matrix2x2"/> representing the result of the computation</returns>
        public Matrix2x2 Inverse()
        {
            _underlyingValues.Update(base.InverseBase());
            return this;
        }

        /// <summary>
        /// Evaluates the matrix to a given power.
        /// Throws if the power is negative and the determinant is zero.
        /// </summary>
        /// <returns>A new <see cref="Matrix2x2"/> representing the result of the computation</returns>
        public Matrix2x2 Pow(BigInteger value)
        {
            if (value < 0)
            {
                return this.Inverse().Pow(0 - value);
            }
            if (value == 0)
            {
                return Copy(Constants.Matrices.I);
            }
            else if (value == 1)
            {
                return this;
            }

            var matrix = this;
            for (int i = 1; i < value; i++)
            {
                matrix = this * matrix;
            }
            return matrix;
        }

        public static Matrix2x2 operator +(Matrix2x2 left, IMatrix2x2 right)
        {
            return left.Add(right);
        }

        public static Matrix2x2 operator *(Matrix2x2 left, IMatrix2x2 right)
        {
            return left.MultiplyRight(right);
        }

        public static ImmutableVector2D operator *(Matrix2x2 left, ImmutableVector2D right)
        {
            return left.Multiply(right);
        }

        public static Matrix2x2 operator ^(Matrix2x2 left, BigInteger right)
        {
            return left.Pow(right);
        }
    }
}