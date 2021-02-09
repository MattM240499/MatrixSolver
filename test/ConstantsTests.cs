
using Xunit;

namespace MatrixSolver.Computations.Tests
{
    public class ConstantsTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("X")]
        [InlineData("S")]
        [InlineData("R")]
        [InlineData("RR")]
        [InlineData("XSR")]
        [InlineData("XRRS")]
        [InlineData("XSRRSRRSRR")]
        [InlineData("XRRR", false)]
        [InlineData("XX", false)]
        [InlineData("SS", false)]
        [InlineData("XRSRRSS", false)]
        [InlineData("RRR", false)]
        [InlineData("XRRXS", false)]
        [InlineData("XRRSRRSRSRRSRSRRSSRSRRSRSRR", false)]
        public void CanonicalAcceptsOnlyCanonicalWords(string word, bool isValid = true)
        {
            var canA = Constants.Automaton.Canonical;
            Assert.Equal(isValid, canA.IsValidWord(word));
        }
    }
}