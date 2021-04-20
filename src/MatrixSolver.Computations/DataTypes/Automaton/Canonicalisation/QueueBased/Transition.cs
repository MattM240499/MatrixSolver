namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    internal class Transition
    {
        public int StateFrom { get; }
        public int StateTo { get; }
        public char Symbol { get; }
        public bool Negated { get; }

        public Transition(int stateFrom, int stateTo, char symbol, bool negated)
        {
            StateFrom = stateFrom;
            StateTo = stateTo;
            Symbol = symbol;
            Negated = negated;
        }
    }
}