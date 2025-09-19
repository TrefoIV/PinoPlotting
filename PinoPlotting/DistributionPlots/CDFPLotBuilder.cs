using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class CDFPLotBuilder : AbstractPlot
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
			cdf = PlotUtils.MakeDataGradinoPlot(CDFUtils.MakeCDF(inputData, steps: steps));
			double[] xs = cdf.Select(x => x.Item1).ToArray();
			double[] ys = cdf.Select(y => y.Item2).ToArray();

			if (LogX)
			{
				double max = xs.Max();
				double min = ys.Min();
				if (_xGenerator is null) _xGenerator = new LogTickGenerator(min, max) { LogBase = LogBaseX };
				else
				{
					_xGenerator.Min = Math.Min(_xGenerator.Min, min);
					_xGenerator.Max = Math.Max(_xGenerator.Max, max);
				}
				xs = xs.Select(x => _xGenerator.Log(x)).ToArray();
			}

			if (LogY)
			{
				double max = ys.Max();
				double min = ys.Min();
				_yGenerator ??= new LogTickGenerator(0, 100) { LogBase = LogBaseY };
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
			BuildAxes();

			FinalizeSettings(xLabel, yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		public void SavePlot(FileInfo outFile, double max, string xLabel = "", string yLabel = "")
		{
			BuildAxes(max);
			FinalizeSettings(xLabel, yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		private void BuildAxes(double? max = null)
		{

			if (LogX)
			{
				_xGenerator ??= new LogTickGenerator(0, 0) { LogBase = LogBaseX };
				_plt.Axes.Bottom.TickGenerator = _xGenerator;
			}
			else
			{
				double[] allX = _plt.PlottableList.Select(x => x as Scatter).SelectMany(s => s.Data.GetScatterPoints().Select(p => p.X)).ToArray();
				double maxX = max.HasValue ? max.Value : allX.Max();
				double magnitude = Math.Floor(Math.Log10(maxX));
				double baseOrder = Math.Pow(10, magnitude);
				if (maxX / baseOrder < 4)
				{
					baseOrder /= 2;
				}
				int bigTicks = (int)Math.Ceiling(maxX / baseOrder);
				double[] xTicks = Enumerable.Range(0, bigTicks).SelectMany(n => Enumerable.Range(0, 4).Select(i => baseOrder * n + baseOrder / 4 * i)).Append(baseOrder * bigTicks).ToArray();

				_plt.Axes.Bottom.TickGenerator = new NumericManual(
					xTicks,
					xTicks.Select(n => PlotUtils.NumericLabeling(n)).ToArray()
				);
			}
		}

		protected void FinalizeSettings(string xLabel, string yLabel)
		{
			if (LogY)
			{
				_yGenerator ??= new LogTickGenerator(0, 100) { LogBase = LogBaseY };
			}
			_plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
			_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
			_plt.Axes.Bottom.TickLabelStyle.AntiAliasText = false;
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
			_plt.Axes.Bottom.TickLabelStyle.FontSize = 14f;
			_plt.Axes.Left.TickLabelStyle.FontSize = 14f;
			//_plt.Legend.Font.Size = 20f;
			_plt.Axes.Bottom.Label.FontSize = 20f;
			_plt.Axes.Left.Label.FontSize = 20f;
			_plt.Axes.SetLimits(bottom: LogY ? null : -1, top: LogY ? 2.01 : 101, left: LogX ? null : 0);
			_plt.Layout.Fixed(new PixelPadding(top: 10, left: 85, right: 10, bottom: 85));
			_plt.XLabel(xLabel);
			_plt.Axes.Bottom.Label.OffsetY = 20f;
			_plt.YLabel(yLabel);
		}


	}
}
