using ScottPlot;

namespace MyPlotting.BoxAndBarPlots
{
	public class PercentileBoxPlotBuilder : StandardDeviationBoxPlotBuilder
	{

		public bool ShowAverage { get; set; } = true;
		public bool ShowVariance { get; set; } = true;

		public PercentileBoxPlotBuilder(bool logY = false) : base(logY)
		{
		}

		protected override void DisplayMax(int pos, PlotUtils.BoxWithAverage box)
		{
			double maxDrawn = box.Max;
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
			Box toDraw = box.Box;
			if (LogY)
			{
				toDraw = new Box
				{

				};
				toDraw.BoxMax = _yGenerator!.Log(box.Box.BoxMax);
				toDraw.BoxMin = _yGenerator.Log(box.Box.BoxMin);
				toDraw.BoxMiddle = box.Box.BoxMiddle != null ? _yGenerator.Log(box.Box.BoxMiddle.Value) : null;
				toDraw.WhiskerMax = box.Box.WhiskerMax != null ? _yGenerator.Log(box.Box.WhiskerMax.Value) : null;
				toDraw.WhiskerMin = box.Box.WhiskerMin != null ? _yGenerator.Log(box.Box.WhiskerMin.Value) : null;
				toDraw.Position = box.Box.Position;
			}

			var b = _plt.Add.Box(toDraw);
			b.LegendText = box.Legend != "" ? $"{GetBoxLabel(box)} - {box.Legend}" : "";


			if (ShowVariance)
			{
				base.DrawBox(pos, box, Colors.Black);
			}
			else if (ShowAverage)
			{
				double posY = LogY ? _yGenerator!.Log(box.Average) : box.Average;
				var s = _plt.Add.Scatter(pos, posY, color: Colors.Black);
				s.MarkerShape = MarkerShape.OpenSquare;
			}
		}

		protected override void GetMinMaxDispalyedValues(out double min, out double max)
		{
			min = LogY ? _boxes.Select(GetOtherIfMinZero).Where(b => b > 0).DefaultIfEmpty(-1).Min() : _boxes.Min(b => b.Min);
			max = LogY ? _boxes.Select(b => b.Max).Where(b => b > 0).DefaultIfEmpty(-1).Max() : _boxes.Max(b => b.Max);
		}

		private double GetOtherIfMinZero(PlotUtils.BoxWithAverage box)
		{
			if (box.Min > 0) return box.Min;
			if (box.Box.BoxMin > 0) return box.Box.BoxMin;
			if (box.Box.BoxMiddle > 0) return box.Box.BoxMiddle.Value;
			return 0;
		}
	}
}
