using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	class MultiThreadDimensionUpdater : IDimensionUpdater
	{
		private readonly LineUpdater lineUpdaterService;

		public MultiThreadDimensionUpdater(LineUpdater lineUpdaterService)
		{
			this.lineUpdaterService = lineUpdaterService;
		}

		public bool Update(int[][] dimensionBlocks, Func<int, Cell[]> get)
		{
			int count = dimensionBlocks.Length;
			var tasks = new Task<bool>[count];

			for (int i = 0; i < count; i++)
			{
				int _i = i;
				var line = get(i);
				var task = new Task<bool>(() => lineUpdaterService.Update(dimensionBlocks[_i], line));
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
	}
}
