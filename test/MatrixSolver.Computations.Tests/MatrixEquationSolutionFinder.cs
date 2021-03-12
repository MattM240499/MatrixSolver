using Xunit;

namespace MatrixSolver.Computations.Tests
{
    public class MatrixEquationSolutionFinderTests
    {
        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ConvertsForAtomicRegex()
        {
            string regex = "X";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.Equal(2, automaton.States.Count);

            Assert.True(automaton.IsValidWord("X"));

            Assert.False(automaton.IsValidWord("XX"));
            Assert.False(automaton.IsValidWord("S"));
            Assert.False(automaton.IsValidWord("R"));
            Assert.False(automaton.IsValidWord("XSRSRSRSRS"));
        }

        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ForUnionRegex()
        {
            string regex = "X|S";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.True(automaton.IsValidWord("X"));
            Assert.True(automaton.IsValidWord("S"));

            Assert.False(automaton.IsValidWord("XX"));
            Assert.False(automaton.IsValidWord("R"));
            Assert.False(automaton.IsValidWord("XSRSRSRSRS"));
        }

        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ForKleeneStarRegex()
        {
            string regex = "X*";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.True(automaton.IsValidWord(""));
            Assert.True(automaton.IsValidWord("X"));

            Assert.False(automaton.IsValidWord("S"));
            Assert.False(automaton.IsValidWord("XSRSRSRSRS"));
        }

        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ForKleeneStarAndUnionRegex()
        {
            string regex = "(X|S)*";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.True(automaton.IsValidWord("S"));
            Assert.True(automaton.IsValidWord("X"));
            Assert.True(automaton.IsValidWord("XS"));

            Assert.False(automaton.IsValidWord("XSRSRSRSRS"));
        }

        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ForKleeneStarAndUnionRegex2()
        {
            string regex = "(S|R)*";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.True(automaton.IsValidWord("S"));
            Assert.True(automaton.IsValidWord("R"));
            Assert.True(automaton.IsValidWord("XS"));
            Assert.True(automaton.IsValidWord("XR"));
            Assert.True(automaton.IsValidWord("XRR"));
            Assert.True(automaton.IsValidWord("XSRRSRSRRSRS"));
        }

        [Fact]
        public void RegularLanguageRegexToCanonicalWordAcceptingDfa_ForKleeneStarAndUnionAndConcatenationRegex()
        {
            string regex = "SRS(XSR)*SRS";
            var automaton = MatrixEquationSolutionFinder.RegularLanguageRegexToCanonicalWordAcceptingDfa(regex);

            Assert.True(automaton.IsValidWord("XSRRS"));
            Assert.True(automaton.IsValidWord("SRRSRS"));
            Assert.True(automaton.IsValidWord("XSRRSRSRS"));
            Assert.True(automaton.IsValidWord("SRRSRSRSRS"));
            
            Assert.False(automaton.IsValidWord("SRS"));
            Assert.False(automaton.IsValidWord("SRRS"));
        }
    }
}