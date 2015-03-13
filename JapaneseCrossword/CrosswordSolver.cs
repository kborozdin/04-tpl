using System;
using System.IO;
using System.Linq;

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
			int count = ParseNumberAfterColon(lines[0]);
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

		private bool CanPlaceBlock(CellState states[], int left, int right)
		{
			//TODO
		}

		private bool CheckLineCorectness(int[] blocks, CellState[] states)
		{
			//FUNC TODO : complete function
			int length = states.Length;
			int count = blocks.Length;

			//dynamic programming : covered first A cells using first B blocks
			bool[,] possible = new bool[length + 1, count + 1];
			possible[0, 0] = true;

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j <= count; j++)
				{
					if (!possible[i, j])
						continue;

					possible[i + 1, j] = true;

					if (j == count)
						continue;

					for (int shift = blocks[j] + 1; i + shift <= length; shift++)
					{
						//TODO check indices
						if (CanPlaceBlock(states, i + shift - blocks[j] - 1, i + shift - 1))
							possible[i + shift, j + 1] = true;
					}
				}
			}
		}

		private CellState[] UpdateLine(int[] blocks, CellState[] states)
		{
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
					continue;

				if (canBeBlack)
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
				if (rowsResult == IterationResult.Failed)
					return SolutionStatus.IncorrectCrossword;
				if (rowsResult == IterationResult.Changed)
					any = true;
			}

			return IsFullySolved() ? SolutionStatus.Solved : SolutionStatus.PartiallySolved;
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
        }
    }
}