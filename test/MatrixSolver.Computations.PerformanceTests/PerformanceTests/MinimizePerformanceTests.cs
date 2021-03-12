
using System;
using System.Diagnostics;
using MatrixSolver.Computations;
using MatrixSolver.Computations.DataTypes.Automata;
using MatrixSolver.Computations.DataTypes.Automata.Canonicalisation;
using Xunit;
using Xunit.Abstractions;

namespace MatrixSolver.Tests.PerformanceTests
{
    public class MinimizePerformanceTests
    {

        private readonly ITestOutputHelper _output;
        private readonly char[] _alphabet = new char[] { 'a', 'b', 'c' };

        public MinimizePerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(Skip="Performance test")]
        public void Minimize_PerformanceTests()
        {
            var automaton = PerformanceTestingUtility.CreateLargeAutomataWithRandomTransitions(_alphabet);
            var sw = Stopwatch.StartNew();
            var minimizedAutomaton = automaton.MinimizeDFA();
            sw.Stop();
            _output.WriteLine($"EquivalenceMethod: {sw.ElapsedMilliseconds}ms");
        }
    }
}