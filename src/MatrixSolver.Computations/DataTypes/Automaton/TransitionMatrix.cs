
using System;
using System.Collections.Generic;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    public class TransitionMatrix<T> : IReadOnlyTransitionMatrix<T> where T : notnull
    {
        private readonly Dictionary<int, Dictionary<T, SortedSet<int>>> _transitionMatrix;

        public TransitionMatrix()
        {
            _transitionMatrix = new Dictionary<int, Dictionary<T, SortedSet<int>>>();
        }

        private TransitionMatrix(Dictionary<int, Dictionary<T, SortedSet<int>>> transitionMatrix)
        {
            _transitionMatrix = transitionMatrix;
        }

        /// <summary>
        /// Adds a transition between two states for a given <paramref name="symbol" />
        /// </summary>
        public bool AddTransition(int fromState, int toState, T symbol)
        {
            if (!_transitionMatrix.TryGetValue(fromState, out var transitionDict))
            {
                transitionDict = new Dictionary<T, SortedSet<int>>();
                _transitionMatrix[fromState] = transitionDict;
            }
            if (!transitionDict.TryGetValue(symbol, out var stateList))
            {
                stateList = new SortedSet<int>();
                transitionDict[symbol] = stateList;
            }

            return stateList.Add(toState);
        }

        /// <summary>
        /// Removes a given state, and any outgoing and incoming transitions to it.
        /// </summary>
        public void RemoveState(int state, bool skipIncomingTransitions = false)
        {
            _transitionMatrix.Remove(state);
            if(skipIncomingTransitions)
            {
                return;
            }
            foreach(var transictionDict in _transitionMatrix.Values)
            {
                foreach(var set in transictionDict.Values)
                {
                    set.Remove(state);
                }
            }
        }

        /// <summary>
        /// Removes a transition between two states for a given <paramref name="symbol" />
        /// </summary>
        public bool RemoveTransition(int fromState, int toState, T symbol)
        {
            if (!_transitionMatrix.TryGetValue(fromState, out var transitionDict))
            {
                return false;
            }
            if (!transitionDict.TryGetValue(symbol, out var stateList))
            {
                return false;
            }
            return stateList.Remove(toState);
        }

        /// <summary>
        /// Get all states from <paramref name="fromState" /> that can be reached with <paramref name="symbol" />
        /// </summary>
        public IReadOnlyCollection<int> GetStates(int fromState, T symbol)
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

        public TransitionMatrix<T> Clone()
        {
            var newTransitionMatrix = new Dictionary<int, Dictionary<T, SortedSet<int>>>();
            foreach(var kvp in _transitionMatrix)
            {
                var newDict = new Dictionary<T, SortedSet<int>>();
                newTransitionMatrix[kvp.Key] = newDict;
                foreach(var transitionKvp in kvp.Value)
                {
                    newDict.Add(transitionKvp.Key, new SortedSet<int>(transitionKvp.Value));
                }
            }
            return new TransitionMatrix<T>(newTransitionMatrix);
        }
    }
}