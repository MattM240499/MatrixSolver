
using System;
using System.Collections.Generic;
using System.Linq;
using MatrixSolver.Computations.DataTypes.Automata;
using Xunit;

namespace MatrixSolver.Computations.Tests.DataTypes.Automata
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_AddsXForRRR()
        {
            var states = new[] { 0, 1, 2, 3, 4 };
            var transitions = new (int, int, char)[]
            {
                (3, 2, Constants.RegularLanguage.R),
                (2, 1, Constants.RegularLanguage.R),
                (1, 0, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (3, 0) }, Array.Empty<(int, int)>());
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_AddsXForSS()
        {
            var states = new[] { 0, 1, 2, 3 };
            var transitions = new (int, int, char)[]
            {
                (2, 1, Constants.RegularLanguage.S),
                (1, 0, Constants.RegularLanguage.S)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (2, 0) }, Array.Empty<(int, int)>());
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_AddsXXAndEpsilonForSSRRR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (5, 4, Constants.RegularLanguage.S),
                (4, 3, Constants.RegularLanguage.S),
                (3, 2, Constants.RegularLanguage.R),
                (2, 1, Constants.RegularLanguage.R),
                (1, 0, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (5, 3), (3, 0) }, new[] { (5, 0) });
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedNumbering_AddsEpsilonForSXS()
        {
            var states = new[] { 0, 1, 2, 3 };
            var transitions = new (int, int, char)[]
            {
                (3, 2, Constants.RegularLanguage.S),
                (2, 1, Constants.RegularLanguage.X),
                (1, 0, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (2, 1) }, new[] { (3, 0) });
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_AddsXForRXRXR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (5, 4, Constants.RegularLanguage.R),
                (4, 3, Constants.RegularLanguage.X),
                (3, 2, Constants.RegularLanguage.R),
                (2, 1, Constants.RegularLanguage.X),
                (1, 0, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (4, 3), (2, 1), (5, 0) }, Array.Empty<(int, int)>());
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_AddsEpsilonForRXXRXR()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6 };
            var transitions = new (int, int, char)[]
            {
                (6, 5, Constants.RegularLanguage.R),
                (5, 4, Constants.RegularLanguage.X),
                (4, 3, Constants.RegularLanguage.X),
                (3, 2, Constants.RegularLanguage.R),
                (2, 1, Constants.RegularLanguage.X),
                (1, 0, Constants.RegularLanguage.R)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (5, 4), (4, 3), (2, 1) }, new[] { (5, 3), (6, 0) });
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
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_ForRSXSRRX_AddsEpsilon()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var transitions = new (int, int, char)[]
            {
                (7, 6, Constants.RegularLanguage.R),
                (6, 5, Constants.RegularLanguage.S),
                (5, 4, Constants.RegularLanguage.X),
                (4, 3, Constants.RegularLanguage.S),
                (3, 2, Constants.RegularLanguage.R),
                (2, 1, Constants.RegularLanguage.R),
                (1, 0, Constants.RegularLanguage.X)
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, new[] { (5, 4), (1, 0), (7, 1) }, new[] { (6, 3), (7, 0), });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_WithSimpleOddSLoop_AddsEpsilonAndX()
        {
            var states = new[] { 0, 1 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.S),
                (1, 0, Constants.RegularLanguage.X),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (0, 1), (1, 0) },
                new[] { (0, 1), (1, 0) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_WithSimpleOddSLoop_AddsEpsilonAndX()
        {
            var states = new[] { 0, 1 };
            var transitions = new (int, int, char)[]
            {
                (1, 0, Constants.RegularLanguage.S),
                (0, 1, Constants.RegularLanguage.X),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (0, 1), (1, 0) },
                new[] { (0, 1), (1, 0) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_WithEvenRLoop_AddsX()
        {
            var states = new[] { 0, 1 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 0, Constants.RegularLanguage.X),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (0, 1), (1, 0)}, 
                Array.Empty<(int,int)>());
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_WithEvenRLoop_AddsX()
        {
            var states = new[] { 0, 1 };
            var transitions = new (int, int, char)[]
            {
                (1, 0, Constants.RegularLanguage.R),
                (0, 1, Constants.RegularLanguage.X),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (0, 1), (1, 0)}, 
                Array.Empty<(int,int)>());
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
                new[] { (0, 1), (1, 2), (2, 4), (4, 0) }, 
                new[] { (0, 2), (0, 5), (1, 4), (2, 0), (3, 6), (4, 1) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_ReversedOrdering_WithOddLoop_AddsEpsilonAndX()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5, 6 };
            var transitions = new (int, int, char)[]
            {
                (6, 5, Constants.RegularLanguage.R),
                (5, 4, Constants.RegularLanguage.X),
                (4, 3, Constants.RegularLanguage.S),
                (3, 2, Constants.RegularLanguage.S),
                (2, 6, Constants.RegularLanguage.X),
                (2, 1, Constants.RegularLanguage.R),
                (1, 0, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (6, 5), (5, 4), (4, 2), (2, 6) }, 
                new[] { (6, 4), (6, 1), (5, 2), (4, 6), (3, 0), (2, 5) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_WithXTransitionAddedInMiddleAfterS_AddsCorrectly()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (0, 2, Constants.RegularLanguage.S),
                (2, 1, Constants.RegularLanguage.X),
                (1, 3, Automaton.Epsilon),
                (3, 4, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (2, 1) }, 
                new[] { (0, 4), (1, 3) });
        }

        [Fact]
        public void PopulateDFAWithXAndEpsilonTransitions_WithXTransitionAddedInMiddleAfterS2_AddsCorrectly()
        {
            var states = new[] { 0, 1, 2, 3, 4, 5 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Automaton.Epsilon),
                (1, 2, Constants.RegularLanguage.S),
                (3, 0, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.PopulateDFAWithXAndEpsilonTransitions();

            AssertCorrectTransitionsAddedOnly(automaton, 
                new[] { (3, 2) }, 
                new[] { (0, 1) });
        }

        [Fact]
        public void AddXSurroundedPaths_CorrectlyAddsTransitions()
        {
            var states = new[] { 0, 1, 2 };
            var transitions = new (int, int, char)[]
            {
                (0, 1, Constants.RegularLanguage.R),
                (1, 2, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(2);
            AutomatonUtil.AddXSurroundedPaths(automaton);

            Assert.Equal(7, automaton.States.Count);
            Assert.True(automaton.IsValidWord("RS"));
            Assert.True(automaton.IsValidWord("XRXS"));
            Assert.True(automaton.IsValidWord("XRSX"));
            Assert.True(automaton.IsValidWord("RXSX"));

            Assert.False(automaton.IsValidWord("X"));
            Assert.False(automaton.IsValidWord("XRXSX"));
            Assert.False(automaton.IsValidWord("XXRS"));
            Assert.False(automaton.IsValidWord("RXXSX"));
        }

        [Fact]
        public void AddXSurroundedPaths_CorrectlyAddsTransitionsInSelfLoop()
        {
            var states = new[] { 0, 1 };
            var transitions = new (int, int, char)[]
            {
                (0, 0, Constants.RegularLanguage.S),
                (0, 1, Constants.RegularLanguage.R),
                (1, 1, Constants.RegularLanguage.S),
            };
            var automaton = InitialiseAutomaton(states, transitions);
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(1);
            AutomatonUtil.AddXSurroundedPaths(automaton);

            Assert.Equal(8, automaton.States.Count);
            Assert.True(automaton.IsValidWord("R"));
            Assert.True(automaton.IsValidWord("RS"));
            Assert.True(automaton.IsValidWord("SR"));
            Assert.True(automaton.IsValidWord("SRS"));
            Assert.True(automaton.IsValidWord("XRX"));
            Assert.True(automaton.IsValidWord("RXSX"));
            Assert.True(automaton.IsValidWord("SXRXS"));
            Assert.True(automaton.IsValidWord("XSRSX"));
            Assert.True(automaton.IsValidWord("XSXXRXXSXXSX"));

            Assert.False(automaton.IsValidWord("X"));
            Assert.False(automaton.IsValidWord("XR"));
            Assert.False(automaton.IsValidWord("RX"));
            Assert.False(automaton.IsValidWord("XSR"));
            Assert.False(automaton.IsValidWord("SRX"));
            Assert.False(automaton.IsValidWord("SXR"));
            Assert.False(automaton.IsValidWord("XSXRX"));
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
            var missingXTransitions = new List<(int fromState, int toState)>();
            var missingEpsilonTransitions = new List<(int fromState, int toState)>();
            foreach (var transition in expectedXTransitions)
            {
                var xStates = automaton.TransitionMatrix.GetStates(transition.fromState, Constants.RegularLanguage.X);
                
                if(!xStates.Contains(transition.toState))
                {
                    missingXTransitions.Add(transition);
                }
            }
            Assert.True(missingXTransitions.Count() == 0 , 
                $"Missing X transitions: {String.Join(", ", missingXTransitions)}");
            foreach (var transition in expectedEpsilonTransitions)
            {
                var epsilonStates = automaton.TransitionMatrix.GetStates(transition.fromState, Automaton.Epsilon);
                if(!epsilonStates.Contains(transition.toState))
                {
                    missingEpsilonTransitions.Add(transition);
                }
            }
            Assert.True(missingEpsilonTransitions.Count() == 0 , 
                $"Missing Epsilon transitions: {String.Join(", ", missingEpsilonTransitions)}");
            // Assert remaining possible transitions are not in place.

            var unexpectedXTransitions = new List<(int fromState, int toState)>();
            var unexpectedEpsilonTransitions = new List<(int fromState, int toState)>();
            foreach (var fromState in automaton.States)
            {
                var xStates = automaton.TransitionMatrix.GetStates(fromState, Constants.RegularLanguage.X);
                // Ignore transitions in the ignored list
                var unexpectedXStates = xStates.Where(s => !expectedXTransitions.Contains((fromState, s)));
                foreach(var s in unexpectedXStates)
                {
                    unexpectedXTransitions.Add((fromState, s));
                }
                

                var epsilonStates = automaton.TransitionMatrix.GetStates(fromState, Automaton.Epsilon);
                var unexpectedEpsilonStates = epsilonStates.Where(s => !expectedEpsilonTransitions.Contains((fromState, s)));
                foreach(var s in unexpectedEpsilonStates)
                {
                    unexpectedEpsilonTransitions.Add((fromState, s));
                }
                
            }
            Assert.True(unexpectedXTransitions.Count() == 0,
                $"Found unexpected X transitions: {String.Join(", ", unexpectedXTransitions)}");
            Assert.True(unexpectedEpsilonTransitions.Count() == 0, 
                $"Found unexpected Epsilon transitions: {String.Join(", ", unexpectedEpsilonTransitions)}");
        }
    }
}