using System.Diagnostics;
using TidyHPC.Extensions;

namespace WindowsCommonCLI;

/// <summary>
/// 7z 压缩解压缩
/// </summary>
public class Compress7z
{
    /// <summary>
    /// 7z
    /// </summary>
    public Compress7z()
    {
        FilePath = Path.GetTempFileName();
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    /// <summary>
    /// 7z
    /// </summary>
    /// <param name="path"></param>
    public Compress7z(string path)
    {
        FilePath = path;
    }

    /// <summary>
    /// 压缩包文件路径
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 获取7z.exe路径
    /// </summary>
    /// <returns></returns>
    public static string Get7zPath()
    {
        char[] disks = "CDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        foreach (var disk in disks)
        {
            string path = $"{disk}:\\Program Files\\7-Zip\\7z.exe";
            if (File.Exists(path))
            {
                return path;
            }
        }
        string[] relativePaths =
        [
            "Api/7z.exe",
            "bin/7z.exe",
            "data/7z.exe",
            "7z.exe"
        ];
        var rootDirectory = Path.GetDirectoryName(Environment.ProcessPath);
        foreach (var relativePath in relativePaths)
        {
            var result = Path.Combine(rootDirectory ?? "", relativePath);
            if (File.Exists(result))
            {
                return result;
            }
        }
        throw new FileNotFoundException("7z.exe not found");
    }

    /// <summary>
    /// 添加文件到压缩包
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public async Task Add(IEnumerable<string> paths)
    {
        ProcessStartInfo info = new();
        info.FileName = Get7zPath();
        info.ArgumentList.Add("a");
        info.ArgumentList.Add("-t7z");
        info.ArgumentList.Add(FilePath);
        paths.Foreach(info.ArgumentList.Add);
        var process = Process.Start(info);
        if (process == null) return;
        await process.WaitForExitAsync();
    }

    /// <summary>
    /// 添加文件到压缩包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task Add(string path) => await Add(new string[] { path });

    /// <summary>
    /// 解压到指定目录
    /// </summary>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public async Task Extract(string outputDirectory)
    {
        ProcessStartInfo info = new();
        info.FileName = Get7zPath();
        info.ArgumentList.Add("x");
        info.ArgumentList.Add(FilePath);
        info.ArgumentList.Add("-o" + outputDirectory);
        var process = Process.Start(info);
        if (process == null) return;
        await process.WaitForExitAsync();
    }

    private record Range(int Start, int Length)
    {
        public int End => Start + Length;

        public static Range Empty => new(0, 0);

        public string Substring(string value) => Length < 0 ? value.Substring(Start) : value.Substring(Start, Length);
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="DateTime"></param>
    /// <param name="Attribute"></param>
    /// <param name="Size"></param>
    /// <param name="Compressed"></param>
    /// <param name="Name"></param>
    public record Entry(DateTime DateTime, string Attribute, long Size, long Compressed, string Name);

    /// <summary>
    /// 获取所有入口
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Entry>> GetEntries()
    {
        ProcessStartInfo info = new();
        info.FileName = Get7zPath();
        info.ArgumentList.Add("l");
        info.ArgumentList.Add(FilePath);
        info.RedirectStandardOutput = true;
        var output = new List<Entry>();

        using (var process = Process.Start(info))
        {
            if (process == null) return Enumerable.Empty<Entry>();
            bool isInEntries = false;
            var reader = process.StandardOutput;
            string? line;
            Range dateTimeRange = Range.Empty;
            Range attributeRange = Range.Empty;
            Range sizeRange = Range.Empty;
            Range compressedRange = Range.Empty;
            Range nameRange = Range.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                var trimLine = line.Replace(" ", "");
                if (!isInEntries)
                {
                    if (trimLine.Length > 2 && trimLine.All(x => x == '-'))
                    {
                        isInEntries = true;
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            dateTimeRange = new(0, parts[0].Length);
                            attributeRange = new(dateTimeRange.End + 1, parts[1].Length);
                            sizeRange = new(attributeRange.End + 1, parts[2].Length);
                            compressedRange = new(sizeRange.End + 1, parts[3].Length);
                            nameRange = new(compressedRange.End + 1, -1);
                        }
                    }
                }
                else
                {
                    if (trimLine.Length > 2 && trimLine.All(x => x == '-'))
                    {
                        break;
                    }
                    else
                    {
                        var dateTimeString = dateTimeRange.Substring(line);
                        var attributeString = attributeRange.Substring(line);
                        var sizeString = sizeRange.Substring(line).Trim();
                        var compressedString = compressedRange.Substring(line).Trim();
                        var nameString = nameRange.Substring(line).Trim();
                        output.Add(new Entry(
                            DateTime.Parse(dateTimeString),
                            attributeString,
                            long.Parse(sizeString),
                            compressedString.Length == 0 ? 0 : long.Parse(compressedString),
                            nameString));
                    }
                }
            }
            await process.WaitForExitAsync();
        }
        return output;
    }

    /// <summary>
    /// 移除文件
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public async Task Remove(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            ProcessStartInfo info = new();
            info.FileName = Get7zPath();
            info.ArgumentList.Add("d");
            info.ArgumentList.Add(FilePath);
            info.ArgumentList.Add(path);
            var process = Process.Start(info);
            if (process == null) continue;
            await process.WaitForExitAsync();
        }
    }

    /// <summary>
    /// 解压指定入口到指定目录
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public async Task ExtractEntry(Entry entry, string outputDirectory)
    {
        ProcessStartInfo info = new();
        info.FileName = Get7zPath();
        info.ArgumentList.Add("e");
        info.ArgumentList.Add(FilePath);
        info.ArgumentList.Add("-o" + outputDirectory);
        info.ArgumentList.Add(entry.Name);
        var process = Process.Start(info);
        if (process == null) return;
        await process.WaitForExitAsync();
    }
}