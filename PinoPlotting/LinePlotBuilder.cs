using MyPlotting.Extensions;
using MyPlotting.TickGenerators;
using ScottPlot;

namespace MyPlotting
{
	public class LinePlotBuilder : AbstractPlot
	{
		private LogTickGenerator? _leftYTickGen = null;
		public double xMax { get; protected set; }
		public double yMax { get; protected set; }
		public bool Squeeze { get; set; }

		public LinePlotBuilder(bool logX = false, bool logY = false)
			: base(logX, logY)
		{
		}

		public void AddPlot(IEnumerable<double> data, string label = "", Color? color = null, float size = 5f, LinePattern? linePattern = null, MarkerShape marker = MarkerShape.FilledCircle)
		{
			linePattern ??= LinePattern.Solid;
			double[] xs = data.Select((x, i) => (double)i + 1).ToArray();
			double[] ys = data.ToArray();
			if (LogY && data.Any())
			{
				_leftYTickGen ??= new(data.Min(), data.Max()) { LogBase = LogBaseY };
				_leftYTickGen.Min = Math.Min(_leftYTickGen.Min, data.Min());
				_leftYTickGen.Max = Math.Max(_leftYTickGen.Max, data.Max());
				ys.Apply(_leftYTickGen.Log);
			}

			var scatter = _plt.Add.Scatter(xs, ys, color: color);
			scatter.Axes.YAxis = _plt.Axes.Left;
			scatter.LegendText = label;
			scatter.LinePattern = linePattern.Value;
			scatter.MarkerStyle.Shape = marker;
			scatter.MarkerStyle.Size = size;

			var yMax = ys.Max();
			var xMax = xs[^1];
			if (yMax > this.yMax) this.yMax = yMax;
			if (xMax > this.xMax) this.xMax = xMax;
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			_plt.Axes.SetLimitsY(bottom: -0.01, top: yMax + 0.05);
			_plt.Axes.SetLimits(right: xMax + 5);

			if (LogY)
			{
				_plt.Axes.Left.TickGenerator = _leftYTickGen ?? new(0, 1);
				//_plt.Axes.Left.TickGenerator = new NumericAutomatic()
				//{
				//	MinorTickGenerator = new LogMinorTickGenerator(),
				//	LabelFormatter = x => $"{Math.Pow(10, x):N0}"
				//};
				for (int e = 1; e <= _plt.Axes.GetLimits().Top; e++)
				{
					_plt.Add.HorizontalLine(e, width: 1f, Colors.DarkGrey, LinePattern.Dashed);
				}
			}

			//_plt.Legend.IsVisible = true;
			_plt.Legend.Alignment = Alignment.UpperRight;
			//_plt.Legend.Font.Size = 30f;
			_plt.Grid.MajorLineWidth = 1;
			_plt.Grid.MajorLineColor = Colors.LightGray;
			_plt.Grid.IsVisible = true;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = 30f;
			_plt.Axes.Left.TickLabelStyle.FontSize = 30f;
			_plt.Axes.SetLimits(left: 0);

			int xSize = Squeeze ? 800 : (int)_plt.Axes.GetLimits().Right * 10;
			if (xLabel != null)
				_plt.XLabel(xLabel);
			_plt.Axes.Bottom.Label.OffsetY = 30f;
			if (yLabel != null)
				_plt.YLabel(yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, xSize, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, xSize, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

	}
}
