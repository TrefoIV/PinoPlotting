
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
		public IColormap Colormap { get; set; } = new Solar();
		public bool ScaleColormap { get; set; } = false;

		private int _currentRow = 0;
		private (TRow, TCol, double?, DisplayItem)[][] _matrix;
		private double? _minValue = null;
		private double? _maxValue = null;

		public CorrelationMatrixPlotBuilder(int rows, int cols) : base(false, false)
		{
			Rows = rows;
			Cols = cols;
			_matrix = new (TRow, TCol, double?, DisplayItem)[rows][];
			_currentRow = 0;

			var restrictedColors = Colormap.GetColors(256, minFraction: 0.1, maxFraction: 0.9);
			Colormap = new ScottPlot.Colormaps.CustomInterpolated(restrictedColors.Reverse().ToArray());

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
			List<string> displayedRowsLabels = new();
			int displayedRows = 0;
			for (int cp_index = 1; cp_index <= Rows; cp_index++)
			{
				if (_matrix[cp_index - 1] == null) throw new Exception("Not enough rows added");
				if (_matrix[cp_index - 1].Length != Cols) throw new Exception("Not enough columns added");
				if (_matrix[cp_index - 1].All(rowElem => rowElem.Item4 != DisplayItem.Value))
				{
					//The row has not any meaningful value; don't plot it and go to the next row
					continue;
				}
				//Add the label for the row
				displayedRowsLabels.Add(_matrix[cp_index - 1][0].Item1.ToString() ?? "");
				//Display the row
				displayedRows++;
				for (int yearIndex = 1; yearIndex <= Cols; yearIndex++)
				{
					DisplayItem display = _matrix[cp_index - 1][yearIndex - 1].Item4;
					switch (display)
					{
						case DisplayItem.Value:
							DisplayValue(displayedRows, yearIndex, _matrix[cp_index - 1][yearIndex - 1].Item3.Value);
							break;
						case DisplayItem.FirstSignalEmpty:
							{
								DrawCross(displayedRows, yearIndex, Colors.Blue);
								break;
							}
						case DisplayItem.SecondSignalEmpty:
							{
								DrawCross(displayedRows, yearIndex, Colors.Gray);
								break;
							}
						case DisplayItem.NotEnoughData:
							{
								DrawCross(displayedRows, yearIndex, Colors.Red);
								break;
							}
						case DisplayItem.NoData:
							{
								DrawCross(displayedRows, yearIndex, Colors.Black);
								break;
							}
					}
				}
			}

			if (_minValue != null && _maxValue != null)
			{
				ColormapLegend colormapLegend = new(Colormap, new ScottPlot.Range(_minValue.Value, _maxValue.Value));
				if (!ScaleColormap && _maxValue.Value < 1)
				{
					colormapLegend.ManualRange = new ScottPlot.Range(0, 1);
				}
				var colorLgd = _plt.Add.ColorBar(colormapLegend);
				colorLgd.Axis.TickGenerator = new NumericAutomatic()
				{
					LabelFormatter = x => $"{PlotUtils.NumericLabeling(x)}"
				};
			}



			double[] xTicks = Enumerable.Range(1, Cols).Select(x => (double)x).ToArray();
			double[] yTicks = Enumerable.Range(1, displayedRows).Select(x => (double)x).ToArray();
			string[] xTickLabels = _matrix[0].Select(x => x.Item2.ToString() ?? "").ToArray();
			string[] yTickLabels = displayedRowsLabels.ToArray();

			_plt.Axes.Bottom.TickGenerator = new NumericManual(xTicks, xTickLabels);
			_plt.Axes.Bottom.Label.Text = xLabel;
			_plt.Axes.Left.TickGenerator = new NumericManual(yTicks, yTickLabels);
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Axes.SetLimitsY(0, displayedRows + 1);
			int width = Math.Max(800, Cols * 15);
			int height = Math.Max(600, displayedRows * 15);
			if (PlottingConstants.ImageFormat.EndsWith(".png"))
				_plt.SavePng(outFile.FullName + ".png", width, height);
			else if (PlottingConstants.ImageFormat.EndsWith(".svg"))
				_plt.SaveSvg(outFile.FullName + ".svg", width, height);
		}

		private void DisplayValue(int cp_index, int x_coord, double corr)
		{
			var square = new TextRectangle((x_coord + 1) - 0.4, (x_coord + 1) + 0.4, cp_index - 0.4, cp_index + 0.4, PlotUtils.NumericLabeling(corr));
			square.LineColor = Colors.Transparent;
			if (_maxValue == null || _minValue == null)
			{
				square.FillColor = Colors.Gray;
				return;
			}
			if (_maxValue > 1 || _minValue < 0 || (ScaleColormap && _maxValue < 1))
			{
				//Scala i valori tra 0 e 1, se sono più grandi di 1, oppure se sono molto piccoli e vuoi scalarli
				corr = (corr - _minValue.Value) / (_maxValue.Value - _minValue.Value);
			}

			//Invert the colormap so that dark colors correspond to high values
			square.FillColor = Colormap.GetColor(corr);
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
			if (!_maxValue.HasValue || (_maxValue.Value < value.Value)) _maxValue = value.Value;
			if (!_minValue.HasValue || (_minValue.Value > value.Value)) _minValue = value.Value;
		}
	}
}
