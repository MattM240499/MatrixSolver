using System.Diagnostics;
using MatrixSolver.Computations.DataTypes.Automata.Canonicalisation;
using Xunit;
using Xunit.Abstractions;

namespace MatrixSolver.Computations.PerformanceTests
{
    public class CanonicalisationPerformanceTests
    {

        private readonly ITestOutputHelper _output;

        public CanonicalisationPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory(Skip = "Performance test")]
        [InlineData(50)]
        public void PopulateDFAWithXAndEpsilonTransitions_PerformanceTests(int states)
        {
            var automaton = PerformanceTestingUtility.CreateLargeAutomataWithRandomTransitions(Constants.RegularLanguage.Symbols, states);
            var automaton2 = automaton.Clone();
            PerformanceTestingUtility.AssertAutomatonEqual(automaton, automaton2);

            var sw = Stopwatch.StartNew();
            var naivePopulatedAutomaton = automaton.PopulateDFAWithXAndEpsilonTransitionsNaive();
            sw.Stop();
            var naiveTime = sw.ElapsedMilliseconds;


            sw = Stopwatch.StartNew();
            var queuePopulatedAutomaton = automaton2.PopulateDFAWithXAndEpsilonTransitionsQueueBased();
            sw.Stop();
            var queueTime = sw.ElapsedMilliseconds;

            // Ensure that the automatons created are equal.
            PerformanceTestingUtility.AssertAutomatonEqual(naivePopulatedAutomaton, queuePopulatedAutomaton);

            _output.WriteLine($"Naive Method: {naiveTime}ms");
            _output.WriteLine($"Queue Method: {queueTime}ms");
        }

        [Fact(Skip = "Performance test")]
        public void PopulateDFAWithXAndEpsilonTransitions_MultiplePerformanceTests()
        {
            for (int i = 0; i < 100; i++)
            {
                PopulateDFAWithXAndEpsilonTransitions_PerformanceTests(50);
            }
        }

        [Fact(Skip = "Performance test")]
        public void PopulateDFAWithXAndEpsilonTransitions_PerformanceTests_QueueBasedOnly()
        {
            var automaton = PerformanceTestingUtility.CreateLargeAutomataWithRandomTransitions(Constants.RegularLanguage.Symbols, 300);

            var sw = Stopwatch.StartNew();
            var queuePopulatedAutomaton = automaton.PopulateDFAWithXAndEpsilonTransitionsQueueBased();
            sw.Stop();
            var queueTime = sw.ElapsedMilliseconds;

            _output.WriteLine($"Queue Method: {queueTime}ms");
        }
    }
}