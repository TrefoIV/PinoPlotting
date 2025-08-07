using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyPlotting
{
    public class PDFPLotBuilder : AbstractPlot
    {

        public PDFPLotBuilder(bool logX = false, bool logY = false)
            : base(logX, logY)
        {
        }


        public void AddPDFToPlot(IEnumerable<(double x, double y)> inputData, string label = null, int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
        {

            if (!inputData.Any())
            {
                return;
            }
            if (LogX)
            {
                inputData = inputData.Where(x => x.x > 0).Select(x => (Math.Log10(x.x), x.y)).ToList();
                if (!inputData.Any())
                {
                    return;
                }

            }


            double[] xs = inputData.Select(x => x.x).ToArray();
            double[] ys = inputData.Select(y => y.y).ToArray();
            if (LogY)
            {
                xs = xs.Where((x, i) => ys[i] > 0).ToArray();
                ys = ys.Where(y => y > 0).Select(y => Math.Log10(y)).ToArray();
            }
            var scatter = _plt.Add.Scatter(xs, ys, color);
            scatter.Label = label;
        }

        public void AddPDFToPlot(IEnumerable<(int x, int y)> inputData, string label = null, int steps = CDFUtils.DEFAULT_STEPS, Color? color = null)
        {
            if (!inputData.Any())
            {
                return;
            }
            if (LogX || LogY)
            {
                AddPDFToPlot(inputData.Select(x => ((double)x.x, (double)x.y)), label);
                return;
            }

            int[] xs = inputData.Select(x => x.x).ToArray();
            int[] ys = inputData.Select(y => y.y).ToArray();

            var scatter = _plt.Add.Scatter(xs, ys, color);
            scatter.Label = label;
        }


        public override void SavePlot(FileInfo outFile, string xLabel = "", string yLabel = "")
        {
            if (LogX)
            {
                double[] allX = _plt.PlottableList.Select(x => x as Scatter).SelectMany(s => s.Data.GetScatterPoints().Select(p => p.X)).ToArray();

                _plt.Axes.Bottom.TickGenerator = new NumericManual(allX, allX.Select(x => $"{Math.Pow(10, x):N0}").ToArray());
                //_plt.Axes.Bottom.TickGenerator = new NumericAutomatic()
                //{
                //	MinorTickGenerator = new LogMinorTickGenerator()
                //	{
                //		Divisions = 10
                //	},
                //	LabelFormatter = x => $"{Math.Pow(10, x):N}",
                //};

            }

            FinalizeSettings(xLabel, yLabel);
            if (PlottingConstants.ImageFormat.EndsWith(".png", StringComparison.InvariantCulture))
                _plt.SavePng(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
            else if (PlottingConstants.ImageFormat.EndsWith(".svg", StringComparison.InvariantCulture))
                _plt.SaveSvg(outFile.FullName + PlottingConstants.ImageFormat, 800, 600);
            else
            {
                Console.WriteLine($"FORMATO IMMAGINE NON SUPPORTATO PER IL FILE {outFile.FullName}. Invece di crashare skippo!");
            }
        }

        protected void FinalizeSettings(string xLabel, string yLabel)
        {
            if (LogY)
            {
                _plt.Axes.Left.TickGenerator = new NumericAutomatic()
                {
                    MinorTickGenerator = new LogMinorTickGenerator(),
                    LabelFormatter = x => $"{Math.Pow(10, x):N3}"
                };
            }
            _plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
            _plt.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperLeft;
            _plt.Axes.Bottom.TickLabelStyle.AntiAliasText = false;
            _plt.Legend.IsVisible = true;
            _plt.Legend.Alignment = Alignment.UpperLeft;
            _plt.Grid.MajorLineWidth = 1;
            _plt.Grid.MajorLineColor = Colors.LightGray;
            _plt.Grid.XAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
            _plt.Grid.YAxisStyle.MajorLineStyle.Pattern = LinePattern.Dotted;
            _plt.Grid.MinorLineWidth = 1;
            _plt.Grid.XAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
            _plt.Grid.YAxisStyle.MinorLineStyle.Pattern = LinePattern.Dotted;
            _plt.Grid.IsVisible = true;
            _plt.Axes.Bottom.TickLabelStyle.FontSize = 30f;
            _plt.Axes.Left.TickLabelStyle.FontSize = 24f;
            _plt.Legend.FontSize = 30f;
            _plt.Axes.Bottom.Label.FontSize = 30f;
            _plt.Axes.Left.Label.FontSize = 30f;
            _plt.Axes.SetLimits(bottom: LogY ? null : -1, top: LogY ? 2.01 : 101, left: LogX ? null : 0);
            _plt.Layout.Fixed(new PixelPadding(top: 10, left: 85, right: 10, bottom: 85));
            _plt.XLabel(xLabel);
            _plt.Axes.Bottom.Label.OffsetY = 20f;
            _plt.YLabel(yLabel);
        }
    }
}
