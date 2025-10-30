using AdvancedDataStructures.Extensions;
using MyPlotting.CustomPlottable;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class MultipleDistributionsPlotBuilder : AbstractPlot
	{
		public bool UseColorMap { get; set; } = false;
		public bool CommonScale { get; set; } = false;
		public bool UseHistograms { get; set; } = false;
		public int PDFSteps { get; set; } = 15;
		private int _addedDistributions = 0;
		private List<string> _yLabels = new();

		private List<IEnumerable<double>> _dataList = new();

		public MultipleDistributionsPlotBuilder(bool useColorMap, bool logX = false)
			: base(logX, false)
		{ }


		public void AddDistribution(IEnumerable<double> data, string? yLabel = null)
		{
			if (!data.Any()) return;

			if (LogX)
			{
				(double min, double max) = data.Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) { return; }
				_xGenerator ??= new(min, max) { LogBase = LogBaseX };
				_xGenerator.Min = Math.Min(_xGenerator.Min, min);
				_xGenerator.Max = Math.Max(_xGenerator.Max, max);

				data = data.Select(_xGenerator.Log);
			}

			_addedDistributions++;

			_yLabels.Add(yLabel ?? $"{_addedDistributions}");
			_dataList.Add(data);
		}

		private void CreatePlots()
		{
			IColormap? colormap = null;
			if (UseColorMap)
			{
				var fullColormap = new ScottPlot.Colormaps.Greens();
				var restrictedColors = fullColormap.GetColors(256, minFraction: 0.1, maxFraction: 0.8).Reverse().ToArray();
				colormap = new ScottPlot.Colormaps.CustomInterpolated(restrictedColors);
			}
			if (!_dataList.Any()) return;
			var _pdfsList = _dataList.Select(x => CDFUtils.MakePDF(x, PDFSteps)).ToList();

			double? max = CommonScale ? _pdfsList.Max(pdf => pdf.Max(p => p.Item2)) : null;

			double[] ys = new double[_pdfsList.Count];
			string[] yLabels = new string[_pdfsList.Count];

			for (int i = 0; i < _pdfsList.Count; i++)
			{
				double y = _addedDistributions - i;
				var line = _plt.Add.HorizontalLine(y, color: Colors.DarkGrey, pattern: LinePattern.Dashed);
				line.LineWidth = 0.5f;

				max ??= _pdfsList[i].Max(p => p.Item2);

				foreach (((double, double) bin, double count) point in _pdfsList[i])
				{
					double mean = (point.bin.Item1 + point.bin.Item2) / 2;
					double value = point.count / max.Value;
					//Mappa la size tra 5 e 15. 0 va su 5 e 1 va su 15
					float size = (float)value * 15 + 10;

					Color? c = colormap?.GetColor(value);
					var s = _plt.Add.Marker(mean, y, shape: MarkerShape.FilledCircle, size, c);
				}

				ys[i] = y;
				yLabels[i] = _yLabels[i];
			}

			_plt.Axes.Left.TickGenerator = new NumericManual(ys, yLabels);

			if (UseColorMap)
			{
				ColormapLegend cp = new(colormap, new ScottPlot.Range(0, max.Value));
				var colorLegend = _plt.Add.ColorBar(cp);
				colorLegend.Axis.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 15f;

				colorLegend.Axis.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = x => $"{PlotUtils.NumericLabeling(x)}%"
				};
			}
		}


		private void CreatePlotsHistograms()
		{
			double[] yTicks = new double[_dataList.Count];
			string[] yLabels = new string[_dataList.Count];
			double y = 0;

			for (int i = 0; i < _dataList.Count; i++)
			{
				yTicks[i] = y;
				yLabels[i] = _yLabels[i];
				double[] data = _dataList[i].ToArray();
				if (data.Length < 1) continue;

				double maxCommon = data.Max();
				double minCommon = data.Min();

				double p = maxCommon - minCommon;
				double binSize = p / PDFSteps;


				var hist = ScottPlot.Statistics.Histogram.WithBinSize(binSize, data);

				// Display the histogram as a bar plot
				BarPlot barPlot = _plt.Add.Bars(hist.Bins, hist.GetProbability());

				Color color = PlotUtils.GetRandomColor();
				// Customize the style of each bar
				foreach (Bar bar in barPlot.Bars)
				{
					bar.Value += y;
					bar.ValueBase = y;
					bar.Size = hist.FirstBinSize;
					bar.LineWidth = 0;
					bar.FillStyle.AntiAlias = false;
					bar.FillColor = color.WithAlpha(.2);
				}

				// Plot the probability curve on top the histogram
				ScottPlot.Statistics.ProbabilityDensity pd = new(data);
				double[] xs = Generate.Range(minCommon, maxCommon, binSize);
				double scale = 1.0 / hist.Bins.Select(x => pd.GetY(x)).Sum();
				double[] ys = pd.GetYs(xs, scale).Select(p => p + y).ToArray();

				var curve = _plt.Add.ScatterLine(xs, ys);
				curve.LineWidth = 2;
				curve.LineColor = color;
				curve.LinePattern = LinePattern.DenselyDashed;
				curve.LegendText = _yLabels[i];

				y = barPlot.Bars.Max(bar => bar.Value);
				y += y * 0.1;
			}
			_plt.Axes.Left.TickGenerator = new NumericManual(yTicks, yLabels);
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			if (UseHistograms) CreatePlotsHistograms();
			else CreatePlots();

			if (LogX)
			{
				_xGenerator ??= new(1, 1);
				_plt.Axes.Bottom.TickGenerator = _xGenerator;
				(double bttm, double top) = _xGenerator.GetLimits();
				_plt.Axes.SetLimitsX(bttm, top);
			}

			_plt.Axes.Bottom.Label.Text = xLabel;
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Legend.Alignment = LegendAlignment ?? Alignment.LowerRight;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 14f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;

			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

	}
}
