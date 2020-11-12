
namespace MatrixSolver.DataTypes
{
    /// <summary>
    /// A Wrapper for a Two Dimensional Array. Allows exposing a readonly interface for the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TwoDimensionalArray<T> : IReadOnlyTwoDimensionalArray<T>
    {
        private readonly T[,] _underlyingArray;
        public TwoDimensionalArray(T[,] underlyingArray)
        {
            _underlyingArray = underlyingArray;
        }

        public T this[int i, int j]
        {
            get { return _underlyingArray[i, j]; }
            set { _underlyingArray[i, j] = value; }
        }

        public int Length => _underlyingArray.Length;
    }    
}