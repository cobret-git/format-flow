namespace FormatFlow.Cli
{
    public class ConversionSummary
    {
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public int Total => Succeeded + Failed + Skipped;
    }
}
