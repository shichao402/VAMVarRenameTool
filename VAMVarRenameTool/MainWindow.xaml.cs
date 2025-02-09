using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;

namespace VAMVarRenameTool;

public partial class MainWindow : Window
{
    public ObservableCollection<FileResult> Results { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        dgResults.ItemsSource = Results;
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Folder Selection.",
            Filter = "Folders|.",
            ValidateNames = false
        };

        if (dialog.ShowDialog() == true)
        {
            txtPath.Text = Path.GetDirectoryName(dialog.FileName);
        }
    }

    private void TxtPath_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void TxtPath_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
        txtPath.Text = Directory.Exists(paths[0]) ? paths[0] : Path.GetDirectoryName(paths[0]);
    }

    private void Process_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
        var directory = txtPath.Text;
        if (!Directory.Exists(directory)) return;

        var files = Directory.GetFiles(directory, "*.var", SearchOption.AllDirectories);
        var renamePlan = new List<RenameInfo>();

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true, // 允许末尾逗号
            ReadCommentHandling = JsonCommentHandling.Skip, // 允许注释
            PropertyNameCaseInsensitive = true // 忽略属性名大小写
        };

        foreach (var file in files)
        {
            var result = new FileResult {OriginalPath = file};

            try
            {
                using var archive = ZipFile.OpenRead(file);
                var metaEntry = archive.GetEntry("meta.json") ??
                                throw new FileNotFoundException("meta.json missing");

                using var stream = metaEntry.Open();
                using var reader = new StreamReader(stream);

                var meta = JsonSerializer.Deserialize<MetaData>(reader.ReadToEnd(), options) ??
                           throw new InvalidDataException("Invalid meta.json");

                int error = 0;
                if (string.IsNullOrEmpty(meta.CreatorName))
                {
                    meta.CreatorName = "Unknown";
                    ++error;
                }

                if (string.IsNullOrEmpty(meta.PackageName))
                {
                    meta.CreatorName = "Unknown";
                    ++error;
                }

                if (error < 2)
                {
                    string version = ExtractVersion(Path.GetFileName(file), meta);
                    string newName = $"{meta.CreatorName}.{meta.PackageName}.{version}.var";
                    result.NewName = newName;
                    renamePlan.Add(new RenameInfo(file, newName));
                    result.Status = "Ready";
                }
                else
                {
                    result.Status = "Skip";
                }
            }
            catch (Exception ex)
            {
                result.Status = $"Error: {ex.Message}";
            }

            Results.Add(result);
        }

        DetectConflicts(renamePlan);
        ExecuteRenames(renamePlan);
    }

    private string ExtractVersion(string filename, MetaData meta)
    {
        var patterns = new[]
        {
            $@"^{Regex.Escape(meta.CreatorName)}\.{Regex.Escape(meta.PackageName)}\.(?<version>[0-9\.]+)\.var$",
            @"\.(?<version>[0-9\.]+)\.var$",
            @"\.(?<version>latest)\.var$"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(filename, pattern, RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups["version"].Value;
        }

        throw new FormatException("Version pattern not found");
    }

    private void DetectConflicts(List<RenameInfo> renames)
    {
        var nameCounts = renames
            .GroupBy(r => r.NewName.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var rename in renames)
        {
            if (nameCounts[rename.NewName.ToLower()] > 1)
            {
                rename.HasConflict = true;
                Results.First(r => r.OriginalPath == rename.OriginalPath).Status = "Conflict";
            }
        }
    }

    private void ExecuteRenames(List<RenameInfo> renames)
    {
        foreach (var rename in renames.Where(r => !r.HasConflict))
        {
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(rename.OriginalPath)!, rename.NewName);
                if (rename.OriginalPath != newPath)
                {
                    File.Move(rename.OriginalPath, newPath);
                    Results.First(r => r.OriginalPath == rename.OriginalPath).Status = "Renamed";
                }
                else
                {
                    Results.First(r => r.OriginalPath == rename.OriginalPath).Status = "Same,Skip";
                }
            }
            catch (Exception ex)
            {
                Results.First(r => r.OriginalPath == rename.OriginalPath).Status = $"Failed: {ex.Message}";
            }
        }
    }
}

public class MetaData
{
    public string CreatorName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
}

public class FileResult
{
    public string OriginalPath { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public record RenameInfo(string OriginalPath, string NewName)
{
    public bool HasConflict { get; set; }
}