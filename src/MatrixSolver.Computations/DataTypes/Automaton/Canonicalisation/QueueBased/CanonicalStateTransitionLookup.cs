using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
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
        public IReadOnlyCollection<Transition> AddTransition(int stateFrom, char symbol, int stateTo)
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
            if (symbol != Constants.RegularLanguage.X)
            {
                if (toReachabilityStatus.EvenReachable)
                {
                    // Transition already exists
                    return Array.Empty<Transition>();
                }
                else
                {
                    fromReachabilityStatus.EvenReachable = true;
                    toReachabilityStatus.EvenReachable = true;
                }
            }
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

            Action<int, int, char, bool> addTransition = (fromState, toState, symbol, negated) =>
            {
                if (fromState == toState && symbol == Automaton.Epsilon)
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
                    addTransition(fromState, toState, Constants.RegularLanguage.X, false);
                }
                if ((reachabilityStatus.OddReachable && negated) || (reachabilityStatus.EvenReachable && !negated))
                {
                    addTransition(fromState, toState, Automaton.Epsilon, false);
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

            // Adds transitions for each side combined together with the middle. Should only be called on combinations that make sense.
            Action<Dictionary<int, ReachabilityStatus>, char, int, bool> combineWithBothSides =
                (incomingReachabilityLookup, symbol, rightSymbolCount, negated) =>
            {
                var rightReachabilityDictionary = getEpsilonReachabilityDictionary(stateTo);

                for (int i = 1; i <= rightSymbolCount; i++)
                {
                    rightReachabilityDictionary = ApplyTransitionToReachabilityDictionary(rightReachabilityDictionary, symbol, i != rightSymbolCount);
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

            Action<Dictionary<int, ReachabilityStatus>, bool> combineLeft =
                (incomingReachabilityLookup, negated) =>
            {
                foreach (var stateReachabilityLookup in incomingReachabilityLookup)
                {
                    addTransitionFromReachabilityStatus(stateReachabilityLookup.Key, stateTo, stateReachabilityLookup.Value, negated);
                }
            };

            Action<char, int, bool> combineRight =
                (symbol, rightSymbolCount, negated) =>
            {
                var rightReachabilityDictionary = getEpsilonReachabilityDictionary(stateTo);

                for (int i = 1; i <= rightSymbolCount; i++)
                {
                    rightReachabilityDictionary = ApplyTransitionToReachabilityDictionary(rightReachabilityDictionary, symbol, i != rightSymbolCount);
                }
                foreach (var rightReachabilityLookup in rightReachabilityDictionary)
                {
                    addTransitionFromReachabilityStatus(stateFrom, rightReachabilityLookup.Key, rightReachabilityLookup.Value, negated);
                }
            };

            // Update a given transition set, by iterating through all possibilities specified in the iterate lookup
            Action<int, Dictionary<int, ReachabilityStatus>, Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> updateTransitionSetEpsilon =
                (inState, iterateReachabilityLookup, getToReachabilitySet, negated) =>
            {
                foreach (var stateReachabilityLookup in iterateReachabilityLookup)
                {
                    var currentOutState = stateReachabilityLookup.Key;
                    var currentReachabilityStatus = stateReachabilityLookup.Value;
                    UpdateReachabilityStatus(currentOutState, currentReachabilityStatus, getToReachabilitySet(_incomingTransitionLookup[inState]), negated);
                    UpdateReachabilityStatus(inState, currentReachabilityStatus, getToReachabilitySet(_outgoingTransitionLookup[currentOutState]), negated);
                }
            };

            Action<Dictionary<int, ReachabilityStatus>, Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> updateTransitionSetRight =
                (iterateReachabilityLookup, getToReachabilitySet, negated) =>
            {
                foreach (var stateReachabilityLookup in iterateReachabilityLookup)
                {
                    var currentReachabilityStatus = stateReachabilityLookup.Value;
                    UpdateReachabilityStatus(stateFrom, currentReachabilityStatus, getToReachabilitySet(_incomingTransitionLookup[stateReachabilityLookup.Key]), negated);
                    UpdateReachabilityStatus(stateReachabilityLookup.Key, currentReachabilityStatus, getToReachabilitySet(_outgoingTransitionLookup[stateFrom]), negated);
                }
            };

            Action<Dictionary<int, ReachabilityStatus>, Func<StateTransitions, Dictionary<int, ReachabilityStatus>>, bool> updateTransitionSetLeft =
                (iterateReachabilityLookup, getToReachabilitySet, negated) =>
            {
                foreach (var stateReachabilityLookup in iterateReachabilityLookup)
                {
                    foreach (var epsilonReachability in getEpsilonReachabilityDictionary(stateTo))
                    {
                        var combinedReachability = stateReachabilityLookup.Value.Times(epsilonReachability.Value);
                        UpdateReachabilityStatus(epsilonReachability.Key, combinedReachability, getToReachabilitySet(_outgoingTransitionLookup[stateReachabilityLookup.Key]), false);
                        UpdateReachabilityStatus(stateReachabilityLookup.Key, combinedReachability, getToReachabilitySet(_incomingTransitionLookup[epsilonReachability.Key]), false);
                    }
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
                        UpdateReachabilityStatus(leftState, currentReachabilityStatus, getToReachabilitySet(_incomingTransitionLookup[rightState]), negated);
                        UpdateReachabilityStatus(rightState, currentReachabilityStatus, getToReachabilitySet(_outgoingTransitionLookup[leftState]), negated);
                    }
                }
            };

            // We have sk ---symbol--->  sj.
            // Now combine this with the incoming and outgoing transitions with sk and sjs
            switch (symbol)
            {
                // For S Combine with S transitions on each side (S + S = X)
                case Constants.RegularLanguage.S:
                    updateTransitionSetRight(_outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.STransitions, false);
                    // Combine new S transition with any incoming and outgoing S transitions
                    combineRight('S', 1, true);
                    combineLeft(_incomingTransitionLookup[stateFrom].STransitions, true);
                    break;

                // For R Combine left with R and RR transitions on each side.(R + R = RR, RR + R = R + RR = X, R + R + R = X)
                // In other words:
                // For R + RR = X or RR + R = X  add a transition
                // For R + R + R = X add a transition
                // For R + R = RR update the dictionaries.
                case Constants.RegularLanguage.R:
                    // Update R + X/epsilon
                    updateTransitionSetRight(_outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.RTransitions, false);
                    // Update R + R transitions either side
                    updateTransitionSetLeft(_incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RRTransitions, false);
                    updateTransitionSetRight(_outgoingTransitionLookup[stateTo].RTransitions, (s) => s.RRTransitions, false);
                    // Combine new R transition with any incoming RR transitions
                    combineRight('R', 2, true);
                    combineLeft(_incomingTransitionLookup[stateFrom].RRTransitions, true);
                    // Update incoming and outgoing transitions for R + R + R
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RTransitions, 'R', 1, true);
                    // Combine new R transition with any incoming R transitions
                    break;

                case Constants.RegularLanguage.X:
                case Automaton.Epsilon:
                    // For X and Epsilon Combine with X, S, R and RR transitions on each side
                    var even = true;
                    if (symbol == Constants.RegularLanguage.X)
                    {
                        // Update chains set
                        foreach (var chainEnd in _outgoingTransitionLookup[stateTo].EpsilonChains.Append(stateTo))
                        {
                            _outgoingTransitionLookup[stateFrom].XEpsilonChains.Add(chainEnd);
                            _incomingTransitionLookup[chainEnd].XEpsilonChains.Add(stateFrom);
                        }

                        even = false;
                        // Add epsilon transitions for X + X
                        // TODO: Potential optimisation: 
                        // We shouldn't need to add the state to the queue here as the state should already be consistent at this point,
                        // so we should just add the state to the automaton, so long as it doesn't go to itself.
                        // Counter point: This seems like the wrong behaviour. Why are we collecting X chains?
                        foreach (var xChainStart in _incomingTransitionLookup[stateFrom].XEpsilonChains)
                        {
                            addTransition(xChainStart, stateTo, Automaton.Epsilon, false);
                        }
                        var xStates = _automaton.GetStatesReachableFromStateWithSymbol(stateTo, 'X', true, false);
                        foreach (var xState in xStates)
                        {
                            addTransition(stateFrom, xState, Automaton.Epsilon, false);
                        }
                    }
                    else
                    {
                        // Update chains set
                        _outgoingTransitionLookup[stateFrom].EpsilonChains.Add(stateTo);
                        _incomingTransitionLookup[stateTo].EpsilonChains.Add(stateFrom);
                        foreach (var chainEnd in _outgoingTransitionLookup[stateTo].EpsilonChains.Append(stateTo))
                        {
                            foreach (var chainStart in _incomingTransitionLookup[stateFrom].XEpsilonChains)
                            {
                                _outgoingTransitionLookup[chainStart].XEpsilonChains.Add(chainEnd);
                                _incomingTransitionLookup[chainEnd].XEpsilonChains.Add(chainStart);
                            }
                            foreach (var chainStart in _incomingTransitionLookup[stateFrom].EpsilonChains)
                            {
                                _outgoingTransitionLookup[chainStart].EpsilonChains.Add(chainEnd);
                                _incomingTransitionLookup[chainEnd].EpsilonChains.Add(chainStart);
                            }
                        }

                        // Combine with X on each side X
                        var incomingXEpsilonChains = _incomingTransitionLookup[stateFrom].XEpsilonChains;
                        var xStates = _automaton.GetStatesReachableFromStateWithSymbol(stateTo, 'X', true, false);
                        foreach (var startXState in incomingXEpsilonChains)
                        {
                            foreach (var outgoingX in xStates)
                            {
                                // Both X. Combine.
                                addTransition(startXState, outgoingX, Automaton.Epsilon, false);
                            }
                        }
                    }

                    // First update the Transition lookup.
                    // Update epsilon transitions
                    updateTransitionSetRight(_outgoingTransitionLookup[stateTo].EpsilonTransitions, (s) => s.EpsilonTransitions, !even);
                    // updateTransitionSetLeft(_incomingTransitionLookup[stateFrom].EpsilonTransitions, (s) => s.EpsilonTransitions, !even);
                    updateTransitionSetEpsilon(stateTo, _incomingTransitionLookup[stateFrom].EpsilonTransitions, (s) => s.EpsilonTransitions, !even);
                    // Combine with X,S,R,RR from front side. We only do the front side as the rules we are checking are XX, SXS, RXRXR.
                    // TODO: Review this.
                    foreach (var reachabilityLookup in _outgoingTransitionLookup[stateFrom].EpsilonTransitions)
                    {
                        if (reachabilityLookup.Value.EvenReachable)
                        {
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].STransitions, (s) => s.STransitions, false);
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RTransitions, false);
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RRTransitions, (s) => s.RRTransitions, false);
                        }
                        if (reachabilityLookup.Value.OddReachable)
                        {
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].STransitions, (s) => s.STransitions, true);
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RTransitions, (s) => s.RTransitions, true);
                            updateTransitionSetEpsilon(reachabilityLookup.Key, _incomingTransitionLookup[stateFrom].RRTransitions, (s) => s.RRTransitions, true);
                        }
                    }
                    // Combine R + R
                    var rightReachability = getEpsilonReachabilityDictionary(stateTo);
                    rightReachability = ApplyTransitionToReachabilityDictionary(rightReachability, 'R', false);
                    foreach (var incomingRReachability in _incomingTransitionLookup[stateFrom].RTransitions)
                    {
                        foreach (var epsilonReachability in rightReachability)
                        {
                            var combinedReachability = incomingRReachability.Value.Times(epsilonReachability.Value);
                            UpdateReachabilityStatus(epsilonReachability.Key, combinedReachability, _outgoingTransitionLookup[incomingRReachability.Key].RRTransitions, !even);
                            UpdateReachabilityStatus(incomingRReachability.Key, combinedReachability, _incomingTransitionLookup[epsilonReachability.Key].RRTransitions, !even);
                        }
                    }

                    // Combine S + S
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].STransitions, 'S', 1, even);
                    // Combine R + RR
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RTransitions, 'R', 2, even);
                    // Combine RR + R
                    combineWithBothSides(_incomingTransitionLookup[stateFrom].RRTransitions, 'R', 1, even);

                    break;
                default:
                    throw new ArgumentException($"Character {symbol} has no understood implementation for canonicalisation");
            }
            return toAddTransitions;
        }

        private Dictionary<int, ReachabilityStatus> ApplyTransitionToReachabilityDictionary(Dictionary<int, ReachabilityStatus> reachabilityDictionary,
            char symbol, bool includeEpsilon)
        {
            var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>();
            foreach (var reachbilityLookup in reachabilityDictionary)
            {
                IEnumerable<KeyValuePair<int, ReachabilityStatus>> reachableStates = null!;
                if (includeEpsilon)
                {
                    reachableStates = _outgoingTransitionLookup[reachbilityLookup.Key].GetTransitionDictionary(symbol);
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
        }

        /// <summary>
        /// Updates a reachability status lookup by adding the data from the current reachabilityStatus
        /// </summary>
        private void UpdateReachabilityStatus(int fromState, ReachabilityStatus reachabilityStatus, 
            Dictionary<int, ReachabilityStatus> reachabilityLookup, bool negated)
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
        }
    }
}