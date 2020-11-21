
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MatrixSolver.DataTypes.Automata
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
            int statesCount = 0;
            int startState = 0;
            int goalState = 0;
            var symbols = new HashSet<char>(alphabet);
            Stack<int> preBracketStates = new Stack<int>();
            // Create automaton
            var automaton = new Automaton(alphabet);

            // https://medium.com/swlh/visualizing-thompsons-construction-algorithm-for-nfas-step-by-step-f92ef378581b
            // Convert to postfix
            var postFixRegex = RegexToPostfix(regex);
            Stack<(int startState, int goalState)> NFAPartitions = new Stack<(int, int)>();
            while(postFixRegex.TryDequeue(out char character))
            {
                switch (character)
                {
                    case KleeneStarOperator:
                        var partition = NFAPartitions.Pop();
                        startState = statesCount++;
                        goalState = statesCount++;
                        automaton.AddState(startState);
                        automaton.AddState(goalState);
                        automaton.AddTransition(startState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(startState, goalState, Automaton.Epsilon);
                        automaton.AddTransition(partition.goalState, partition.startState, Automaton.Epsilon);
                        automaton.AddTransition(partition.goalState, goalState, Automaton.Epsilon);
                        NFAPartitions.Push((startState, goalState));
                        break;
                    case ConcatenationOperator:
                        var rightPartition = NFAPartitions.Pop();
                        var leftPartition = NFAPartitions.Pop();
                        // TODO: Remove epsilon transition and merge the states instead
                        automaton.AddTransition(leftPartition.goalState, rightPartition.startState, Automaton.Epsilon);
                        //automaton.MergeStates(firstPartition.goalState, secondPartition.startState);
                        NFAPartitions.Push((leftPartition.startState, rightPartition.goalState));
                        break;
                    case UnionOperator:
                        var topPartition = NFAPartitions.Pop();
                        var bottomPartition = NFAPartitions.Pop();
                        startState = statesCount++;
                        goalState = statesCount++;
                        automaton.AddState(startState);
                        automaton.AddState(goalState);
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
                        startState = statesCount++;
                        goalState = statesCount++;
                        automaton.AddState(startState);
                        automaton.AddState(goalState);
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
            string newRegex = "";
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
                            newRegex += ConcatenationOperator;
                        }
                        skipNext = true;
                        break;
                    default:
                        if (!skipThis)
                        {
                            newRegex += ConcatenationOperator;
                        }
                        break;
                }

                newRegex += character;

            }
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
            while(operatorStack.TryPop(out nextOperator))
            {
                postfixQueue.Enqueue(nextOperator);
            }

            return postfixQueue;
        }
    }
}