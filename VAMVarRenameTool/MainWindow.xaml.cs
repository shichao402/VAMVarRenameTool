using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VAMVarRenameTool.MetaData;
using VAMVarRenameTool.EventSystem;
using VAMVarRenameTool.NameTransform;

namespace VAMVarRenameTool;

public partial class MainWindow : Window
{
    private const string CacheDirectory = "Cache";
    private const string CacheFilePath = "Cache/lastPath.txt";
    private const string WindowSizeCacheFilePath = "Cache/windowSize.txt";
    private const string ColumnWidthCacheFilePath = "Cache/columnWidths.txt";

    public ObservableCollection<FileResult> Results { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        dgResults.ItemsSource = Results;
        LoadLastPath();
        LoadWindowSize();
        LoadColumnWidths();
    }

    private void LoadLastPath()
    {
        if (File.Exists(CacheFilePath))
        {
            txtPath.Text = File.ReadAllText(CacheFilePath);
        }
    }

    private void SaveLastPath(string path)
    {
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
        File.WriteAllText(CacheFilePath, path);
    }

    private void LoadWindowSize()
    {
        if (File.Exists(WindowSizeCacheFilePath))
        {
            var size = File.ReadAllText(WindowSizeCacheFilePath).Split(',');
            if (size.Length == 2 && double.TryParse(size[0], out double width) && double.TryParse(size[1], out double height))
            {
                this.Width = width;
                this.Height = height;
            }
        }
    }

    private void SaveWindowSize()
    {
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
        File.WriteAllText(WindowSizeCacheFilePath, $"{this.Width},{this.Height}");
    }

    private void LoadColumnWidths()
    {
        if (File.Exists(ColumnWidthCacheFilePath))
        {
            var widths = File.ReadAllText(ColumnWidthCacheFilePath).Split(',');
            if (widths.Length == dgResults.Columns.Count)
            {
                for (int i = 0; i < widths.Length; i++)
                {
                    if (double.TryParse(widths[i], out double width))
                    {
                        dgResults.Columns[i].Width = width;
                    }
                }
            }
        }
    }

    private void SaveColumnWidths()
    {
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
        var widths = dgResults.Columns.Select(c => c.Width.ToString()).ToArray();
        File.WriteAllText(ColumnWidthCacheFilePath, string.Join(",", widths));
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
            SaveLastPath(txtPath.Text);
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
        SaveLastPath(txtPath.Text);
    }

    private void Organize_Click(object sender, RoutedEventArgs e)
    {
        Core.Instance.ReloadConfigs();
        Results.Clear();
        var directory = txtPath.Text;
        if (!Directory.Exists(directory)) return;

        var favoriteCreator = new HashSet<string>(File.ReadAllLines("Config/favorite_creator.txt").Select(line => line.Trim()));
        var files = Directory.GetFiles(directory, "*.var", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var result = new FileResult { OriginalPath = file };
            VarMeta varMeta = new VarMeta();
            try
            {
                Core.Instance.VarMetaProcessor.ParseFromVarFile(file, ref varMeta);
            }
            catch (Exception exception)
            {
                result.Status = exception.Message;
                Results.Add(result);
                continue;
            }

            varMeta.CreatorName = Core.Instance.CharTransformer.Transform(varMeta.CreatorName);
            varMeta.PackageName = Core.Instance.CharTransformer.Transform(varMeta.PackageName);

            Core.Instance.CreatorPackageNameTransformer.TryTransform(varMeta);
            Core.Instance.CreatorNameTransformer.TryTransform(varMeta);
            Core.Instance.PackageNameTransformer.TryTransform(varMeta);
            
            VarFileName varFileName = Core.Instance.VarFileNameParser.Parse(file);
            // 有一种异常情况, 有的作者...连meta都不写. meta里的作者都是空的.
            if (varMeta.CreatorName == "Unknown")
            {
                varMeta.CreatorName = varFileName.Creator;
            }
            if (varMeta.PackageName == "Unknown")
            {
                varMeta.PackageName = varFileName.Package;
            }

            string targetDirectory;
            if (favoriteCreator.Contains(varMeta.CreatorName))
            {
                targetDirectory = Path.Combine(directory, "Organized", varMeta.CreatorName);
            }
            else
            {
                targetDirectory = Path.Combine(directory, ".Dependencies", varMeta.CreatorName);
            }

            try
            {
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                var targetPath = Path.Combine(targetDirectory, Path.GetFileName(file));

                if (file.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "Same, skip move";
                }
                else
                {
                    if (File.Exists(targetPath))
                    {
                        if (FilesAreEqual(file, targetPath))
                        {
                            File.Delete(file);
                        }
                        else
                        {
                            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                            var newTargetPath = Path.Combine(targetDirectory, $"{Path.GetFileNameWithoutExtension(file)}.{timestamp}{Path.GetExtension(file)}");
                            File.Move(file, newTargetPath);
                        }
                    }
                    else
                    {
                        File.Move(file, targetPath);
                    }

                    result.NewName = targetPath;
                    result.Status = "Moved";
                }
            }
            catch (Exception exception)
            {
                result.Status = exception.Message;
                Results.Add(result);
                continue;
            }

            Results.Add(result);
        }

        // 重新扫描目录并删除空目录
        RemoveEmptyDirectories(directory);
    }

