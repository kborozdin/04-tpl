using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseCrossword
{
	public class Cell
	{
		public CellState State { get; set; }
		public bool JustChanged { get; set; }

		public Cell()
		{
			State = CellState.Unknown;
			JustChanged = false;
		}
	}
}
