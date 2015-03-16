using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class OneThreadDimensionUpdater : IDimensionUpdater
	{
		private readonly LineUpdater lineUpdaterService;

		public OneThreadDimensionUpdater(LineUpdater lineUpdaterService)
		{
			this.lineUpdaterService = lineUpdaterService;
		}

		public bool Update(int[][] dimensionBlocks, Func<int, Cell[]> get)
		{
			int count = dimensionBlocks.Length;

			for (int i = 0; i < count; i++)
			{
				var line = get(i);
				if (!lineUpdaterService.Update(dimensionBlocks[i], line))
					return false;
			}

			return true;
		}
	}
}