    private void RemoveEmptyDirectories(string startLocation)
    {
        foreach (var directory in Directory.GetDirectories(startLocation))
        {
            RemoveEmptyDirectories(directory);
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
    }

    private bool FilesAreEqual(string filePath1, string filePath2)
    {
        using (var hashAlgorithm = SHA256.Create())
        {
            var hash1 = GetFileHash(hashAlgorithm, filePath1);
            var hash2 = GetFileHash(hashAlgorithm, filePath2);
            return hash1.SequenceEqual(hash2);
        }
    }

    private byte[] GetFileHash(HashAlgorithm hashAlgorithm, string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            return hashAlgorithm.ComputeHash(stream);
        }
    }

    private void FixFileName_Click(object sender, RoutedEventArgs e)
    {
        Core.Instance.ReloadConfigs();
        Results.Clear();
        var directory = txtPath.Text;
        if (!Directory.Exists(directory)) return;

        var files = Directory.GetFiles(directory, "*.var", SearchOption.AllDirectories);
        var renamePlan = new List<RenameInfo>();

        foreach (var file in files)
        {
            var result = new FileResult {OriginalPath = file};
            VarMeta varMeta = new VarMeta();
            try
            {
                Core.Instance.VarMetaProcessor.ParseFromVarFile(file, ref varMeta);
            }
            catch (Exception exception)
            {
                result.Status = exception.Message;
                Results.Add(result);
                continue;
            }
            
            // meta内的值转换特殊字符. 这是vam var本身的设计逻辑, 尊重.
            varMeta.CreatorName = Core.Instance.CharTransformer.Transform(varMeta.CreatorName);
            varMeta.PackageName = Core.Instance.CharTransformer.Transform(varMeta.PackageName);
            // 尝试做映射转换, 处理一些更加异常的状况， 比如作者根本不用Vam里的打包工具。
            // 注意这里有顺序. CreatorPackage合并的转换优先级最高.
            Core.Instance.CreatorPackageNameTransformer.TryTransform(varMeta);
            Core.Instance.CreatorNameTransformer.TryTransform(varMeta);
            Core.Instance.PackageNameTransformer.TryTransform(varMeta);

            VarFileName varFileName = Core.Instance.VarFileNameParser.Parse(file);

            // 到这里， 理论上varMeta和varFileName里的数据应该完全一致。 如果不一致， 应该用varMeta里的数据替换到VarFileName
            // 有一种异常情况, 有的作者...连meta都不写. meta里的作者都是空的.
            if (varMeta.CreatorName == "Unknown")
            {
                varMeta.CreatorName = varFileName.Creator;
            }
            if (varMeta.PackageName == "Unknown")
            {
                varMeta.PackageName = varFileName.Package;
            }
            if (varFileName.Creator != varMeta.CreatorName)
            {
                varFileName.Creator = varMeta.CreatorName;
                result.Status = "Renamed";
            }

            if (varFileName.Package != varMeta.PackageName)
            {
                varFileName.Package = varMeta.PackageName;
                result.Status = "Renamed";
            }

            if (varFileName.OriginalFileName != varFileName.NewFileName)
            {
                result.NewName = varFileName.NewFileName;
                result.Status = "Renamed";
                renamePlan.Add(new RenameInfo(file, result.NewName));
            }
            else
            {
                result.Status = "Same";
            }

            Results.Add(result);
        }

        DetectConflicts(renamePlan);
        ExecuteRenames(renamePlan);
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

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        SaveWindowSize();
        SaveColumnWidths();
    }
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