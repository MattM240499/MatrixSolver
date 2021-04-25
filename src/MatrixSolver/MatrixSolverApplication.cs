using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Extreme.Mathematics;
using MatrixSolver.Computations;
using MatrixSolver.Computations.DataTypes;
using Newtonsoft.Json;

namespace MatrixSolver
{
    class MatrixSolverApplication : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Application is running
            // Process command line args
            var args = e.Args;
            if(args.Length == 0)
            {
                Console.WriteLine("Expected a file path as an argument but received no arguments");
                Shutdown();
                return;
            }
            var file = args[0];
            string json;
            try
            {
                json = File.ReadAllText(file);
            }
            catch
            {
                Console.WriteLine($"An error occured while reading file {file}");
                Shutdown();
                return;
            }
            
            InputData data;
            try
            {
                data = JsonConvert.DeserializeObject<InputData>(json);
                data.ThrowIfNull();
            }
            catch
            {
                Console.WriteLine($"File {file} contained invalid data. Please make sure all parameters are set correctly");
                Shutdown();
                return;
            }
            
            try
            {
                var vectorX = new ImmutableVector2D(data.VectorX);
                var vectorY = new ImmutableVector2D(data.VectorY);
                var matrices = data.Matrices.Select(m => new ImmutableMatrix2x2(As2DArray(m, 2, 2))).ToArray();

                Console.WriteLine("Vector Reachability Problem Input data: ");
                Console.WriteLine("-------------------------");
                Console.WriteLine($"M1, ..., Mn =  {String.Join(", ", matrices.Select(m => m.ToString()))}");
                Console.WriteLine($"x =  {vectorX}");
                Console.WriteLine($"y =  {vectorY}");
                Console.WriteLine("-------------------------");

                // Solve equation
                var sw = Stopwatch.StartNew();
                var automaton = MatrixEquationSolutionFinder.SolveVectorReachabilityProblem(matrices, vectorX, vectorY);
                sw.Stop();
                Console.WriteLine($"Solution found in {sw.ElapsedMilliseconds}ms");

                // Create main application window, starting minimized if specified
                MainWindow mainWindow = new MainWindow(automaton);
                mainWindow.Show();
            }
            catch(ArgumentException exc)
            {
                Console.WriteLine($"VRP data provided from file {file} was invalid: \n{exc.Message}");
                Shutdown();
                return;
            }
            catch(Exception)
            {
                Console.WriteLine("An unexpected error occured while running");
                Shutdown();
                return;
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

        [STAThread]
        static void Main(string[] args)
        {
            new MatrixSolverApplication { Args = args }.Run();
        }

        public string[] Args { get; set; }
    }
}
