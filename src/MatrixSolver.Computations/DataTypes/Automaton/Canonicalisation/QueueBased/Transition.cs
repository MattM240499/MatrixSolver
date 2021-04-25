namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    internal class Transition
    {
        public int StateFrom { get; }
        public int StateTo { get; }
        public char Symbol { get; }

        public Transition(int stateFrom, int stateTo, char symbol)
        {
            StateFrom = stateFrom;
            StateTo = stateTo;
            Symbol = symbol;
        }
    }
}