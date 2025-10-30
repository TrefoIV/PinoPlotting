using MyPlotting.Axis;
using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class DateSegmentDurationCDFPlotBuilder : CDFPLotBuilder
	{
		public DateSegmentDurationCDFPlotBuilder(bool logX = false, bool logY = false) : base(logX, logY) { }

		public bool UseRotation { get; set; } = false;

		public void AddCDFToPlot(IEnumerable<TimeSpan> segments, string label = "", int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
		{
			AddCDFToPlot(segments.Select(x => x.TotalSeconds), label, steps, color);
		}

		public void SavePlot(FileInfo outFile, TimeSpan? maxValue, string xLabel = "", string yLabel = "")
		{
			BuildXAxis(maxValue);
			FinalizeSettings(xLabel, yLabel);

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

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			SavePlot(outFile, null, xLabel, yLabel);
		}

		private void BuildXAxis(TimeSpan? maxValue = null)
		{
			if (UseRotation)
			{
				_plt.Axes.Remove(_plt.Axes.Bottom);
				RotatedLabelAdaptableAxis axis = new(new NumericAutomatic());
				_plt.Axes.AddBottomAxis(axis);
				_plt.PlottableList.ForEach(p => p.Axes.XAxis = axis);
				_plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
				_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
			}
			if (LogX)
			{
				_xGenerator ??= new LogTickGenerator(0, 0) { LogBase = LogBaseX, IsTimeSpan = true };
				if (maxValue.HasValue)
				{
					_xGenerator.Max = maxValue.Value.TotalSeconds;

				}
				_xGenerator.IsTimeSpan = true;
				_plt.Axes.Bottom.TickGenerator = _xGenerator;
				(double bttm, double top) = _xGenerator.GetLimits();
				_plt.Axes.SetLimitsX(bttm, top);
			}
			else
			{
				_plt.Axes.Bottom.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = PlotUtils.SpanLabeling
				};
			}




		}


	}
}
