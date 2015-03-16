using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	static class CellArrayExtensions
	{
		public static Cell[] GetRow(this Cell[,] picture, int row)
		{
			int count = picture.GetLength(1);
			var result = new Cell[count];
			for (int j = 0; j < count; j++)
				result[j] = picture[row, j];
			return result;
		}

		public static Cell[] GetColumn(this Cell[,] picture, int column)
		{
			int count = picture.GetLength(0);
			var result = new Cell[count];
			for (int i = 0; i < count; i++)
				result[i] = picture[i, column];
			return result;
		}

		public static bool IsFullySolved(this Cell[,] picture)
		{
			for (int i = 0; i < picture.GetLength(0); i++)
				for (int j = 0; j < picture.GetLength(1); j++)
					if (picture[i, j].State == CellState.Unknown)
						return false;
			return true;
		}

		public static void MarkAllCellsAsChanged(this Cell[,] picture)
		{
			foreach (var cell in picture)
				cell.JustChanged = true;
		}

		public static bool HasChanged(this Cell[,] picture)
		{
			for (int i = 0; i < picture.GetLength(0); i++)
				for (int j = 0; j < picture.GetLength(1); j++)
					if (picture[i, j].JustChanged)
						return true;
			return false;
		}
	}
}
