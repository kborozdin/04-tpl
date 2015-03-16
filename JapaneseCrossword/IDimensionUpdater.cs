using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	public interface IDimensionUpdater
	{
		bool Update(int[][] dimensionBlocks, Func<int, Cell[]> get);
	}
}
