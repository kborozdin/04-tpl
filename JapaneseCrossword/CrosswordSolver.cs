using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

//GLOBAL TODO : refactor into separate classes

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
		int[][] rowBlocks;
		int[][] columnBlocks;
		Cell[,] picture;

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

		private void ReadFile(string filePath)
		{
			var lines = File.ReadAllLines(filePath);
			rowBlocks = ParseDimension(lines, 0);
			columnBlocks = ParseDimension(lines, rowBlocks.Length + 1);

			picture = new Cell[rowBlocks.Length, columnBlocks.Length];
			for (int i = 0; i < rowBlocks.Length; i++)
				for (int j = 0; j < columnBlocks.Length; j++)
					picture[i, j] = new Cell();
		}

		private Cell[] GetRow(int row)
		{
			int count = picture.GetLength(1);
			var result = new Cell[count];
			for (int j = 0; j < count; j++)
				result[j] = picture[row, j];
			return result;
		}

		private Cell[] GetColumn(int column)
		{
			int count = picture.GetLength(0);
			var result = new Cell[count];
			for (int i = 0; i < count; i++)
				result[i] = picture[i, column];
			return result;
		}

		private bool IsFullyColored(int[] prefixSum, int left, int right)
		{
			int length = right - left + 1;
			if (length <= 0)
				return true;
			int sum = prefixSum[right] - (left > 0 ? prefixSum[left - 1] : 0);
			return sum == length;
		}

		private int[] CalculatePrefixSum(Cell[] states, Func<Cell, bool> isGood)
		{
			int length = states.Length;
			var prefixSum = new int[length];

			for (int i = 0; i < length; i++)
			{
				if (i > 0)
					prefixSum[i] = prefixSum[i - 1];
				if (isGood(states[i]))
					prefixSum[i]++;
			}

			return prefixSum;
		}

		private bool CheckLineCorectness(int[] blocks, Cell[] states)
		{
			int length = states.Length;
			int count = blocks.Length;

			var prefixSumWhite = CalculatePrefixSum(states, c => c.State != CellState.Black);
			var prefixSumBlack = CalculatePrefixSum(states, c => c.State != CellState.White);

			//dynamic programming : covered first A cells using first B blocks
			bool[,] possible = new bool[length + 1, count + 1];
			possible[0, 0] = true;

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j <= count; j++)
				{
					if (!possible[i, j])
						continue;

					if (IsFullyColored(prefixSumWhite, i, i))
						possible[i + 1, j] = true;

					if (j == count)
						continue;

					int space = (j == count - 1 ? 0 : 1);
					if (i + blocks[j] + space <= length)
					{
						if (IsFullyColored(prefixSumBlack, i, i + blocks[j] - 1) &&
							IsFullyColored(prefixSumWhite, i + blocks[j], i + blocks[j] + space - 1))
						{
							possible[i + blocks[j] + space, j + 1] = true;
						}
					}
				}
			}

			return possible[length, count];
		}

		private bool UpdateLine(int[] blocks, Cell[] states)
		{
			if (!states.Any(c => c.JustChanged))
				return true;
			foreach (var cell in states)
				cell.JustChanged = false;

			if (!CheckLineCorectness(blocks, states))
				return false;

			foreach (var cell in states)
			{
				if (cell.State != CellState.Unknown)
					continue;

				cell.State = CellState.White;
				bool canBeWhite = CheckLineCorectness(blocks, states);

				cell.State = CellState.Black;
				bool canBeBlack = CheckLineCorectness(blocks, states);

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

		private bool UpdateDimension(int[][] dimensionBlocks, Func<int, Cell[]> get)
		{
			int count = dimensionBlocks.Length;
			var tasks = new Task<bool>[count];

			for (int i = 0; i < count; i++)
			{
				int _i = i;
				var line = get(i);
				var task = new Task<bool>(() => UpdateLine(dimensionBlocks[_i], line));
				tasks[i] = task;
			}

			foreach (var task in tasks)
				task.Start();
			Task.WaitAll(tasks);

			for (int i = 0; i < count; i++)
				if (!tasks[i].Result)
					return false;
			return true;
		}

		private bool IsFullySolved()
		{
			int rows = picture.GetLength(0);
			int columns = picture.GetLength(1);
			for (int i = 0; i < rows; i++)
				for (int j = 0; j < columns; j++)
					if (picture[i, j].State == CellState.Unknown)
						return false;
			return true;
		}

		private bool HasChanged()
		{
			for (int i = 0; i < picture.GetLength(0); i++)
				for (int j = 0; j < picture.GetLength(1); j++)
					if (picture[i, j].JustChanged)
						return true;
			return false;
		}

		private void MarkAllCellsAsChanged()
		{
			foreach (var cell in picture)
				cell.JustChanged = true;
		}

		private SolutionStatus SolveCrossword()
		{
			bool firstRun = true;
			MarkAllCellsAsChanged();

			while (HasChanged())
			{
				if (!UpdateDimension(rowBlocks, GetRow))
					return SolutionStatus.IncorrectCrossword;
				if (firstRun)
				{
					firstRun = false;
					MarkAllCellsAsChanged();
				}
				if (!UpdateDimension(columnBlocks, GetColumn))
					return SolutionStatus.IncorrectCrossword;
			}

			return IsFullySolved() ? SolutionStatus.Solved : SolutionStatus.PartiallySolved;
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

		private void WriteFile(string filePath)
		{
			var mapping = new Dictionary<CellState, char>
			{
				{CellState.Unknown, '?'},
				{CellState.White, '.'},
				{CellState.Black, '*'}
			};
			int height = picture.GetLength(0);
			int width = picture.GetLength(1);

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

        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            try
			{
				ReadFile(inputFilePath);
			}
			catch (Exception) //TODO : bad practice maybe
			{
				return SolutionStatus.BadInputFilePath;
			}

			//var status = SolveCrossword();
			var status = SolveCrosswordBruteforce();

			if (status == SolutionStatus.PartiallySolved ||
				status == SolutionStatus.Solved)
			{
				try
				{
					WriteFile(outputFilePath);
				}
				catch (Exception) //TODO : bad practice maybe
				{
					return SolutionStatus.BadOutputFilePath;
				}
			}

			return status;
        }
    }
}