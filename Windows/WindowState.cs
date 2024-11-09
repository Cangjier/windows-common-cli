using TidyHPC.LiteJson;
using TidyWin32;

namespace WindowsCommonCLI.Windows;

/// <summary>
/// 状态
/// </summary>
/// <param name="target"></param>
public class WindowState(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicit convert to State
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator WindowState(Json target) => new(target);

    /// <summary>
    /// 是否匹配
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public async Task<bool> IsMatch(Cache cache)
    {
        int interval = 0;
        int timeout = 0;
        List<Window> windows = [];
        if (Target.IsObject)
        {
            windows.Add(Target);
        }
        else if (Target.IsArray)
        {
            Target.ForeachArray(item => windows.Add(item));
        }
        foreach(var window in windows)
        {
            var target = window.Target;
            if (target.ContainsKey("Interval"))
            {
                interval = target.Read("Interval", 0);
            }
            if (target.ContainsKey("Timeout"))
            {
                timeout = target.Read("Timeout", 0);
            }
        }
        
        var startTime = DateTime.Now;
        while (true)
        {
            Win32.WindowInterface lastWindow;
            Win32.WindowInterface[] lastWindows = cache.CloneTopWindows();
            bool matched = true;
            foreach (var item in windows)
            {
                bool finded = false;
                foreach (var window in lastWindows)
                {
                    if (item.IsMatch(window))
                    {
                        finded = true;
                        window.InitializeInfomation();
                        item.Target.Set("Window", window.Target);
                        lastWindow = window;
                        lastWindows = window.GetChildren();
                        break;
                    }
                }
                if (!finded)
                {
                    matched = false;
                    break;
                }
            }
            if (matched) return true;
            if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(timeout))
            {
                return false;
            }
            await Task.Delay(interval);
        }
    }
}
