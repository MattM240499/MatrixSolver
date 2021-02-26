using Xunit;
using MatrixSolver.Computations.DataTypes.Automata;
using System;
using System.Linq;
using MatrixSolver.Computations.DataTypes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MatrixSolver.Computations.Tests.DataTypes.Automata
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
            var states = 3;
            var automaton = CreateEmptyAutomatonWithStates(3);

            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < states; j++)
                {
                    automaton.AddTransition(i, j, 'a');
                }
            }
        }

        [Fact]
        public void AddTransition_Throws_IfFromStateNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(3);

            Assert.Throws<InvalidOperationException>(() => automaton.AddTransition(3, 1, _validSymbol));
        }

        [Fact]
        public void AddTransition_Throws_IfToStateNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(3);

            Assert.Throws<InvalidOperationException>(() => automaton.AddTransition(1, 3, _validSymbol));
        }

        [Fact]
        public void AddTransition_Throws_IfSymbolNotInList()
        {
            var automaton = CreateEmptyAutomatonWithStates(3);

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

        [Fact]
        public void Minimize_MinimizesCorrectly()
        {
            // https://www.youtube.com/watch?v=0XaGAkY09Wc&t=2s
            var automaton = CreateEmptyAutomatonWithStates(5);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(4);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 2, 'b');
            automaton.AddTransition(1, 1, 'a');
            automaton.AddTransition(1, 3, 'b');
            automaton.AddTransition(2, 1, 'a');
            automaton.AddTransition(2, 2, 'b');
            automaton.AddTransition(3, 1, 'a');
            automaton.AddTransition(3, 4, 'b');
            automaton.AddTransition(4, 1, 'a');
            automaton.AddTransition(4, 2, 'b');
            var minimizedAutomaton = automaton.MinimizeDFA();

            Assert.Equal(4, minimizedAutomaton.States.Count);
        }

        [Fact]
        public void Minmize_MinimizesCorrectly2()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(3);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 3, 'b');
            automaton.AddTransition(1, 2, 'a');
            automaton.AddTransition(1, 3, 'b');
            automaton.AddTransition(2, 2, 'a');
            automaton.AddTransition(2, 3, 'b');
            automaton.AddTransition(3, 3, 'a');
            automaton.AddTransition(3, 2, 'b');
            var minimizedAutomaton = automaton.MinimizeDFA();

            Assert.Equal(2, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("aaaaaabaaabbaaa"));
            Assert.True(minimizedAutomaton.IsValidWord("b"));
        }

        [Fact]
        public void Minmize_MinimizesCorrectly3()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(3);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 1, 'b');
            automaton.AddTransition(1, 2, 'a');
            automaton.AddTransition(1, 3, 'b');
            automaton.AddTransition(2, 2, 'a');
            automaton.AddTransition(2, 3, 'b');
            automaton.AddTransition(3, 3, 'a');
            automaton.AddTransition(3, 2, 'b');
            var minimizedAutomaton = automaton.MinimizeDFA();

            Assert.Equal(3, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("ab"));
            Assert.True(minimizedAutomaton.IsValidWord("abaaaaaaaaaaabaabbaaab"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly4()
        {
            // https://www.youtube.com/watch?v=ex9sPLq5CRg
            var automaton = CreateEmptyAutomatonWithStates(8);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(2);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 5, 'b');
            automaton.AddTransition(1, 6, 'a');
            automaton.AddTransition(1, 2, 'b');
            automaton.AddTransition(2, 0, 'a');
            automaton.AddTransition(2, 2, 'b');
            automaton.AddTransition(3, 2, 'a');
            automaton.AddTransition(3, 6, 'b');
            automaton.AddTransition(4, 7, 'a');
            automaton.AddTransition(4, 5, 'b');
            automaton.AddTransition(5, 2, 'a');
            automaton.AddTransition(5, 6, 'b');
            automaton.AddTransition(6, 6, 'a');
            automaton.AddTransition(6, 4, 'b');
            automaton.AddTransition(7, 6, 'a');
            automaton.AddTransition(7, 2, 'b');

            var minimizedAutomaton = automaton.MinimizeDFA();

            Assert.Equal(5, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("ab"));
            Assert.True(minimizedAutomaton.IsValidWord("aababbbb"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly5_MultipleGoalStates()
        {
            // https://www.youtube.com/watch?v=DV8cZp-2VmM
            var automaton = CreateEmptyAutomatonWithStates(6);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(2);
            automaton.SetAsGoalState(3);
            automaton.SetAsGoalState(4);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 2, 'b');
            automaton.AddTransition(1, 0, 'a');
            automaton.AddTransition(1, 3, 'b');
            automaton.AddTransition(2, 4, 'a');
            automaton.AddTransition(2, 5, 'b');
            automaton.AddTransition(3, 4, 'a');
            automaton.AddTransition(3, 5, 'b');
            automaton.AddTransition(4, 4, 'a');
            automaton.AddTransition(4, 5, 'b');
            automaton.AddTransition(5, 5, 'a');
            automaton.AddTransition(5, 5, 'b');

            var minimizedAutomaton = automaton.MinimizeDFA();
            Assert.Equal(2, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("b"));
            Assert.True(minimizedAutomaton.IsValidWord("ba"));
            Assert.True(minimizedAutomaton.IsValidWord("ab"));
            Assert.True(minimizedAutomaton.IsValidWord("aba"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly7_IncompleteAutomata()
        {
            // https://www.youtube.com/watch?v=DV8cZp-2VmM
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(2);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(1, 2, 'a');
            automaton.AddTransition(1, 3, 'b');

            var minimizedAutomaton = automaton.MinimizeDFA();
            Assert.Equal(3, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("aa"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly6_IncompleteAutomata2()
        {
            // https://www.youtube.com/watch?v=DV8cZp-2VmM
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(2);
            automaton.AddTransition(0, 1, 'a');
            automaton.AddTransition(0, 1, 'b');
            automaton.AddTransition(1, 2, 'a');
            automaton.AddTransition(2, 2, 'a');

            var minimizedAutomaton = automaton.MinimizeDFA();
            Assert.Equal(3, minimizedAutomaton.States.Count);
            Assert.True(minimizedAutomaton.IsValidWord("aa"));
            Assert.True(minimizedAutomaton.IsValidWord("ba"));
            Assert.True(minimizedAutomaton.IsValidWord("aaaaaaa"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly_WithOnlyStartStates()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            for (int i = 0; i < 4; i++)
            {
                automaton.AddTransition(i, (2 * i + 2) % 4, 'a');
                automaton.AddTransition(i, (5 * i + 1) % 4, 'b');
                automaton.AddTransition(i, (3 * i + 1) % 4, 'c');
            }

            var minimizedAutomaton = automaton.MinimizeDFA();
            Assert.Equal(1, minimizedAutomaton.States.Count);
            Assert.False(minimizedAutomaton.IsValidWord("abc"));
        }

        [Fact]
        public void Minimize_MinimizesCorrectly_WithOnlyGoalStates()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            for (int i = 0; i < 4; i++)
            {
                automaton.SetAsGoalState(i);
                // Do some random-ish states
                automaton.AddTransition(i, (2 * i + 2) % 4, 'a');
                automaton.AddTransition(i, (5 * i + 1) % 4, 'b');
                automaton.AddTransition(i, (3 * i + 1) % 4, 'c');
            }

            var minimizedAutomaton = automaton.MinimizeDFA();
            Assert.Equal(1, minimizedAutomaton.States.Count);
            Assert.Equal(1, minimizedAutomaton.GoalStates.Count);
            Assert.True(minimizedAutomaton.IsValidWord("ababcbcbbaccbabbcabcabcabcababcabcbc"));
        }

        [Fact]
        public void Minimize_Throws_IfNFAWithMultipleStartStates()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.SetAsStartState(1);
            for (int i = 0; i < 4; i++)
            {
                // Do some random-ish states
                automaton.AddTransition(i, (2 * i + 2) % 4, 'a');
                automaton.AddTransition(i, (5 * i + 1) % 4, 'b');
                automaton.AddTransition(i, (3 * i + 1) % 4, 'c');
            }

            Assert.Throws<InvalidOperationException>(() => automaton.MinimizeDFA());
        }

        [Fact]
        public void Minimize_Throws_IfNFAWithEpsilonTransitions()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            for (int i = 0; i < 4; i++)
            {
                // Do some random-ish states
                automaton.AddTransition(i, (2 * i + 2) % 4, 'a');
                automaton.AddTransition(i, (5 * i + 1) % 4, 'b');
                automaton.AddTransition(i, (3 * i + 1) % 4, 'c');
                automaton.AddTransition(i, (7 * i + 1) % 4, Automaton.Epsilon);
            }

            Assert.Throws<InvalidOperationException>(() => automaton.MinimizeDFA());
        }

        [Fact]
        public void Minimize_Throws_IfNFAWithMultipleTransition()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            for (int i = 0; i < 4; i++)
            {
                // Do some random-ish states
                automaton.AddTransition(i, (i + 1) % 4, 'a');
                automaton.AddTransition(i, (i + 2) % 4, 'a');
            }

            Assert.Throws<InvalidOperationException>(() => automaton.MinimizeDFA());
        }

        [Fact]
        public void IsDFA_ReturnsFalse_WithNoStates()
        {
            var automaton = CreateEmptyAutomatonWithStates(0);
            Assert.False(automaton.IsDFA(out var _));
        }

        [Fact]
        public void IsDFA_ReturnsFalse_WithNoStartStates()
        {
            var automaton = CreateEmptyAutomatonWithStates(1);
            Assert.False(automaton.IsDFA(out var _));
        }

        [Fact]
        public void IsDFA_ReturnsFalse_WithTwoTransitionsWithSameSymbol()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.AddTransition(0, 1, _alphabet[0]);
            automaton.AddTransition(0, 2, _alphabet[0]);
            Assert.False(automaton.IsDFA(out var _));
        }

        [Fact]
        public void IsDFA_ReturnsFalse_WithEpsilonTransitions()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.AddTransition(0, 1, Automaton.Epsilon);
            Assert.False(automaton.IsDFA(out var _));
        }

        [Fact]
        public void IsDFA_ReturnsTrue_With1StartStateAndNoTransitions()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            automaton.AddTransition(0, 1, _alphabet[0]);
            Assert.True(automaton.IsDFA(out var _));
        }

        [Fact]
        public void IsDFA_ReturnsTrue_With1StartStateAnd1TransitionPerSymbolPerState()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
            automaton.SetAsStartState(0);
            foreach (var state in automaton.States)
            {
                foreach (var symbol in _alphabet)
                {
                    // Add some randomish looking transitions
                    var bytes = Encoding.ASCII.GetBytes(symbol.ToString());
                    var stateTo = (3 * state + (2 * bytes[0] % 7)) % 4;
                    automaton.AddTransition(state, stateTo, symbol);
                }
            }

            Assert.True(automaton.IsDFA(out var _));
        }

        private Automaton CreateEmptyAutomatonWithStates(int states)
        {
            var automaton = new Automaton(_alphabet);

            for (int i = 0; i < states; i++)
            {
                automaton.AddState();
            }
            return automaton;
        }

        private Automaton CreateDFA1()
        {
            var automaton = CreateEmptyAutomatonWithStates(4);
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
            var automaton = CreateEmptyAutomatonWithStates(4);
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
            var automaton = CreateEmptyAutomatonWithStates(8);
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