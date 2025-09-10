using MyPlotting.TickGenerators;
using ScottPlot;
using ScottPlot.TickGenerators;
using System.Globalization;

namespace MyPlotting
{
	public abstract class TimelinePlotBuilder : AbstractPlot
	{
		public enum DateTimeLabelingStrategy
		{
			WeeklyDay,
			Montly,
			FullDate
		}
		public DateTimeLabelingStrategy LabelinStrategy { get; set; }
		public bool Squeeze { get; set; }


		protected LogTickGenerator? _logTickGen = null;
		protected SortedSet<DateTime> _allDates = new();

		public TimelinePlotBuilder(bool logX, bool logY) : base(logX, logY)
		{
		}

		protected void AddTimelineDates(IEnumerable<DateTime> dates)
		{
			foreach (var date in dates) _allDates.Add(date);
		}

		protected abstract void PlotAllTimelines();

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{

			PlotAllTimelines();

			double[] xs = Enumerable.Range(1, _allDates.Count).Select(x => (double)x).ToArray();
			string[] xlabels = _allDates.Select(x =>
			{
				return LabelinStrategy switch
				{
					DateTimeLabelingStrategy.WeeklyDay => DayDateLabeling(x),
					DateTimeLabelingStrategy.Montly => MonthDateLabeling(x),
					DateTimeLabelingStrategy.FullDate => FullDateLabeling(x),
					_ => "Label error"
				};
			}).ToArray();

			_plt.Axes.Bottom.TickGenerator = new NumericManual(xs, xlabels);
			_plt.Axes.Bottom.TickLabelStyle.FontSize *= 0.7f;
			_plt.Axes.Bottom.TickLabelStyle.Rotation = 90;
			_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;

			if (LogY) _plt.Axes.Left.TickGenerator = _logTickGen ?? new(0, 1);
			else _plt.Axes.Left.TickGenerator = new NumericAutomatic() { LabelFormatter = PlotUtils.NumericLabeling };

			_plt.Grid.MinorLineWidth = 0.5f;
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Axes.Bottom.Label.Text = xLabel;
			_plt.Layout.Fixed(new PixelPadding(top: 10, left: 75, right: 75, bottom: 105));
			int xSize = Squeeze ? 800 : Math.Max(800, xs.Length * 10);
			_plt.Save(outFile.FullName + ".png", xSize, 600);
		}

		protected static string DayDateLabeling(DateTime date)
		{
			return date.DayOfWeek == DayOfWeek.Monday ? date.ToString("d") : $"{date.DayOfWeek.ToString()[0]}";
		}
		protected static string MonthDateLabeling(DateTime date)
		{
			return date.ToString("MMM/yyyy", CultureInfo.InvariantCulture);
		}
		protected static string FullDateLabeling(DateTime date)
		{
			return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
		}
	}
}
