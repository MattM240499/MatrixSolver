
using System;
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
        [InlineData("a*", new[] { 'a', 'b' }, new[] { "", "a", "aa", "aaa", "aaaa" }, new[] { "b", "bb", "ab", "ba" })]
        [InlineData("(ab)*", new[] { 'a', 'b' }, new[] { "", "ab", "abab", "abab" }, new[] { "a", "b", "ba", "aab", "bab" })]
        [InlineData("a|b", new[] { 'a', 'b' }, new[] { "a", "b" }, new[] { "", "aa", "bb", "ab", "ba" })]
        [InlineData("(a|b)*", new[] { 'a', 'b' }, new[] { "", "a", "b", "abababbaababaab" }, new string[] { })]
        [InlineData("a*b*(ab|ba)*c", new[] { 'a', 'b', 'c' }, new[] { "c", "aaaabbac", "aaaabbbbababbac" }, new[] { "aaabbbab", "abbbabbbc", "bbca" })]
        public void RegexToNFA_ConvertsValidRegexCorrectly(string regex, char[] alphabet, string[] validWords, string[] invalidWords)
        {
            var automaton = AutomatonUtil.RegexToAutomaton(regex, alphabet);

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