using System;

namespace JapaneseCrossword
{
    class Program
    {
        static void Main(string[] args)
        {
			var lineCorrectnessChecker = new LineCorrectnessChecker();
			var lineUpdater = new LineUpdater(lineCorrectnessChecker);
			var dimensionUpdater = new MultiThreadDimensionUpdater(lineUpdater);
			var solver = new CrosswordSolver(dimensionUpdater);

			Console.WriteLine(solver.Solve("../../SampleInput.txt", "../../res.txt"));	
        }
    }
}
