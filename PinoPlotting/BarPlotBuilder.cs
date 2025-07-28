using ScottPlot;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MyPlotting
{
    public class BarPlotBuilder : AbstractPlot
    {

        private int maxLabelLen = 0;
        private double maxY = double.NegativeInfinity;


        public BarPlotBuilder(bool logY = false)
            : base(false, logY)
        {
        }

        public void AddBars(double[] data, string[] xTickLabels = null)
        {

            if (LogY)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = data[i] > 0 ? Math.Log10(data[i]) : 0;
                    if (data[i] > maxY) maxY = data[i];
                }
            }

            var existBars = _plt.PlottableList;
            double[] positions = new double[(data?.Length ?? 0) + existBars.Count];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = i + 1;
            }

            Bar[] bars = data.Select((x, i) => new Bar()
            {
                Position = positions[existBars.Count + i],
                ValueBase = 0,
                Value = x
            }).ToArray();

            foreach (var bar in bars)
            {
                _plt.Add.Bar(bar);
            }

            string[] newXLabels;
            if (xTickLabels is null || data.Length != xTickLabels.Length)
            {
                Console.WriteLine($"Tick len {data.Length} different from tickLabel lenght {xTickLabels?.Length}. Ignoring labels");
                newXLabels = positions.Select(x => x.ToString()).ToArray();
            }
            else
            {
                newXLabels = new string[positions.Length];
                int lastIndex = 0;
                foreach ((string x, int i) in _plt.Axes.Bottom.TickGenerator.Ticks.Select((x, i) => (x.Label, i)))
                {
                    newXLabels[i] = x;
                    lastIndex++;
                }
                for (int i = 0; i < xTickLabels.Length; i++)
                {
                    newXLabels[lastIndex + i] = xTickLabels[i];
                }
            }
            try
            {
                maxLabelLen = newXLabels.Select(x => x.Length).Max();
            }
            catch (Exception) { maxLabelLen = 0; }

            _plt.Axes.SetLimits(top: maxY + 1);
            _plt.Axes.Bottom.TickGenerator = new NumericManual(positions, newXLabels);

        }

        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            if (LogY)
            {
                _plt.Axes.Left.TickGenerator = new NumericAutomatic()
                {
                    MinorTickGenerator = new LogMinorTickGenerator(),
                    LabelFormatter = x => $"{Math.Pow(10, x):N0}"
                };
                for (int e = 0; e <= _plt.Axes.GetLimits().Top; e++)
                {
                    _plt.Add.HorizontalLine(e, width: 1f, Colors.DarkGrey, LinePattern.Dashed);
                }
            }
            _plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
            _plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
            _plt.Legend.IsVisible = true;
            _plt.Legend.Location = Alignment.UpperCenter;
            _plt.Grid.MajorLineWidth = 1;
            _plt.Grid.MajorLineColor = Colors.LightGray;
            _plt.Grid.IsVisible = true;
            _plt.Axes.SetLimits(left: 0, bottom: 0);
            _plt.XLabel(xLabel);
            _plt.Axes.Bottom.Label.OffsetY = 30f;
            _plt.YLabel(yLabel);
            _plt.Layout.Fixed(new PixelPadding(top: 10, right: 10, left: 50, bottom: Math.Max(maxLabelLen * 7, 50)));
            if (Constants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + Constants.ImageFormat, 800, 600);
            else if (Constants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + Constants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
        }

    }
}
