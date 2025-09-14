using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class BoxplotBuilder : AbstractPlot
	{
		private List<PlotUtils.BoxWithAverage> _boxes = new();
		private Dictionary<Box, string?> _labels = new();
		private HashSet<int> _usedPositions = new();

		public double xMax { get; protected set; }
		public double yMax { get; protected set; }

		public BoxplotBuilder(bool logY = false)
			: base(false, logY)
		{
		}

		public void AddBoxToPlot(IEnumerable<double> data, string? xLabel = null, string legend = "")
		{
			PlotUtils.BoxWithAverage box = PlotUtils.GetPercentileBox(data);
			box.Legend = legend;
			_boxes.Add(box);
			_labels[box.Box] = xLabel;
		}
		public void AddBoxToPlot(int pos, IEnumerable<double> data, string? xLabel = null, string legend = "")
		{
			PlotUtils.BoxWithAverage box = PlotUtils.GetPercentileBox(data);
			box.Legend = legend;
			box.Box.Position = pos;
			_usedPositions.Add(pos);
			_labels[box.Box] = xLabel ?? $"{PlotUtils.NumericLabeling(pos)}";
			_boxes.Add(box);
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			int pos = 1;
			foreach (PlotUtils.BoxWithAverage box in _boxes)
			{
				while (_usedPositions.Contains(pos)) pos++;
				if (box.Box.Position == 0)
				{
					box.Box.Position = pos;
					_usedPositions.Add(pos);
				}
				else
				{
					pos = (int)box.Box.Position;
				}
				var b = _plt.Add.Box(box.Box);
				b.LegendText = box.Legend != "" ? $"{GetBoxLabel(box)} - {box.Legend}" : "";
				var s = _plt.Add.Scatter(pos, box.Average, color: Colors.Black);
				s.MarkerShape = MarkerShape.OpenSquare;

				var m = _plt.Add.Line(pos - 0.2, box.Variance, pos + 0.2, box.Variance);
				m.Color = Colors.Red;
				m.LineStyle.Pattern = LinePattern.Dashed;
				pos = 1;
			}

			if (LogY)
			{
				_plt.Axes.Left.TickGenerator = new NumericAutomatic()
				{
					MinorTickGenerator = new LogMinorTickGenerator(),
					LabelFormatter = x => PlotUtils.NumericLabeling(Math.Pow(10, x))
				};
				for (int e = 1; e <= _plt.Axes.GetLimits().Top; e++)
				{
					_plt.Add.HorizontalLine(e, width: 1f, Colors.DarkGrey, LinePattern.Dashed);
				}
			}

			double[] xs = _boxes.OrderBy(b => b.Box.Position).Select(b => b.Box.Position).ToArray();
			string[] xLabels = _boxes.OrderBy(b => b.Box.Position).Select(GetBoxLabel).ToArray();
			_plt.Axes.Bottom.TickGenerator = new NumericManual(xs, xLabels);

			_plt.Legend.IsVisible = true;
			_plt.Legend.FontSize = 13f;
			if (LegendAlignment != null) _plt.Legend.Alignment = LegendAlignment.Value;
			_plt.Grid.MajorLineWidth = 1;
			_plt.Grid.MajorLineColor = Colors.LightGray;
			_plt.Grid.IsVisible = true;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = 20f;
			_plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
			_plt.Axes.SetLimits(left: 0, bottom: -5, top: 10);
			_plt.Axes.Left.Max = 10;
			if (xLabel != null)
				_plt.XLabel(xLabel);
			_plt.Axes.Bottom.Label.OffsetY = 30f;
			if (yLabel != null)
				_plt.YLabel(yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, Math.Max((int)_plt.Axes.GetLimits().Right * 10, 800), 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 155 * 10, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		private string GetBoxLabel(PlotUtils.BoxWithAverage b)
		{
			return _labels[b.Box] ?? $"{PlotUtils.NumericLabeling(b.Box.Position)}";
		}
	}
}
