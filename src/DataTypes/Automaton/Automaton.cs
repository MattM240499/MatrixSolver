
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.DataTypes.Automata
{
    /// <summary>
    /// An Automaton.
    /// </summary>
    /// <remarks> Not thread safe </remarks>
    public class Automaton
    {
        private static readonly CurrentStateListComparer _currentStateListComparer = new CurrentStateListComparer();
        /// <summary>A list of start states for this automaton</summary>
        private readonly HashSet<int> _startStates;
        /// <summary>A list of goal states for this automaton</summary>
        private readonly HashSet<int> _goalStates;
        /// <summary>A list of all states for this automaton</summary>
        private readonly HashSet<int> _states;
        /// <summary>An adjacency matrix of transition states</summary>
        private readonly TransitionMatrix<char> _transitionMatrix;
        private readonly HashSet<char> _alphabet;
        /// <summary>
        /// The Transition Matrix representing all possible paths between states
        /// </summary>
        public IReadOnlyTransitionMatrix<char> TransitionMatrix => _transitionMatrix;
        public IReadOnlyCollection<int> States => _states;
        public IReadOnlyCollection<int> StartStates => _startStates;
        public IReadOnlyCollection<int> GoalStates => _goalStates;
        public const char Epsilon = 'Ɛ';

        public Automaton(IReadOnlyCollection<char> alphabet)
        {
            _states = new HashSet<int>();
            _startStates = new HashSet<int>();
            _goalStates = new HashSet<int>();
            _transitionMatrix = new TransitionMatrix<char>();
            _alphabet = new HashSet<char>(alphabet);
        }

        /// <summary>
        /// Adds an empty state to the automaton. Throws if state Id already in use
        /// </summary>
        public void AddState(int currentStateId, bool isGoalState = false, bool isStartState = false)
        {
            if (!_states.Add(currentStateId))
            {
                throw new InvalidOperationException($"State with Id={currentStateId} already exists");
            }

            if (isGoalState)
            {
                _goalStates.Add(currentStateId);
            }
            if (isStartState)
            {
                _startStates.Add(currentStateId);
            }
        }

        /// <summary>
        /// Adds a one way transition between two existing states. Throws if the states do not exist, or a transition already exists for that state
        /// </summary>
        public void AddTransition(int fromcurrentStateId, int tocurrentStateId, char symbol)
        {
            if (!_alphabet.Contains(symbol) && symbol != Epsilon)
            {
                throw new InvalidOperationException($"Symbol {symbol} was not in alphabet: {String.Join(',', _alphabet)}");
            }
            if (!_states.TryGetValue(fromcurrentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {fromcurrentStateId} does not exist");
            }
            if (!_states.TryGetValue(tocurrentStateId, out var toState))
            {
                throw new InvalidOperationException($"State with id {tocurrentStateId} does not exist");
            }

            // Perform update
            _transitionMatrix.Add(fromcurrentStateId, tocurrentStateId, symbol);
        }

        public bool SetAsStartState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _startStates.Add(currentStateId);
        }

        public bool SetAsGoalState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _goalStates.Add(currentStateId);
        }

        public bool UnsetStartState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _startStates.Remove(currentStateId);
        }

        public bool UnsetGoalState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _goalStates.Remove(currentStateId);
        }

        public bool IsValidWord(IEnumerable<char> word)
        {
            if (_startStates.Count == 0 || _goalStates.Count == 0)
            {
                return false;
            }

            HashSet<int> currentStates = new HashSet<int>(_startStates);
            AddEpsilonStates(currentStates, currentStates);
            foreach (var symbol in word)
            {
                var nextStates = new HashSet<int>();
                foreach (var state in currentStates)
                {
                    var states = _transitionMatrix.GetStates(state, symbol);
                    foreach (var nextState in states)
                    {
                        nextStates.Add(nextState);
                    }
                }
                AddEpsilonStates(nextStates, nextStates);
                if (nextStates.Count == 0)
                {
                    // No states can be reached. Word is invalid.
                    return false;
                }
                currentStates = nextStates;
            }
            foreach(var state in currentStates)
            {
                if(_goalStates.Contains(state))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a new <see cref="Automaton" /> which is a DFA
        /// </summary>
        public Automaton ToDFA()
        {
            // A list of states. Each list will become a new state
            var automaton = new Automaton(_alphabet);
            int states = 0;
            var newAutomatonStates = new Dictionary<SortedSet<int>, int>(_currentStateListComparer);
            var stateStack = new Stack<SortedSet<int>>();

            // Setup subprocedure
            Func<SortedSet<int>, int> AddNewState = (SortedSet<int> set) =>
            {
                var currentStateId = states++;
                stateStack.Push(set);
                newAutomatonStates.Add(set, currentStateId);
                bool isStartState = false;
                bool isGoalState = false;
                foreach (var currentState in set)
                {
                    if (this._startStates.Contains(currentState))
                    {
                        isStartState = true;
                        if (isGoalState)
                        {
                            break;
                        }
                    }
                    if (this._goalStates.Contains(currentState))
                    {
                        isGoalState = true;
                        if (isStartState)
                        {
                            break;
                        }
                    }
                }
                automaton.AddState(currentStateId, isStartState: isStartState, isGoalState: isGoalState);
                return currentStateId;
            };

            // Begin DFA calculation
            var set = new SortedSet<int>(_startStates);
            AddEpsilonStates(set, set);
            AddNewState(set);
            while (stateStack.TryPop(out var currentStateList))
            {
                var currentStateId = newAutomatonStates[currentStateList];
                foreach (var symbol in _alphabet)
                {
                    var reachableStates = new SortedSet<int>();
                    foreach (var currentState in currentStateList)
                    {
                        var transitionStates = _transitionMatrix.GetStates(currentState, symbol);
                        foreach (var transitionState in transitionStates)
                        {
                            reachableStates.Add(transitionState);
                        }
                    }
                    AddEpsilonStates(reachableStates, reachableStates);
                    if(reachableStates.Count == 0)
                    {
                        continue;
                    }
                    // We have all reachable states with the given symbol. If a state in the new automata
                    // already exists for this combination, then we don't need to add a new state.
                    if (!newAutomatonStates.TryGetValue(reachableStates, out var toStateId))
                    {
                        toStateId = AddNewState(reachableStates);
                    }
                    automaton.AddTransition(currentStateId, toStateId, symbol);
                }
            }
            return automaton;
        }

        /// <summary>
        /// Populates <paramref name="transitionList" /> with all states that can be 
        /// reached from epsilon transition from <paramref name="fromStates" />
        /// </summary>
        private void AddEpsilonStates(ISet<int> transitionList, IReadOnlyCollection<int> fromStates)
        {
            // Add epsilon states
            var transitionStatesQueue = new Queue<int>(fromStates);
            while (transitionStatesQueue.TryDequeue(out var state))
            {
                var epsilonReachableStates = _transitionMatrix.GetStates(state, Epsilon);
                foreach (var epsilonState in epsilonReachableStates)
                {
                    if (transitionList.Add(epsilonState))
                    {
                        transitionStatesQueue.Enqueue(epsilonState);
                    }
                }
            }
        }
    }

    public class CurrentStateListComparer : IEqualityComparer<SortedSet<int>>
    {
        public bool Equals(SortedSet<int>? left, SortedSet<int>? right)
        {
            if(left is null)
            {
                return right is null;
            }
            if(right is null)
            {
                return false;
            }
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left.Count != right.Count)
            {
                return false;
            }
            return left.SetEquals(right);
        }

        public int GetHashCode(SortedSet<int> set)
        {
            var result = 0;
            foreach(var element in set)
            {
                unchecked
                {
                    result = result * 23 + element;
                }
            }
            return result;
        }
    }
}