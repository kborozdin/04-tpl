using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JapaneseCrossword
{
	[TestFixture]
	class CrosswordSolverOneThreadTests : CrosswordSolverTests
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			var lineCorrectnessChecker = new LineCorrectnessChecker();
			var lineUpdater = new LineUpdater(lineCorrectnessChecker);
			var oneDimensionUpdater = new OneThreadDimensionUpdater(lineUpdater);
			solver = new CrosswordSolver(oneDimensionUpdater);
		}
	}
}
