using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

//GLOBAL TODO : refactor into separate classes

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
		int[][] rowBlocks;
		int[][] columnBlocks;
		CellState[,] picture;

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
			picture = new CellState[rowBlocks.Length, columnBlocks.Length];
		}

		private CellState[] GetRow(int row)
		{
			int count = picture.GetLength(1);
			var result = new CellState[count];
			for (int j = 0; j < count; j++) //TODO : faster copying maybe
				result[j] = picture[row, j];
			return result;
		}

		private bool SetRow(int row, CellState[] result)
		{
			int count = picture.GetLength(1);
			bool any = false;
			for (int j = 0; j < count; j++)
			{
				any |= picture[row, j] != result[j];
				picture[row, j] = result[j];
			}
			return any;
		}

		private CellState[] GetColumn(int column)
		{
			int count = picture.GetLength(0);
			var result = new CellState[count];
			for (int i = 0; i < count; i++) //TODO : faster copying maybe
				result[i] = picture[i, column];
			return result;
		}

		private bool SetColumn(int column, CellState[] result)
		{
			int count = picture.GetLength(0);
			bool any = false;
			for (int i = 0; i < count; i++)
			{
				any |= picture[i, column] != result[i];
				picture[i, column] = result[i];
			}
			return any;
		}

		private bool IsFullyColored(int[] prefixSum, int left, int right)
		{
			int length = right - left + 1;
			if (length <= 0)
				return true;
			int sum = prefixSum[right] - (left > 0 ? prefixSum[left - 1] : 0);
			return sum == length;
		}

		private int[] CalculatePrefixSum(CellState[] states, Func<CellState, bool> isGood)
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

		private bool CheckLineCorectness(int[] blocks, CellState[] states)
		{
			int length = states.Length;
			int count = blocks.Length;

			var prefixSumWhite = CalculatePrefixSum(states, c => c != CellState.Black);
			var prefixSumBlack = CalculatePrefixSum(states, c => c != CellState.White);

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

					for (int pos = i; pos + blocks[j] + space <= length; pos++)
					{
						if (IsFullyColored(prefixSumWhite, i, pos - 1) &&
							IsFullyColored(prefixSumBlack, pos, pos + blocks[j] - 1) &&
							IsFullyColored(prefixSumWhite, pos + blocks[j], pos + blocks[j] + space - 1))
						{
							possible[pos + blocks[j] + space, j + 1] = true;
						}
					}
				}
			}

			return possible[length, count];
		}

		private CellState[] UpdateLine(int[] blocks, CellState[] states)
		{
			if (!CheckLineCorectness(blocks, states))
				return null;
			int length = states.Length;

			for (int i = 0; i < length; i++)
			{
				if (states[i] != CellState.Unknown)
					continue;

				states[i] = CellState.White;
				bool canBeWhite = CheckLineCorectness(blocks, states);

				states[i] = CellState.Black;
				bool canBeBlack = CheckLineCorectness(blocks, states);

				if (canBeBlack && canBeWhite)
					states[i] = CellState.Unknown;
				else if (canBeBlack)
					states[i] = CellState.Black;
				else if (canBeWhite)
					states[i] = CellState.White;
				else
					return null;
			}

			return states;
		}

		private IterationResult UpdateDimension(int[][] dimensionBlocks, Func<int, CellState[]> get,
			Func<int, CellState[], bool> set)
		{
			int count = dimensionBlocks.Length;
			var any = false;
			for (int i = 0; i < count; i++)
			{
				var line = get(i);
				var result = UpdateLine(dimensionBlocks[i], line);
				if (result == null)
					return IterationResult.Failed;
				any |= set(i, result);
			}
			return any ? IterationResult.Changed : IterationResult.DidNotChange;
		}

		private bool IsFullySolved()
		{
			int rows = picture.GetLength(0);
			int columns = picture.GetLength(1);
			for (int i = 0; i < rows; i++)
				for (int j = 0; j < columns; j++)
					if (picture[i, j] == CellState.Unknown)
						return false;
			return true;
		}

		private SolutionStatus SolveCrossword()
		{
			bool any = true;

			while (any)
			{
				any = false;

				var rowsResult = UpdateDimension(rowBlocks, GetRow, SetRow);
				if (rowsResult == IterationResult.Failed)
					return SolutionStatus.IncorrectCrossword;
				if (rowsResult == IterationResult.Changed)
					any = true;

				var columnsResult = UpdateDimension(columnBlocks, GetColumn, SetColumn);
				if (columnsResult == IterationResult.Failed)
					return SolutionStatus.IncorrectCrossword;
				if (columnsResult == IterationResult.Changed)
					any = true;
			}

			return IsFullySolved() ? SolutionStatus.Solved : SolutionStatus.PartiallySolved;
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
						.Select(j => mapping[picture[i, j]])));
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

			var status = SolveCrossword();

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