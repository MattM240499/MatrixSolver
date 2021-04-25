using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extreme.Mathematics;
using MatrixSolver;
using MatrixSolver.Computations;
using MatrixSolver.Computations.DataTypes;
using Newtonsoft.Json;

namespace VectorReachabilitySolutionChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            string matrixProduct;
            string inputFile = "";
            if(args.Length < 1)
            {
               Console.WriteLine("Error: No argument received for input file");
               return;
            }
            if(args.Length < 2)
            {
                Console.WriteLine("No argument received for string. String assumed to be the empty string.");
                matrixProduct = "";
            }
            else
            {
                inputFile = args[0];
                matrixProduct = args[1];
            }
            
            var symbols = new HashSet<char>(MatrixSolver.Computations.Constants.RegularLanguage.Symbols);
            foreach(var symbol in matrixProduct)
            {
                if(!symbols.Contains(symbol))
                {
                    throw new ArgumentException($"String contained unknown symbol {symbol}");
                }
            }

            // TODO: Duplicate of main function. Expose this procedure somewhere to reduce duplicate code.
            var json = File.ReadAllText(inputFile);
            var data = JsonConvert.DeserializeObject<InputData>(json);
            data.ThrowIfNull();
            var vectorX = new ImmutableVector2D(data.VectorX);
            var vectorY = new ImmutableVector2D(data.VectorY);
            var matrices = data.Matrices.Select(m => new ImmutableMatrix2x2(As2DArray(m, 2, 2))).ToArray();

            Console.WriteLine($"Computing matrix product M = {matrixProduct}");
            var M = RegularLanguageHelper.MatrixProductStringToMatrix(matrixProduct);
            Console.WriteLine($"Calculated M = {M}");

            var expectedY = M * vectorX;
            if(expectedY.Equals(vectorY))
            {
                Console.WriteLine($"Mx = y is solved for M = {M}, x = {vectorX}, y = {vectorY}");
            }
            else
            {
                Console.WriteLine($"Expected vector y = {vectorY}, but instead found y = {expectedY}");
            }
        }

        public static BigRational[,] As2DArray(BigRational[][] Array2D, int leftSize, int rightSize)
        {
            var newArray = new BigRational[leftSize, rightSize];
            for (int i = 0; i < Array2D.Length; i++)
            {
                for (int j = 0; j < Array2D[i].Length; j++)
                {
                    newArray[i, j] = Array2D[i][j];
                }
            }
            return newArray;
        }
    }
}
