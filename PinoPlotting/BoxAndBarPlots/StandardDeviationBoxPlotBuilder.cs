using ScottPlot;

namespace MyPlotting.BoxAndBarPlots
{
	public class StandardDeviationBoxPlotBuilder : AbstractBoxplotBuilder
	{

		public StandardDeviationBoxPlotBuilder(bool logY = false) : base(logY) { }

		protected override void DisplayMax(int pos, PlotUtils.BoxWithAverage box)
		{
			double maxDrawn = box.Average + box.StandardDeviation;
			if (LogY)
			{
				maxDrawn = _yGenerator!.Log(maxDrawn);
			}
			double max = box.Max;
			var text = _plt.Add.Text(PlotUtils.NumericLabeling(max), pos, maxDrawn + 0.5);
			text.LabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 14f;
			text.LabelAlignment = Alignment.LowerCenter;

		}

		protected override void DrawBox(int pos, PlotUtils.BoxWithAverage box, Color? color = null)
		{
			double avg = box.Average;
			double minDrawn = avg - box.StandardDeviation;
			double maxDrawn = avg + box.StandardDeviation;

			if (LogY)
			{
				avg = _yGenerator!.Log(avg);
				minDrawn = _yGenerator!.Log(minDrawn);
				maxDrawn = _yGenerator!.Log(maxDrawn);
			}

			var line = _plt.Add.Line(pos, minDrawn, pos, maxDrawn);
			line.Color = color ?? Colors.Blue;
			var point = _plt.Add.Scatter(pos, avg);
			point.MarkerShape = MarkerShape.OpenSquare;
			//point.MarkerSize = 12;
			point.Color = color ?? Colors.Blue;

			line = _plt.Add.Line(pos - 0.25, minDrawn, pos + 0.25, minDrawn);
			line.Color = color ?? Colors.Blue;
			line = _plt.Add.Line(pos - 0.25, maxDrawn, pos + 0.25, maxDrawn);
			line.Color = color ?? Colors.Blue;


		}

		protected override void GetMinMaxDispalyedValues(out double min, out double max)
		{
			min = LogY ? _boxes.Select(b => b.Average - b.StandardDeviation).Where(b => b > 0).DefaultIfEmpty(-1).Min() : _boxes.Min(b => b.Average - b.StandardDeviation);
			max = LogY ? _boxes.Select(b => b.Average + b.StandardDeviation).Where(b => b > 0).DefaultIfEmpty(-1).Max() : _boxes.Max(b => b.Average + b.StandardDeviation);
		}
	}
}
