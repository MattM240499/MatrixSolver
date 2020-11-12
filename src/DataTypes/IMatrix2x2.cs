
using Extreme.Mathematics;

namespace MatrixSolver.DataTypes
{
    /// <summary>
    /// A 2x2 Matrix
    /// </summary>
    public interface IMatrix2x2
    {
        /// <summary>
        /// The underlying matrix values
        /// </summary>
        IReadOnlyTwoDimensionalArray<BigRational> UnderlyingValues { get; }
    }
}