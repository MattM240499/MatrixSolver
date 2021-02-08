namespace MatrixSolver.Computations.DataTypes
{
    /// <summary>
    /// A 2D Array that is read only.
    /// </summary>
    public interface IReadOnlyTwoDimensionalArray<T>
    {
        T this[int i, int j] { get; }

        int Length { get; }
    }
}