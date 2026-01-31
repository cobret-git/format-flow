namespace FormatFlow.Cli
{
    public class ConvertOptions
    {
        public string Path { get; init; } = "";
        public string? FromFormat { get; init; }
        public required string ToFormat { get; init; }
        public bool Recursive { get; init; }
        public bool Overwrite { get; init; }
        public string? Error { get; init; }

        public static ConvertOptions Parse(string[] args)
        {
            if (args.Length == 0)
                return new ConvertOptions { ToFormat = "", Error = "No path specified" };

            var path = args[0];
            string? from = null;
            string? to = null;
            bool recursive = false;
            bool overwrite = false;

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--from" when i + 1 < args.Length:
                        from = args[++i].TrimStart('.');
                        break;
                    case "--to" when i + 1 < args.Length:
                        to = args[++i].TrimStart('.');
                        break;
                    case "--recursive" or "-r":
                        recursive = true;
                        break;
                    case "--overwrite" or "-f":
                        overwrite = true;
                        break;
                }
            }

            if (to == null)
                return new ConvertOptions { ToFormat = "", Error = "Target format (--to) is required" };

            return new ConvertOptions
            {
                Path = path,
                FromFormat = from,
                ToFormat = to,
                Recursive = recursive,
                Overwrite = overwrite
            };
        }
    }
}
