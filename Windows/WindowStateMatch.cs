using TidyHPC.LiteJson;
using TidyHPC.Locks;

namespace WindowsCommonCLI.Windows;

/// <summary>
/// State Machine
/// </summary>
public class WindowStateMatch(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicit convert to StateMachine
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator WindowStateMatch(Json target) => new(target);

    /// <summary>
    /// Implicit convert to Json
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator Json(WindowStateMatch target) => target.Target;

    /// <summary>
    /// 匹配
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public async Task<string[]> Match(Cache cache)
    {
        Lock<List<string>> result = new([]);
        List<Task> tasks = [];
        foreach(var pair in Target.GetObjectEnumerable())
        {
            var stateKey = pair.Key;
            WindowState state = pair.Value;
            tasks.Add(Task.Run(async () =>
            {
                var stateResult = await state.IsMatch(cache);
                if (stateResult)
                {
                    result.Process(x => x.Add(stateKey));
                }
            }));
        }
        await Task.WhenAny(tasks);
        await Task.WhenAll(Task.WhenAll(tasks), Task.Delay(1000));
        return result.Value.ToArray();
    }
}
