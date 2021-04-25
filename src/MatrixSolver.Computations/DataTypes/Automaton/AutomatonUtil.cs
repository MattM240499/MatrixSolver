
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    public static class AutomatonUtil
    {
        private const char KleeneStarOperator = '*';
        private const char ConcatenationOperator = '?';
        private const char UnionOperator = '|';
        private static readonly Dictionary<char, int> _operatorPrecedence = new Dictionary<char, int>
        {
            [KleeneStarOperator] = 1,
            [ConcatenationOperator] = 2,
            [UnionOperator] = 3
        };

        /// <summary>
        /// Converts a regex to an automaton
        /// Based on https://en.wikipedia.org/wiki/Thompson%27s_construction
        /// </summary>
        public static Automaton RegexToAutomaton(string regex, char[] alphabet)
        {
            // Initialise variables
            int startState = 0;
            int finalState = 0;
            var symbols = new HashSet<char>(alphabet);
            symbols.Add(Automaton.Epsilon);
            // Create automaton
            var automaton = new Automaton(alphabet);

            // https://medium.com/swlh/visualizing-thompsons-construction-algorithm-for-nfas-step-by-step-f92ef378581b
            // Convert to postfix
            var postFixRegex = RegexToPostfix(regex);
            Stack<(int startState, int finalState)> NFAPartitions = new Stack<(int, int)>();
            foreach (var character in postFixRegex)
            {
                switch (character)
                {
                    case KleeneStarOperator:
                        var partition = NFAPartitions.Pop();
                        startState = automaton.AddState();
                        finalState = automaton.AddState();
                        automaton.AddTransition(startState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(startState, finalState, Automaton.Epsilon);
                        automaton.AddTransition(partition.finalState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(partition.finalState, finalState, Automaton.Epsilon);
                        NFAPartitions.Push((startState, finalState));
                        break;
                    case ConcatenationOperator:
                        var rightPartition = NFAPartitions.Pop();
                        var leftPartition = NFAPartitions.Pop();
                        // Because each partition will always have 0 incoming transitions from their start states,
                        // to merge the states, it is simply a case of copying the transitions from the right partition's
                        // start state to the final state of the left partition, and then finally remove the first state.
                        int copyState = rightPartition.startState;
                        int copiedToState = leftPartition.finalState;
                        foreach (var symbol in symbols)
                        {
                            var states = automaton.TransitionMatrix.GetStates(copyState, symbol);
                            foreach (var state in states)
                            {
                                automaton.AddTransition(copiedToState, state, symbol);
                            }
                        }
                        automaton.DeleteState(copyState, skipIncomingTransitions: true);
                        NFAPartitions.Push((leftPartition.startState, rightPartition.finalState));
                        break;
                    case UnionOperator:
                        var topPartition = NFAPartitions.Pop();
                        var bottomPartition = NFAPartitions.Pop();
                        startState = automaton.AddState();
                        finalState = automaton.AddState();
                        automaton.AddTransition(startState, topPartition.startState, Automaton.Epsilon);
                        automaton.AddTransition(startState, bottomPartition.startState, Automaton.Epsilon);
                        automaton.AddTransition(topPartition.finalState, finalState, Automaton.Epsilon);
                        automaton.AddTransition(bottomPartition.finalState, finalState, Automaton.Epsilon);
                        NFAPartitions.Push((startState, finalState));
                        break;
                    default:
                        // Not an operator. It is a symbol
                        if (!symbols.Contains(character))
                        {
                            throw new ArgumentException($"Invalid regex. Character {character} not a member of the alphabet");
                        }
                        startState = automaton.AddState();
                        finalState = automaton.AddState();
                        automaton.AddTransition(startState, finalState, character);
                        NFAPartitions.Push((startState, finalState));
                        break;
                }
            }
            var automataPartition = NFAPartitions.Pop();
            automaton.SetAsStartState(automataPartition.startState);
            automaton.SetAsFinalState(automataPartition.finalState);
            return automaton;
        }

        public static IReadOnlyList<char> RegexToPostfix(string regex)
        {
            var regexWithConcatenationOperator = AddConcatenationOperatorToRegex(regex);
            // Now apply the Shunting-Yard Algorithm
            // https://medium.com/@gregorycernera/converting-regular-expressions-to-postfix-notation-with-the-shunting-yard-algorithm-63d22ea1cf88
            var postfixCharacters = new List<char>(regexWithConcatenationOperator.Length);
            var operatorStack = new Stack<char>();
            char nextOperator;
            foreach (var character in regexWithConcatenationOperator)
            {
                switch (character)
                {
                    case KleeneStarOperator:
                    case ConcatenationOperator:
                    case UnionOperator:
                        // If the operator on the stack has precedence,
                        while (operatorStack.Count > 0)
                        {
                            var topOperator = operatorStack.Peek();
                            var charPrecedence = _operatorPrecedence[character];
                            // If the item on top of the stack is not an operator (i.e. a bracket)
                            // or it has higher precedence, then stop.
                            if (!_operatorPrecedence.TryGetValue(topOperator, out var stackTopPrecedence) || charPrecedence < stackTopPrecedence)
                            {
                                break;
                            }

                            nextOperator = operatorStack.Pop();
                            postfixCharacters.Add(nextOperator);
                        }
                        operatorStack.Push(character);
                        break;
                    case '(':
                        operatorStack.Push(character);
                        break;
                    case ')':
                        if (!operatorStack.TryPop(out nextOperator))
                        {
                            throw new ArgumentException("Regex supplied had inconsistent bracketing.");
                        }
                        while (nextOperator != '(')
                        {
                            postfixCharacters.Add(nextOperator);
                            nextOperator = operatorStack.Pop();
                        }
                        break;
                    default:
                        postfixCharacters.Add(character);
                        break;
                }
            }
            while (operatorStack.TryPop(out nextOperator))
            {
                postfixCharacters.Add(nextOperator);
            }

            return postfixCharacters;
        }

        private static string AddConcatenationOperatorToRegex(string regex)
        {
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
            return newRegex;
        }
    }
}