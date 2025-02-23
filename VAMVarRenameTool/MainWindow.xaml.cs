using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using VAMVarRenameTool.MetaData;
using VAMVarRenameTool.EventSystem;
using VAMVarRenameTool.NameTransform;

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

        foreach (var file in files)
        {
            var result = new FileResult {OriginalPath = file};
            VarMeta varMeta = new VarMeta();
            Core.Instance.VarMetaProcessor.ParseFromVarFile(file, ref varMeta);
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