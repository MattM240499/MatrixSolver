
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    public static class AutomatonUtil
    {
        private static readonly Dictionary<char, int> _operatorPrecedence;
        private static readonly HashSet<char> _operators = new HashSet<char> { KleeneStarOperator, ConcatenationOperator, UnionOperator };
        private const char KleeneStarOperator = '*';
        private const char ConcatenationOperator = '?';
        private const char UnionOperator = '|';


        static AutomatonUtil()
        {
            _operatorPrecedence = new Dictionary<char, int>();
            _operatorPrecedence[KleeneStarOperator] = 1;
            _operatorPrecedence[ConcatenationOperator] = 2;
            _operatorPrecedence[UnionOperator] = 3;
        }

        /// <summary>
        /// Converts a regex to an automaton
        /// Based on https://en.wikipedia.org/wiki/Thompson%27s_construction
        /// </summary>
        public static Automaton RegexToAutomaton(string regex, char[] alphabet)
        {
            // Initialise variables
            int startState = 0;
            int goalState = 0;
            var symbols = new HashSet<char>(alphabet);
            symbols.Add(Automaton.Epsilon);
            Stack<int> preBracketStates = new Stack<int>();
            // Create automaton
            var automaton = new Automaton(alphabet);

            // https://medium.com/swlh/visualizing-thompsons-construction-algorithm-for-nfas-step-by-step-f92ef378581b
            // Convert to postfix
            var postFixRegex = RegexToPostfix(regex);
            Stack<(int startState, int goalState)> NFAPartitions = new Stack<(int, int)>();
            while (postFixRegex.TryDequeue(out char character))
            {
                switch (character)
                {
                    case KleeneStarOperator:
                        var partition = NFAPartitions.Pop();
                        startState = automaton.AddState();
                        goalState = automaton.AddState();
                        automaton.AddTransition(startState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(startState, goalState, Automaton.Epsilon);
                        automaton.AddTransition(partition.goalState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(partition.goalState, goalState, Automaton.Epsilon);
                        NFAPartitions.Push((startState, goalState));
                        break;
                    case ConcatenationOperator:
                        var rightPartition = NFAPartitions.Pop();
                        var leftPartition = NFAPartitions.Pop();
                        // Because each partition will always have 0 incoming transitions from their start states,
                        // to merge the states, it is simply a case of copying the transitions from the right partition's
                        // start state to the final state of the left partition, and then finally remove the first state.
                        int copyState = rightPartition.startState;
                        int copiedToState = leftPartition.goalState;
                        foreach (var symbol in symbols)
                        {
                            var states = automaton.TransitionMatrix.GetStates(copyState, symbol);
                            foreach (var state in states)
                            {
                                automaton.AddTransition(copiedToState, state, symbol);
                            }
                        }
                        automaton.DeleteState(copyState, skipIncomingTransitions: true);
                        NFAPartitions.Push((leftPartition.startState, rightPartition.goalState));
                        break;
                    case UnionOperator:
                        var topPartition = NFAPartitions.Pop();
                        var bottomPartition = NFAPartitions.Pop();
                        startState = automaton.AddState();
                        goalState = automaton.AddState();
                        automaton.AddTransition(startState, topPartition.startState, Automaton.Epsilon);
                        automaton.AddTransition(startState, bottomPartition.startState, Automaton.Epsilon);
                        automaton.AddTransition(topPartition.goalState, goalState, Automaton.Epsilon);
                        automaton.AddTransition(bottomPartition.goalState, goalState, Automaton.Epsilon);
                        NFAPartitions.Push((startState, goalState));
                        break;
                    default:
                        // Not an operator. It is a symbol
                        if (!symbols.Contains(character))
                        {
                            throw new ArgumentException($"Invalid regex. Character {character} not a member of the alphabet");
                        }
                        startState = automaton.AddState();
                        goalState = automaton.AddState();
                        automaton.AddTransition(startState, goalState, character);
                        NFAPartitions.Push((startState, goalState));
                        break;
                }
            }
            var automataPartition = NFAPartitions.Pop();
            automaton.SetAsStartState(automataPartition.startState);
            automaton.SetAsGoalState(automataPartition.goalState);
            return automaton;
        }

        public static Queue<char> RegexToPostfix(string regex)
        {
            var postfixQueue = new Queue<char>();
            var operatorStack = new Stack<char>();
            char nextOperator;
            // First add the concatenationOperator variable
            bool skipNext = true;
            var newRegexSb = new StringBuilder(regex.Length * 2);
            for (int i = 0; i < regex.Length; i++)
            {
                var character = regex[i];
                bool skipThis = skipNext;
                skipNext = false;
                switch (character)
                {
                    case KleeneStarOperator:
                    case ')':
                        break;
                    case UnionOperator:
                        // Skip the next *
                        skipNext = true;
                        break;
                    case '(':
                        if (!skipThis)
                        {
                            newRegexSb.Append(ConcatenationOperator);
                        }
                        skipNext = true;
                        break;
                    default:
                        if (!skipThis)
                        {
                            newRegexSb.Append(ConcatenationOperator);
                        }
                        break;
                }

                newRegexSb.Append(character);
            }
            var newRegex = newRegexSb.ToString();
            // Now apply the Shunting-Yard Algorithm
            // https://medium.com/@gregorycernera/converting-regular-expressions-to-postfix-notation-with-the-shunting-yard-algorithm-63d22ea1cf88
            foreach (var character in newRegex)
            {
                switch (character)
                {
                    case KleeneStarOperator:
                    case ConcatenationOperator:
                    case UnionOperator:
                        // If the operator on the stack has precedence,
                        while (operatorStack.Count > 0 && _operators.Contains(operatorStack.Peek()) && (_operatorPrecedence[character] >= _operatorPrecedence[operatorStack.Peek()]))
                        {
                            nextOperator = operatorStack.Pop();
                            postfixQueue.Enqueue(nextOperator);
                        }
                        operatorStack.Push(character);
                        break;
                    case '(':
                        operatorStack.Push(character);
                        break;
                    case ')':
                        // TODO: Could change these into trypop for error handling.
                        nextOperator = operatorStack.Pop();
                        while (nextOperator != '(')
                        {
                            postfixQueue.Enqueue(nextOperator);
                            nextOperator = operatorStack.Pop();
                        }
                        break;
                    default:
                        postfixQueue.Enqueue(character);
                        break;
                }
            }
            while (operatorStack.TryPop(out nextOperator))
            {
                postfixQueue.Enqueue(nextOperator);
            }

            return postfixQueue;
        }

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

        [Obsolete("Old Canonicalise.")]
        internal static Automaton PopulateDFAWithXAndEpsilonTransitionsOld(this Automaton automaton)
        {
            bool changes = true;
            while (changes)
            {
                changes = false;
                // First, Look for XX and add epsilon transition where possible
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
                // Look for Identity statements (RRR/SS)
                foreach (var startState in automaton.States)
                {
                    // Now look for S * X^alpha * S states
                    var SSreachabilityDictionary = automaton.GetStatesReachableFromStateWithSymbol(startState, Constants.RegularLanguage.S, false)
                        .ToDictionary((s) => s, (s) => ReachabilityStatus.Even())
                        .ApplyMultipleXTransitionToReachabilityDictionary(automaton)
                        .ApplyTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.S, false);
                    foreach (var (finalState, reachability) in SSreachabilityDictionary)
                    {
                        if (AddTransitionsFromReachabilityStatus(automaton, startState, finalState, reachability))
                        {
                            changes = true;
                        }
                    }
                    // Add R * X^alpha1 * R * X^alpha2 * R
                    var RRRreachabilityDictionary = automaton.GetStatesReachableFromStateWithSymbol(startState, Constants.RegularLanguage.R, false)
                        .ToDictionary((s) => s, (s) => ReachabilityStatus.Even())
                        .ApplyMultipleXTransitionToReachabilityDictionary(automaton)
                        .ApplyTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.R, true)
                        .ApplyMultipleXTransitionToReachabilityDictionary(automaton)
                        .ApplyTransitionToReachabilityDictionary(automaton, Constants.RegularLanguage.R, false);
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

        internal static Automaton PopulateDFAWithXAndEpsilonTransitions(this Automaton automaton)
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
                        var transitions = canonicalStateTransitionLookup.AddTransition(state, CanonicalStateTransitionLookup.ConvertToSymbol(symbol), toState, false);
                        foreach (var transition in transitions)
                        {
                            transitionQueue.Enqueue(transition);
                        }
                    }
                }
            }
            while (transitionQueue.TryDequeue(out var transition))
            {
                if(transition.StateFrom == transition.StateTo && transition.Symbol == TransitionSymbol.Epsilon)
                {
                    continue;
                }
                if(CanonicalStateTransitionLookup.TryConvertToChar(transition.Symbol, out var symbolChar))
                {
                    if(!automaton.AddTransition(transition.StateFrom, transition.StateTo, symbolChar!.Value))
                    {
                        continue;
                    }
                }
                
                var transitions = canonicalStateTransitionLookup.AddTransition(transition.StateFrom, transition.Symbol, transition.StateTo, transition.Negated);
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
                changes = false;
                foreach (var state in automaton.States)
                {
                    var xStates = automaton.GetStatesReachableFromStateWithSymbol(state, Constants.RegularLanguage.X);
                    foreach (var xState in xStates)
                    {
                        var epsilonStates = automaton.GetStatesReachableFromStateWithSymbol(xState, Constants.RegularLanguage.X, false);
                        foreach (var epsilonState in epsilonStates)
                        {
                            if (automaton.AddTransition(state, epsilonState, Automaton.Epsilon))
                            {
                                changes = true;
                            }
                        }
                    }
                }
            }
            return automaton;
        }

        private static IReadOnlyDictionary<int, ReachabilityStatus> ApplyMultipleXTransitionToReachabilityDictionary(
            this IReadOnlyDictionary<int, ReachabilityStatus> currentReachabilityDictionary, Automaton automaton
        )
        {
            var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>(currentReachabilityDictionary);
            foreach (var (currentState, x1Reachability) in currentReachabilityDictionary)
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
                    stateReachability.OddReachable |= x1Reachability.EvenReachable;
                    stateReachability.EvenReachable |= x1Reachability.OddReachable;
                }
            }
            return newReachabilityDictionary;
        }

        private static IReadOnlyDictionary<int, ReachabilityStatus> ApplyTransitionToReachabilityDictionary(
            this IReadOnlyDictionary<int, ReachabilityStatus> currentReachabilityDictionary, Automaton automaton,
             char symbol, bool useEpsilonStatesAtEnd)
        {
            var newReachabilityDictionary = new Dictionary<int, ReachabilityStatus>();
            foreach (var (state, reachabilityStatus) in currentReachabilityDictionary)
            {
                var newStates = automaton.GetStatesReachableFromStateWithSymbol(state, symbol, false, useEpsilonStatesAtEnd);
                foreach (var newState in newStates)
                {
                    if (!newReachabilityDictionary.TryGetValue(newState, out var newStatereachabilityStatus))
                    {
                        newStatereachabilityStatus = new ReachabilityStatus();
                        newReachabilityDictionary[newState] = newStatereachabilityStatus;
                    }
                    newStatereachabilityStatus.EvenReachable |= reachabilityStatus.EvenReachable;
                    newStatereachabilityStatus.OddReachable |= reachabilityStatus.OddReachable;
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

    public class ReachabilityStatus
    {
        public bool EvenReachable { get; set; }
        public bool OddReachable { get; set; }
        public ReachabilityStatus()
        { }

        public static ReachabilityStatus Even()
        {
            return new ReachabilityStatus() { EvenReachable = true };
        }

        public ReachabilityStatus Times(ReachabilityStatus status)
        {
            var newRs = new ReachabilityStatus();
            newRs.EvenReachable = (status.EvenReachable && this.EvenReachable) || (status.OddReachable && this.OddReachable);
            newRs.OddReachable = (status.EvenReachable && this.OddReachable) || (status.OddReachable && this.EvenReachable);
            return newRs;
        }
    }
}