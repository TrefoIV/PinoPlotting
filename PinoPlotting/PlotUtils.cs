using AdvancedDataStructures.Extensions;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using System.Globalization;

namespace MyPlotting
{
	public static class PlotUtils
	{

		public static List<(double, double)> MakeDataGradinoPlot(List<(double, double)> cdf)
		{
			List<(double, double)> result = new();

			if (cdf.Count == 0) { return result; }
			if (cdf[0].Item1 != 0 || cdf[0].Item2 != 0)
			{
				result.Add((0, 0));
			}
			if (cdf[0].Item1 != 0)
			{
				result.Add((cdf[0].Item1, 0));
			}
			result.Add(cdf.First());
			for (int i = 1; i < cdf.Count; i++)
			{
				if (cdf[i].Item2 != cdf[i - 1].Item2)
				{
					result.Add((cdf[i].Item1, cdf[i - 1].Item2));
					result.Add(cdf[i]);
				}
			}
			result.Add(cdf.Last());
			result.Add((cdf.Last().Item1, 100));

			return result;
		}
		public static List<(int, double)> MakeDataGradinoPlot(List<(int, double)> cdf)
		{
			List<(int, double)> result = new();

			if (cdf.Count == 0) { return result; }
			if (cdf[0].Item1 != 0 || cdf[0].Item2 != 0)
			{
				result.Add((0, 0));
			}
			if (cdf[0].Item1 != 0)
			{
				result.Add((cdf[0].Item1, 0));
			}
			result.Add(cdf.First());
			for (int i = 1; i < cdf.Count; i++)
			{
				if (cdf[i].Item2 != cdf[i - 1].Item2)
				{
					result.Add((cdf[i].Item1, cdf[i - 1].Item2));
					result.Add(cdf[i]);
				}
			}
			result.Add(cdf.Last());
			result.Add((cdf.Last().Item1, 100));

			return result;
		}

		public class BoxWithAverage
		{
			public Box Box { get; set; }
			public double Average { get; set; }
			public double Variance { get; set; }
			public string Legend { get; set; }

			public double Min
			{
				get
				{
					if (Box.WhiskerMin != null) return Box.WhiskerMin.Value;
					return Box.BoxMin;
				}
			}
			public double Max
			{
				get
				{
					if (Box.WhiskerMax != null) return Box.WhiskerMax.Value;
					return Box.BoxMax;
				}
			}
		}

		public static BoxWithAverage GetPercentileBox(IEnumerable<int> data)
		{
			return GetPercentileBox(data.Select(x => (double)x));
		}
		public static BoxWithAverage GetPercentileBox(IEnumerable<double> data)
		{
			double[] ordered = data.Order().ToArray();
			double average = data.Any() ? data.Average() : 0;
			double variance = data.Any() ? data.Select(x => Math.Pow(x - average, 2)).Average() : 0;
			switch (ordered.Length)
			{
				case 0:
					return new BoxWithAverage() { Box = new Box() { BoxMax = 0, BoxMin = 0 }, Average = average, Variance = variance };
				case 1:
					return new BoxWithAverage() { Box = new Box() { BoxMin = ordered[0], BoxMax = ordered[0] }, Average = average, Variance = variance };
				case 2:
					return new BoxWithAverage() { Box = new Box() { BoxMin = ordered[0], BoxMax = ordered[1] }, Average = average, Variance = variance };
				case 3:
					return new BoxWithAverage() { Box = new Box() { BoxMin = ordered[0], BoxMax = ordered[2], BoxMiddle = ordered[1] }, Average = average, Variance = variance };
				case 4:
					return new BoxWithAverage() { Box = new Box() { WhiskerMin = ordered[0], BoxMin = ordered[1], BoxMax = ordered[2], WhiskerMax = ordered[3] }, Average = average, Variance = variance };
			}

			int median = ordered.Length / 2;
			int percentile = (ordered.Length - median) / 2;
			if (percentile < 0) percentile = 0;
			return new BoxWithAverage()
			{
				Box = new Box()
				{
					WhiskerMax = ordered[^1],
					BoxMax = ordered[median + percentile],
					BoxMiddle = ordered[median],
					BoxMin = ordered[percentile],
					WhiskerMin = ordered[0]
				},
				Average = average,
				Variance = variance
			};
		}

		public static string NumericLabeling(double n)
		{
			string result = "";

			if (n < 0)
			{
				result += "-";
				n *= -1;
			}
			if (n >= 1_000_000_000_000)
			{
				//Usa notazionescientifica
				result += n.ToString("E1", CultureInfo.InvariantCulture);
			}
			else if (n >= 1_000_000_000)
			{
				long k = (long)(n % 1_000_000_000 / 100_000_000);
				string ks = k == 0 ? "" : "." + k.ToString("D");
				result += $"{(long)(n / 1_000_000_000)}{ks}B";
			}
			else if (n >= 1_000_000)
			{
				long k = (long)(n % 1_000_000 / 100_000);
				string ks = k == 0 ? "" : "." + k.ToString("D");
				result += $"{(long)(n / 1_000_000)}{ks}M";
			}
			else if (n >= 1_000)
			{
				long cent = (long)(n % 1_000 / 100);
				string centS = cent == 0 ? "" : "." + cent.ToString("D");
				result += $"{(long)(n / 1_000)}{centS}K";
			}
			else if (n == 0)
			{
				return "0";
			}
			else if (n < 0.01)
			{
				result += n.ToString("E2", CultureInfo.InvariantCulture);
			}
			//Se n è intero, printalo intero
			else if (((int)n) == n)
			{
				result += ((int)n).ToString();
			}
			else
			{
				result += n.ToString("F2");
			}

			return result;
		}

