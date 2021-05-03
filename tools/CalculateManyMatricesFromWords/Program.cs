using System;
using System.IO;
using MatrixSolver.Computations;

namespace CalculateManyMatricesFromWords
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No argument for filename provided");
                return;
            }
            var fileName = args[0];

            var words = File.ReadAllText(fileName).Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );
            foreach(var word in words)
            {
                var matrix = RegularLanguageHelper.MatrixProductStringToMatrix(word);

                Console.WriteLine($"phi({word}) = {matrix}");
            }
        }
    }
}
