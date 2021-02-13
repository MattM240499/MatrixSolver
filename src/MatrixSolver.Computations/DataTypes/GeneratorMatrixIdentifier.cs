
namespace MatrixSolver.Computations.DataTypes
{
    public enum GeneratorMatrixIdentifier
    {
        /// <summary>
        /// Placeholder.
        /// </summary> 
        None = 0,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.T" />
        /// </summary>
        T = 1,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.S" />
        /// </summary>
        S = 2,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.R" />
        /// </summary>
        R = 3,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.X" />
        /// </summary>
        X = 4,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.T" />^-1
        /// </summary>
        TInverse = 5,
        /// <summary>
        /// Identifier for the matrix <see cref="Constants.Matrices.S" />^-1
        /// </summary>
        SInverse = 6,
    }
}