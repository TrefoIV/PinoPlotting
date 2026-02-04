using AdvancedDataStructures.Extensions;
using MyPlotting.DistributionPlots;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class CDFPLotBuilder : AbstractDistributionPlot
	{
		public CDFPLotBuilder(bool logX = false, bool logY = false)
			: base(logX, logY)
		{

		}

		public void AddCDFToPlot(IEnumerable<double> inputData, string label = "", int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
		{

			List<(double, double)> cdf;
			if (!inputData.Any())
			{
				return;
			}

			if (LogX)
			{
				(double min, double max) = inputData.Where(x => x > 0).DefaultIfEmpty(-1).MinMax();

				if (_xGenerator is null) _xGenerator = new LogTickGenerator(min, max) { LogBase = LogBaseX };
				else
				{
					_xGenerator.Min = Math.Min(_xGenerator.Min, min);
					_xGenerator.Max = Math.Max(_xGenerator.Max, max);
				}
				inputData = inputData.Select(x => _xGenerator.Log(x)).ToArray();
			}

			cdf = PlotUtils.MakeDataGradinoPlot(CDFUtils.MakeCDF(inputData, steps: steps));
			double[] xs = cdf.Select(x => x.Item1).ToArray();
			double[] ys = cdf.Select(y => y.Item2).ToArray();

			if (LogY)
			{
				(double min, double max) = ys.Where(y => y > 0).DefaultIfEmpty(-1).MinMax();
				_yGenerator ??= new LogTickGenerator(min, 100) { LogBase = LogBaseY };
				ys = ys.Select(y => _yGenerator.Log(y)).ToArray();
			}
			var scatter = _plt.Add.Scatter(xs, ys, color);
			scatter.LegendText = label;
		}

		public void AddCDFToPlot(IEnumerable<int> inputData, string label = null, int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
		{
			AddCDFToPlot(inputData.Select(x => (double)x), label, steps, color);
		}


		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			SavePlot(outFile, null, xLabel, yLabel);
		}

		public void SavePlot(FileInfo outFile, string xLabel, string yLabel, int width, int height)
		{
			SavePlot(outFile, null, xLabel, yLabel, width, height);
		}

		public void SavePlot(FileInfo outFile, double? max, string xLabel = "", string yLabel = "", int width = 800, int height = 600)
		{
			BuildXAxis(max);
			FinalizeSettings(xLabel, yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, width, height);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, width, height);
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, width, height);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}



		protected void FinalizeSettings(string xLabel, string yLabel)
		{
			if (LogY)
			{
				_yGenerator ??= new LogTickGenerator(1, 100) { LogBase = LogBaseY };
				_plt.Axes.Left.TickGenerator = _yGenerator;
				_yGenerator.LabelFormatter = PlotUtils.PercentagesFormatter;
				(double bttm, double top) = _yGenerator.GetLimits();
				_plt.Axes.SetLimitsY(bttm, top);
			}
			else _plt.Axes.Left.TickGenerator = new NumericFixedInterval(10)
			{
				LabelFormatter = PlotUtils.PercentagesFormatter
			};

			foreach (var bar in _verticalBars)
			{
				double x = LogX ? _xGenerator.Log(bar.x) : bar.x;
				var vline = _plt.Add.VerticalLine(x, color: bar.c);
				vline.Text = bar.label;
				vline.LabelOppositeAxis = true;
				vline.LineStyle.Pattern = LinePattern.Dashed;
			}

			_plt.Legend.IsVisible = true;
			if (LegendAlignment != null) _plt.Legend.Alignment = LegendAlignment.Value;
			_plt.Grid.MajorLineWidth = 1;
			_plt.Grid.MajorLineColor = Colors.LightGray;
			_plt.Grid.XAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.YAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.MinorLineWidth = 1;
			_plt.Grid.XAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.YAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.IsVisible = true;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 14f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;

			_plt.XLabel(xLabel);
			_plt.YLabel(yLabel);
		}


	}
}
