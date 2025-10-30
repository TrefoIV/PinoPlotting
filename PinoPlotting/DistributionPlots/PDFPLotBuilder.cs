using AdvancedDataStructures.Extensions;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class PDFPLotBuilder : AbstractPlot
	{

		public PDFPLotBuilder(bool logX = false, bool logY = false)
			: base(logX, logY)
		{
		}


		public void AddPDFToPlot(IEnumerable<double> inputData, string label = "", int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
		{

			if (!inputData.Any())
			{
				return;
			}
			if (LogX)
			{
				(double min, double max) = inputData.Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_xGenerator ??= new(min, max)
				{
					LogBase = LogBaseX
				};
				inputData = inputData.Select(x => _xGenerator.Log(x)).ToList();
			}

			List<((double, double) bin, double y)> pdf = CDFUtils.MakePDF(inputData);

			double[] xs = pdf.Select(x => x.bin.Item1).ToArray();
			double[] ys = pdf.Select(y => y.y).ToArray();
			if (LogY)
			{
				(double min, double max) = ys.Where(y => y > 0).DefaultIfEmpty(-1).MinMax();
				_yGenerator ??= new(min, max) { ShowZero = true, LogBase = LogBaseY };
				ys = ys.Select(y => _yGenerator.Log(y)).ToArray();
			}
			var scatter = _plt.Add.Scatter(xs, ys, color);
			scatter.LegendText = label;
		}

		public void AddPDFToPlot(IEnumerable<int> inputData, string label = "", int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
		{
			if (!inputData.Any())
			{
				return;
			}

			AddPDFToPlot(inputData.Select(x => (double)x), label);
		}


		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{

			FinalizeSettings(xLabel, yLabel);
			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, 800, 700);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		protected void FinalizeSettings(string xLabel, string yLabel)
		{
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
				_yGenerator.LabelFormatter = PlotUtils.PercentagesFormatter;
				(double bttm, double top) = _yGenerator.GetLimits();
				_plt.Axes.SetLimitsY(bttm, top);
			}
			else
			{
				_plt.Axes.Left.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = PlotUtils.PercentagesFormatter
				};
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
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 13f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 30f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 30f;
			_plt.Layout.Fixed(new PixelPadding(top: 10, left: 85, right: 10, bottom: 85));
			_plt.XLabel(xLabel);
			_plt.Axes.Bottom.Label.OffsetY = 20f;
			_plt.YLabel(yLabel);
		}
	}
}
