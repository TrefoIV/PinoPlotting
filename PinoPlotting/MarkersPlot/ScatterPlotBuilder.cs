using AdvancedDataStructures.Extensions;
using MyPlotting.CustomPlottable;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class ScatterPlotBuilder : AbstractPlot
	{

		private Dictionary<(double x, double y), double?> _points = new();
		public IColormap Colormap { get; private set; } = new Greens();

		public ScatterPlotBuilder(bool logX = false, bool logY = false, IColormap? baseColormap = null)
			: base(logX, logY)
		{
			var restrictedColors = baseColormap?.GetColors(256, minFraction: 0.1, maxFraction: 0.9) ??
										Colormap.GetColors(256, minFraction: 0.1, maxFraction: 0.9);
			Colormap = new ScottPlot.Colormaps.CustomInterpolated(restrictedColors.Reverse().ToArray());
		}


		public void AddDataToPlot(IEnumerable<(double x, double y, double? v)> inputData)
		{
			if (!inputData.Any()) return;
			if (LogX)
			{
				(double min, double max) = inputData.Select(x => x.x).Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_xGenerator ??= new(min, max) { LogBase = LogBaseX, ShowZero = true };
				inputData = inputData.Where(x => x.x >= 0).Select(x => (_xGenerator.Log(x.x), x.y, x.v)).ToList();
			}
			if (!inputData.Any()) return;
			if (LogX)
			{
				(double min, double max) = inputData.Select(x => x.y).Where(x => x > 0).DefaultIfEmpty(-1).MinMax();
				if (min == -1 || max == -1) return;
				_yGenerator ??= new(min, max) { LogBase = LogBaseX, ShowZero = true };
				inputData = inputData.Where(x => x.y >= 0).Select(x => (x.x, _yGenerator.Log(x.y), x.v)).ToList();
			}

			foreach (var p in inputData)
			{
				double? existingValue = _points.GetValueOrDefault((p.x, p.y));
				double? newValue = existingValue.HasValue || p.v.HasValue ? (existingValue ?? 0) + (p.v ?? 0) : null;

				_points[(p.x, p.y)] = newValue;
			}
		}

		public void AddDataToPlot(IEnumerable<(int x, int y)> inputData)
		{

			AddDataToPlot(inputData.Select(p => ((double)p.x, (double)p.y, (double?)null)));

		}

		public void AddDataToPlot(IEnumerable<(double x, int y, double v)> inputData)
		{
			AddDataToPlot(inputData.Select(p => (p.x, (double)p.y, (double?)p.v)));
		}


		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			SavePlot(outFile, xLabel, yLabel, "");
		}

		public void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "", string colormapLabel = "")
		{

			double? vMax = _points.Where(p => p.Value.HasValue).Select(p => p.Value).DefaultIfEmpty(null).Max();
			double? vMin = _points.Where(p => p.Value.HasValue).Select(p => p.Value).DefaultIfEmpty(null).Min();

			foreach (var p in _points)
			{
				double? v = p.Value;
				if (v.HasValue)
				{
					double t = (v.Value - vMin!.Value) / vMax!.Value;
					Color c = Colormap.GetColor(t);
					var marker = _plt.Add.Marker(p.Key.x, p.Key.y, size: (float)(t * 10) + 5, color: c);
				}
				else
				{
					var marker = _plt.Add.Marker(p.Key.x, p.Key.y, size: 3, color: Colors.Gray);
				}
			}

			if (vMax != null && vMin != null)
			{
				ColormapLegend colormapLegend = new(Colormap, new ScottPlot.Range(vMin!.Value, vMax!.Value));

				var colorLgd = _plt.Add.ColorBar(colormapLegend);
				colorLgd.Axis.Label.Text = colormapLabel;
				colorLgd.Axis.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 18f;
				colorLgd.Axis.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 16f;
			}

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
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

	}
}
