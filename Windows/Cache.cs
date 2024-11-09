using System.Collections.Concurrent;
using TidyWin32;

namespace WindowsCommonCLI.Windows;

/// <summary>
/// 缓存
/// </summary>
public class Cache
{
    /// <summary>
    /// 所有缓存的窗口
    /// </summary>
    public Win32.WindowInterface[] TopWindows { get; set; } = [];

    /// <summary>
    /// Clone top windows
    /// </summary>
    /// <returns></returns>
    public Win32.WindowInterface[] CloneTopWindows()
    {
        var result = new Win32.WindowInterface[TopWindows.Length];
        for (int i = 0; i < TopWindows.Length; i++)
        {
            try
            {
                result[i] = TopWindows[i].Target.Clone();
            }
            catch
            {
                Console.WriteLine($"result.Length={result.Length},TopWindows.Length={TopWindows.Length},i={i}");
            }
            
        }
        return result;
    }
}
