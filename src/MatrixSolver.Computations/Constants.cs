
using System.Collections.Generic;
using Extreme.Mathematics;
using MatrixSolver.Computations.DataTypes;

namespace MatrixSolver.Computations
{
    public static class Constants
    {
        public static class Matrices
        {
            public static ImmutableMatrix2x2 T { get; }
            public static ImmutableMatrix2x2 S { get; }
            public static ImmutableMatrix2x2 R { get; }
            public static ImmutableMatrix2x2 X { get; }
            /// <summary>
            /// The Identity matrix.
            /// </summary>
            public static ImmutableMatrix2x2 I { get; }

            public static Dictionary<GeneratorMatrixIdentifier, ImmutableMatrix2x2> MatrixIdentifierDictionary { get; }

            public static Dictionary<GeneratorMatrixIdentifier, GeneratorMatrixIdentifier[]> GeneratorMatrixIdentifierLookup { get; }

            static Matrices()
            {
                var tMatrix = new BigRational[2, 2];
                tMatrix[0, 0] = 1;
                tMatrix[0, 1] = 1;
                tMatrix[1, 0] = 0;
                tMatrix[1, 1] = 1;
                T = new ImmutableMatrix2x2(tMatrix);

                var sMatrix = new BigRational[2, 2];
                sMatrix[0, 0] = 0;
                sMatrix[0, 1] = -1;
                sMatrix[1, 0] = 1;
                sMatrix[1, 1] = 0;
                S = new ImmutableMatrix2x2(sMatrix);

                var rMatrix = new BigRational[2, 2];
                rMatrix[0, 0] = 0;
                rMatrix[0, 1] = -1;
                rMatrix[1, 0] = 1;
                rMatrix[1, 1] = 1;
                R = new ImmutableMatrix2x2(rMatrix);

                var xMatrix = new BigRational[2, 2];
                xMatrix[0, 0] = -1;
                xMatrix[0, 1] = 0;
                xMatrix[1, 0] = 0;
                xMatrix[1, 1] = -1;
                X = new ImmutableMatrix2x2(xMatrix);

                var iMatrix = new BigRational[2, 2];
                iMatrix[0, 0] = 1;
                iMatrix[0, 1] = 0;
                iMatrix[1, 0] = 0;
                iMatrix[1, 1] = 1;
                I = new ImmutableMatrix2x2(iMatrix);

                MatrixIdentifierDictionary = new Dictionary<GeneratorMatrixIdentifier, ImmutableMatrix2x2>
                {
                    [GeneratorMatrixIdentifier.T] = Constants.Matrices.T,
                    [GeneratorMatrixIdentifier.S] = Constants.Matrices.S,
                    [GeneratorMatrixIdentifier.R] = Constants.Matrices.R,
                    [GeneratorMatrixIdentifier.X] = Constants.Matrices.X,
                    [GeneratorMatrixIdentifier.SInverse] = Constants.Matrices.S.Inverse(), // Or -S
                    [GeneratorMatrixIdentifier.TInverse] = Constants.Matrices.T.Inverse()
                };

                GeneratorMatrixIdentifierLookup = new Dictionary<GeneratorMatrixIdentifier, GeneratorMatrixIdentifier[]>
                {
                    [GeneratorMatrixIdentifier.T] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.S, GeneratorMatrixIdentifier.R },
                    [GeneratorMatrixIdentifier.TInverse] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.R, GeneratorMatrixIdentifier.R, GeneratorMatrixIdentifier.S },
                    [GeneratorMatrixIdentifier.SInverse] = new GeneratorMatrixIdentifier[] { GeneratorMatrixIdentifier.X, GeneratorMatrixIdentifier.S }
                };
            }
        }

        public static class RegularLanguage
        {
            public static char X = 'X';
            public static char S = 'S';
            public static char R = 'R';
            public static char[] Symbols = new char[] { X, S, R };
        }

        public static class Automaton
        {
            public static DataTypes.Automata.Automaton Canonical;

            static Automaton()
            {
                Canonical = new DataTypes.Automata.Automaton(RegularLanguage.Symbols);
                var stateIdLookup = new Dictionary<int, int>();
                var transitions = new[]
                {
                    // Qstart
                    (0,1, RegularLanguage.X),
                    (0,2, RegularLanguage.S),
                    (0,3, RegularLanguage.R),
                    // Qx
                    (1,2, RegularLanguage.S),
                    (1,3, RegularLanguage.R),
                    // Qs
                    (2,3, RegularLanguage.R),
                    // Qr
                    (3,4, RegularLanguage.R),
                    (3,2, RegularLanguage.S),
                    // Qrr
                    (4,2, RegularLanguage.S),

                };
                for(int i = 0; i <5; i++)
                {
                    var state = Canonical.AddState();
                    Canonical.SetAsGoalState(state);
                    stateIdLookup[i] = state;
                }
                Canonical.SetAsStartState(stateIdLookup[0]);
                
                foreach(var transition in transitions)
                {
                    Canonical.AddTransition(stateIdLookup[transition.Item1],stateIdLookup[transition.Item2], transition.Item3);
                }
            }
        }
    }
}