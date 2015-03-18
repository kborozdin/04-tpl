using System.IO;
using NUnit.Framework;

namespace JapaneseCrossword
{
    [TestFixture]
    public class CrosswordSolverTests
    {
        private CrosswordSolver[] solvers;

        [TestFixtureSetUp]
        public void SetUp()
        {
			var lineCorrectnessChecker = new LineCorrectnessChecker();
			var lineUpdater = new LineUpdater(lineCorrectnessChecker);

			var oneDimensionUpdater = new OneThreadDimensionUpdater(lineUpdater);
			var multiDimensionUpdater = new MultiThreadDimensionUpdater(lineUpdater);

			solvers = new CrosswordSolver[2];
			solvers[0] = new CrosswordSolver(oneDimensionUpdater);
			solvers[1] = new CrosswordSolver(multiDimensionUpdater);
        }

        [Test]
        public void InputFileNotFound()
        {
			foreach (var solver in solvers)
			{
				var solutionStatus = solver.Solve(Path.GetRandomFileName(), Path.GetRandomFileName());
				Assert.AreEqual(SolutionStatus.BadInputFilePath, solutionStatus);
			}
        }

        [Test]
        public void IncorrectOutputFile()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\SampleInput.txt";
				var outputFilePath = "///.&*#";
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.BadOutputFilePath, solutionStatus);
			}
        }

        [Test]
        public void IncorrectCrossword()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\IncorrectCrossword.txt";
				var outputFilePath = Path.GetRandomFileName();
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.IncorrectCrossword, solutionStatus);
			}
        }

        [Test]
        public void Simplest()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\SampleInput.txt";
				var outputFilePath = Path.GetRandomFileName();
				var correctOutputFilePath = @"TestFiles\SampleInput.solved.txt";
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
				CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
			}
        }

        [Test]
        public void Car()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\Car.txt";
				var outputFilePath = Path.GetRandomFileName();
				var correctOutputFilePath = @"TestFiles\Car.solved.txt";
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
				CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
			}
        }

        [Test]
        public void Flower()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\Flower.txt";
				var outputFilePath = Path.GetRandomFileName();
				var correctOutputFilePath = @"TestFiles\Flower.solved.txt";
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
				CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
			}
        }

        [Test]
        public void Winter()
        {
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\Winter.txt";
				var outputFilePath = Path.GetRandomFileName();
				//var correctOutputFilePath = @"TestFiles\Winter.solved.txt";
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
				//CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
			}
        }

		[Test]
		public void SuperBig()
		{
			foreach (var solver in solvers)
			{
				var inputFilePath = @"TestFiles\SuperBig.txt";
				var outputFilePath = Path.GetRandomFileName();
				var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
				Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
			}
		}
    }
}