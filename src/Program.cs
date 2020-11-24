using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using MatrixSolver.DataTypes;
using Extreme.Mathematics;
using System.Diagnostics;

namespace MatrixSolver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Step 0. Retrieve and parse input
            var file = args[0];
            var json = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<InputData>(json);
            data.ThrowIfNull();
            var vectorX = new ImmutableVector2D(data.VectorX);
            var vectorY = new ImmutableVector2D(data.VectorY);
            var matrices = data.Matrices.Select(m => new ImmutableMatrix2x2(As2DArray(m, 2, 2))).ToArray();
            // Solve equation
            var sw = Stopwatch.StartNew();
            MatrixEquationSolutionFinder.TrySolveVectorReachabilityProblem(matrices, vectorX, vectorY);
            sw.Stop();
            Console.WriteLine($"Program completed in {sw.ElapsedMilliseconds}ms");
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
