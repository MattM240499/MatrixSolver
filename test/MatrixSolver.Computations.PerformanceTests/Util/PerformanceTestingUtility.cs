
using System;
using MatrixSolver.Computations.DataTypes.Automata;

namespace MatrixSolver.Computations.PerformanceTests
{
    public static class PerformanceTestingUtility
    {
        internal static Automaton CreateLargeAutomataWithRandomTransitions(char[] alphabet, int states = 10000)
        {
            var automaton = new Automaton(alphabet);
            var rng = new Random();
            for (int i = 0; i < states; i++)
            {
                automaton.AddState();
            }
            automaton.SetAsStartState(0);
            automaton.SetAsFinalState(states - 1);
            for (int i = 0; i < states; i++)
            {
                foreach (var symbol in alphabet)
                {
                    var state = rng.Next(states);
                    automaton.AddTransition(i, state, symbol);
                }
            }
            return automaton;
        }
    }
}