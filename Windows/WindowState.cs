using TidyHPC.Extensions;
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
    public async Task<bool> IsMatch(Cache cache) => await IsMatchV2(cache);

    /// <summary>
    /// 是否匹配
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public async Task<bool> IsMatchV1(Cache cache)
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
        foreach (var window in windows)
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

    /// <summary>
    /// 是否匹配
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public async Task<bool> IsMatchV2(Cache cache)
    {
        int interval = 0;
        int timeout = 0;
        List<Window> rules = [];
        if (Target.IsObject)
        {
            rules.Add(Target);
        }
        else if (Target.IsArray)
        {
            Target.ForeachArray(item => rules.Add(item));
        }
        foreach (var window in rules)
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
            Win32.WindowInterface[]? matchResult = null;
            Win32.WindowInterface[] lastWindows = cache.CloneTopWindows();
            var firsRule = rules.First();
            var nextRules = rules.Skip(1).ToArray();
            foreach (var window in lastWindows)
            {
                if (firsRule.IsMatch(window))
                {
                    Match(nextRules, [window], x =>
                    {
                        matchResult = x;
                        return MatchFlag.StopMatch;
                    });
                }
            }
            if (matchResult != null)
            {
                for(int i = 0; i < rules.Count; i++)
                {
                    var rule = rules[i];
                    var window = matchResult[i];
                    window.InitializeInfomation();
                    rule.Target.Set("Window", window.Target);
                }
                return true;
            }
            if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(timeout))
            {
                return false;
            }
            await Task.Delay(interval);
        }
    }

    /// <summary>
    /// 匹配所有窗口
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="parentWindows"></param>
    /// <param name="onMatch"></param>
    public static void Match(Window[] rules, Win32.WindowInterface[] parentWindows, Func<Win32.WindowInterface[],MatchFlag> onMatch)
    {
        var windows = parentWindows.Last().GetChildren();
        var firstRule = rules.First();
        var nextRules = rules.Skip(1).ToArray();
        foreach (var item in windows)
        {
            if (firstRule.IsMatch(item))
            {
                var nextWindows = item.GetChildren();
                Win32.WindowInterface[] result = [.. parentWindows, item];
                if (nextRules.Length == 0)
                {
                    if (onMatch(result)== MatchFlag.StopMatch)
                    {
                        return;
                    }
                }
                else
                {
                    Match(nextRules, result, onMatch);
                }
            }
        }
    }
}
