using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
	public abstract class AbstractPlot : IDisposable
	{
		protected Plot _plt;
		protected bool _disposed;
		public bool LogX { get; protected set; } = false;
		public bool LogY { get; protected set; } = false;
		public int LogBaseX { get; set; } = 10;
		public int LogBaseY { get; set; } = 10;
		public Alignment? LegendAlignment { get; set; } = null;

		protected LogTickGenerator? _xGenerator;
		protected LogTickGenerator? _yGenerator;

		public AbstractPlot(bool logX, bool logY)
		{
			_plt = new();
			_disposed = false;
			LogX = logX;
			LogY = logY;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				//Free managed resources here. Assign large managed object references to null to make them more likely to be unreachable
			}
			_plt.Dispose();

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public abstract void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "");


	}
}
