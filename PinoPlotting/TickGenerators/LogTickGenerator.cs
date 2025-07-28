using ScottPlot;
using ScottPlot.TickGenerators;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MyPlotting.TickGenerators
{
    public class LogTickGenerator : ITickGenerator
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public bool IsTimeSpan { get; set; }

        public LogTickGenerator(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public int LogBase { get; set; } = 10;
        public bool NaturalLog { get; set; } = false;

        public Tick[] Ticks { get; set; } = Array.Empty<Tick>();

        public int MaxTickCount { get; set; }

        public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, SKPaint paint, LabelStyle labelStyle)
        {
            if (Min < 0 || Max < 0) throw new Exception($"Cannot compute log axis with negative values Range: ({Min} - {Max})");

            //trova l'ordine di grandezza del max value
            int minOrder = Min != 0 ? (int)Math.Floor(Log(Min)) : 0;
            int order = (int)Math.Ceiling(Log(Max));
            Ticks = Min == 0 ? new Tick[] { Tick.Major(Log(0), "0") } : Array.Empty<Tick>();
            Ticks = Ticks.Concat(Enumerable.Range(minOrder, order - minOrder).SelectMany(order => Enumerable.Range(0, LogBase == 2 ? LogBase : LogBase - 1).Select(i =>
            {
                double b = Pow(order);
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
            if (value == 0 && Min == 0) return -0.1;
            if (value == 0) return Log(Min) - 0.1;
            if (NaturalLog) return Math.Log(value);
            if (LogBase == 2) return Math.Log2(value);
            if (LogBase == 10) return Math.Log10(value);
            return Math.Log10(value) / Math.Log10(LogBase);
        }
    }
}
