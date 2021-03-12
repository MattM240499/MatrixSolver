
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
        /// <summary>A list of goal states for this automaton</summary>
        private readonly HashSet<int> _goalStates;
        /// <summary>A list of all states for this automaton</summary>
        private readonly HashSet<int> _states;
        /// <summary>An adjacency matrix of transition states</summary>
        private readonly TransitionMatrix<char> _transitionMatrix;
        private readonly SortedSet<char> _alphabet;
        private int _statesIdCounter;
        /// <summary>
        /// The Transition Matrix representing all possible paths between states
        /// </summary>
        public IReadOnlyTransitionMatrix<char> TransitionMatrix => _transitionMatrix;
        public IReadOnlyCollection<int> States => _states;
        public IReadOnlyCollection<int> StartStates => _startStates;
        public IReadOnlyCollection<int> GoalStates => _goalStates;
        public IReadOnlyCollection<char> Alphabet => _alphabet;
        public const char Epsilon = 'Ɛ';

        public Automaton(IReadOnlyCollection<char> alphabet)
        {
            _states = new HashSet<int>();
            _startStates = new HashSet<int>();
            _goalStates = new HashSet<int>();
            _transitionMatrix = new TransitionMatrix<char>();
            _alphabet = new SortedSet<char>(alphabet);
        }

        private Automaton(HashSet<int> states, HashSet<int> startStates, HashSet<int> goalStates, TransitionMatrix<char> transitionMatrix, SortedSet<char> alphabet)
        {
            _states = states;
            _startStates = startStates;
            _goalStates = goalStates;
            _transitionMatrix = transitionMatrix;
            _alphabet = alphabet;
        }

        /// <summary>
        /// Adds an empty state to the automaton. Throws if state Id already in use
        /// </summary>
        public int AddState(bool isGoalState = false, bool isStartState = false)
        {
            var stateId = _statesIdCounter++;
            if (!_states.Add(stateId))
            {
                throw new InvalidOperationException($"State with Id={stateId} already exists");
            }

            if (isGoalState)
            {
                _goalStates.Add(stateId);
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
            if (!_states.TryGetValue(fromcurrentStateId, out var fromState))
            {
                throw new InvalidOperationException($"State with id {fromcurrentStateId} does not exist");
            }
            if (!_states.TryGetValue(tocurrentStateId, out var toState))
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
            _goalStates.Remove(state);
            _transitionMatrix.RemoveState(state, skipIncomingTransitions);
        }

        public bool DeleteTransition(int fromState, int toState, char symbol)
        {
            return _transitionMatrix.RemoveTransition(fromState, toState, symbol);
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
            foreach (var state in currentStates)
            {
                if (_goalStates.Contains(state))
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
                AddEpsilonStates(states, states);
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
            if(useEpsilonStatesAtEnd)
            {
                AddEpsilonStates(symbolStates, symbolStates);
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
            var newAutomatonStates = new Dictionary<SortedSet<int>, int>(_currentStateListComparer);
            var stateStack = new Stack<SortedSet<int>>();

            // Setup subprocedure
            Func<SortedSet<int>, bool, int> AddNewState = (SortedSet<int> set, bool isStartState) =>
            {
                bool isGoalState = false;
                foreach (var currentState in set)
                {
                    if (this._goalStates.Contains(currentState))
                    {
                        isGoalState = true;
                        break;
                    }
                }
                var currentStateId = automaton.AddState(isStartState: isStartState, isGoalState: isGoalState);
                stateStack.Push(set);
                newAutomatonStates.Add(set, currentStateId);
                return currentStateId;
            };

            // Begin DFA calculation
            var set = new SortedSet<int>(_startStates);
            AddEpsilonStates(set, set);
            AddNewState(set, true);
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
                    // We have all reachable states with the given symbol. If a state in the new automata
                    // already exists for this combination, then we don't need to add a new state.
                    if (reachableStates.Count == 0)
                    {
                        // Ignore the empty state
                        continue;
                    }
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
        /// TODO: Add unit tests
        /// </summary>
        public Automaton IntersectionWithDFA(Automaton automaton)
        {
            // Check they have the same alphabet.
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
                bool isGoalState = false;
                if (this.GoalStates.Contains(stateTuple.firstState) && automaton.GoalStates.Contains(stateTuple.secondState))
                {
                    isGoalState = true;
                }
                var stateId = newAutomaton.AddState(isGoalState: isGoalState, isStartState: isStartState);
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
        /// Returns a new automaton which is a minimzed version
        /// The input must be a DFA
        /// </summary>
        public Automaton MinimizeDFA(bool validateDfa = true)
        {
            if(validateDfa && !IsDFA(out string error))
            {
                throw new InvalidOperationException(error);
            }
            
            // Perform the minimization calculation
            var equivalenceTree = new EquivalenceTree(this);
            equivalenceTree.SeperateEquivalencesIntoBranches();
            // Create the new Automaton
            var newAutomaton = new Automaton(_alphabet);
            var newAutomatonStateLookup = new Dictionary<LinkedList<int>, int>();
            var equivalenceSetQueue = new Queue<LinkedList<int>>();
            // Add the start state to the automaton.
            var equivalenceStateSet = equivalenceTree.EquivalenceLookup[_startStates.First()].States;
            newAutomatonStateLookup[equivalenceStateSet] = newAutomaton.AddState(isStartState: true, isGoalState: _goalStates.Contains(equivalenceStateSet.First!.Value));
            equivalenceSetQueue.Enqueue(equivalenceStateSet);

            while(equivalenceSetQueue.TryDequeue(out var equivalenceSet))
            {
                // Pick any state in the equivalence. Each equivalence must have atleast 1 state.
                var stateFrom = equivalenceSet.First!.Value;
                
                foreach (var symbol in _alphabet)
                {
                    var states = TransitionMatrix.GetStates(stateFrom, symbol);
                    if (states.Count != 0)
                    {
                        var stateTo = states.First();
                        var stateToEquivalence = equivalenceTree.EquivalenceLookup[stateTo].States;
                        // -1 Indicates the state where each transition goes to a dead end. Ignore transitions that lead here, because the definition of DFA used in this project
                        // is that where a state has at most one transition.
                        if(stateToEquivalence.First!.Value == -1)
                        {
                            continue;
                        }
                        if(!newAutomatonStateLookup.TryGetValue(stateToEquivalence, out var newAutomatonStateTo))
                        {
                            // State is unknown. Add it to the automaton, and the state queue.
                            newAutomatonStateTo = newAutomaton.AddState(isStartState: false, isGoalState: _goalStates.Contains(stateTo));
                            var equivalenceSetTo = equivalenceTree.EquivalenceLookup[stateTo].States;
                            equivalenceSetQueue.Enqueue(equivalenceSetTo);
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
            error  = "";
            if(StartStates.Count != 1)
            {
                error = $"Automaton should contain exactly 1 start state but contained {StartStates.Count}, thus is not DFA, and therefore minimization cannot be performed.";
                return false;
            }
            foreach(var state in States)
            {
                foreach(var symbol in _alphabet)
                {
                    if(_transitionMatrix.GetStates(state, symbol).Count > 1)
                    {
                        error = $"Automaton contains multiple transition from state {state} with symbol {symbol}, thus is not a DFA" +
                            ", and therefore minimization cannot be performed.";
                    }
                }
                if(_transitionMatrix.GetStates(state, Automaton.Epsilon).Count != 0)
                {
                    error = $"Automaton contains an epsilon transition from state {state}, thus is not a DFA" +
                        ", and therefore minimization cannot be performed.";
                }
            }
            if(error != "")
            {
                return false;
            }
            return true;
        }

        public Automaton Clone()
        {
            return new Automaton(new HashSet<int>(_states), new HashSet<int>(_startStates), 
                new HashSet<int>(_goalStates), _transitionMatrix.Clone(), new SortedSet<char>(_alphabet));
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