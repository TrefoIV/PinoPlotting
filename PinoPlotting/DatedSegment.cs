namespace MyPlotting
{
    public class DatedSegment
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSpan Duration => End - Start;
    }
}