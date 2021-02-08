
using System;
using Extreme.Mathematics;

namespace MatrixSolver.Computations.DataTypes
{
    /// <summary>
    /// A 2x2 Matrix which is Immutable.
    /// </summary>
    public class ImmutableMatrix2x2 : IMatrix2x2
    {
        /// <summary>
        /// The underlying matrix values
        /// </summary>
        public IReadOnlyTwoDimensionalArray<BigRational> UnderlyingValues { get; }
        private const int Order = 2;

        public ImmutableMatrix2x2(BigRational[,] values)
        {
            if (values.GetLongLength(0) != Order || values.GetLongLength(1) != Order)
            {
                throw new ArgumentException("Could not create matrix as values provided were not of size 2x2");
            }
            // Check no null values
            for (int i = 0; i < Order; i++)
            {
                for (int j = 0; j < Order; j++)
                {
                    if (values[i, j] == null)
                    {
                        throw new ArgumentException($"Element [{i},{j}] was null");
                    }
                }
            }
            // TODO: Might want to Clone the array
            UnderlyingValues = new TwoDimensionalArray<BigRational>(values);
        }

        /// <summary>
        /// Returns a new <see cref="ImmutableMatrix2x2" /> which is the sum of this and another <see cref="ImmutableMatrix2x2" />
        /// </summary>
        public ImmutableMatrix2x2 Add(IMatrix2x2 right)
        {
            var values = new BigRational[Order, Order];
            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    values[rowIndex, columnIndex] = this.UnderlyingValues[rowIndex, columnIndex] + right.UnderlyingValues[rowIndex, columnIndex];
                }
            }
            return new ImmutableMatrix2x2(values);
        }

        /// <summary>
        /// Returns a new <see cref="ImmutableMatrix2x2" /> which is the product of this and another <see cref="ImmutableMatrix2x2" />
        /// </summary>
        public ImmutableMatrix2x2 Multiply(IMatrix2x2 right)
        {
            var values = new BigRational[Order, Order];

            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    BigRational value = 0;
                    for (int i = 0; i < Order; i++)
                    {
                        value += this.UnderlyingValues[rowIndex, i] * right.UnderlyingValues[i, columnIndex];
                    }

                    values[rowIndex, columnIndex] = value;
                }
            }

            return new ImmutableMatrix2x2(values);
        }

        /// <summary>
        /// Multiply the matrix by a given vector.
        /// </summary>
        /// <returns>A new <see cref="ImmutableVector2D"/> with the result of the computation</returns>
        public ImmutableVector2D Multiply(ImmutableVector2D right)
        {
            var vector = new BigRational[Order];

            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                BigRational value = 0;
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    value += this.UnderlyingValues[rowIndex, columnIndex] * right.UnderlyingVector[columnIndex];
                }
                vector[rowIndex] = value;
            }
            return new ImmutableVector2D(vector);
        }

        /// <summary>
        /// Evaluates the matrix to a given power.
        /// Throws if the power is negative and the determinant is zero.
        /// </summary>
        /// <returns>A new <see cref="ImmutableMatrix2x2"/> representing the result of the computation</returns>
        public ImmutableMatrix2x2 Pow(BigInteger value)
        {
            if (value < 0)
            {
                return this.Inverse().Pow(0 - value);
            }
            if (value == 0)
            {
                return Constants.Matrices.I;
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

        /// <summary>
        /// Calculates the Inverse of the matrix. 
        /// Throws if the matrix has determinant zero.
        /// </summary>
        /// <returns>A new <see cref="ImmutableMatrix2x2"/> representing the result of the computation</returns>
        public ImmutableMatrix2x2 Inverse()
        {
            var det = Determinant();
            if (det == 0)
            {
                throw new InvalidOperationException("Cannot Inverse Matrix as Determinant is 0");
            }
            var inverseMatrix = new BigRational[Order, Order];
            inverseMatrix[0, 0] = UnderlyingValues[1, 1] / det;
            inverseMatrix[0, 1] = -UnderlyingValues[0, 1] / det;
            inverseMatrix[1, 0] = -UnderlyingValues[1, 0] / det;
            inverseMatrix[1, 1] = UnderlyingValues[0, 0] / det;
            return new ImmutableMatrix2x2(inverseMatrix);
        }

        /// <summary>
        /// Calculates the determinant of the matrix
        /// </summary>
        public BigRational Determinant()
        {
            return this.UnderlyingValues[0, 0] * this.UnderlyingValues[1, 1] - this.UnderlyingValues[0, 1] * this.UnderlyingValues[1, 0];
        }

        public override string ToString()
        {
            var returnString = "[";
            for (int i = 0; i < Order; i++)
            {
                returnString += "[";
                for (int j = 0; j < Order; j++)
                {
                    returnString += this.UnderlyingValues[i, j].ToString();
                    if (j != Order - 1)
                    {
                        returnString += ", ";
                    }
                }
                returnString += "]";
                if (i != Order - 1)
                {
                    returnString += ", ";
                }
            }
            returnString += "]";

            return returnString;
        }

        public override int GetHashCode()
        {
            return this.UnderlyingValues.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            var objAsMatrix = obj as IMatrix2x2;
            if (objAsMatrix is null)
            {
                return false;
            }
            return Equals(objAsMatrix);
        }

        public bool Equals(IMatrix2x2 matrix)
        {
            if (matrix.UnderlyingValues.Length != this.UnderlyingValues.Length)
            {
                return false;
            }
            for (int i = 0; i < Order; i++)
            {
                for (int j = 0; j < Order; j++)
                {
                    if (matrix.UnderlyingValues[i, j] != this.UnderlyingValues[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
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

        public static ImmutableMatrix2x2 operator ^(ImmutableMatrix2x2 left, BigInteger right)
        {
            return left.Pow(right);
        }

        public static bool operator ==(ImmutableMatrix2x2 left, ImmutableMatrix2x2 right)
        {
            return Object.ReferenceEquals(left, right) || left.Equals(right);
        }

        public static bool operator !=(ImmutableMatrix2x2 left, ImmutableMatrix2x2 right)
        {
            return !Object.ReferenceEquals(left, right) && !left.Equals(right);
        }
    }
}