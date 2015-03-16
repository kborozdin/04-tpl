using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

//GLOBAL TODO : refactor into separate classes

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
		private readonly IDimensionUpdater dimensionUpdaterService;

		int[][] rowBlocks;
		int[][] columnBlocks;
		Cell[,] picture;

		public CrosswordSolver(IDimensionUpdater dimensionUpdaterService)
		{
			this.dimensionUpdaterService = dimensionUpdaterService;
		}

		private SolutionStatus SolveCrossword()
		{
			bool firstRun = true;
			picture.MarkAllCellsAsChanged();

			while (picture.HasChanged())
			{
				if (!dimensionUpdaterService.Update(rowBlocks, picture.GetRow))
					return SolutionStatus.IncorrectCrossword;
				if (firstRun)
				{
					firstRun = false;
					picture.MarkAllCellsAsChanged();
				}
				if (!dimensionUpdaterService.Update(columnBlocks, picture.GetColumn))
					return SolutionStatus.IncorrectCrossword;
			}

			return picture.IsFullySolved() ?
				SolutionStatus.Solved :
				SolutionStatus.PartiallySolved;
		}

		private SolutionStatus SolveCrosswordBruteforce()
		{
			var status = SolveCrossword();
			if (status != SolutionStatus.PartiallySolved)
				return status;

			bool found = false;

			for (int i = 0; i < picture.GetLength(0) && !found; i++)
				for (int j = 0; j < picture.GetLength(1) && !found; j++)
					if (picture[i, j].State == CellState.Unknown)
					{
						found = true;

						picture[i, j].State = CellState.White;
						picture[i, j].JustChanged = true;
						var result = SolveCrosswordBruteforce();
						if (result == SolutionStatus.Solved)
							return result;

						picture[i, j].State = CellState.Black;
						picture[i, j].JustChanged = true;
						result = SolveCrosswordBruteforce();
						if (result == SolutionStatus.Solved)
							return result;

						picture[i, j].State = CellState.Unknown;
						picture[i, j].JustChanged = false;
					}

			return SolutionStatus.IncorrectCrossword;
		}

        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
			if (!new CrosswordReader(inputFilePath)
				.ReadFile(out rowBlocks, out columnBlocks, out picture))
			{
				return SolutionStatus.BadInputFilePath;
			}

			var status = SolveCrosswordBruteforce();

			if (status == SolutionStatus.Solved)
			{
				if (!new CrosswordWriter(outputFilePath)
					.WriteFile(picture))
				{
					return SolutionStatus.BadOutputFilePath;
				}
			}

			return status;
        }
    }
}