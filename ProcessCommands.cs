using System.Diagnostics;
using System.Text.RegularExpressions;
using TidyHPC.Routers.Args;

namespace WindowsCommonCLI;
/// <summary>
/// 进程命令
/// </summary>
public class ProcessCommands
{
    /// <summary>
    /// 杀死进程
    /// </summary>
    /// <param name="regexString"></param>
    /// <returns></returns>
    public static async Task Kill(
    [ArgsIndex] string regexString)
    {
        await Task.CompletedTask;
        var regex = new Regex(regexString, RegexOptions.IgnoreCase);
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            if (regex.IsMatch(process.ProcessName))
            {
                process.Kill();
            }
        }
    }
}
