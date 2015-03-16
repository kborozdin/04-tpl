using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class LineCorrectnessChecker
	{
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

		public bool Check(int[] blocks, Cell[] states)
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
	}
}
