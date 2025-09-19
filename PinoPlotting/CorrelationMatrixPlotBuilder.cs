
using MyPlotting.CustomPlottable;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class CorrelationMatrixPlotBuilder<TRow, TCol> : AbstractPlot where TRow : notnull where TCol : notnull
	{

		private enum DisplayItem
		{
			Value,
			FirstSignalEmpty,
			SecondSignalEmpty,
			NotEnoughData,
			NoData
		}

		public int Rows { get; private set; }
		public int Cols { get; private set; }
		public bool DataThresholding { get; set; } = false;
		public int WhatToCompute { get; set; } = 0; // 0=correlation, 1=percentage of sums 
		public bool ScaleColormap { get; set; } = false;

		private int _currentRow = 0;
		private (TRow, TCol, double?, DisplayItem)[][] _matrix;
		private double? minValue = null;
		private double? maxValue = null;

		public CorrelationMatrixPlotBuilder(int rows, int cols) : base(false, false)
		{
			Rows = rows;
			Cols = cols;
			_matrix = new (TRow, TCol, double?, DisplayItem)[rows][];
			_currentRow = 0;
		}

		public void AddSignalsPairsTimeline(TRow cp, IEnumerable<(TCol, IEnumerable<(int, int)>)> correlationSignals)
		{
			if (_currentRow >= Rows) throw new Exception("Too many rows added");
			_matrix[_currentRow] = new (TRow, TCol, double?, DisplayItem)[Cols];
			int j = 0;
			foreach ((TCol col, IEnumerable<(int, int)> signals) in correlationSignals)
			{
				if (j >= Cols) throw new Exception("Too many columns added");

				(double? value, DisplayItem display) v;
				if (WhatToCompute == 0) v = ComputeCorrelation(signals);
				else v = ComputePercentageOfSums(signals);
				if (v.display == DisplayItem.Value) ChangeMinMax(v.value);
				_matrix[_currentRow][j] = (cp, col, v.value, v.display);

				j++;
			}
			_currentRow++;
		}

		private (double? value, DisplayItem display) ComputePercentageOfSums(IEnumerable<(int, int)> signals)
		{
			(double? value, DisplayItem display) = CheckEnoughData(signals);
			if (display != DisplayItem.Value)
			{
				return (value, display);
			}
			int sum1 = signals.Sum(x => x.Item1);
			int sum2 = signals.Sum(x => x.Item2);
			return ((double)sum1 / sum2, DisplayItem.Value);
		}

		private (double? value, DisplayItem display) CheckEnoughData(IEnumerable<(int, int)> signals)
		{
			if (!signals.Any()) return (null, DisplayItem.NoData);
			bool firstAllZero = signals.All(x => x.Item1 == 0);
			bool secondAllZero = signals.All(x => x.Item2 == 0);
			if (firstAllZero && secondAllZero) return (null, DisplayItem.NoData);
			if (secondAllZero) return (null, DisplayItem.SecondSignalEmpty);
			if (firstAllZero) return (null, DisplayItem.FirstSignalEmpty);
			if (DataThresholding && (signals.Where(x => x.Item2 > 0).Count() < signals.Count() / 2)) return (null, DisplayItem.NotEnoughData);

			return (null, DisplayItem.Value);
		}

		private (double? value, DisplayItem display) ComputeCorrelation(IEnumerable<(int, int)> signals)
		{
			(double? value, DisplayItem display) = CheckEnoughData(signals);
			if (display != DisplayItem.Value)
			{
				return (value, display);
			}

			double max1 = signals.Max(x => x.Item1);
			double norm1 = max1 * Math.Sqrt(signals.Sum(x => (x.Item1 / max1) * (x.Item1 / max1)));

			double max2 = signals.Max(x => x.Item2);
			double norm2 = max2 * Math.Sqrt(signals.Sum(x => (x.Item2 / max2) * (x.Item2 / max2)));

			double productSum = signals.Sum(x => (x.Item1 / norm1) * (x.Item2 / norm2));
			double correlation = Math.Sqrt(Math.Abs(productSum));
			return (correlation, DisplayItem.Value);
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			Solar colormap = new();
			double[] xTicks = Enumerable.Range(1, Cols).Select(x => (double)x).ToArray();
			double[] yTicks = Enumerable.Range(1, Rows).Select(x => (double)x).ToArray();
			string[] xTickLabels = _matrix[0].Select(x => x.Item2.ToString() ?? "").ToArray();
			string[] yTickLabels = _matrix.Select(x => x[0].Item1.ToString() ?? "").ToArray();

			for (int cp_index = 1; cp_index <= Rows; cp_index++)
			{
				if (_matrix[cp_index - 1] == null) throw new Exception("Not enough rows added");
				if (_matrix[cp_index - 1].Length != Cols) throw new Exception("Not enough columns added");

				for (int yearIndex = 1; yearIndex <= Cols; yearIndex++)
				{
					DisplayItem display = _matrix[cp_index - 1][yearIndex - 1].Item4;
					switch (display)
					{
						case DisplayItem.Value:
							DisplayValue(colormap, cp_index, yearIndex);
							break;
						case DisplayItem.FirstSignalEmpty:
							{
								DrawCross(cp_index, yearIndex, Colors.Blue);
								break;
							}
						case DisplayItem.SecondSignalEmpty:
							{
								DrawCross(cp_index, yearIndex, Colors.Gray);
								break;
							}
						case DisplayItem.NotEnoughData:
							{
								DrawCross(cp_index, yearIndex, Colors.Red);
								break;
							}
						case DisplayItem.NoData:
							{
								DrawCross(cp_index, yearIndex, Colors.Black);
								break;
							}
					}
				}
			}

			_plt.Axes.Bottom.TickGenerator = new NumericManual(xTicks, xTickLabels);
			_plt.Axes.Bottom.Label.Text = xLabel;
			_plt.Axes.Left.TickGenerator = new NumericManual(yTicks, yTickLabels);
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Axes.SetLimitsY(0, Rows + 1);
			int width = Math.Max(800, Cols * 15);
			int height = Math.Max(600, Rows * 15);
			_plt.SavePng(outFile.FullName + ".png", width, height);
		}

		private void DisplayValue(Solar colormap, int cp_index, int yearIndex)
		{
			double corr = _matrix[cp_index - 1][yearIndex - 1].Item3 ?? 0;
			var square = new TextRectangle((yearIndex + 1) - 0.4, (yearIndex + 1) + 0.4, cp_index - 0.4, cp_index + 0.4, PlotUtils.NumericLabeling(corr));
			square.LineColor = Colors.Transparent;
			if (maxValue == null)
			{
				square.FillColor = Colors.Gray;
				return;
			}
			if (maxValue > 1)
			{
				//Mappa il valore di correlazione
				corr /= maxValue.Value;
			}
			if (ScaleColormap && maxValue < 1)
			{
				//Devo mappare il max in 0.9, ed il min a 0
				corr = (corr / maxValue.Value) * 0.9;
			}
			//Invert the colormap so that dark colors correspond to high values
			square.FillColor = colormap.GetColor(1 - corr);
			_plt.Add.Plottable(square);
			_plt.Add.Plottable(square.Text);
		}

		private void DrawCross(int cp_index, int yearIndex, Color color)
		{
			var line = _plt.Add.Line((yearIndex + 1) - 0.4, cp_index - 0.4, (yearIndex + 1) + 0.4, cp_index + 0.4);
			line.Color = color;
			line = _plt.Add.Line((yearIndex + 1) - 0.4, cp_index + 0.4, (yearIndex + 1) + 0.4, cp_index - 0.4);
			line.Color = color;
		}

		private void ChangeMinMax(double? value)
		{
			if (!value.HasValue) return;
			if (!maxValue.HasValue || (maxValue.Value < value.Value)) maxValue = value.Value;
			if (!minValue.HasValue || (minValue.Value > value.Value)) minValue = value.Value;
		}
	}
}
