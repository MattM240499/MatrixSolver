
using System;
using System.Collections.Generic;

namespace MatrixSolver.DataTypes.Automata
{
    public class TransitionMatrix<T> : IReadOnlyTransitionMatrix<T> where T : notnull
    {
        private readonly Dictionary<int, Dictionary<T, List<int>>> _transitionMatrix;

        public TransitionMatrix()
        {
            _transitionMatrix = new Dictionary<int, Dictionary<T, List<int>>>();
        }

        public bool Add(int fromState, int toState, T symbol)
        {
            if (!_transitionMatrix.TryGetValue(fromState, out var transitionDict))
            {
                transitionDict = new Dictionary<T, List<int>>();
                _transitionMatrix[fromState] = transitionDict;
            }
            if (!transitionDict.TryGetValue(symbol, out var stateList))
            {
                stateList = new List<int>();
                transitionDict[symbol] = stateList;
            }

            // TODO: Potential binary search here
            if (stateList.Contains(toState))
            {
                return false;
            }
            stateList.Add(toState);
            return true;
            // Otherwise, the transition already exists.
        }

        /// <summary>
        /// Get all states from <paramref name="fromState" /> that can be reached with <paramref name="symbol" />
        /// </summary>
        public IReadOnlyList<int> GetStates(int fromState, T symbol)
        {
            if(_transitionMatrix.TryGetValue(fromState, out var transitionDict))
            {
                if(transitionDict.TryGetValue(symbol, out var states))
                {
                    return states;
                }
            }
            return Array.Empty<int>();
        }
    }
}