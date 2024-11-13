using System.Text.RegularExpressions;
using TidyHPC.LiteJson;
using TidyWin32;

namespace WindowsCommonCLI.Windows;

/// <summary>
/// 路径项
/// </summary>
public class Window(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target { get; } = target;

    /// <summary>
    /// Implicit convert to PathItem
    /// </summary>
    /// <param name="json"></param>
    public static implicit operator Window(Json json)
    {
        return new Window(json);
    }

    /// <summary>
    /// Implicit convert to Json
    /// </summary>
    /// <param name="item"></param>
    public static implicit operator Json(Window item)
    {
        return item.Target;
    }

    /// <summary>
    /// 转字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Target.ToString();
    }

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title
    {
        get => Target.Read(nameof(Title), string.Empty);
    }

    /// <summary>
    /// 文本
    /// </summary>
    public string Text => Target.Read(nameof(Text), string.Empty);

    /// <summary>
    /// 类名
    /// </summary>
    public string ClassName => Target.Read(nameof(ClassName), string.Empty);

    /// <summary>
    /// 启用
    /// </summary>
    public string Enable => Target.Read(nameof(Enable), string.Empty);

    /// <summary>
    /// 是否满足
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public bool IsMatch(Win32.WindowInterface window)
    {
        if (string.IsNullOrEmpty(Title)==false)
        {
            var regex = new Regex(Title);
            if (!regex.IsMatch(window.WindowText))
            {
                return false;
            }
        }
        if (string.IsNullOrEmpty(Text) == false)
        {
            var regex = new Regex(Text);
            if (!regex.IsMatch(window.WindowText))
            {
                return false;
            }
        }
        if (string.IsNullOrEmpty(ClassName) == false)
        {
            var regex = new Regex(ClassName);
            if (!regex.IsMatch(window.ClassName))
            {
                return false;
            }
        }
        if (string.IsNullOrEmpty(Enable) == false)
        {
            var enale = Enable == "true";
            if (window.Enable != enale)
            {
                return false;
            }
        }
        return true;
    }
}
