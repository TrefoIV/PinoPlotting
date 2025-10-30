using AdvancedDataStructures.Extensions;
using MyPlotting.Extensions;
using ScottPlot;

namespace MyPlotting
{
	public class LinePlotBuilder : AbstractPlot
	{
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
				(double min, double max) = data.Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_yGenerator ??= new(min, max) { LogBase = LogBaseY };
				_yGenerator.Min = Math.Min(_yGenerator.Min, min);
				_yGenerator.Max = Math.Max(_yGenerator.Max, max);
				ys.Apply(_yGenerator.Log);
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
			_plt.Axes.SetLimits(left: 0);

			if (LogY)
			{
				_yGenerator ??= new(1, 1);
				_plt.Axes.Left.TickGenerator = _yGenerator;

				for (int e = 1; e <= _plt.Axes.GetLimits().Top; e++)
				{
					_plt.Add.HorizontalLine(e, width: 1f, Colors.DarkGrey, LinePattern.Dashed);
				}
			}


			if (LegendAlignment != null) _plt.Legend.Alignment = LegendAlignment.Value;
			_plt.Grid.MajorLineWidth = 1;
			_plt.Grid.MajorLineColor = Colors.LightGray;
			_plt.Grid.IsVisible = true;

			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 14f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;

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
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, xSize, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

	}
}
