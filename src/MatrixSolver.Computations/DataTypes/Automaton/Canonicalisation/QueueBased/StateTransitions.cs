using System;
using System.Collections.Generic;

namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    internal class StateTransitions
    {
        public Dictionary<int, ReachabilityStatus> XTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> RTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> RRTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        public Dictionary<int, ReachabilityStatus> STransitions { get; } = new Dictionary<int, ReachabilityStatus>();

        public Dictionary<int, ReachabilityStatus> XEpsilonChains { get; } = new Dictionary<int, ReachabilityStatus>();

        public Dictionary<int, ReachabilityStatus> GetTransitionDictionary(char symbol)
        {
            switch (symbol)
            {
                case Constants.RegularLanguage.R:
                    return RTransitions;
                case Constants.RegularLanguage.S:
                    return STransitions;
                case Constants.RegularLanguage.X:
                case Automaton.Epsilon:
                    return XTransitions;
                default:
                    throw new ArgumentException($"Character {symbol} has no transition dictionary");
            }
        }
    }
}