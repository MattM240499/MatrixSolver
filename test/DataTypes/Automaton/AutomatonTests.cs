using Xunit;
using MatrixSolver.DataTypes.Automata;
using System;
using System.Linq;
using MatrixSolver.DataTypes;
using System.Collections.Generic;

namespace MatrixSolver.Tests.DataTypes.Automata
{
    public class AutomatonTests
    {
        private readonly char[] _alphabet = new[] { 'a', 'b', 'c', 'd' };
        private char _validSymbol => _alphabet[0];
        private char _invalidSymbol = 'z';

        [Fact]
        public void AddState_AddsState_IfIdUnique()
        {
            var automaton = new Automaton(_alphabet);
            var stateIds = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                var stateId = automaton.AddState();
                stateIds.Add(stateId);
            }

            var states = automaton.States;
            Assert.Equal(3, states.Count);
            foreach (var state in states)
            {
                Assert.Contains(state, stateIds);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void AddState_SetsStateTypes(bool isStartState, bool isGoalState)
        {
            var automaton = new Automaton(_alphabet);

            int stateId = automaton.AddState(isGoalState: isGoalState, isStartState: isStartState);
            Assert.Single(automaton.States);
            Assert.Single(automaton.States);
            if (isStartState)
            {
                Assert.Single(automaton.StartStates);
                Assert.Contains(stateId, automaton.StartStates);
            }
            else
            {
                Assert.Empty(automaton.StartStates);
            }
            if (isGoalState)
            {
                Assert.Single(automaton.GoalStates);
                Assert.Contains(stateId, automaton.GoalStates);
            }
            else
            {
                Assert.Empty(automaton.GoalStates);
            }
        }

        [Fact]
        public void AddTransition_AddsTransition_IfStatesInList()
        {
            var stateIds = new[] { 0, 1, 2 };
            var automaton = CreateEmptyAutomatonWithStates(stateIds);

            foreach (var stateIdFrom in stateIds)
            {
                foreach (var stateIdTo in stateIds)
                {
                    automaton.AddTransition(stateIdFrom, stateIdTo, 'a');
                }
            }
        }

        [Fact]
        public void AddTransition_Throws_IfFromStateNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 0, 1, 2});

            Assert.Throws<InvalidOperationException>(() => automaton.AddTransition(3, 1, _validSymbol));
        }

        [Fact]
        public void AddTransition_Throws_IfToStateNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 0, 1, 2 });

            Assert.Throws<InvalidOperationException>(() => automaton.AddTransition(1, 3, _validSymbol));
        }

        [Fact]
        public void AddTransition_Throws_IfSymbolNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 1, 2, 3 });

            Assert.Throws<InvalidOperationException>(() => automaton.AddTransition(1, 2, _invalidSymbol));
        }

        [Theory]
        [InlineData("abb", false)]
        [InlineData("abc", true)]
        [InlineData("abcbcbc", true)]
        [InlineData("bbbaacc", false)]
        public void IsValidWord_ReturnsTrue_ForNFAIfValidWord(string word, bool expected)
        {
            var automaton = CreateNFA1();

            Assert.Equal(expected, automaton.IsValidWord(word));
        }

        [Theory]
        [InlineData("abb", false)]
        [InlineData("abc", true)]
        [InlineData("abcbcbc", true)]
        [InlineData("bbbaacc", false)]
        public void IsValidWord_CalculatesCorrectly_ForDFAIfValidWord(string word, bool expected)
        {
            var automaton = CreateDFA1();

            Assert.Equal(expected, automaton.IsValidWord(word));
        }

        [Fact]
        public void IsValidWord_ReturnsFalse_IfNoStartStates()
        {
            var automaton = CreateDFA1();
            automaton.UnsetStartState(0);
            Assert.False(automaton.IsValidWord("abc"));
        }

        [Fact]
        public void IsValidWord_ReturnsFalse_IfNoGoalStates()
        {
            var automaton = CreateDFA1();
            automaton.UnsetGoalState(3);
            Assert.False(automaton.IsValidWord("abc"));
        }

        [Fact]
        public void ToDFA_ConvertsCorrectly()
        {
            var automaton = CreateNFA1();
            var automtonDFA = automaton.ToDFA();
            Assert.Equal(4, automtonDFA.States.Count);
            Assert.True(automaton.IsValidWord("abc"));
            Assert.True(automaton.IsValidWord("abcbcbc"));
        }

        [Fact]
        public void ToDFA_ConvertsCorrectly_WithEpsilonStates()
        {
            var automaton = CreateNFA2();
            var dfa = automaton.ToDFA();

            Assert.Equal(3, dfa.States.Count);
            Assert.True(dfa.IsValidWord(""));
            Assert.True(dfa.IsValidWord("a"));
            Assert.True(dfa.IsValidWord("b"));
            Assert.True(dfa.IsValidWord("aababbaababababaaababbbababab"));
        }

        private Automaton CreateEmptyAutomatonWithStates(int[] stateIds)
        {
            var automaton = new Automaton(_alphabet);

            foreach (var id in stateIds)
            {
                automaton.AddState();
            }
            return automaton;
        }

        private Automaton CreateDFA1()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 0, 1, 2, 3 });
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(3);
            // TODO: Add report section for these tests
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(1, 2, 'b');
            automaton.AddTransition(2, 3, 'c');
            automaton.AddTransition(3, 2, 'b');
            return automaton;
        }

        private Automaton CreateNFA1()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 0, 1, 2, 3 });
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(3);
            // TODO: Add report section for these tests
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(1, 2, 'b');
            automaton.AddTransition(2, 1, 'c');
            automaton.AddTransition(2, 3, 'c');

            return automaton;
        }

        /// <summary>
        /// Thompson construction of (a | b)*
        /// </summary>
        private Automaton CreateNFA2()
        {
            var automaton = CreateEmptyAutomatonWithStates(new[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            // TODO: Add report section for these tests
            // Union
            automaton.AddTransition(0, 2, Automaton.Epsilon);
            automaton.AddTransition(0, 1, Automaton.Epsilon);
            automaton.AddTransition(1, 3, 'a');
            automaton.AddTransition(2, 4, 'b');
            automaton.AddTransition(3, 5, Automaton.Epsilon);
            automaton.AddTransition(4, 5, Automaton.Epsilon);
            // Kleene star
            automaton.AddTransition(6, 0, Automaton.Epsilon);
            automaton.AddTransition(6, 7, Automaton.Epsilon);
            automaton.AddTransition(5, 0, Automaton.Epsilon);
            automaton.AddTransition(5, 7, Automaton.Epsilon);

            automaton.SetAsStartState(6);
            automaton.SetAsGoalState(7);

            return automaton;
        }
    }
}