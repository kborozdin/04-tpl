using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class CrosswordReader
	{
		private string filePath;

		public CrosswordReader(string filePath)
		{
			this.filePath = filePath;
		}

		private int ParseNumberAfterColon(string line)
		{
			int colon = line.IndexOf(':');
			return int.Parse(line.Substring(colon + 1));
		}

		private int[] ParseNumbersDelimitedBySpaces(string line)
		{
			return line.Split().Select(int.Parse).ToArray();
		}

		private int[][] ParseDimension(string[] lines, int shift)
		{
			int count = ParseNumberAfterColon(lines[shift]);
			return Enumerable
				.Range(0, count)
				.Select(i => ParseNumbersDelimitedBySpaces(lines[shift + 1 + i]))
				.ToArray();
		}

		public bool ReadFile(out int[][] rowBlocks,
			out int[][] columnBlocks, out Cell[,] picture)
		{
			rowBlocks = columnBlocks = null;
			picture = null;

			try
			{
				var lines = File.ReadAllLines(filePath);
				rowBlocks = ParseDimension(lines, 0);
				columnBlocks = ParseDimension(lines, rowBlocks.Length + 1);
			}
			catch (Exception)
			{
				return false;
			}

			picture = new Cell[rowBlocks.Length, columnBlocks.Length];
			for (int i = 0; i < rowBlocks.Length; i++)
				for (int j = 0; j < columnBlocks.Length; j++)
					picture[i, j] = new Cell();

			return true;
		}
	}
}
