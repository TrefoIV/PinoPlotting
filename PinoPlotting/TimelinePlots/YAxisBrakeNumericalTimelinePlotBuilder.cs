
using MyPlotting.Axis;
using MyPlotting.TickGenerators;
using ScottPlot.TickGenerators;

namespace MyPlotting.TimelinePlots
{
	public class YAxisBrakeNumericalTimelinePlotBuilder : NumericalTimelinePlotBuilder
	{

		public ScottPlot.Range? YLimits { get; set; } = null;

		public YAxisBrakeNumericalTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true) : base(logX, logY, drawBoxes)
		{
		}

		protected override void BuilYAxis()
		{
			yLabelFormatter ??= PlotUtils.NumericLabeling;
			base.BuilYAxis();
			if (YLimits != null)
			{
				if (LogY)
				{
					_yGenerator ??= new(1, 10);
					YLimits = new ScottPlot.Range(_yGenerator.Log(Math.Max(YLimits.Value.Min, 0)), _yGenerator.Log(Math.Max(YLimits.Value.Max, 0)));
				}
				_plt.Axes.SetLimitsY(YLimits.Value.Min, YLimits.Value.Max);
			}

			return;

			_plt.Axes.Remove(_plt.Axes.Left);

			List<double> allYValues = _timelines.SelectMany(tm => tm.Item1.Select(p => p.Item2.Average)).ToList();

			(double breakStart, double breakEnd) = PlotUtils.FindAxisBreak(allYValues);

			Console.WriteLine($"Break range {breakStart} -- {breakEnd}");

			BrokenAxisTickGenerator tickGenerator = new(breakStart, breakEnd, new NumericAutomatic()
			{
				LabelFormatter = yLabelFormatter
			});
			AxisWithBreakRange axis = new(breakStart, breakEnd)
			{
				TickGenerator = tickGenerator
			};

			_plt.Axes.AddLeftAxis(axis);

			_plt.Axes.DefaultGrid.YAxis = axis;
			foreach (var plottable in _plt.GetPlottables())
			{
				plottable.Axes.YAxis = axis;
			}
		}
	}
}
