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

        [Fact(Skip="Performance test")]
        public void PopulateDFAWithXAndEpsilonTransitions_PerformanceTests()
        {
            var automaton = PerformanceTestingUtility.CreateLargeAutomataWithRandomTransitions(Constants.RegularLanguage.Symbols, 1000);
            var automaton2 = automaton.Clone();

            var sw = Stopwatch.StartNew();
            var naivePopulatedAutomaton = automaton.PopulateDFAWithXAndEpsilonTransitions();
            sw.Stop();
            var naiveTime = sw.ElapsedMilliseconds;
            

            sw = Stopwatch.StartNew();
            var queuePopulatedAutomaton = automaton2.PopulateDFAWithXAndEpsilonTransitionsQueueBased();
            sw.Stop();
            var queueTime = sw.ElapsedMilliseconds;

            _output.WriteLine($"Naive Method: {naiveTime}ms");
            _output.WriteLine($"Queue Method: {queueTime}ms");
        }
    }
}