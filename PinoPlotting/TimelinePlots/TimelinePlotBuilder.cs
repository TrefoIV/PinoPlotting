using AdvancedDataStructures.ConcatenatedList;
using MyPlotting.Axis;
using MyPlotting.TimeUnits;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public abstract class TimelinePlotBuilder : AbstractPlot
	{
		public DateTimeIntervalUnit TimeUnit { get; set; } = DateTimeIntervalUnit.Second;
		public bool Squeeze { get; set; }
		public Func<double, string>? yLabelFormatter { get; set; } = null;

		protected SortedSet<DateTime> _allDates = new();

		private Dictionary<DateTime, int>? _allDatesToPos = null;

		protected Dictionary<DateTime, int> DatePositions
		{
			get
			{
				if (_allDatesToPos is null)
				{
					(var min, var max) = (_allDates.First(), _allDates.Last());
					int pos = 1;
					_allDatesToPos = new Dictionary<DateTime, int>();
					DateTime d = min;
					IEnumerator<DateTime> dateEnum = _allDates.Skip(1).GetEnumerator();

					while (d <= max)
					{
						_allDatesToPos[d] = pos++;
						if (TimeUnit == DateTimeIntervalUnit.None)
							d = dateEnum.MoveNext() ? dateEnum.Current : max.AddDays(1);
						else d = TimeUnit.GetTimeUnit().Next(d, 1);
					}
				}
				return _allDatesToPos;
			}
		}

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
			int xLen = BuildXaxis();

			BuilYAxis();


			if (LegendAlignment != null) _plt.Legend.Alignment = LegendAlignment.Value;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 25f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 25f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 13f;

			_plt.Axes.Bottom.TickLabelStyle.Bold = true;
			_plt.Axes.Left.TickLabelStyle.Bold = true;
			_plt.Legend.FontName = Fonts.Serif;

			_plt.Grid.MinorLineWidth = Squeeze ? 0f : 0.5f;
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Axes.Bottom.Label.Text = xLabel;
			int legendSize = 300;
			//_plt.Axes.Right.MinimumSize = TimeUnit.GetLabelingStrategy() == DateTimeLabelingStrategy.FullDate ? 75 : 0;
			//float leftPadding = _plt.Axes.Left.TickLabelStyle.FontSize * 2.5f;
			////float bttmPadding = _pl
			//_plt.Layout.Fixed(new PixelPadding(top: 10, left: leftPadding, right: 75, bottom: 105));
			int xSize = (Squeeze ? 800 : Math.Max(800, xLen * 10)) + legendSize + 75;
			//_plt.Layout.Fixed(new PixelPadding(top: null, right: legendSize + 25, left: 50, bottom: 105));
			//_plt.Legend.Orientation = Orientation.Horizontal;

			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				_plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, xSize, 800);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				_plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, xSize, 800);
			else if (PlottingConstants.ImageFormat.EndsWith(".pdf", StringComparison.InvariantCulture))
				SavePdf(outFile.FullName + PlottingConstants.ImageFormat, xSize, 800);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}

		protected virtual int BuildXaxis()
		{
			_plt.Axes.Remove(_plt.Axes.Bottom);

			bool rotateLabels = true;

			ITickGenerator tickGenerator = new NumericManual(GenerateTicks());
			RotatedLabelAdaptableAxis axis = new(tickGenerator);
			_plt.Axes.AddBottomAxis(axis);
			_plt.Axes.Bottom.TickGenerator = tickGenerator;
			_plt.Axes.Bottom.TickLabelStyle.Rotation = rotateLabels ? -45 : 0;
			_plt.Axes.Bottom.TickLabelStyle.Alignment = rotateLabels ? Alignment.UpperRight : Alignment.UpperCenter;
			_plt.Grid.XAxis = axis;

			return _allDates.Count;
		}

		protected virtual void BuilYAxis()
		{
			yLabelFormatter ??= PlotUtils.NumericLabeling;
			if (LogY)
			{
				_yGenerator ??= new(1, 1) { ShowZero = false };
				_yGenerator.LabelFormatter = yLabelFormatter;
				_plt.Axes.Left.TickGenerator = _yGenerator;
				(double bttm, double top) = _yGenerator.GetLimits();
				_plt.Axes.SetLimitsY(bttm, top);
			}
			else _plt.Axes.Left.TickGenerator = new NumericAutomatic() { LabelFormatter = yLabelFormatter };
		}

		private Tick[] GenerateTicks()
		{
			if (TimeUnit == DateTimeIntervalUnit.None)
			{
				return GenerateTicksOnNullTimeUnit();
			}

			DateTimeLabelingStrategy labelingStrategy = TimeUnit.GetLabelingStrategy() ?? DateTimeLabelingStrategy.FullDate;
			var labelFormatter = labelingStrategy.GetFormatFunc();
			var nextDateFunc = TimeUnit.GetTimeUnit();

			DateTime startDate = _allDates.First();
			DateTime endDate = _allDates.Last();
			int majorTickSpacing = TimeUnit.GetIdealMajorTickSpacing(new CoordinateRange(0, _allDates.Count));

			ConcatenatedLinkedList<Tick> ticks = new ConcatenatedLinkedList<Tick>();
			double tickPosition;
			DateTime tickDate = startDate;
			while (tickDate <= endDate)
			{
				tickPosition = DatePositions[tickDate];
				Tick major = new Tick(tickPosition, labelFormatter(tickDate), isMajor: true);
				ticks.Add(major);

				DateTime minorDate = nextDateFunc.Next(tickDate, 1);
				tickDate = nextDateFunc.Next(tickDate, majorTickSpacing);
				while (minorDate <= endDate && minorDate < tickDate)
				{
					tickPosition = DatePositions[minorDate];
					Tick minor = new(tickPosition, "", false);
					ticks.Add(minor);
					minorDate = nextDateFunc.Next(minorDate, 1);
				}
			}

			return ticks.ToArray();
		}

		private Tick[] GenerateTicksOnNullTimeUnit()
		{
			ConcatenatedLinkedList<Tick> ticks = new ConcatenatedLinkedList<Tick>();
			var labelFormatter = DateTimeLabelingStrategy.FullDate.GetFormatFunc();
			foreach (var date in _allDates)
			{
				double tickPosition = DatePositions[date];
				Tick tick = new Tick(tickPosition, labelFormatter(date), isMajor: true);
				ticks.Add(tick);
			}
			return ticks.ToArray();
		}
	}
}
