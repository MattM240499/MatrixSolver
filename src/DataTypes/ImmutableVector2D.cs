
using System;
using System.Collections.ObjectModel;
using Extreme.Mathematics;

namespace MatrixSolver.DataTypes
{
    /// <summary>
    /// A vector in 2d space
    /// </summary>
    public class ImmutableVector2D
    {
        public ReadOnlyCollection<BigRational> UnderlyingVector { get; }
        public const int Order = 2;
        public ImmutableVector2D(BigRational[] values)
        {
            if (values.Length != 2) throw new ArgumentException($"Received {values.Length} but expected 2");
            UnderlyingVector = Array.AsReadOnly(values);
        }

        /// <summary>
        /// Returns a new vector which is the sum of two vectors together
        /// </summary>
        public ImmutableVector2D Add(ImmutableVector2D right)
        {
            return new ImmutableVector2D(new[] { this.UnderlyingVector[0] + right.UnderlyingVector[0], this.UnderlyingVector[1] + right.UnderlyingVector[1] });
        }

        /// <summary>
        /// Returns a new vector which is equal to this vector minus anright vector
        /// </summary>
        public ImmutableVector2D Subtract(ImmutableVector2D right)
        {
            return new ImmutableVector2D(new[] { this.UnderlyingVector[0] - right.UnderlyingVector[0], this.UnderlyingVector[1] - right.UnderlyingVector[1] });
        }

        public ImmutableVector2D Multiply(BigRational value)
        {
            return new ImmutableVector2D(new[] { this.UnderlyingVector[0] * value, this.UnderlyingVector[1] * value });
        }

        public override string ToString()
        {
            var returnString = "[";
            for (int i = 0; i < Order; i++)
            {
                returnString += UnderlyingVector[i].ToString();
                if (i != Order - 1)
                {
                    returnString += ", ";
                }
            }
            returnString += "]";

            return returnString;
        }

        public bool Equals(ImmutableVector2D other)
        {
            if (this.UnderlyingVector.Count != other.UnderlyingVector.Count)
            {
                return false;
            }

            for (int i = 0; i < this.UnderlyingVector.Count; i++)
            {
                if (this.UnderlyingVector[i] != other.UnderlyingVector[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static ImmutableVector2D operator +(ImmutableVector2D left, ImmutableVector2D right)
        {
            return left.Add(right);
        }

        public static ImmutableVector2D operator -(ImmutableVector2D left, ImmutableVector2D right)
        {
            return left.Subtract(right);
        }

        public static ImmutableVector2D operator *(ImmutableVector2D left, BigRational right)
        {
            return left.Multiply(right);
        }

        public static ImmutableVector2D operator *(BigRational left, ImmutableVector2D right)
        {
            return right.Multiply(left);
        }
    }
}