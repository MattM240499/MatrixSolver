using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    internal class CanonicalStateTransitionLookup
    {
        private Dictionary<int, StateTransitions> _incomingTransitionLookup = new Dictionary<int, StateTransitions>();
        private Dictionary<int, StateTransitions> _outgoingTransitionLookup = new Dictionary<int, StateTransitions>();
        private Automaton _automaton;
        public CanonicalStateTransitionLookup(Automaton automaton)
        {
            _automaton = automaton;
            foreach (var state in automaton.States)
            {
                _incomingTransitionLookup.Add(state, new StateTransitions());
                _outgoingTransitionLookup.Add(state, new StateTransitions());
            }
        }

        /// <summary>
        /// Adds a transition to the internally stored canonical state substrings. Returns a list of all transitions that should be added directly resulting from this
        /// </summary>
        public IReadOnlyCollection<Transition> AddTransition(int stateFrom, TransitionSymbol symbol, int stateTo, bool negative)
        {
            // This process works as follows:
            // - Check whether the transition already exists. If it already does, then just return.
            // - Add the transition to the lookup
            // - Calculate newly created transitions from the lookups

            var outTransitionLookup = _outgoingTransitionLookup[stateFrom].GetTransitionDictionary(symbol);
            var inTransitionLookup = _incomingTransitionLookup[stateTo].GetTransitionDictionary(symbol);
            // Check whether the transition already exists, and add it if not.
            // We will check the outgoing transition from the stateFrom, but we expect it to be consistent between the outgoing and incoming transition
            // I.e. if s0 ----S----> s1, then we have S outgoing from s0 to s1, and S incoming on s1 from s0
            // Therefore, if something breaks here then we know that our code has produced an inconsistent state
            ReachabilityStatus fromReachabilityStatus;
            if (outTransitionLookup.TryGetValue(stateTo, out var toReachabilityStatus))
            {
                fromReachabilityStatus = inTransitionLookup[stateFrom];
            }
            else
            {
                toReachabilityStatus = new ReachabilityStatus();
                outTransitionLookup[stateTo] = toReachabilityStatus;
                fromReachabilityStatus = new ReachabilityStatus();
                inTransitionLookup[stateFrom] = fromReachabilityStatus;
            }
            if (symbol != TransitionSymbol.X)
            {
                if (toReachabilityStatus.EvenReachable && !negative || toReachabilityStatus.OddReachable && negative)
                {
                    // Transition already exists
                    return Array.Empty<Transition>();
                }
                else
                {
                    if (negative)
                    {
                        fromReachabilityStatus.OddReachable = true;
                        toReachabilityStatus.OddReachable = true;
                    }
                    else
                    {
                        fromReachabilityStatus.EvenReachable = true;
                        toReachabilityStatus.EvenReachable = true;
                    }
                }
            }
            // TODO: Hopefully we can remove this section once we refactor the TransitionSymbol.X out of the code.
            else
            {
                if (toReachabilityStatus.OddReachable)
                {
                    // Transition already exists
                    return Array.Empty<Transition>();
                }
                else
                {
                    toReachabilityStatus.OddReachable = true;
                    fromReachabilityStatus.OddReachable = true;
                }
            }

            var toAddTransitions = new List<Transition>();

            Action<int, int, TransitionSymbol, bool> addTransition = (fromState, toState, symbol, negated) =>
            {
                if (fromState == toState && symbol == TransitionSymbol.Epsilon)
                {
                    return;
                }
                toAddTransitions.Add(new Transition(fromState, toState, symbol, negated));
            };
            // Adds an X or epsilon transition from a given state to another, based on an existing reachability status
            Action<int, int, ReachabilityStatus, bool> addTransitionFromReachabilityStatus = (fromState, toState, reachabilityStatus, negated) =>
            {
                if ((reachabilityStatus.EvenReachable && negated) || (reachabilityStatus.OddReachable && !negated))
                {
                    addTransition(fromState, toState, TransitionSymbol.X, false);
                }
                if ((reachabilityStatus.OddReachable && negated) || (reachabilityStatus.EvenReachable && !negated))
                {
                    addTransition(fromState, toState, TransitionSymbol.Epsilon, false);
                }
            };

            Action<int, int, ReachabilityStatus, bool> addRRTransition = (fromState, toState, reachabilityStatus, negated) =>
            {
                if ((reachabilityStatus.EvenReachable && negated) || (reachabilityStatus.OddReachable && !negated))
                {
                    addTransition(fromState, toState, TransitionSymbol.RR, true);
                }
                if ((reachabilityStatus.OddReachable && negated) || (reachabilityStatus.EvenReachable && !negated))
                {
                    addTransition(fromState, toState, TransitionSymbol.RR, false);
                }
            };

            // Updates a reachability status lookup by adding the data from the current reachabilityStatus
            Action<int, ReachabilityStatus, Dictionary<int, ReachabilityStatus>, bool> updateReachabilityStatus =
                (fromState, reachabilityStatus, reachabilityLookup, negated) =>
            {
                if (!reachabilityLookup.TryGetValue(fromState, out var currentReachabilityStatus))
                {
                    currentReachabilityStatus = new ReachabilityStatus();
                    reachabilityLookup[fromState] = currentReachabilityStatus;
                }
                if (negated)
                {
                    currentReachabilityStatus.EvenReachable |= reachabilityStatus.OddReachable;
                    currentReachabilityStatus.OddReachable |= reachabilityStatus.EvenReachable;
                }
                else
                {
                    currentReachabilityStatus.EvenReachable |= reachabilityStatus.EvenReachable;
                    currentReachabilityStatus.OddReachable |= reachabilityStatus.OddReachable;
                }
            };

            Func<int, Dictionary<int, ReachabilityStatus>> getEpsilonReachabilityDictionary = (state) =>
            {
                var rightReachabilityDictionary = _outgoingTransitionLookup[state].EpsilonTransitions.ToDictionary(kv => kv.Key, kv => kv.Value);
                ReachabilityStatus? reachabilityStatus = null;
                if (!rightReachabilityDictionary.TryGetValue(stateTo, out reachabilityStatus))
                {
                    reachabilityStatus = new ReachabilityStatus();
                    rightReachabilityDictionary[stateTo] = reachabilityStatus;
                }
                reachabilityStatus.EvenReachable |= true;
                return rightReachabilityDictionary;
            };

            Func<Dictionary<int, ReachabilityStatus>, char, bool, Dictionary<int, ReachabilityStatus>> applyTransitionToReachabilityDictionary = (reachabilityDictionary, symbol, includeEpsilon) =>
            {
                var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>();
                foreach (var reachbilityLookup in reachabilityDictionary)
                {
                    IEnumerable<KeyValuePair<int, ReachabilityStatus>> reachableStates = null!;
                    if (includeEpsilon)
                    {
                        reachableStates = _outgoingTransitionLookup[reachbilityLookup.Key].GetTransitionDictionary(ConvertToSymbol(symbol));
                    }
                    else
                    {
                        reachableStates = _automaton.TransitionMatrix.GetStates(reachbilityLookup.Key, symbol).Select(s =>
                            KeyValuePair.Create(s, ReachabilityStatus.Even()));
                    }

                    foreach (var state in reachableStates)
                    {
                        var newReachability = state.Value.Times(reachbilityLookup.Value);
                        if (!newReachabilityDictionary.TryGetValue(state.Key, out var newStatereachabilityStatus))
                        {
                            newStatereachabilityStatus = new ReachabilityStatus();
                            newReachabilityDictionary[state.Key] = newStatereachabilityStatus;
                        }
                        newStatereachabilityStatus.EvenReachable |= newReachability.EvenReachable;
                        newStatereachabilityStatus.OddReachable |= newReachability.OddReachable;
                    }
                }
                return newReachabilityDictionary;
            };

            // Adds transitions for each side combined together with the middle. Should only be called on combinations that make sense.
            Action<Dictionary<int, ReachabilityStatus>, char, int, bool> combineWithBothSides =
                (incomingReachabilityLookup, symbol, count, negated) =>
            {
                var rightReachabilityDictionary = getEpsilonReachabilityDictionary(stateTo);

                for (int i = 1; i <= count; i++)
                {
                    rightReachabilityDictionary = applyTransitionToReachabilityDictionary(rightReachabilityDictionary, symbol, i != count);
                }

                foreach (var leftReachabilityStatus in incomingReachabilityLookup)
                {
                    foreach (var rightReachabilityStatus in rightReachabilityDictionary)
                    {
                        var stateReachabilityStatus = leftReachabilityStatus.Value.Times(rightReachabilityStatus.Value);
                        addTransitionFromReachabilityStatus(leftReachabilityStatus.Key, rightReachabilityStatus.Key, stateReachabilityStatus, negated);
                    }
                }
            };

            // Adds transitions for each side combined together with the middle. Should only be called on combinations that make sense.
            Action<Dictionary<int, ReachabilityStatus>, bool> combineWithBothSidesRR =
                (incomingReachabilityLookup, negated) =>
            {
                var rightReachabilityDictionary = getEpsilonReachabilityDictionary(stateTo);

                rightReachabilityDictionary = applyTransitionToReachabilityDictionary(rightReachabilityDictionary, 'R', false);

                foreach (var leftReachabilityStatus in incomingReachabilityLookup)
                {
                    foreach (var rightReachabilityStatus in rightReachabilityDictionary)
                    {
                        var stateReachabilityStatus = leftReachabilityStatus.Value.Times(rightReachabilityStatus.Value);
                        addRRTransition(leftReachabilityStatus.Key, rightReachabilityStatus.Key, stateReachabilityStatus, negated);
                    }
                }
            };

            Action<Dictionary<int, ReachabilityStatus>, bool> combineLeft =
                (incomingReachabilityLookup, negated) =>
            {
                foreach (var stateReachabilityLookup in incomingReachabilityLookup)
                {
                    addTransitionFromReachabilityStatus(stateReachabilityLookup.Key, stateTo, stateReachabilityLookup.Value, negated);
                }
            };

            Action<char, int, bool> combineRight =
                (symbol, count, negated) =>
            {
                var rightReachabilityDictionary = getEpsilonReachabilityDictionary(stateTo);

                for (int i = 1; i <= count; i++)
                {
                    rightReachabilityDictionary = applyTransitionToReachabilityDictionary(rightReachabilityDictionary, symbol, i != count);
                }
                foreach (var rightReachabilityLookup in rightReachabilityDictionary)
                {
                    addTransitionFromReachabilityStatus(stateFrom, rightReachabilityLookup.Key, rightReachabilityLookup.Value, negated);
                }
            };

            Action<Dictionary<int, ReachabilityStatus>, bool> combineLeftRR =
                (incomingReachabilityLookup, negated) =>
            {
                foreach (var stateReachabilityLookup in incomingReachabilityLookup)
                {
                    addRRTransition(stateReachabilityLookup.Key, stateTo, stateReachabilityLookup.Value, negated);
                }
            };

            Action<Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> combineRightRR =
                (getRightTransitionSet, negated) =>
            {
                foreach (var leftReachabilityLookup in _outgoingTransitionLookup[stateTo].EpsilonTransitions)
                {
                    foreach (var rightReachabilityLookup in getRightTransitionSet(_outgoingTransitionLookup[leftReachabilityLookup.Key]))
                    {
                        var combinedReachability = leftReachabilityLookup.Value.Times(rightReachabilityLookup.Value);
                        addRRTransition(stateFrom, rightReachabilityLookup.Key, combinedReachability, negated);
                    }
                }
            };

            // Update a given transition set, by iterating through all possibilities specified in the iterate lookup
            Action<int?, int?, Dictionary<int, ReachabilityStatus>, Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> updateTransitionSetSingleSide =
                (outState, inState, iterateReachabilityLookup, getToReachabilitySet, negated) =>
            {
                if (inState is null && outState is null) throw new InvalidOperationException("Expected one of inState and outState to be null");
                foreach (var stateReachabilityLookup in iterateReachabilityLookup)
                {
                    var currentOutState = outState ?? stateReachabilityLookup.Key;
                    var currentInState = inState ?? stateReachabilityLookup.Key;
                    var currentReachabilityStatus = stateReachabilityLookup.Value;
                    updateReachabilityStatus(currentOutState, currentReachabilityStatus, getToReachabilitySet(_incomingTransitionLookup[currentInState]), negated);
                    updateReachabilityStatus(currentInState, currentReachabilityStatus, getToReachabilitySet(_outgoingTransitionLookup[currentOutState]), negated);
                }
            };

            Action<Dictionary<int, ReachabilityStatus>, Dictionary<int, ReachabilityStatus>, Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> updateTransitionSetBothSides =
                (leftIterateSet, rightIterateSet, getToReachabilitySet, negated) =>
            {
                foreach (var lefStateReachabilityLookup in leftIterateSet)
                {
                    foreach (var rightStateReachabilityLookup in rightIterateSet)
                    {
                        var leftState = lefStateReachabilityLookup.Key;
                        var rightState = rightStateReachabilityLookup.Key;
                        var currentReachabilityStatus = lefStateReachabilityLookup.Value.Times(rightStateReachabilityLookup.Value);
                        updateReachabilityStatus(leftState, currentReachabilityStatus, getToReachabilitySet(_incomingTransitionLookup[rightState]), negated);
                        updateReachabilityStatus(rightState, currentReachabilityStatus, getToReachabilitySet(_outgoingTransitionLookup[leftState]), negated);
                    }
                }
            };

            // We have sk ---symbol--->  sj.
            // Now combine this with the incoming and outgoing transitions with sk and sjs
            switch (symbol)
            {
                // For S Combine with S transitions on each side (S + S = X)
                case TransitionSymbol.S:
                    updateTransitionSetSingleSide(stateFrom, null, _outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.STransitions, false);
                    // Combine new S transition with any incoming and outgoing S transitions
                    combineRight('S', 1, true);
                    combineLeft(_incomingTransitionLookup[stateFrom].STransitions, true);
                    break;

                // For R Combine left with R and RR transitions on each side.(R + R = RR, RR + R = R + RR = X, R + R + R = X)
                // In other words:
                // For R + RR = X or RR + R = X  add a transition
                // For R + R + R = X add a transition
                // For R + R = RR update the dictionaries. 
                case TransitionSymbol.R:
                    // Update R + X/epsilon
                    updateTransitionSetSingleSide(stateFrom, null, _outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.RTransitions, false);
                    // Update R + R transitions either side
                    /*foreach(var incomingRReachability in _incomingTransitionLookup[stateFrom].RTransitions)
                    {
                        foreach(var epsilonReachability in _outgoingTransitionLookup[stateTo].EpsilonTransitions.Append(KeyValuePair.Create(stateTo, ReachabilityStatus.Even())))
                        {
                            var combinedReachability = incomingRReachability.Value.Times(incomingRReachability.Value);
                            updateReachabilityStatus(epsilonReachability.Key, combinedReachability,  _outgoingTransitionLookup[incomingRReachability.Key].RRTransitions, false);
                            updateReachabilityStatus(incomingRReachability.Key, combinedReachability,  _incomingTransitionLookup[epsilonReachability.Key].RRTransitions, false);
                        }
                    }
                    foreach(var outgoingRReachability in _outgoingTransitionLookup[stateFrom].RTransitions)
                    {
                        updateReachabilityStatus(outgoingRReachability.Key, outgoingRReachability.Value,  _outgoingTransitionLookup[stateFrom].RRTransitions, false);
                        updateReachabilityStatus(stateFrom, outgoingRReachability.Value,  _incomingTransitionLookup[outgoingRReachability.Key].RRTransitions, false);
                    }*/
                    //updateTransitionSetSingleSide(null, stateTo, _incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RRTransitions, false);
                    // Update R + R transitions either side
                    //updateTransitionSetSingleSide(stateFrom, null, _outgoingTransitionLookup[stateTo].RTransitions, (s) => s.RRTransitions, false);
                    // Combine new R transition with any incoming RR transitions
                    combineRight('R', 2, true);
                    combineLeft(_incomingTransitionLookup[stateFrom].RRTransitions, true);
                    // Update incoming and outgoing transitions for R + R + R
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RTransitions, 'R', 1, true);
                    // Combine new R transition with any incoming R transitions
                    combineLeftRR(_incomingTransitionLookup[stateFrom].RTransitions, false);
                    combineRightRR(s => s.RTransitions, false);
                    break;

                case TransitionSymbol.RR:
                    updateTransitionSetSingleSide(stateFrom, null, _outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.RRTransitions, negative);
                    // Combine new R transition with any incoming RR transitions
                    combineRight('R', 1, !negative);
                    combineLeft(_incomingTransitionLookup[stateFrom].RTransitions, !negative);
                    break;

                case TransitionSymbol.X:
                case TransitionSymbol.Epsilon:
                    // For X and Epsilon Combine with X, S, R and RR transitions on each side
                    var even = true;
                    if (symbol == TransitionSymbol.X)
                    {
                        even = false;
                        // Add epsilon transitions for X + X
                        foreach (var stateReachabilityLookup in _incomingTransitionLookup[stateFrom].EpsilonTransitions)
                        {
                            if (stateReachabilityLookup.Value.OddReachable)
                            {
                                addTransition(stateReachabilityLookup.Key, stateTo, TransitionSymbol.Epsilon, false);
                            }
                        }
                        foreach (var stateReachabilityLookup in _outgoingTransitionLookup[stateTo].EpsilonTransitions)
                        {
                            if (stateReachabilityLookup.Value.OddReachable)
                            {
                                addTransition(stateFrom, stateReachabilityLookup.Key, TransitionSymbol.Epsilon, false);
                            }
                        }
                    }
                    else
                    {
                        // Combine with X on each side X
                        var incomingEpsilonLookup = _incomingTransitionLookup[stateFrom].EpsilonTransitions;
                        var outgoingEpsilonLookup = _outgoingTransitionLookup[stateTo].EpsilonTransitions;
                        foreach (var incomingEpsilonTransition in incomingEpsilonLookup)
                        {
                            if (!incomingEpsilonTransition.Value.OddReachable)
                            {
                                continue;
                            }
                            foreach (var outgoingEpsilonTransition in outgoingEpsilonLookup)
                            {
                                if (!outgoingEpsilonTransition.Value.OddReachable)
                                {
                                    continue;
                                }
                                // Both X. Combine.
                                addTransition(incomingEpsilonTransition.Key, outgoingEpsilonTransition.Key, TransitionSymbol.Epsilon, false);
                            }
                        }
                    }

                    // First update the Transition lookup.
                    // Update epsilon transitions
                    updateTransitionSetSingleSide(stateFrom, null, _outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.EpsilonTransitions, !even);
                    updateTransitionSetSingleSide(null, stateTo, _incomingTransitionLookup[stateFrom].EpsilonTransitions, (s) => s.EpsilonTransitions, !even);
                    updateTransitionSetBothSides(_incomingTransitionLookup[stateFrom].EpsilonTransitions, _outgoingTransitionLookup[stateTo].EpsilonTransitions,
                        (s) => s.EpsilonTransitions, !even);
                    // Combine with X,S,R,RR from front side. We only do the front side as the rules we are checking are XX, SXS, RXRXR.
                    foreach (var reachabilityLookup in _outgoingTransitionLookup[stateFrom].EpsilonTransitions)
                    {
                        if (reachabilityLookup.Value.EvenReachable)
                        {
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].STransitions, (s) => s.STransitions, false);
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RTransitions, false);
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RRTransitions, (s) => s.RRTransitions, false);
                        }
                        if (reachabilityLookup.Value.OddReachable)
                        {
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].STransitions, (s) => s.STransitions, true);
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RTransitions, true);
                            updateTransitionSetSingleSide(null, reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RRTransitions, (s) => s.RRTransitions, true);
                        }
                    }

                    // Combine S + S
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].STransitions, 'S', 1, even);
                    // Combine R + RR
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RTransitions, 'R', 2, even);
                    // Combine RR + R
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RRTransitions, 'R', 1, even);
                    // Combine R + R
                    combineWithBothSidesRR(_incomingTransitionLookup[stateFrom].RTransitions, !even);

                    break;
                default:
                    throw new ArgumentException($"Character {symbol} has no understood implementation for canonicalisation");
            }
            return toAddTransitions;
        }

        public static bool TryConvertToChar(TransitionSymbol symbol, out char? symbolCharacter)
        {
            symbolCharacter = null;
            switch (symbol)
            {
                case TransitionSymbol.Epsilon:
                    symbolCharacter = Automaton.Epsilon;
                    break;
                case TransitionSymbol.X:
                    symbolCharacter = Constants.RegularLanguage.X;
                    break;
                case TransitionSymbol.S:
                    symbolCharacter = Constants.RegularLanguage.S;
                    break;
                case TransitionSymbol.R:
                    symbolCharacter = Constants.RegularLanguage.R;
                    break;
                case TransitionSymbol.RR:
                default:
                    return false;
            }
            return true;
        }

        public static TransitionSymbol ConvertToSymbol(char symbol)
        {
            switch (symbol)
            {
                case Automaton.Epsilon:
                    return TransitionSymbol.Epsilon;
                case Constants.RegularLanguage.X:
                    return TransitionSymbol.X;
                case Constants.RegularLanguage.S:
                    return TransitionSymbol.S;
                case Constants.RegularLanguage.R:
                    return TransitionSymbol.R;
                default:
                    throw new ArgumentException($"No known transition symbol {symbol}");
            }
        }
    }

    internal class StateTransitions
    {
        public Dictionary<int, ReachabilityStatus> EpsilonTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> RTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> RRTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> STransitions { get; } = new Dictionary<int, ReachabilityStatus>();

        public Dictionary<int, ReachabilityStatus> GetTransitionDictionary(TransitionSymbol symbol)
        {
            switch (symbol)
            {
                case TransitionSymbol.R:
                    return RTransitions;
                case TransitionSymbol.S:
                    return STransitions;
                case TransitionSymbol.X:
                case TransitionSymbol.Epsilon:
                    return EpsilonTransitions;
                case TransitionSymbol.RR:
                    return RRTransitions;
                default:
                    throw new ArgumentException($"Character {symbol} has no understood implementation for canonicalisation");
            }
        }
    }

    internal class Transition
    {
        public int StateFrom { get; }
        public int StateTo { get; }
        public TransitionSymbol Symbol { get; }
        public bool Negated { get; }

        public Transition(int stateFrom, int stateTo, TransitionSymbol symbol, bool negated)
        {
            StateFrom = stateFrom;
            StateTo = stateTo;
            Symbol = symbol;
            Negated = negated;
        }
    }

    internal enum TransitionSymbol
    {
        None = 0,
        Epsilon = 1,
        X = 2,
        S = 3,
        R = 4,
        RR = 5
    }
}