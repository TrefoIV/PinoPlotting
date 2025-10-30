using MathNet.Numerics;
using ScottPlot;

namespace MyPlotting.TimelinePlots
{
	public class RegressionNumericalTimelinePlotBuilder : YAxisBrakeNumericalTimelinePlotBuilder
	{

		public int PolynomialDegree { get; set; } = 2;
		public bool ExponentialRegression { get; set; } = false;

		public RegressionNumericalTimelinePlotBuilder(bool logX, bool logY, bool drawBoxes = true) : base(logX, logY, drawBoxes)
		{
		}


		protected override void PlotAllTimelines()
		{
			for (int i = 0; i < _timelines.Count; i++)
			{
				(IEnumerable<(DateTime, PlotUtils.BoxWithAverage)> timeline, string label, ScottPlot.Color color) = _timelines[i];
				_timelines[i] = (timeline, label, color.WithAlpha(0.4f));
			}
			base.PlotAllTimelines();

			//Adesso, per ogni timeline aggiungi la sua regression
			foreach ((IEnumerable<(DateTime, PlotUtils.BoxWithAverage)> timeline, string label, ScottPlot.Color color) in _timelines)
			{
				(double[] x, double[] regressionLine) = ExponentialRegression ? ComputeExponentialRegression(timeline, label) : ComputeTimelineRegression(timeline, label);

				var showColor = Colors.Black;// color.WithAlpha(1f).Darken(0.2);

				var reg = _plt.Add.ScatterLine(x, regressionLine, showColor);
				reg.LineStyle.Pattern = LinePattern.Dashed;
				reg.LineStyle.Width *= 2;
			}
		}

		private (double[] x, double[] p) ComputeTimelineRegression(IEnumerable<(DateTime, PlotUtils.BoxWithAverage)> timeline, string label)
		{
			double[] x = timeline.Select(x => (double)DatePositions[x.Item1]).ToArray();
			double[] y = timeline.Select(x => x.Item2.Average).ToArray();

			if (PolynomialDegree < 0)
			{
				throw new InvalidOperationException("Cannot make a polynomial fit with negative degree");
			}

			// Fit polynomial (degree 2)
			double[] pCoefficients = Fit.Polynomial(x, y, PolynomialDegree);

			Console.Write(label + ": ");
			for (int i = 0; i < pCoefficients.Length; i++)
			{
				Console.Write($"{pCoefficients[i]}*x^{i} + ");
			}
			Console.WriteLine();

			//p sono i coefficienti => valutali per ogni x?
			for (int i = 0; i < x.Length; i++)
			{
				y[i] = pCoefficients[0];
				for (int degree = 1; degree <= PolynomialDegree; degree++)
				{
					y[i] += pCoefficients[degree] * Math.Pow(x[i], degree);
				}
			}
			return (x, y);
		}

		private (double[] x, double[] y) ComputeExponentialRegression(IEnumerable<(DateTime, PlotUtils.BoxWithAverage)> timeline, string label)
		{
			double[] x = timeline.Where(x => x.Item2.Average > 0).Select(x => (double)DatePositions[x.Item1]).ToArray();
			double[] y = timeline.Where(x => x.Item2.Average > 0).Select(x => x.Item2.Average).ToArray();
			// Fit exponential
			//y = a * exp(b * x)
			//ln(y) = ln(a) + b * x
			//fitting ln(y) = A + B * x
			double[] logY = y.Select(v => Math.Log2(v)).ToArray();
			double[] pCoefficients = Fit.Polynomial(x, logY, 1); //p[0] = A, p[1] = B
			Console.WriteLine($"{label} Exponential fit: y = {Math.Exp(pCoefficients[0])} * 2^({pCoefficients[1]} * x)");
			//p sono i coefficienti => valutali per ogni x?
			for (int i = 0; i < x.Length; i++)
			{
				y[i] = Math.Pow(2, pCoefficients[0]) * Math.Pow(2, pCoefficients[1] * x[i]);
			}
			return (x, y);
		}

	}
}
