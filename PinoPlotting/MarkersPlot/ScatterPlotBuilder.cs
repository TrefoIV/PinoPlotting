using AdvancedDataStructures.Extensions;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class ScatterPlotBuilder : AbstractPlot
	{

		private HashSet<(double x, double y)> _points = new();

		public ScatterPlotBuilder(bool logX = false, bool logY = false)
			: base(logX, logY)
		{
		}

		public void AddDataToPlot(IEnumerable<(double x, double y)> inputData)
		{
			if (!inputData.Any()) return;
			if (LogX)
			{
				(double min, double max) = inputData.Select(x => x.x).Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_xGenerator ??= new(min, max) { LogBase = LogBaseX, ShowZero = true };
				inputData = inputData.Where(x => x.x >= 0).Select(x => (_xGenerator.Log(x.x), x.y)).ToList();
			}
			if (!inputData.Any()) return;
			if (LogX)
			{
				(double min, double max) = inputData.Select(x => x.y).Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_yGenerator ??= new(min, max) { LogBase = LogBaseX, ShowZero = true };
				inputData = inputData.Where(x => x.y >= 0).Select(x => (x.x, _yGenerator.Log(x.y))).ToList();
			}

			foreach (var p in inputData) _points.Add(p);
		}

		public void AddDataToPlot(IEnumerable<(int x, int y)> inputData)
		{

			AddDataToPlot(inputData.Select(p => ((double)p.x, (double)p.y)));

		}


		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			var scatter = _plt.Add.Markers(_points.Select(p => p.x).ToArray(), _points.Select(p => p.y).ToArray(), size: 3);

			if (LogX)
			{
				_xGenerator ??= new(1, 1);
				_plt.Axes.Bottom.TickGenerator = _xGenerator;
				(double bttm, double top) = _xGenerator.GetLimits();
				_plt.Axes.SetLimitsX(bttm, top);
			}
			else
			{
				_plt.Axes.Bottom.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = PlotUtils.NumericLabeling
				};
			}

			if (LogY)
			{
				_yGenerator ??= new(1, 1);
				_plt.Axes.Left.TickGenerator = _yGenerator;
				(double bttm, double top) = _yGenerator.GetLimits();
				_plt.Axes.SetLimitsY(bttm, top);
			}
			else
			{
				_plt.Axes.Left.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = PlotUtils.NumericLabeling
				};
			}

			_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter;
			_plt.Grid.MajorLineWidth = 1;
			_plt.Grid.MajorLineColor = Colors.Black.WithLightness(0.7f);

			_plt.Layout.Fixed(new PixelPadding(top: 15, right: 10, left: 150, bottom: 50));

			_plt.XLabel(xLabel);
			_plt.YLabel(yLabel);

			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

	}
}
