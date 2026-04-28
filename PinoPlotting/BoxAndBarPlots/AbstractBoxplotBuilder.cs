using MyPlotting.Axis;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public abstract class AbstractBoxplotBuilder : AbstractPlot, IRotatableAxis
	{
		protected List<PlotUtils.BoxWithAverage> _boxes = new();
		protected Dictionary<Box, string?> _labels = new();
		protected HashSet<int> _usedPositions = new();

		public double xMax { get; protected set; }
		public double yMax { get; protected set; }
		public bool RotateAxis { get; set; } = false;
		public bool DisplayMaxOnTop { get; set; } = false;
		public bool RemoveOutliers { get; set; } = false;

		public AbstractBoxplotBuilder(bool logY = false)
			: base(false, logY)
		{
		}

		public void AddBoxToPlot(IEnumerable<double> data, string? xLabel = null, string legend = "")
		{
			PlotUtils.BoxWithAverage box = PlotUtils.GetPercentileBox(data);
			if (RemoveOutliers)
			{
				var iqr = box.Box.BoxMax - box.Box.BoxMin;
				data = data.Where(d => d >= box.Box.BoxMin - (1.5 * iqr) && d <= box.Box.BoxMax + (1.5 * iqr));
				box = PlotUtils.GetPercentileBox(data);
			}
			box.Legend = legend;
			_boxes.Add(box);
			_labels[box.Box] = xLabel;
		}
		public void AddBoxToPlot(int pos, IEnumerable<double> data, string? xLabel = null, string legend = "")
		{
			_usedPositions.Add(pos);
			string l = xLabel ?? $"{PlotUtils.NumericLabeling(pos)}";
			AddBoxToPlot(data, l, legend);
			_boxes[^1].Box.Position = pos;
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			if (LogY)
			{

				GetMinMaxDispalyedValues(out double min, out double max);
				_yGenerator = new LogTickGenerator(min, max)
				{
					LogBase = LogBaseY
				};
				_plt.Axes.Left.TickGenerator = _yGenerator;
			}

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

				DrawBox(pos, box);
				if (DisplayMaxOnTop)
					DisplayMax(pos, box);
				pos = 1;
			}

			BuildXAxis();


			_plt.Legend.IsVisible = true;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 13f;
			if (LegendAlignment != null) _plt.Legend.Alignment = LegendAlignment.Value;

			_plt.Grid.MajorLineWidth = 2;
			_plt.Grid.MajorLineColor = Colors.Gray;
			_plt.Grid.XAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.YAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.MinorLineWidth = 2;
			_plt.Grid.MinorLineColor = Colors.LightGray;
			_plt.Grid.XAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.YAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
			_plt.Grid.IsVisible = true;


			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 20f;

			_plt.XLabel(xLabel);
			_plt.YLabel(yLabel);

			int width = Math.Max((int)_plt.Axes.GetLimits().Right * 15, 800);

			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, width, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, width, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, width, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		protected abstract void DisplayMax(int pos, PlotUtils.BoxWithAverage box);

		private void BuildXAxis()
		{
			double[] xs = _boxes.OrderBy(b => b.Box.Position).Select(b => b.Box.Position).ToArray();
			string[] xLabels = _boxes.OrderBy(b => b.Box.Position).Select(GetBoxLabel).ToArray();
			_plt.Axes.Bottom.TickGenerator = new NumericManual(xs, xLabels);
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;

			if (RotateAxis)
			{
				ITickGenerator tickGenerator = _plt.Axes.Bottom.TickGenerator;
				_plt.Axes.Remove(_plt.Axes.Bottom);
				RotatedLabelAdaptableAxis axis = new(tickGenerator);
				_plt.Axes.AddBottomAxis(axis);
				_plt.PlottableList.ToList().ForEach(s => s.Axes.XAxis = axis);
				_plt.Axes.Bottom.TickLabelStyle.Rotation = -45;
				_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperRight;
			}
		}

		protected abstract void DrawBox(int pos, PlotUtils.BoxWithAverage box, Color? color = null);
		protected abstract void GetMinMaxDispalyedValues(out double min, out double max);

		protected string GetBoxLabel(PlotUtils.BoxWithAverage b)
		{
			return _labels[b.Box] ?? $"{PlotUtils.NumericLabeling(b.Box.Position)}";
		}
	}
}
