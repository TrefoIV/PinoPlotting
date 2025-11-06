using ScottPlot;

namespace MyPlotting
{
	public class WithSpikesNumericalTimelinePlotBuilder : NumericalTimelinePlotBuilder
	{
		public double SpikeThresholdIQRFactor { get; set; } = 1.5;

		public WithSpikesNumericalTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true) : base(logX, logY, drawBoxes)
		{
		}

		protected override void PlotAllTimelines()
		{
			base.PlotAllTimelines();

			//Ora calcolo gli spikes
			IEnumerable<double> yvalues = _timelines.SelectMany(t => t.Item1.Select(x => LogY ? _yGenerator.Pow(x.Item2.Average) : x.Item2.Average));
			var box = PlotUtils.GetPercentileBox(yvalues);
			double iqr = box.Box.BoxMax - box.Box.BoxMin;
			double upperThreshold = box.Box.BoxMax + SpikeThresholdIQRFactor * iqr;
			double lowerThreshold = box.Box.BoxMin - SpikeThresholdIQRFactor * iqr;

			if (LogY)
			{
				upperThreshold = _yGenerator.Log(upperThreshold);
				lowerThreshold = _yGenerator.Log(lowerThreshold);
				box.Box.BoxMax = _yGenerator.Log(box.Box.BoxMax);
				box.Box.BoxMin = _yGenerator.Log(box.Box.BoxMin);
				if (box.Box.BoxMiddle != null) box.Box.BoxMiddle = _yGenerator.Log(box.Box.BoxMiddle.Value);
			}

			_plt.Add.HorizontalLine(upperThreshold, width: 0.5f, color: Colors.Red, pattern: LinePattern.DenselyDashed);
			_plt.Add.HorizontalLine(lowerThreshold, width: 0.5f, color: Colors.Red, pattern: LinePattern.DenselyDashed);
			_plt.Add.HorizontalLine(box.Box.BoxMax, width: 0.5f, color: Colors.Black, pattern: LinePattern.DenselyDashed);
			_plt.Add.HorizontalLine(box.Box.BoxMin, width: 0.5f, color: Colors.Black, pattern: LinePattern.DenselyDashed);
			if (box.Box.BoxMiddle != null) _plt.Add.HorizontalLine(box.Box.BoxMiddle.Value, width: 0.5f, color: Colors.Black, pattern: LinePattern.DenselyDashed);
		}
	}
}
