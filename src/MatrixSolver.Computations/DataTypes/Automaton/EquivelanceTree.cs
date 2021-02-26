
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    public class EquivalenceTree
    {
        public Dictionary<int, EquivalenceBranch> EquivalenceLookup { get; }
        /// <summary>
        /// All known equivalences.
        /// </summary>
        public List<EquivalenceBranch> Equivalences { get; }
        public Automaton Automaton { get; }
        /// <summary> Branches directly from the root </summary>
        private readonly List<EquivalenceBranch> _branches;

        public EquivalenceTree(Automaton automaton)
        {
            _branches = new List<EquivalenceBranch>();
            EquivalenceLookup = new Dictionary<int, EquivalenceBranch>();
            Equivalences = new List<EquivalenceBranch>();
            var finalStates = new LinkedList<int>();
            var nonFinalStates = new LinkedList<int>();
            Automaton = automaton;

            nonFinalStates.AddLast(-1);
            foreach (var state in automaton.States)
            {
                if (automaton.GoalStates.Contains(state))
                {
                    finalStates.AddLast(state);
                }
                else
                {
                    nonFinalStates.AddLast(state);
                }
            }
            foreach (var stateSet in new[] { nonFinalStates, finalStates })
            {
                if (stateSet.Count == 0)
                {
                    continue;
                }
                var stateSetEquivalence = new EquivalenceBranch(stateSet, this);
                foreach (var state in stateSet)
                {
                    EquivalenceLookup[state] = stateSetEquivalence;
                }
                _branches.Add(stateSetEquivalence);
                Equivalences.Add(stateSetEquivalence);
            }
        }

        public void SeperateEquivalencesIntoBranches()
        {
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (var branch in _branches)
                {
                    changes |= branch.UpdateEquivalences();
                }
            }
        }
    }

    public class EquivalenceBranch
    {
        public List<EquivalenceBranch?> Equivalence => CalculateEquivalenceForEachSymbol(States.First!.Value);
        public LinkedList<int> States { get; }
        private readonly EquivalenceTree _root;
        private readonly List<EquivalenceBranch> _branches;
        private Automaton _automaton => _root.Automaton;

        public EquivalenceBranch(LinkedList<int> states, EquivalenceTree root)
        {
            States = states;
            _root = root;
            _branches = new List<EquivalenceBranch>();
        }

        public bool UpdateEquivalences()
        {
            bool changes = false;
            var newBranches = new List<EquivalenceBranch>();
            foreach (var branch in _branches)
            {
                changes |= branch.UpdateEquivalences();
            }
            var startState = States.First!;
            var state = States.Last!;
            var equivalenceUpdates = new List<(int state, EquivalenceBranch branch)>();
            var compareEquivalence = CalculateEquivalenceForEachSymbol(startState.Value);
            while (state != startState)
            {
                var nextState = state.Previous!;
                var stateEquivalence = CalculateEquivalenceForEachSymbol(state.Value);

                if (!compareEquivalence.SequenceEqual(stateEquivalence))
                {
                    // Different equivalence, split them off into a new branch, or check whether they fit into an existing one
                    changes = true;
                    States.Remove(state);

                    bool equivalenceFound = false;
                    foreach (var branch in newBranches)
                    {
                        if (branch.Equivalence.SequenceEqual(stateEquivalence))
                        {
                            // Match found, remove the state from this equivalence, and add it into the branch
                            branch.AddState(state.Value);
                            equivalenceUpdates.Add((state.Value, branch));
                            equivalenceFound = true;
                            break;
                        }
                    }
                    if (!equivalenceFound)
                    {
                        // No equivalence found, create a new branch
                        var newList = new LinkedList<int>();
                        newList.AddLast(state.Value);
                        var newBranch = new EquivalenceBranch(newList, _root);
                        newBranches.Add(newBranch);
                        equivalenceUpdates.Add((state.Value, newBranch));
                        _root.Equivalences.Add(newBranch);
                    }
                }
                state = nextState;
            }
            foreach (var update in equivalenceUpdates)
            {
                _root.EquivalenceLookup[update.state] = update.branch;
            }
            foreach (var branch in newBranches)
            {
                _branches.Add(branch);
            }
            return changes;
        }

        public void AddState(int state)
        {
            States.AddLast(state);
        }

        public List<EquivalenceBranch?> CalculateEquivalenceForEachSymbol(int state)
        {
            var previousEquivalences = new List<EquivalenceBranch?>(_automaton.Alphabet.Count);
            foreach (var character in _automaton.Alphabet)
            {
                if (state == -1)
                {
                    previousEquivalences.Add(_root.EquivalenceLookup[-1]);
                    continue;
                }
                var states = _automaton.TransitionMatrix.GetStates(state, character);
                if (states.Count == 0)
                {
                    previousEquivalences.Add(_root.EquivalenceLookup[-1]);
                }
                else
                {
                    previousEquivalences.Add(_root.EquivalenceLookup[states.First()]);
                }
            }
            return previousEquivalences;
        }
    }
}