using System.Collections.Generic;

namespace MatrixSolver.DataTypes.Automata
{
    /// <summary>
    /// A transition matrix that is readonly.
    /// </summary>
    public interface IReadOnlyTransitionMatrix<T> where T : notnull
    {
        /// <summary>
        /// Return a list of all states that can be reached from a given state with a given symbol
        /// </summary>
        IReadOnlyList<int> GetStates(int fromState, T symbol);
    }
}