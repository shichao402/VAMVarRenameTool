using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

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
        var metaDataProcessor = new MetaDataProcessor();

        foreach (var file in files)
        {
            var result = new FileResult {OriginalPath = file};

            var meta = metaDataProcessor.ProcessMetaData(file, out var status, out var newName);
            result.Status = status;
            result.NewName = newName;

            if (meta != null && status == "Ready")
            {
                renamePlan.Add(new RenameInfo(file, newName));
            }

            Results.Add(result);
        }

        DetectConflicts(renamePlan);
        ExecuteRenames(renamePlan);
    }

    public string ExtractVersion(string filename, MetaData meta)
    {
        var patterns = new[]
        {
            // 精确匹配 creator.package 开头的模式
            $@"^{Regex.Escape(meta.CreatorName)}\.{Regex.Escape(meta.PackageName)}\.(?<version>[^\s\.]+)[^\.]*\.var$",
            // 匹配 latest 的特殊情况
            @"\.(?<version>latest)\.var$",
            // 通用匹配模式：捕获最后一个点之前的数字/字母组合
            @"(?:\.)(?<version>\d+)\.var$",
            // 三段式
            @".*\..*\.(?<version>\d+)\.var$",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(filename, pattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (match.Success)
            {
                var version = match.Groups["version"].Value;
                if (Int32.TryParse(version, out var number))
                {
                    version = number.ToString();
                }
                else
                {
                    break;
                }

                // 清理可能包含的额外字符
                return Regex.Replace(version, @"[^\w\.-]", "");
            }
        }

        throw new FormatException($"Version pattern not found in: {filename}");
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