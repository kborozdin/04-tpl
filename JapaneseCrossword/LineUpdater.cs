using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class LineUpdater
	{
		private readonly LineCorrectnessChecker correctnessCheckerService;

		public LineUpdater(LineCorrectnessChecker correctnessCheckerService)
		{
			this.correctnessCheckerService = correctnessCheckerService;
		}

		public bool Update(int[] blocks, Cell[] cells)
		{
			if (!cells.Any(c => c.JustChanged))
				return true;
			foreach (var cell in cells)
				cell.JustChanged = false;

			if (!correctnessCheckerService.Check(blocks, cells))
				return false;

			foreach (var cell in cells)
			{
				if (cell.State != CellState.Unknown)
					continue;

				cell.State = CellState.White;
				bool canBeWhite = correctnessCheckerService.Check(blocks, cells);

				cell.State = CellState.Black;
				bool canBeBlack = correctnessCheckerService.Check(blocks, cells);

				if (canBeBlack && canBeWhite)
					cell.State = CellState.Unknown;
				else if (canBeBlack)
				{
					cell.State = CellState.Black;
					cell.JustChanged = true;
				}
				else if (canBeWhite)
				{
					cell.State = CellState.White;
					cell.JustChanged = true;
				}
				else
					return false;
			}

			return true;
		}
	}
}
