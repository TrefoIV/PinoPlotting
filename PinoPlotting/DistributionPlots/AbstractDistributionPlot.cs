using MyPlotting.Axis;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

namespace MyPlotting.DistributionPlots
{
	public abstract class AbstractDistributionPlot : AbstractPlot
	{
		protected AbstractDistributionPlot(bool logX, bool logY) : base(logX, logY)
		{
		}

		public bool RotateAxis { get; set; } = false;
		public Func<double, string> XLabelFormatter { get; set; } = PlotUtils.NumericLabeling;


		protected void BuildXAxis(double? max = null)
		{


			if (LogX)
			{
				_xGenerator ??= new LogTickGenerator(1, 1) { LogBase = LogBaseX, LabelFormatter = XLabelFormatter };
				_plt.Axes.Bottom.TickGenerator = _xGenerator;
				(double bttm, double top) = _xGenerator.GetLimits();
				_plt.Axes.SetLimitsX(bttm, top);
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
					xTicks.Select(n => XLabelFormatter(n)).ToArray()
				);
			}

			if (RotateAxis)
			{
				ITickGenerator tickGenerator = _plt.Axes.Bottom.TickGenerator;
				_plt.Axes.Remove(_plt.Axes.Bottom);
				RotatedLabelAdaptableAxis axis = new(tickGenerator);
				_plt.Axes.AddBottomAxis(axis);
				_plt.PlottableList.ToList().ForEach(s => s.Axes.XAxis = axis);
			}
		}
	}
}
