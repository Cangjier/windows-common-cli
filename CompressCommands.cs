using TidyHPC.Routers.Args;

namespace WindowsCommonCLI;

/// <summary>
/// 压缩命令
/// </summary>
public class CompressCommands
{
    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="compressFilePath"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public static async Task Extract(
    [ArgsIndex] string compressFilePath,
    [ArgsIndex] string? outputDirectory = null)
    {
        Compress7z file = new(compressFilePath);
        if (outputDirectory == null)
        {
            outputDirectory = Path.Combine(Path.GetDirectoryName(compressFilePath) ?? "", Path.GetFileNameWithoutExtension(compressFilePath));
        }
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        await file.Extract(outputDirectory);
    }

    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="compressFilePath"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static async Task Compress(
           [ArgsIndex] string compressFilePath,
           [SubArgs] string[] paths)
    {
        if (Path.IsPathRooted(compressFilePath) == false)
        {
            compressFilePath = Path.Combine(Environment.CurrentDirectory, compressFilePath);
        }
        Compress7z file = new(compressFilePath);
        await file.Add(paths);
    }
}
