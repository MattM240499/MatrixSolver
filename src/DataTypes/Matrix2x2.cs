
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Extreme.Mathematics;

namespace MatrixSolver.DataTypes
{
    /// <summary>
    /// A 2x2 Matrix
    /// </summary>
    public class Matrix2x2
    {
        public BigRational[,] UnderlyingValues { get; }
        private const int Order = 2;

        public Matrix2x2(BigRational[,] values)
        {
            if (values.Length != 4) throw new ArgumentException($"Expected 4 values, but found {values.Length}");
            UnderlyingValues = values;
        }

        /// <summary>
        /// Returns a new vector which is the sum of two vectors together
        /// </summary>
        public Matrix2x2 Add(Matrix2x2 right)
        {
            var values = new BigRational[Order, Order];
            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    values[rowIndex, columnIndex] = this.UnderlyingValues[rowIndex, columnIndex] + right.UnderlyingValues[rowIndex, columnIndex];
                }
            }
            return new Matrix2x2(values);
        }

        /// <summary>
        /// Returns a new vector which is equal to this vector minus anright vector
        /// </summary>
        public Matrix2x2 Subtract(Matrix2x2 right)
        {
            var values = new BigRational[Order, Order];
            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    values[rowIndex, columnIndex] = this.UnderlyingValues[rowIndex, columnIndex] - right.UnderlyingValues[rowIndex, columnIndex];
                }
            }
            return new Matrix2x2(values);
        }

        /// <summary>
        /// Returns a new vector which is equal to this vector minus anright vector
        /// </summary>
        public Matrix2x2 Multiply(Matrix2x2 right)
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

            return new Matrix2x2(values);
        }

        public Vector2D Multiply(Vector2D right)
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
            return new Vector2D(vector);
        }

        public Matrix2x2 Pow(BigInteger value)
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

        public Matrix2x2 Inverse()
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
            return new Matrix2x2(inverseMatrix);
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

        public bool Equals(Matrix2x2 matrix)
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

        public BigRational Determinant()
        {
            return this.UnderlyingValues[0, 0] * this.UnderlyingValues[1, 1] - this.UnderlyingValues[0, 1] * this.UnderlyingValues[1, 0];
        }

        public static Matrix2x2 operator +(Matrix2x2 left, Matrix2x2 right)
        {
            return left.Add(right);
        }

        public static Matrix2x2 operator -(Matrix2x2 left, Matrix2x2 right)
        {
            return left.Subtract(right);
        }

        public static Matrix2x2 operator *(Matrix2x2 left, Matrix2x2 right)
        {
            return left.Multiply(right);
        }

        public static Vector2D operator *(Matrix2x2 left, Vector2D right)
        {
            return left.Multiply(right);
        }

        public static Matrix2x2 operator ^(Matrix2x2 left, BigInteger right)
        {
            return left.Pow(right);
        }

        public static bool operator ==(Matrix2x2 left, Matrix2x2 right)
        {
            return Object.ReferenceEquals(left, right) || left.Equals(right);
        }

        public static bool operator !=(Matrix2x2 left, Matrix2x2 right)
        {
            return !Object.ReferenceEquals(left, right) && !left.Equals(right);
        }
    }
}