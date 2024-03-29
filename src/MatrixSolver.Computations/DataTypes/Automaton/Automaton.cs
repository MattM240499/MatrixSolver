
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.Computations.DataTypes.Automata
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
        /// <summary>A list of final states for this automaton</summary>
        private readonly HashSet<int> _finalStates;
        /// <summary>A list of all states for this automaton</summary>
        private readonly HashSet<int> _states;
        /// <summary>An adjacency matrix of transition states</summary>
        private readonly TransitionMatrix<char> _transitionMatrix;
        private readonly char[] _alphabet;
        private int _statesIdCounter;
        /// <summary>
        /// The Transition Matrix representing all possible paths between states
        /// </summary>
        public IReadOnlyTransitionMatrix<char> TransitionMatrix => _transitionMatrix;
        public IReadOnlyCollection<int> States => _states;
        public IReadOnlyCollection<int> StartStates => _startStates;
        public IReadOnlyCollection<int> FinalStates => _finalStates;
        public IReadOnlyList<char> Alphabet => _alphabet;
        public const char Epsilon = 'Ɛ';

        public Automaton(char[] alphabet)
        {
            _states = new HashSet<int>();
            _startStates = new HashSet<int>();
            _finalStates = new HashSet<int>();
            _transitionMatrix = new TransitionMatrix<char>();
            _alphabet = alphabet.ToArray();
        }

        private Automaton(HashSet<int> states, HashSet<int> startStates, HashSet<int> finalStates, TransitionMatrix<char> transitionMatrix, char[] alphabet)
        {
            _states = states;
            _startStates = startStates;
            _finalStates = finalStates;
            _transitionMatrix = transitionMatrix;
            _alphabet = alphabet.ToArray();
        }

        /// <summary>
        /// Adds an empty state to the automaton. Throws if state Id already in use
        /// </summary>
        public int AddState(bool isFinalState = false, bool isStartState = false)
        {
            var stateId = _statesIdCounter++;
            if (!_states.Add(stateId))
            {
                throw new InvalidOperationException($"State with Id={stateId} already exists");
            }

            if (isFinalState)
            {
                _finalStates.Add(stateId);
            }
            if (isStartState)
            {
                _startStates.Add(stateId);
            }
            return stateId;
        }

        /// <summary>
        /// Adds a one way transition between two existing states. Throws if the states do not exist, or a transition already exists for that state
        /// </summary>
        public bool AddTransition(int fromcurrentStateId, int tocurrentStateId, char symbol)
        {
            if (!_alphabet.Contains(symbol) && symbol != Epsilon)
            {
                throw new InvalidOperationException($"Symbol {symbol} was not in alphabet: {String.Join(',', _alphabet)}");
            }
            if (!_states.Contains(fromcurrentStateId))
            {
                throw new InvalidOperationException($"State with id {fromcurrentStateId} does not exist");
            }
            if (!_states.Contains(tocurrentStateId))
            {
                throw new InvalidOperationException($"State with id {tocurrentStateId} does not exist");
            }

            // Perform update
            return _transitionMatrix.AddTransition(fromcurrentStateId, tocurrentStateId, symbol);
        }

        /// <summary>
        /// Deletes a state and all transitions associated with it. Can specify to skip removing incoming transitions.
        /// This should only be used when it is known that there are no incoming transitions. If used incorrectly,
        /// this can result in an incorrect final automata.
        /// </summary>
        public void DeleteState(int state, bool skipIncomingTransitions = false)
        {
            _states.Remove(state);
            _startStates.Remove(state);
            _finalStates.Remove(state);
            _transitionMatrix.RemoveState(state, skipIncomingTransitions);
        }

        public bool SetAsStartState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _startStates.Add(currentStateId);
        }

        public bool SetAsFinalState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _finalStates.Add(currentStateId);
        }

        public bool UnsetStartState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _startStates.Remove(currentStateId);
        }

        public bool UnsetFinalState(int currentStateId)
        {
            if (!_states.TryGetValue(currentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {currentStateId} does not exist");
            }

            return _finalStates.Remove(currentStateId);
        }

        public bool IsValidWord(IEnumerable<char> word)
        {
            if (_startStates.Count == 0 || _finalStates.Count == 0)
            {
                return false;
            }

            HashSet<int> currentStates = new HashSet<int>(_startStates);
            AddEpsilonStates(currentStates);
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
                AddEpsilonStates(nextStates);
                if (nextStates.Count == 0)
                {
                    // No states can be reached. Word is invalid.
                    return false;
                }
                currentStates = nextStates;
            }
            foreach (var state in currentStates)
            {
                if (_finalStates.Contains(state))
                {
                    return true;
                }
            }
            return false;
        }

        public IReadOnlyCollection<int> GetStatesReachableFromStateWithSymbol(int state, char symbol, bool useEpsilonStatesFromInitial = true, bool useEpsilonStatesAtEnd = true)
        {
            var states = new HashSet<int>() { state };
            // Find all epsilon states from the original state.
            if (useEpsilonStatesFromInitial)
            {
                AddEpsilonStates(states);
            }

            var symbolStates = new HashSet<int>();
            // Then add R transitions
            foreach (var symbolState in states)
            {
                var reachableStates = TransitionMatrix.GetStates(symbolState, symbol);
                foreach (var reachableState in reachableStates)
                {
                    symbolStates.Add(reachableState);
                }
            }
            // Finally add the epsilon states again.
            if (useEpsilonStatesAtEnd)
            {
                AddEpsilonStates(symbolStates);
            }

            return symbolStates;
        }

        /// <summary>
        /// Returns a new <see cref="Automaton" /> which is a DFA
        /// </summary>
        public Automaton ToDFA()
        {
            // A list of states. Each list will become a new state
            var automaton = new Automaton(_alphabet);
            var newAutomatonStates = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer());
            var stateQueue = new Queue<HashSet<int>>();

            // Setup subprocedure
            Func<HashSet<int>, bool, int> AddNewState = (HashSet<int> set, bool isStartState) =>
            {
                bool isFinalState = false;
                foreach (var currentState in set)
                {
                    if (_finalStates.Contains(currentState))
                    {
                        isFinalState = true;
                        break;
                    }
                }
                var currentStateId = automaton.AddState(isStartState: isStartState, isFinalState: isFinalState);
                stateQueue.Enqueue(set);
                newAutomatonStates.Add(set, currentStateId);
                return currentStateId;
            };

            // Begin DFA calculation
            var set = new HashSet<int>(_startStates);
            AddEpsilonStates(set);
            AddNewState(set, true);
            while (stateQueue.TryDequeue(out var currentStateList))
            {
                var currentStateId = newAutomatonStates[currentStateList];
                foreach (var symbol in Alphabet)
                {
                    var reachableStates = new HashSet<int>();
                    foreach (var currentState in currentStateList)
                    {
                        var transitionStates = TransitionMatrix.GetStates(currentState, symbol);
                        foreach (var transitionState in transitionStates)
                        {
                            reachableStates.Add(transitionState);
                        }
                    }
                    if (reachableStates.Count == 0)
                    {
                        // Ignore the empty state
                        continue;
                    }
                    AddEpsilonStates(reachableStates);
                    // We have all reachable states with the given symbol. If a state in the new automata
                    // already exists for this combination, then we don't need to add a new state.
                    if (!newAutomatonStates.TryGetValue(reachableStates, out var toStateId))
                    {
                        toStateId = AddNewState(reachableStates, false);
                    }
                    automaton.AddTransition(currentStateId, toStateId, symbol);
                }
            }
            return automaton;
        }

        /// <summary>
        /// Returns a new automaton which is the result of intersection with another DFA.
        /// Both Automaton must be DFA.
        /// </summary>
        public Automaton IntersectionWithDFA(Automaton automaton)
        {
            // Check they have the same alphabet.
            if (automaton.Alphabet.Count != this.Alphabet.Count)
            {
                throw new InvalidOperationException("Could not calculate the intersection as the alphabets were distinct");
            }
            foreach (var symbol in automaton.Alphabet)
            {
                if (!_alphabet.Contains(symbol))
                {
                    throw new InvalidOperationException("Could not calculate the intersection as the alphabets were distinct");
                }
            }

            var newAutomaton = new Automaton(_alphabet);

            var stateLookup = new Dictionary<(int, int), int>();
            // Only one start state per dfa
            var stateStart = (this.StartStates.First(), automaton.StartStates.First());
            var stateQueue = new Queue<(int firstState, int secondState)>();
            Func<(int, int), bool, int> AddState = ((int firstState, int secondState) stateTuple, bool isStartState) =>
            {
                bool isFinalState = false;
                if (this.FinalStates.Contains(stateTuple.firstState) && automaton.FinalStates.Contains(stateTuple.secondState))
                {
                    isFinalState = true;
                }
                var stateId = newAutomaton.AddState(isFinalState: isFinalState, isStartState: isStartState);
                stateLookup[stateTuple] = stateId;
                stateQueue.Enqueue(stateTuple);
                return stateId;
            };

            AddState(stateStart, true);
            while (stateQueue.TryDequeue(out var stateTuple))
            {
                var fromStateId = stateLookup[stateTuple];
                foreach (var symbol in _alphabet)
                {
                    var reachableStates1 = this.TransitionMatrix.GetStates(stateTuple.firstState, symbol);
                    if (reachableStates1.Count == 0)
                    {
                        continue;
                    }
                    var reachableStates2 = automaton.TransitionMatrix.GetStates(stateTuple.secondState, symbol);
                    if (reachableStates2.Count == 0)
                    {
                        continue;
                    }
                    // DFA, so must only be one more state.
                    var newStateTuple = (reachableStates1.First(), reachableStates2.First());
                    if (!stateLookup.TryGetValue(newStateTuple, out var stateId))
                    {
                        stateId = AddState(newStateTuple, false);
                    }
                    newAutomaton.AddTransition(fromStateId, stateId, symbol);
                }
            }
            return newAutomaton;
        }

        /// <summary>
        /// Returns a new automaton which is minized.
        /// The input must be a DFA.
        /// </summary>
        public Automaton MinimizeDFA(bool validateDfa = true)
        {
            if (validateDfa && !IsDFA(out string error))
            {
                throw new InvalidOperationException(error);
            }

            // Perform the minimization calculation
            var equivalenceFinder = new EquivalenceFinder(this);
            equivalenceFinder.SeperateBlocksIntoEquivalences();
            // Create the new Automaton
            var newAutomaton = new Automaton(_alphabet);
            var newAutomatonStateLookup = new Dictionary<Block, int>();
            var blockSetQueue = new Queue<Block>();
            // Add the start state to the automaton.
            var blockStateSet = equivalenceFinder.BlockLookup[StartStates.First()];
            newAutomatonStateLookup[blockStateSet] = newAutomaton.AddState(isStartState: true, isFinalState: FinalStates.Contains(blockStateSet.States.First!.Value));
            blockSetQueue.Enqueue(blockStateSet);

            while (blockSetQueue.TryDequeue(out var equivalenceSet))
            {
                // Pick any state in the equivalence. Each equivalence must have atleast 1 state.
                var stateFrom = equivalenceSet.States.First!.Value;

                foreach (var symbol in _alphabet)
                {
                    var states = TransitionMatrix.GetStates(stateFrom, symbol);
                    if (states.Count != 0)
                    {
                        var stateTo = states.First();
                        var stateToBlock = equivalenceFinder.BlockLookup[stateTo];
                        // -1 Indicates the state where each transition goes to a sink state. Ignore transitions that lead here, 
                        // because the sink state should be emitted for the transition matrix.
                        if (stateToBlock.States.First!.Value == -1)
                        {
                            continue;
                        }
                        if (!newAutomatonStateLookup.TryGetValue(stateToBlock, out var newAutomatonStateTo))
                        {
                            // State is unknown. Add it to the automaton and the state queue.
                            newAutomatonStateTo = newAutomaton.AddState(isStartState: false, isFinalState: FinalStates.Contains(stateTo));
                            var equivalenceSetTo = equivalenceFinder.BlockLookup[stateTo];
                            blockSetQueue.Enqueue(equivalenceSetTo);
                            newAutomatonStateLookup[equivalenceSetTo] = newAutomatonStateTo;
                        }
                        newAutomaton.AddTransition(newAutomatonStateLookup[equivalenceSet], newAutomatonStateTo, symbol);
                    }
                }
            }

            return newAutomaton;
        }

        /// <summary>
        /// Tests whether a given Automaton is a DFA.
        /// </summary>
        public bool IsDFA(out string error)
        {
            // Validate automaton is a DFA
            error = "";
            if (StartStates.Count != 1)
            {
                error = $"Automaton should contain exactly 1 start state but contained {StartStates.Count}, thus is not DFA, and therefore minimization cannot be performed.";
                return false;
            }
            foreach (var state in States)
            {
                foreach (var symbol in _alphabet)
                {
                    if (TransitionMatrix.GetStates(state, symbol).Count > 1)
                    {
                        error = $"Automaton contains multiple transition from state {state} with symbol {symbol}, thus is not a DFA" +
                            ", and therefore minimization cannot be performed.";
                    }
                }
                if (TransitionMatrix.GetStates(state, Automaton.Epsilon).Count != 0)
                {
                    error = $"Automaton contains an epsilon transition from state {state}, thus is not a DFA" +
                        ", and therefore minimization cannot be performed.";
                }
            }
            if (error != "")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Populates <paramref name="transitionList" /> with all states that can be 
        /// reached from epsilon transition from <paramref name="fromStates" />
        /// </summary>
        private void AddEpsilonStates(ISet<int> transitionList)
        {
            // Add epsilon states
            var transitionStatesQueue = new Queue<int>(transitionList);
            while (transitionStatesQueue.TryDequeue(out var state))
            {
                var epsilonReachableStates = TransitionMatrix.GetStates(state, Epsilon);
                foreach (var epsilonState in epsilonReachableStates)
                {
                    if (transitionList.Add(epsilonState))
                    {
                        transitionStatesQueue.Enqueue(epsilonState);
                    }
                }
            }
        }

        public Automaton Clone()
        {
            return new Automaton(new HashSet<int>(_states), new HashSet<int>(_startStates),
                new HashSet<int>(_finalStates), _transitionMatrix.Clone(), _alphabet.ToArray());
        }
    }

    public class CurrentStateListComparer : IEqualityComparer<SortedSet<int>>
    {
        public bool Equals(SortedSet<int>? left, SortedSet<int>? right)
        {
            if (left is null)
            {
                return right is null;
            }
            if (right is null)
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
            foreach (var element in set)
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