
using System;
using System.Collections.Generic;
using System.Linq;
using MatrixSolver.Computations.DataTypes.Automata;
using MatrixSolver.Computations.DataTypes.Automata.Canonicalisation;
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
    }
}