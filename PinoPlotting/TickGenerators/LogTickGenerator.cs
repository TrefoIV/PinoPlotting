using ScottPlot;
using SkiaSharp;

namespace MyPlotting.TickGenerators
{
	public class LogTickGenerator : ITickGenerator
	{
		public double Min { get; set; }
		public double Max { get; set; }
		public bool IsTimeSpan { get; set; }

		public LogTickGenerator(double min, double max)
		{
			if (min <= 0 || max <= 0) throw new Exception($"Cannot create log axis with negative values Range: ({min}, {max})");
			Min = min;
			Max = max;
		}

		public int LogBase { get; set; } = 10;
		public bool NaturalLog { get; set; } = false;
		public bool ShowZero { get; set; } = true;

		public Tick[] Ticks { get; set; } = Array.Empty<Tick>();

		public int MaxTickCount { get; set; }

		public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, SKPaint paint, LabelStyle labelStyle)
		{
			if (Min <= 0 || Max <= 0) throw new Exception($"Cannot compute log axis with negative values Range: ({Min}, {Max})");

			//trova l'ordine di grandezza del min e max value
			int minOrder = Min > 0 ? (int)Math.Floor(Log(Min)) : throw new Exception("Taking log of negative value");
			int order = Max > 0 ? (int)Math.Ceiling(Log(Max)) : throw new Exception("Taking log of negative value");
			Ticks = ShowZero ? new Tick[] { Tick.Major(Log(0), "0") } : Array.Empty<Tick>();
			Ticks = Ticks.Concat(Enumerable.Range(minOrder, order - minOrder).SelectMany(o => Enumerable.Range(0, LogBase == 2 ? LogBase : LogBase - 1).Select(i =>
			{
				double b = Pow(o);
				if (LogBase == 2 && i == 1) b *= 0.75;
				double pos = Log(b + b * i);
				string label = CreateLabel(pos);
				return i == 0 ? Tick.Major(pos, label) : Tick.Minor(pos);
			})).Append(Tick.Major(order, CreateLabel(order)))).ToArray();
		}

		private string CreateLabel(double pos)
		{
			return IsTimeSpan ? PlotUtils.SpanLabeling(Pow(pos)) : PlotUtils.NumericLabeling(Pow(pos));
		}

		public double Pow(double order)
		{
			return NaturalLog ? Math.Pow(Math.E, order) : Math.Pow(LogBase, order);
		}

		public double Log(double value)
		{
			if (value == 0)
			{
				double minOrder = Math.Floor(Log(Min));
				return minOrder - 0.1;
			}
			if (NaturalLog) return Math.Log(value);
			if (LogBase == 2) return Math.Log2(value);
			if (LogBase == 10) return Math.Log10(value);
			return Math.Log10(value) / Math.Log10(LogBase);
		}
	}
}
