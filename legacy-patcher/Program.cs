namespace Gk2UltrawidePatcher;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("GK2 Ultrawide Fix");
        Console.WriteLine("=================");
        Console.WriteLine();

        bool restore =
            args.Any(
                argument =>
                    argument.Equals(
                        "--restore",
                        StringComparison.OrdinalIgnoreCase));

        bool verbose =
            args.Any(
                argument =>
                    argument.Equals(
                        "--verbose",
                        StringComparison.OrdinalIgnoreCase));

        bool help =
            args.Any(
                argument =>
                    argument.Equals(
                        "--help",
                        StringComparison.OrdinalIgnoreCase) ||
                    argument.Equals(
                        "-h",
                        StringComparison.OrdinalIgnoreCase));

        if (help)
        {
            PrintUsage();
            return 0;
        }

        string? gamePath =
            args.FirstOrDefault(
                argument =>
                    !argument.StartsWith(
                        "-",
                        StringComparison.Ordinal));

        if (gamePath == null)
        {
            Console.WriteLine(
                "[*] Searching for Graveyard Keeper 2...");

            gamePath =
                SteamLocator.FindGameDirectory(
                    verbose);

            if (gamePath == null)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(
                    "[ERROR] Graveyard Keeper 2 was not found.");

                Console.Error.WriteLine(
                    "Specify the game directory manually.");

                Console.Error.WriteLine();

                PrintUsage();

                return 1;
            }

            Console.WriteLine(
                "[+] Graveyard Keeper 2 found:");

            Console.WriteLine(
                $"    {gamePath}");

            Console.WriteLine();
        }

        try
        {
            if (restore)
            {
                return UltrawidePatcher.Restore(
                    gamePath);
            }

            return UltrawidePatcher.Patch(
                gamePath,
                verbose);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(
                $"[ERROR] {exception.Message}");

            if (verbose)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(
                    exception);
            }

            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");

        Console.WriteLine(
            "  gk2-ultrawide-patcher [options] [game-directory]");

        Console.WriteLine();

        Console.WriteLine("Options:");

        Console.WriteLine(
            "  --restore    Restore the original game assembly");

        Console.WriteLine(
            "  --verbose    Show technical details");

        Console.WriteLine(
            "  --help       Show this help");
    }
}