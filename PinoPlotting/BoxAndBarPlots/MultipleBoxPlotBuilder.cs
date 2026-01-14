
using AdvancedDataStructures.ConcatenatedList;
using AdvancedDataStructures.Extensions;
using MyPlotting.Axis;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.TickGenerators;

namespace MyPlotting
{
	public class MultipleBoxPlotBuilder : AbstractPlot
	{
		private int _classes;
		private string[]? _classLabels;

		private List<double[]> _barGroups = [];
		private List<string> _groupsLabels = [];
		private Color[] _colormap;

		private static Category20 palette = new();

		public MultipleBoxPlotBuilder(bool logY, int classes) : base(false, logY)
		{
			_classes = classes;
			_colormap = new Color[classes];
			for (int i = 0; i < _colormap.Length; i++)
				_colormap[i] = palette.GetColor(i % 20);
		}

		public void AddClassesLabels(string?[] labels)
		{
			if (labels.Length != _classes)
			{
				throw new ArgumentException($"The number of labels must match the number of classes. Expected {_classes}, got {labels.Length}");
			}
			_classLabels = labels.Select(l => l ?? "").ToArray();
		}

		public void AddBoxGroup(double[] data, string? groupLabel)
		{
			if (data.Length != _classes)
			{
				throw new ArgumentException($"Each bar group must have the same number of classes. Expected {_classes}, got {data.Length}");
			}
			_barGroups.Add(data);
			_groupsLabels.Add(groupLabel ?? "");
		}

		public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
		{
			GenerateXAxis();
			GenerateBars();
			GenerateLegend();
			_plt.Axes.Bottom.Label.Text = xLabel;
			_plt.Axes.Left.Label.Text = yLabel;
			_plt.Axes.Bottom.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Left.TickLabelStyle.FontSize = PlottingConstants.GlobalTicksLabelFontSize ?? 20f;
			_plt.Axes.Bottom.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 25f;
			_plt.Axes.Left.Label.FontSize = PlottingConstants.GlobalAxisLabelFontSize ?? 25f;
			_plt.Legend.FontSize = PlottingConstants.GlobalLegendFontSize ?? 13f;
			_plt.Legend.Alignment = LegendAlignment ?? Alignment.UpperRight;
			int xSize = Math.Max(1200, _groupsLabels.Count * Math.Max(_classes, 15));
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

		private void GenerateXAxis()
		{
			_plt.Axes.Remove(Edge.Bottom);

			Tick[] ticks = Enumerable.Range(0, _barGroups.Count).Select(i =>
			{
				int baseIndex = i * (_classes + 1) + 1;
				int lastIndex = baseIndex + _classes - 1;
				double pos = ((double)(lastIndex + baseIndex)) / 2;
				return new Tick(pos, _groupsLabels[i], true);
			}).ToArray();
			var tickGenerator = new NumericManual(ticks);
			RotatedLabelAdaptableAxis bottomAxis = new(tickGenerator);
			_plt.Axes.AddBottomAxis(bottomAxis);
			_plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
			_plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
		}

		private void GenerateLegend()
		{
			for (int i = 0; i < _classes; i++)
			{
				_plt.Legend.ManualItems.Add(new()
				{
					FillColor = _colormap[i],
					LabelText = _classLabels?[i] ?? $"Class {i}"
				});
			}
		}

		private void GenerateBars()
		{
			(double min, double max) = _barGroups.SelectMany(g => g).MinMax();
			if (LogY)
			{
				_yGenerator = new TickGenerators.LogTickGenerator(min, max) { LogBase = LogBaseY, ShowZero = true };
				foreach (var g in _barGroups)
				{
					for (int i = 0; i < g.Length; i++)
					{
						g[i] = _yGenerator.Log(i);
					}
				}
				_plt.Axes.Left.TickGenerator = _yGenerator;
			}

			ConcatenatedLinkedList<Bar> bars = [];

			//Assign x-coordinates and create bars;
			int x = 1;
			foreach (var g in _barGroups)
			{
				for (int i = 0; i < g.Length; i++)
				{
					bars.Add(new Bar()
					{
						Position = x,
						Value = g[i],
						ValueBase = 0,
						FillColor = _colormap[i]
					});
					x++;
				}
				//Leave a blank space between groups
				x++;
			}
			_plt.Add.Bars(bars.ToArray());
			if (!LogY)
			{
				_plt.Axes.SetLimitsY(0, (int)(max + max / 10));
			}
		}

	}
}
