using TidyHPC.Routers.Args;

namespace WindowsCommonCLI;

/// <summary>
/// Markdown commands
/// </summary>
public class MarkdownCommands
{
    /// <summary>
    /// 对标题进行增加级别
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task Increase([ArgsIndex] string? inputPath = null,
    [ArgsIndex] string? outputPath = null)
    {
        string text;
        if (inputPath == null)
        {
            text = await Util.GetClipboardText();
        }
        else
        {
            text = File.ReadAllText(inputPath, Util.UTF8);
        }
        var lines = text.Replace("\r", "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith('#'))
            {
                var count = lines[i].TakeWhile(x => x == '#').Count();
                lines[i] = string.Concat(new string('#', count + 1), lines[i].AsSpan(count));
            }
        }
        if (outputPath == null)
        {
            await Util.SetClipboardText(string.Join("\r\n", lines));
        }
        else
        {
            File.WriteAllText(outputPath, string.Join("\r\n", lines), Util.UTF8);
        }
    }
}
