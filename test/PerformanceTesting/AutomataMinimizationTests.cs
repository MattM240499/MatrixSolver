
using System;
using System.Diagnostics;
using MatrixSolver.DataTypes.Automata;
using Xunit;

namespace MatrixSolver.Tests.PerformanceTests
{
    public class AutomataMinimizationPerformanceTests
    {
        private readonly char[] _alphabet = new[] { 'a', 'b', 'c' };

        [Fact(Skip="Performance test")]
        public void Minimize_PerformanceTests()
        {
            var automaton = CreateLargeAutomataWithRandomTransitions();
            var sw = Stopwatch.StartNew();
            var minimizedAutomaton = automaton.MinimizeDFA();
            sw.Stop();
            Console.WriteLine($"EquivalenceMethod: {sw.ElapsedMilliseconds}ms");
        }

        private Automaton CreateLargeAutomataWithRandomTransitions()
        {
            var states = 10000;
            var automaton = new Automaton(_alphabet);
            var rng = new Random();
            for (int i = 0; i < states; i++)
            {
                automaton.AddState();
            }
            automaton.SetAsStartState(0);
            automaton.SetAsGoalState(999);
            for (int i = 0; i < states; i++)
            {
                foreach (var symbol in _alphabet)
                {
                    var state = rng.Next(states);
                    automaton.AddTransition(i, state, symbol);
                }
            }
            return automaton;
        }
    }
}