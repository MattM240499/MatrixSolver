using System;
using System.Collections.Generic;

namespace MatrixSolver.Computations.DataTypes.Automata.Canonicalisation
{
    internal class StateTransitions
    {
        /// <summary>
        /// Any reachable states with a path labelled (X|Ɛ)*
        /// </summary>
        public Dictionary<int, ReachabilityStatus> EpsilonTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        /// <summary>
        /// Any reachable states with a path labelled R(X|Ɛ)*
        /// </summary>
        public Dictionary<int, ReachabilityStatus> RTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        /// <summary>
        /// Any reachable states with a path labelled R(X|Ɛ)*R(X|Ɛ)*
        /// </summary>
        public Dictionary<int, ReachabilityStatus> RRTransitions { get; } = new Dictionary<int, ReachabilityStatus>();
        /// <summary>
        /// Any reachable states with a path labelled S(X|Ɛ)*
        /// </summary>
        public Dictionary<int, ReachabilityStatus> STransitions { get; } = new Dictionary<int, ReachabilityStatus>();

        /// <summary>
        /// Any reachable states with a path labelled XƐ*
        /// </summary>
        public HashSet<int> XEpsilonChains { get; } = new HashSet<int>();
        /// <summary>
        /// Any reachable states with a path labelled Ɛ*
        /// </summary>
        public HashSet<int> EpsilonChains { get; } = new HashSet<int>();

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
                    return EpsilonTransitions;
                default:
                    throw new ArgumentException($"Character {symbol} has no transition dictionary");
            }
        }
    }
}