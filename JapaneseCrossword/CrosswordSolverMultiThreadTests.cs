using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JapaneseCrossword
{
	[TestFixture]
	class CrosswordSolverMultiThreadTests : CrosswordSolverTests
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			var lineCorrectnessChecker = new LineCorrectnessChecker();
			var lineUpdater = new LineUpdater(lineCorrectnessChecker);
			var multiDimensionUpdater = new MultiThreadDimensionUpdater(lineUpdater);
			solver = new CrosswordSolver(multiDimensionUpdater);
		}
	}
}
