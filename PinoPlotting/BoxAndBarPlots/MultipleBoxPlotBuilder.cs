
using AdvancedDataStructures.ConcatenatedList;
using AdvancedDataStructures.Extensions;
using ScottPlot;
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

		public MultipleBoxPlotBuilder(bool logY, int classes) : base(false, logY)
		{
			_classes = classes;
			_colormap = new Color[classes];
			for (int i = 0; i < _colormap.Length; i++)
				_colormap[i] = _plt.Add.GetNextColor();
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
			GenerateBars();
			GenerateLegend();
			GenerateXAxis();
		}

		private void GenerateXAxis()
		{
			Tick[] ticks = Enumerable.Range(0, _barGroups.Count).Select(i =>
			{
				int baseIndex = i * _barGroups.Count + i + 1;
				int lastIndex = baseIndex + _classes;
				double pos = ((double)(lastIndex - baseIndex)) / 2;
				return new Tick(pos, _groupsLabels[i], true);
			}).ToArray();
			_plt.Axes.Bottom.TickGenerator = new NumericManual(ticks);
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
			if (LogY)
			{
				(double min, double max) = _barGroups.SelectMany(g => g).MinMax();
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
		}

	}
}
