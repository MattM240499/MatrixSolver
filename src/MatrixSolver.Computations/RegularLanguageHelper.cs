using System.Collections.Generic;
using System.Linq;
using MatrixSolver.Computations.DataTypes;

namespace MatrixSolver.Computations
{
    public static class RegularLanguageHelper
    {
        public static readonly Dictionary<char, ImmutableMatrix2x2> MatrixLookup;

        static RegularLanguageHelper()
        {
            MatrixLookup = new Dictionary<char, ImmutableMatrix2x2>();
            MatrixLookup['R'] = Constants.Matrices.R;
            MatrixLookup['S'] = Constants.Matrices.S;
            MatrixLookup['X'] = Constants.Matrices.X;
        }

        public static ImmutableMatrix2x2 MatrixProductStringToMatrix(IEnumerable<char> matrixProduct)
        {
            return MatrixProductToMatrix(matrixProduct.Select(m => MatrixLookup[m]));
        }

        public static ImmutableMatrix2x2 MatrixProductToMatrix(IEnumerable<ImmutableMatrix2x2> matrices)
        {
            var currentMatrix = Constants.Matrices.I;
            foreach(var matrix in matrices)
            {
                currentMatrix = currentMatrix * matrix;
            }
            return currentMatrix;
        }

        public static ImmutableMatrix2x2 MatrixProductIdentifiersToMatrix(string matrixProduct)
        {
            var workingMatrix = Constants.Matrices.I;
            foreach (var matrixIdentifier in matrixProduct)
            {
                var nextMatrix = MatrixLookup[matrixIdentifier];
                workingMatrix = workingMatrix * nextMatrix;
            }
            return workingMatrix;
        }

        /// <summary>
        /// Verifies a matrix product is equal to a given matrix
        /// </summary>
        public static bool IsEqual(string matrixProduct, ImmutableMatrix2x2 compareMatrix)
        {
            var matrix = MatrixProductIdentifiersToMatrix(matrixProduct);

            if (!matrix.Equals(compareMatrix))
            {
                return false;
            }
            return true;
        }
    }
}