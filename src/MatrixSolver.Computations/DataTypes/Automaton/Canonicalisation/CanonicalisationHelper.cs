
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    public static class CanonicalisationHelper
    {
        /// <summary>
        /// Updates a DFA to canonical form.
        /// </summary>
        public static Automaton UpdateAutomatonToAcceptCanonicalWords(this Automaton automaton)
        {
            return automaton
                .PopulateDFAWithXAndEpsilonTransitions()
                .AddXSurroundedPaths()
                ;
        }

        internal static Automaton PopulateDFAWithXAndEpsilonTransitions(this Automaton automaton)
        {
            return automaton.PopulateDFAWithXAndEpsilonTransitionsQueueBased();
        }

        [Obsolete("Queue based approach is much faster.")]
        internal static Automaton PopulateDFAWithXAndEpsilonTransitionsNaive(this Automaton automaton)
        {
            bool changes = true;
            while (changes)
            {
                changes = AddEpsilonStatesForXXSubPaths(automaton);
                // Look for paths (RRR/SS)
                foreach (var startState in automaton.States)
                {
                    // Now look for S * X^alpha * S states
                    var SSreachabilityDictionary = automaton.GetStatesReachableFromStateWithSymbol(startState, Constants.RegularLanguage.S,
                        useEpsilonStatesFromInitial: false, useEpsilonStatesAtEnd: true)
                        .ToDictionary((s) => s, (s) => ReachabilityStatus.Even())
                        .ApplyXTransitionToReachabilityDictionary(automaton)
                        .ApplySOrRTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.S, false);
                    foreach (var (finalState, reachability) in SSreachabilityDictionary)
                    {
                        if (AddTransitionsFromReachabilityStatus(automaton, startState, finalState, reachability))
                        {
                            changes = true;
                        }
                    }
                    // Add R * X^alpha1 * R * X^alpha2 * R
                    var RRRreachabilityDictionary = automaton.GetStatesReachableFromStateWithSymbol(startState, Constants.RegularLanguage.R,
                        useEpsilonStatesFromInitial: false, useEpsilonStatesAtEnd: true)
                        .ToDictionary((s) => s, (s) => ReachabilityStatus.Even())
                        .ApplyXTransitionToReachabilityDictionary(automaton)
                        .ApplySOrRTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.R, true)
                        .ApplyXTransitionToReachabilityDictionary(automaton)
                        .ApplySOrRTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.R, false);
                    foreach (var (finalState, reachability) in RRRreachabilityDictionary)
                    {
                        if (AddTransitionsFromReachabilityStatus(automaton, startState, finalState, reachability))
                        {
                            changes = true;
                        }
                    }
                }
            }
            return automaton;
        }

        /// <summary>
        /// A queue based approach to performing the transition additions to the automaton where we only consider new transitions
        /// that are reachable from the transitions we have added
        /// </summary>
        internal static Automaton PopulateDFAWithXAndEpsilonTransitionsQueueBased(this Automaton automaton)
        {
            var canonicalStateTransitionLookup = new CanonicalStateTransitionLookup(automaton);
            var transitionQueue = new Queue<Transition>();
            foreach (var state in automaton.States)
            {
                foreach (var symbol in automaton.Alphabet.Append(Automaton.Epsilon))
                {
                    var toStates = automaton.TransitionMatrix.GetStates(state, symbol);
                    foreach (var toState in toStates)
                    {
                        transitionQueue.Enqueue(new Transition(state, toState, symbol));
                    }
                }
            }
            while (transitionQueue.TryDequeue(out var transition))
            {
                var transitions = canonicalStateTransitionLookup.AddTransition(transition.StateFrom, transition.Symbol, transition.StateTo);
                foreach (var newTransition in transitions)
                {
                    transitionQueue.Enqueue(newTransition);
                }
            }

            return automaton;
        }

        internal static Automaton AddXSurroundedPaths(this Automaton automaton)
        {
            var states = new List<int>(automaton.States);
            var symbols = new[] { Constants.RegularLanguage.R, Constants.RegularLanguage.S };
            foreach (var fromState in states)
            {
                foreach (var symbol in symbols)
                {
                    var toStates = new List<int>(automaton.TransitionMatrix.GetStates(fromState, symbol));
                    foreach (var toState in toStates)
                    {
                        // Add a new state before and after
                        int beforeState = automaton.AddState();
                        int afterState = automaton.AddState();
                        // Now add the new transitions between the states
                        automaton.AddTransition(fromState, beforeState, Constants.RegularLanguage.X);
                        automaton.AddTransition(beforeState, afterState, symbol);
                        automaton.AddTransition(afterState, toState, Constants.RegularLanguage.X);
                    }
                }
            }
            // Add epsilon transitions between all new X states
            bool changes = true;
            while (changes)
            {
                changes = AddEpsilonStatesForXXSubPaths(automaton);
            }
            return automaton;
        }

        private static bool AddEpsilonStatesForXXSubPaths(Automaton automaton)
        {
            var changes = false;
            foreach (var state in automaton.States)
            {
                var states = automaton.GetStatesReachableFromStateWithSymbol(state, Constants.RegularLanguage.X, false);

                foreach (var xReachableState in states)
                {
                    var epsilonStates = automaton.GetStatesReachableFromStateWithSymbol(xReachableState, Constants.RegularLanguage.X, false, false);
                    foreach (var epsilonState in epsilonStates)
                    {
                        // Discard epsilon transition that loop back on the same state as they add no value
                        if (state != epsilonState)
                        {
                            if (automaton.AddTransition(state, epsilonState, Automaton.Epsilon))
                            {
                                changes = true;
                            }
                        }
                    }
                }
            }
            return changes;
        }

        private static IReadOnlyDictionary<int, ReachabilityStatus> ApplyXTransitionToReachabilityDictionary(
            this IReadOnlyDictionary<int, ReachabilityStatus> currentReachabilityDictionary, Automaton automaton
        )
        {
            var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>(currentReachabilityDictionary);
            foreach (var (currentState, currentReachabilityStatus) in currentReachabilityDictionary)
            {
                var reachableStates = automaton.GetStatesReachableFromStateWithSymbol(currentState, Constants.RegularLanguage.X, false);
                // Now combine all those states with the current reachability dictionary
                foreach (var state in reachableStates)
                {
                    if (!newReachabilityDictionary.TryGetValue(state, out var stateReachability))
                    {
                        stateReachability = new ReachabilityStatus();
                        newReachabilityDictionary[state] = stateReachability;
                    }
                    // This state has reachability of the opposite of the previous state
                    stateReachability.OddReachable |= currentReachabilityStatus.EvenReachable;
                    stateReachability.EvenReachable |= currentReachabilityStatus.OddReachable;
                }
            }
            return newReachabilityDictionary;
        }

        private static IReadOnlyDictionary<int, ReachabilityStatus> ApplySOrRTransitionToReachabilityDictionary(
            this IReadOnlyDictionary<int, ReachabilityStatus> currentReachabilityDictionary, Automaton automaton,
            char symbol, bool useEpsilonStatesAtEnd)
        {
            var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>();
            foreach (var (currentState, currentReachabilityStatus) in currentReachabilityDictionary)
            {
                var reachableStates = automaton.GetStatesReachableFromStateWithSymbol(currentState, symbol, false, useEpsilonStatesAtEnd);
                foreach (var state in reachableStates)
                {
                    if (!newReachabilityDictionary.TryGetValue(state, out var stateReachability))
                    {
                        stateReachability = new ReachabilityStatus();
                        newReachabilityDictionary[state] = stateReachability;
                    }
                    stateReachability.EvenReachable |= currentReachabilityStatus.EvenReachable;
                    stateReachability.OddReachable |= currentReachabilityStatus.OddReachable;
                }
            }
            return newReachabilityDictionary;
        }

        private static bool AddTransitionsFromReachabilityStatus(Automaton automaton, int startState, int finalState, ReachabilityStatus reachabilityStatus)
        {
            bool changes = false;
            if (reachabilityStatus.EvenReachable)
            {
                if (automaton.AddTransition(startState, finalState, Constants.RegularLanguage.X))
                {
                    changes = true;
                }
            }
            else if (reachabilityStatus.OddReachable)
            {
                // Odd reachable so add X transition
                if (startState != finalState && automaton.AddTransition(startState, finalState, Automaton.Epsilon))
                {
                    changes = true;
                }
            }
            return changes;
        }
    }
}