using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class CrosswordWriter
	{
		private readonly Dictionary<CellState, char> mapping =
			new Dictionary<CellState, char>
			{
				{CellState.Unknown, '?'},
				{CellState.White, '.'},
				{CellState.Black, '*'}
			};

		string filePath;

		public CrosswordWriter(string filePath)
		{
			this.filePath = filePath;
		}

		public bool WriteFile(Cell[,] picture)
		{
			int height = picture.GetLength(0);
			int width = picture.GetLength(1);

			try
			{
				using (var file = new StreamWriter(filePath))
				{
					for (int i = 0; i < height; i++)
					{
						file.WriteLine(
							string.Join("",
							Enumerable
							.Range(0, width)
							.Select(j => mapping[picture[i, j].State])));
					}
				}
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}
}
