
using System;
using System.Linq;
using MatrixSolver.Computations.DataTypes.Automata;
using Xunit;

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

        internal static void AssertAutomatonEqual(Automaton left, Automaton right)
        {
            Assert.Equal(left.Alphabet.OrderBy(i => i), right.Alphabet.OrderBy(i => i));
            Assert.Equal(left.States.OrderBy(i => i), right.States.OrderBy(i => i));
            Assert.Equal(left.StartStates.OrderBy(i => i), right.StartStates.OrderBy(i => i));
            Assert.Equal(left.FinalStates.OrderBy(i => i), right.FinalStates.OrderBy(i => i));
            foreach(var state in left.States)
            {
                foreach(var letter in left.Alphabet)
                {
                    var leftStates = left.TransitionMatrix.GetStates(state, letter);
                    var rightStates = right.TransitionMatrix.GetStates(state, letter);
                    Assert.Equal(leftStates.OrderBy(i => i), rightStates.OrderBy(i => i));
                }
            }
        }
    }
}