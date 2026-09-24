using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Gk2UltrawidePatcher;

internal static class SteamLocator
{
    private const string GameDirectoryName =
        "Graveyard Keeper 2";

    private const string GameAppId =
        "4358690";

    public static string? FindGameDirectory(
        bool verbose = false)
    {
        foreach (string steamDirectory in FindSteamDirectories())
        {
            if (verbose)
            {
                Console.WriteLine(
                    $"[DEBUG] Checking Steam directory: {steamDirectory}");
            }

            string? gameDirectory =
                FindGameInSteamDirectory(
                    steamDirectory,
                    verbose);

            if (gameDirectory != null)
            {
                return gameDirectory;
            }
        }

        return null;
    }

    private static IEnumerable<string> FindSteamDirectories()
    {
        var directories =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        if (RuntimeInformation.IsOSPlatform(
                OSPlatform.Windows))
        {
            AddWindowsSteamDirectories(
                directories);
        }
        else if (RuntimeInformation.IsOSPlatform(
                     OSPlatform.Linux))
        {
            AddLinuxSteamDirectories(
                directories);
        }

        return directories;
    }

    private static void AddLinuxSteamDirectories(
        HashSet<string> directories)
    {
        string home =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

        AddIfDirectoryExists(
            directories,
            Path.Combine(
                home,
                ".local",
                "share",
                "Steam"));

        AddIfDirectoryExists(
            directories,
            Path.Combine(
                home,
                ".steam",
                "steam"));

        AddIfDirectoryExists(
            directories,
            Path.Combine(
                home,
                ".var",
                "app",
                "com.valvesoftware.Steam",
                ".local",
                "share",
                "Steam"));
    }

    [SupportedOSPlatform("windows")]
    private static void AddWindowsSteamDirectories(
        HashSet<string> directories)
    {
        try
        {
            string? steamPath =
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Valve\Steam",
                    "SteamPath",
                    null) as string;

            if (!string.IsNullOrWhiteSpace(
                    steamPath))
            {
                AddIfDirectoryExists(
                    directories,
                    steamPath);
            }
        }
        catch
        {
            // Registry lookup is optional.
        }

        string? programFilesX86 =
            Environment.GetEnvironmentVariable(
                "ProgramFiles(x86)");

        if (!string.IsNullOrWhiteSpace(
                programFilesX86))
        {
            AddIfDirectoryExists(
                directories,
                Path.Combine(
                    programFilesX86,
                    "Steam"));
        }

        string? programFiles =
            Environment.GetEnvironmentVariable(
                "ProgramFiles");

        if (!string.IsNullOrWhiteSpace(
                programFiles))
        {
            AddIfDirectoryExists(
                directories,
                Path.Combine(
                    programFiles,
                    "Steam"));
        }
    }

    private static string? FindGameInSteamDirectory(
        string steamDirectory,
        bool verbose)
    {
        var libraries =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                steamDirectory
            };

        string libraryFile =
            Path.Combine(
                steamDirectory,
                "steamapps",
                "libraryfolders.vdf");

        if (File.Exists(libraryFile))
        {
            foreach (string library in
                     ReadLibraryDirectories(
                         libraryFile))
            {
                libraries.Add(library);
            }
        }

        foreach (string library in libraries)
        {
            if (verbose)
            {
                Console.WriteLine(
                    $"[DEBUG] Checking library: {library}");
            }

            string manifestPath =
                Path.Combine(
                    library,
                    "steamapps",
                    $"appmanifest_{GameAppId}.acf");

            string gameDirectory =
                Path.Combine(
                    library,
                    "steamapps",
                    "common",
                    GameDirectoryName);

            if (File.Exists(manifestPath) &&
                Directory.Exists(gameDirectory))
            {
                return gameDirectory;
            }
        }

        return null;
    }

    private static IEnumerable<string>
        ReadLibraryDirectories(
            string libraryFile)
    {
        string content =
            File.ReadAllText(libraryFile);

        MatchCollection matches =
            Regex.Matches(
                content,
                "\"path\"\\s+\"([^\"]+)\"");

        foreach (Match match in matches)
        {
            if (!match.Success ||
                match.Groups.Count < 2)
            {
                continue;
            }

            string path =
                match.Groups[1].Value
                    .Replace(
                        @"\\",
                        @"\");

            if (Directory.Exists(path))
            {
                yield return path;
            }
        }
    }

    private static void AddIfDirectoryExists(
        HashSet<string> directories,
        string directory)
    {
        if (Directory.Exists(directory))
        {
            directories.Add(
                Path.GetFullPath(directory));
        }
    }
}