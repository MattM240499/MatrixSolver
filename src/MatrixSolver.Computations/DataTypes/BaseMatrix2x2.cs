
using System;
using Extreme.Mathematics;

namespace MatrixSolver.Computations.DataTypes
{
    /// <summary>
    /// Base Matrix class. Contains standard matrix operations.
    /// </summary>
    public abstract class BaseMatrix2x2 : IMatrix2x2
    {
        public abstract IReadOnlyTwoDimensionalArray<BigRational> UnderlyingValues { get; }
        protected int Order { get; } = 2;

        protected void ValidateValues(BigRational[,] values)
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
        }

        protected BigRational[,] MultiplyBase(IMatrix2x2 left, IMatrix2x2 right)
        {
            var values = new BigRational[Order, Order];

            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    BigRational value = 0;
                    for (int i = 0; i < Order; i++)
                    {
                        value += left.UnderlyingValues[rowIndex, i] * right.UnderlyingValues[i, columnIndex];
                    }

                    values[rowIndex, columnIndex] = value;
                }
            }
            return values;
        }

        protected BigRational[] MultiplyBase(ImmutableVector2D right)
        {
            var vector = new BigRational[Order];

            for (int rowIndex = 0; rowIndex < Order; rowIndex++)
            {
                BigRational value = 0;
                for (int columnIndex = 0; columnIndex < Order; columnIndex++)
                {
                    value += UnderlyingValues[rowIndex, columnIndex] * right.UnderlyingVector[columnIndex];
                }
                vector[rowIndex] = value;
            }
            return vector;
        }

        protected BigRational[,] InverseBase()
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
            return inverseMatrix;
        }

        public BigRational Determinant()
        {
            return UnderlyingValues[0, 0] * UnderlyingValues[1, 1] - UnderlyingValues[0, 1] * UnderlyingValues[1, 0];
        }

        public override string ToString()
        {
            var returnString = "[";
            for (int i = 0; i < Order; i++)
            {
                returnString += "[";
                for (int j = 0; j < Order; j++)
                {
                    returnString += UnderlyingValues[i, j].ToString();
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
            return UnderlyingValues.GetHashCode();
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
                    if (matrix.UnderlyingValues[i, j] != UnderlyingValues[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool operator ==(BaseMatrix2x2 left, IMatrix2x2 right)
        {
            return ReferenceEquals(left, right) || left.Equals(right);
        }

        public static bool operator !=(BaseMatrix2x2 left, IMatrix2x2 right)
        {
            return !ReferenceEquals(left, right) && !left.Equals(right);
        }
    }
}