		public static string PercentagesFormatter(double y)
		{
			return NumericLabeling(y) + "%";
		}

		public static string SpanLabeling(double x)
		{
			TimeSpan span = TimeSpan.FromSeconds(x);
			string label = "";
			int years = 0;
			if (span.Days > 365)
			{
				years = span.Days / 365;
				span -= TimeSpan.FromDays(years * 365);
			}
			if (years > 0) label += $"{years}y";
			if (span.Days > 0) label += $"{span.Days}d";
			if (years == 0 && span.Hours > 0) label += $"{span.Hours}h";
			if (years == 0 && span.Days == 0 && span.Minutes > 0) label += $"{span.Minutes}m";
			if (years == 0 && span.Days == 0 && span.Hours == 0 && span.Seconds > 0) label += $"{span.Seconds}s";
			return label;
		}

		public static void TripleBarChartPlot(FileInfo outFile, double[][] data, string[]? xTickLabels = null)
		{

			Bar[] bars = data[0].SelectMany((x, index) =>
			{
				return new Bar[] {
					new Bar() { Position = index + 1, ValueBase = 0, Value = x, FillColor = Colors.GreenYellow },
					new Bar() { Position = index + 1, ValueBase = x, Value = x + data[1][index], FillColor = Colors.Red },
					new Bar() { Position = index + 1, ValueBase = x + data[1][index], Value = x + data[1][index] + data[2][index], FillColor = Colors.Blue },
				};
			}).ToArray();

			double[] positions = data[0].Select((x, i) => (double)i + 1).ToArray();
			if (xTickLabels == null || positions.Length != xTickLabels.Length)
			{
				Console.WriteLine($"Tick len {positions.Length} different from tickLabel lenght {xTickLabels.Length} for plot {outFile.FullName}. Ignoring labels");
				xTickLabels = positions.Select(x => x.ToString()).ToArray();
			}
			using Plot plt = new();
			BarPlot bar = plt.Add.Bars(bars);

			plt.Axes.SetLimitsY(bottom: 0, top: bars.Select(x => x.Value).Max() + 15);
			plt.Legend = new(plt)
			{
				Alignment = Alignment.UpperLeft
			};


			plt.Axes.Bottom.TickGenerator = new NumericManual(positions, xTickLabels);
			plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
			plt.Axes.Left.TickGenerator = new NumericAutomatic()
			{
				IntegerTicksOnly = true
			};
			plt.Axes.Margins(bottom: 0);
			plt.Axes.Bottom.TickLabelStyle.OffsetY = -8;
			plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
			plt.Layout.Fixed(new PixelPadding(top: 10, right: 10, left: 50, bottom: 75));

			if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
				plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
				plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
			else
			{
				Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
			}
		}


		public static (double BreakStart, double BreakEnd) FindAxisBreak(List<double> values)
		{
			if (values == null || values.Count == 0)
				throw new ArgumentException("Values list must not be empty.");


			// Compute quartiles
			BoxWithAverage percentileBox = GetPercentileBox(values);
			double Q1 = percentileBox.Box.BoxMin;
			double Q3 = percentileBox.Box.BoxMax;
			double IQR = Q3 - Q1;

			double lowerCutoff = Q1 - 1.5 * IQR;
			double upperCutoff = Q3 + 1.5 * IQR;

			(double minVal, double maxVal) = values.MinMax();

			// Case 1: High outliers
			if (maxVal > upperCutoff)
			{
				double breakStart = values.Where(v => v <= upperCutoff).Max();
				return (breakStart, maxVal);
			}
			// Case 2: Low outliers
			else if (minVal < lowerCutoff && minVal >= 0)
			{
				double breakEnd = values.Where(v => v >= lowerCutoff).Min();
				return (minVal, breakEnd);
			}
			// Case 3: No outliers
			else
			{
				return (maxVal, maxVal); // break "on top", size ~0
			}
		}

		public static Color GetRandomColor()
		{
			byte r = (byte)Random.Shared.Next(32, 224);
			byte g = (byte)Random.Shared.Next(32, 224);
			byte b = (byte)Random.Shared.Next(32, 224);
			return new Color(r, g, b);
		}

		public static Color GetHashColor<T>(T obj)
		{
			int hash = obj.GetHashCode();
			byte r = (byte)((hash & 0xFF0000) >> 16);
			byte g = (byte)((hash & 0x00FF00) >> 8);
			byte b = (byte)((hash & 0x0000FF));
			return new Color(r, g, b);
		}
	}
}