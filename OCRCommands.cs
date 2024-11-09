using TidyHPC.Routers.Args;
using TypeSharp.System;

namespace WindowsCommonCLI;

/// <summary>
/// OCR commands
/// </summary>
public class OCRCommands
{
    /// <summary>
    /// 识别图片中的文本
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task Detect(
        [ArgsIndex]string imagePath,
        [ArgsIndex]string outputPath = "")
    {
        await Task.CompletedTask;
        var ocrPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "data", "ocr", "ocr.exe");
        context.exec(ocrPath, "detect", imagePath, outputPath);
    }
}
