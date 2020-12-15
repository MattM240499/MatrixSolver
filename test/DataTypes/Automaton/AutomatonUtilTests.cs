
using System;
using System.Collections.Generic;
using System.Linq;
using MatrixSolver.DataTypes.Automata;
using Xunit;

namespace MatrixSolver.Tests.DataTypes.Automata
{
    public class AutomatonUtilTests
    {
        [Theory]
        [InlineData("a*b*c*", "a*b*?c*?")]
        [InlineData("abb*|abb(a*b*)*|b", "ab?b*?ab?b?a*b*?*?|b|")]
        public void RegexToPostFix_ConvertsCorrectly(string infixRegex, string expectedPostfix)
        {
            Assert.Equal(expectedPostfix, String.Join("", AutomatonUtil.RegexToPostfix(infixRegex)));
        }

        [Theory]
        [InlineData("a*", new[] { 'a', 'b' }, new[] { "", "a", "aa", "aaa", "aaaa" }, new[] { "b", "bb", "ab", "ba" }, 4)]
        [InlineData("(ab)*", new[] { 'a', 'b' }, new[] { "", "ab", "abab", "abab" }, new[] { "a", "b", "ba", "aab", "bab" }, 5)]
        [InlineData("a|b", new[] { 'a', 'b' }, new[] { "a", "b" }, new[] { "", "aa", "bb", "ab", "ba" }, 6)]
        [InlineData("(a|b)*", new[] { 'a', 'b' }, new[] { "", "a", "b", "abababbaababaab" }, new string[] { }, 8)]
        [InlineData("a*b*(ab|ba)*c", new[] { 'a', 'b', 'c' }, new[] { "c", "aaaabbac", "aaaabbbbababbac" }, new[] { "aaabbbab", "abbbabbbc", "bbca" }, 17)]
        public void RegexToNFA_ConvertsValidRegexCorrectly(string regex, char[] alphabet, string[] validWords, string[] invalidWords, int expectedStates)
        {
            var automaton = AutomatonUtil.RegexToAutomaton(regex, alphabet);
            Assert.Equal(expectedStates, automaton.States.Count);
            foreach (var validWord in validWords)
            {
                Assert.True(automaton.IsValidWord(validWord));
            }
            foreach (var invalidWord in invalidWords)
            {
                Assert.False(automaton.IsValidWord(invalidWord));
            }
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsXForRRR()
        {
            var states = new[] { 0, 1, 2, 3, 4 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.R),
                (2, 3, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (0, 3) }, Array.Empty<(int, int)>());
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsXForSS()
        {
            var states = new[] { 0, 1, 2, 3 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.S),
                (1, 2, Constants.RegularLanguage.S)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (0, 2) }, Array.Empty<(int, int)>());
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsXXAndEpsilonForSSRRR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.S),
                (1, 2, Constants.RegularLanguage.S),
                (2, 3, Constants.RegularLanguage.R),
                (3, 4, Constants.RegularLanguage.R),
                (4, 5, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (0, 2), (2, 5) }, new[] { (0, 5) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsEpsilonForSXS()
        {
            var states = new[] { 0, 1, 2, 3 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.S),
                (1, 2, Constants.RegularLanguage.X),
                (2, 3, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (1, 2) }, new[] { (0, 3) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsXForRXRXR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.X),
                (2, 3, Constants.RegularLanguage.R),
                (3, 4, Constants.RegularLanguage.X),
                (4, 5, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (1, 2), (3, 4), (0, 5) }, Array.Empty<(int, int)>());
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_AddsEpsilonForRXXRXR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.X),
                (2, 3, Constants.RegularLanguage.X),
                (3, 4, Constants.RegularLanguage.R),
                (4, 5, Constants.RegularLanguage.X),
                (5, 6, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (1, 2), (2, 3), (4, 5) }, new[] { (1, 3), (0, 6) });
        }


        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_ForRSXSRRX_AddsEpsilon()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.S),
                (2, 3, Constants.RegularLanguage.X),
                (3, 4, Constants.RegularLanguage.S),
                (4, 5, Constants.RegularLanguage.R),
                (5, 6, Constants.RegularLanguage.R),
                (6, 7, Constants.RegularLanguage.X)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (0, 6), (2, 3), (6, 7) }, new[] { (0, 7), (1, 4), });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_WithOddLoop_AddsEpsilonAndX()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.X),
                (2, 3, Constants.RegularLanguage.S),
                (3, 4, Constants.RegularLanguage.S),
                (4, 0, Constants.RegularLanguage.X),
                (4, 5, Constants.RegularLanguage.R),
                (5, 6, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (0, 1), (0, 4), (1, 2), (2, 1), (2, 4), (4, 0) }, 
                new[] { (0, 2), (0, 5), (1, 4), (2, 0), (2, 5), (3, 6), (4, 1) });
        }

        private Automaton InitialiseAutomaton(int[] states, (int fromState, int toState, char symbol)[] transitions)
        {
            var automaton = new Automaton(Constants.RegularLanguage.Symbols);
            foreach (var state in states)
            {
                automaton.AddState();
            }
            foreach (var transition in transitions)
            {
                automaton.AddTransition(transition.fromState, transition.toState, transition.symbol);
            }
            return automaton;
        }

        private void AssertCorrectTransitionsAddedOnly(Automaton automaton, (int fromState, int toState)[] expectedXTransitions,
            (int fromState, int toState)[] expectedEpsilonTransitions)
        {
            // Assert expected transitions are in place
            foreach (var transition in expectedXTransitions)
            {
                Assert.Contains(transition.toState, automaton.TransitionMatrix.GetStates(transition.fromState, Constants.RegularLanguage.X));
            }
            foreach (var transition in expectedEpsilonTransitions)
            {
                Assert.Contains(transition.toState, automaton.TransitionMatrix.GetStates(transition.fromState, Automaton.Epsilon));
            }
            // Assert remaining possible transitions are not in place.
            foreach (var fromState in automaton.States)
            {
                var xStates = automaton.TransitionMatrix.GetStates(fromState, Constants.RegularLanguage.X);
                // Ignore transitions to the empty state and those in the ignored list
                Assert.Empty(xStates.Where(s => !expectedXTransitions.Contains((fromState, s))));
                var epsilonStates = automaton.TransitionMatrix.GetStates(fromState, Automaton.Epsilon);
                Assert.Empty(epsilonStates.Where(s => !expectedEpsilonTransitions.Contains((fromState, s))));
            }
        }
    }
}