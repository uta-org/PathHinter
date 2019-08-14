using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PathHinter
{
    /// <summary>
    /// The PathHint class
    /// </summary>
    public static class PathHint
    {
        /// <summary>
        /// The Path Suggestion Style enum
        /// </summary>
        public enum PathSuggestionStyle
        {
            Windows,
            Linux
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns></returns>
        public static string ReadLine()
        {
            return ReadLine(string.Empty, PathSuggestionStyle.Linux);
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ReadLine(string text)
        {
            return ReadLine(text, PathSuggestionStyle.Linux);
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="style">The style.</param>
        /// <param name="inputRegex">The input regex.</param>
        /// <param name="hintColor">Color of the hint.</param>
        /// <returns></returns>
        public static string ReadLine(string text, PathSuggestionStyle style, string inputRegex = ".*", ConsoleColor hintColor = ConsoleColor.DarkGray)
        {
            if (!string.IsNullOrEmpty(text))
                Console.Write(text);

            ConsoleKeyInfo input;

            var suggestion = string.Empty;
            var userInput = string.Empty;
            var readLine = string.Empty;

            string[] hintSource = null;
            int suggestionIndex = 0;

            // TODO: Check if userInput is a path and change behaviour between normal read line and this read line
            while (ConsoleKey.Enter != (input = Console.ReadKey()).Key)
            {
                if (input.Key == ConsoleKey.Backspace)
                {
                    userInput = SimulateBackspace(userInput);

                    // Update hints if the user input changed
                    hintSource = UpdateHints(userInput, out suggestionIndex);
                }
                else if (input.Key == ConsoleKey.Tab && userInput.StartsWith("/"))
                {
                    bool suggestionIsEmpty = string.IsNullOrEmpty(suggestion);

                    // Load suggestions for current path
                    if (suggestionIsEmpty)
                        hintSource = DisplayFolders(userInput, style);

                    if (hintSource?.Length > 1)
                        userInput = suggestion ?? userInput;
                    else if (hintSource?.Length == 1)
                        userInput = hintSource[0];

                    if (!suggestionIsEmpty)
                        MustResetSuggestions(ref suggestion, ref suggestionIndex);
                }
                else if (input.Key == ConsoleKey.UpArrow || input.Key == ConsoleKey.DownArrow)
                {
                    bool isUp = input.Key == ConsoleKey.UpArrow;
                    int nextIndex = hintSource.NavigateBounds(ref suggestionIndex, isUp);

                    suggestion = hintSource?[nextIndex];
                }
                else if (input.Key == ConsoleKey.LeftArrow || input.Key == ConsoleKey.RightArrow)
                {
                    if (input.Key == ConsoleKey.LeftArrow)
                        userInput = SimulateBackspace(userInput);
                    else
                        userInput += suggestion[userInput.Length];

                    // Update hints if the user input changed
                    hintSource = UpdateHints(userInput, out suggestionIndex);
                }
                else if (input != default && Regex.IsMatch(input.KeyChar.ToString(), inputRegex))
                {
                    userInput += input.KeyChar;
                }

                if (string.IsNullOrEmpty(suggestion))
                    suggestion = hintSource?
                        .FirstOrDefault(item => item.Length > userInput.Length && item.Substring(0, userInput.Length) == userInput);

                readLine = string.IsNullOrEmpty(suggestion) || userInput.Length > readLine.Length ? userInput : suggestion;

                ClearCurrentConsoleLine();

                Console.Write(text + userInput);

                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = hintColor;

                if (userInput.Any())
                    Console.Write(readLine.Substring(userInput.Length, readLine.Length - userInput.Length));

                Console.ForegroundColor = originalColor;
            }

            Console.WriteLine(readLine);

            return userInput.Any() ? readLine : string.Empty;
        }

        /// <summary>
        /// Updates the hints.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <param name="suggestionIndex">Index of the suggestion.</param>
        /// <returns></returns>
        private static string[] UpdateHints(string userInput, out int suggestionIndex)
        {
            // Update hints
            var hintSource = DisplayFolders(userInput);

            // Reset suggestion index
            suggestionIndex = 0;

            return hintSource;
        }

        /// <summary>
        /// Must the reset suggestions?
        /// </summary>
        /// <param name="currentSuggestion">The current suggestion.</param>
        /// <param name="currentSuggestionIndex">Index of the current suggestion.</param>
        private static void MustResetSuggestions(ref string currentSuggestion, ref int currentSuggestionIndex)
        {
            bool isWinStyle = GetStyle(currentSuggestion) == PathSuggestionStyle.Windows;
            bool isWinPlatform = Environment.OSVersion.Platform == PlatformID.Win32NT;

            string localSuggestion = (string)currentSuggestion.Clone();

            if (!isWinStyle && isWinPlatform)
                localSuggestion = ToWinDir(currentSuggestion);

            if (Directory.Exists(localSuggestion))
            {
                currentSuggestion = string.Empty;
                currentSuggestionIndex = 0;
            }
        }

        /// <summary>
        /// Simulates the backspace.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        /// <returns></returns>
        private static string SimulateBackspace(string userInput)
        {
            return userInput.Any() ? userInput.Remove(userInput.Length - 1, 1) : string.Empty;
        }

        /// <summary>
        /// Displays the folders.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="style">The style.</param>
        /// <returns></returns>
        private static string[] DisplayFolders(string path, PathSuggestionStyle style = PathSuggestionStyle.Linux)
        {
            bool isWinStyle = GetStyle(path) == PathSuggestionStyle.Windows;
            bool isWinPlatform = Environment.OSVersion.Platform == PlatformID.Win32NT;

            char separator = isWinPlatform
                ? Path.DirectorySeparatorChar
                : Path.AltDirectorySeparatorChar;

            if (isWinPlatform)
                path = ToWinDir(path);

            if (string.IsNullOrEmpty(path))
            {
                // Show drives
                var drives = DriveInfo.GetDrives()
                                .Select(drive => isWinPlatform && style == PathSuggestionStyle.Linux ? ToUnixDir(drive.Name) : drive.Name)
                                .ToArray();

                drives.DrawAsTable();

                return drives;
            }

            string subfolder = path.Split(separator).Last().ToLowerInvariant();
            bool isNeedle = false;

            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                isNeedle = true;
            }

            var directories = !isNeedle
                ? Directory.GetDirectories(path)
                : Directory.GetDirectories(path)
                    .Where(dir => dir.Split(separator).Last().ToLowerInvariant().StartsWith(subfolder))
                    .ToArray();

            if (directories.Length >= 50)
            {
                Console.WriteLine();
                Console.Write($"Display all {directories.Length} possibilities? (y or n)");

                ConsoleKey key;
                do
                {
                    key = Console.ReadKey().Key;
                    // TODO: Don't display pressed key on console
                }
                while (!(key == ConsoleKey.Y || key == ConsoleKey.N));

                if (key == ConsoleKey.N)
                    return null;
            }

            var dirs = directories.Select(dir => SanitizePath(dir, path));

            if (dirs.Count() > 1)
                dirs.DrawAsTable();

            if (!isWinStyle && isWinPlatform)
                directories = directories
                    .Select(ToUnixDir)
                    .ToArray();

            return directories;
        }

        /// <summary>
        /// Sanitizes the path.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string SanitizePath(string dir, string path)
        {
            return dir
                .Replace(path, string.Empty)
                .Replace("\\", newValue: string.Empty)
                .Replace("/", string.Empty);
        }

        /// <summary>
        /// Converts the path to a Windows styled path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ToWinDir(string path)
        {
            try
            {
                if (path.StartsWith("/"))
                {
                    string drive = path.Substring(1, 1).ToUpperInvariant();
                    path = drive + ":" + path.Substring(2);
                }

                if (path.Contains("/"))
                    path = path.Replace("/", "\\");

                return path;
            }
            catch
            {
                // Caused by absolute paths like '/'
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts the path to a Unix styled path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Drives can't have 'A' letter. - path</exception>
        private static string ToUnixDir(string path)
        {
            string driveLetter = path[0].ToString().ToLowerInvariant();

            if (driveLetter == "a")
                throw new ArgumentException("Drives can't have 'A' letter.", nameof(path));

            if (path.Contains(":"))
                path = path.Replace(":", "");

            if (path.Contains("\\"))
                path = path.Replace("\\", "/");

            if (char.IsLetter(path[0]))
                path = "/" + driveLetter + path.Substring(1);

            return path;
        }

        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static PathSuggestionStyle GetStyle(string path)
        {
            return path.Contains(":\\")
                ? PathSuggestionStyle.Windows
                : PathSuggestionStyle.Linux;
        }

        /// <summary>
        /// Clears the current console line.
        /// </summary>
        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}