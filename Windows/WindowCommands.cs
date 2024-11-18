using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using TidyHPC.LiteJson;
using TidyHPC.Routers;
using TidyHPC.Routers.Args;
using TidyWin32;

namespace WindowsCommonCLI.Windows;

/// <summary>
/// 所有Windows相关的操作
/// </summary>
public class WindowCommands
{
    /// <summary>
    /// 列举所有窗口
    /// </summary>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task List([ArgsIndex] string? outputPath = null)
    {
        await Task.CompletedTask;
        var windows = Win32.WindowInterface.GetWindows();
        foreach (var window in windows)
        {
            window.InitializeInfomation();
        }
        Json result = Json.NewArray();
        foreach (var window in windows)
        {
            result.Add(window);
        }
        if (outputPath != null)
        {
            result.Save(outputPath);
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    }

    /// <summary>
    /// 列举所有用户窗口
    /// </summary>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task ListUser([ArgsIndex] string? outputPath = null)
    {
        await Task.CompletedTask;
        var windows = Win32.WindowInterface.GetWindows();
        foreach (var window in windows)
        {
            window.InitializeInfomation();
        }
        Json result = Json.NewArray();
        foreach (var window in windows)
        {
            if (window.IsUserWindow())
            {
                result.Add(window.Clone());
            }
        }
        if (outputPath != null)
        {
            result.Save(outputPath);
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    }

    /// <summary>
    /// 列举所有的子窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task ListChildren(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string? outputPath = null)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        var children = window.GetChildren();
        foreach (var child in children)
        {
            child.InitializeInfomation();
        }
        Json result = Json.NewArray();
        foreach (var child in children)
        {
            result.Add(child.Clone());
        }
        if (outputPath != null)
        {
            result.Save(outputPath);
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    }

    /// <summary>
    /// 点击窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="method"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static async Task Click(
        [ArgsIndex] string hWnd,
        [ArgsAliases("--method")] int method = 0,
        [ArgsAliases("--count")] int count = 1)
    {
        await Task.CompletedTask;
        if(count <= 0)
        {
            count = 1;
        }
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        for (int i = 0; i < count; i++)
        {
            switch (method)
            {
                case 0:
                    window.WM_Click();
                    break;
                case 1:
                    window.BM_Click();
                    break;
                case 2:
                    window.MOUSEEVENTF_Click();
                    break;
                case 3:
                    window.SendInput_Click();
                    break;
            }
        }
    }

    /// <summary>
    /// 右键点击窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static async Task RightClick(
        [ArgsIndex] string hWnd,
        [ArgsAliases("--count")] int count = 1)
    {
        await Task.CompletedTask;
        if(count <= 0)
        {
            count = 1;
        }
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        for (int i = 0; i < count; i++)
        {
            window.WM_RightClick();
        }
    }

    /// <summary>
    /// 设置窗口文本
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="text"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task SetText(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string text,
        [ArgsAliases("--type")] string type = "window-text")
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        if (type == "window-text")
        {
            window.WindowText = text;
        }
        else if (type == "text")
        {
            window.Text = text;
        }
    }

    /// <summary>
    /// 根据窗口标题查找窗口
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task FindWindowText(
        [ArgsIndex] string regex,
        [ArgsIndex] string? outputPath = null)
    {
        await Task.CompletedTask;
        var regexInstance = new Regex(regex);
        var windows = Win32.WindowInterface.GetWindows();
        Win32.WindowInterface? findedWindow = null;
        foreach (var window in windows)
        {
            if (regexInstance.IsMatch(window.WindowText))
            {
                findedWindow = window;
                break;
            }
        }
        if (outputPath != null)
        {
            if (findedWindow != null)
            {
                findedWindow.InitializeInfomation();
                findedWindow.Target.Save(outputPath);
            }
            else
            {
                File.WriteAllText(outputPath, "{}", Util.UTF8);
            }
        }
        else
        {
            Console.WriteLine(findedWindow?.ToString());
        }
    }

    /// <summary>
    /// 根据窗口标题查找子窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="regexString"></param>
    /// <param name="outputPath"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task FindChildWindow(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string regexString,
        [ArgsIndex] string? outputPath = null,
        [ArgsAliases("--type")] string? type = null)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        var regex = new Regex(regexString);
        var children = window.GetChildren();
        Win32.WindowInterface? findedWindow = null;
        foreach (var child in children)
        {
            if (type == "window-text")
            {
                if (regex.IsMatch(child.WindowText))
                {
                    findedWindow = child;
                    break;
                }
            }
            else if (type == "text")
            {
                if (regex.IsMatch(child.Text))
                {
                    findedWindow = child;
                    break;
                }
            }
            else if (type == "class-name")
            {
                if (regex.IsMatch(child.ClassName))
                {
                    findedWindow = child;
                    break;
                }
            }
            else
            {
                if (regex.IsMatch(child.WindowText) || regex.IsMatch(child.Text) || regex.IsMatch(child.ClassName))
                {
                    findedWindow = child;
                    break;
                }
            }
        }
        if (outputPath != null)
        {
            if (findedWindow != null)
            {
                findedWindow.InitializeInfomation();
                findedWindow.Target.Save(outputPath);
            }
            else
            {
                File.WriteAllText(outputPath, "{}", Util.UTF8);
            }
        }
        else
        {
            Console.WriteLine(findedWindow?.ToString());
        }
    }

    /// <summary>
    /// 选中ComboBox的某个选项
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static async Task SelectComboboxIndex(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string index)
    {
        await Task.CompletedTask;
        Win32.ComboBoxInterface comboBox = Util.ConvertStringToIntptr(hWnd);
        comboBox.SelectedIndex = int.Parse(index);
    }

    /// <summary>
    /// 根据文本选中ComboBox的某个选项
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static async Task SelectComboboxText(
        [ArgsIndex] string hWnd,
        [SubArgs] string[] items)
    {
        await Task.CompletedTask;
        Regex[] regexes = items.Select(x => new Regex(x)).ToArray();
        Win32.ComboBoxInterface comboBox = Util.ConvertStringToIntptr(hWnd);
        var comboboxItems = comboBox.Items;
        for (int i = 0; i < comboboxItems.Length; i++)
        {
            if(regexes.Where(x => x.IsMatch(comboboxItems[i])).Count() != 0)
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }
        Console.WriteLine("未找到匹配的选项");
        Console.WriteLine($"选项：{string.Join(",", items)}");
        Console.WriteLine($"ComboBox选项：{string.Join(",", comboboxItems)}");
    }

    /// <summary>
    /// 获取Window的信息
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task GetWindow(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string outputPath)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        window.InitializeInfomation();
        window.Target.Save(outputPath);
    }

    /// <summary>
    /// 发送按键
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static async Task SendKeys(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string keys)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        window.SendKeys(keys);
    }

    /// <summary>
    /// 发送文本
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static async Task SendText(
        [ArgsIndex] string text)
    {
        await Task.CompletedTask;
        Win32.WindowInterface.SendText(text);
    }

    /// <summary>
    /// 模拟键盘操作
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static async Task Keyboard(
        [ArgsIndex] string keys,
        [ArgsAliases("--delay")]string delay="")
    {
        await Task.CompletedTask;
        int delayTime = 0;
        if (delay != "")
        {
            delayTime = int.Parse(delay);
        }
        var keyList = keys.Split(",");
        var enumNames = Enum.GetNames(typeof(Win32.Keys));
        foreach (var key in keyList)
        {
            if (int.TryParse(key, out int keyCode))
            {
                Win32.KeyboardInterface.SendKey((Win32.Keys)keyCode);
            }
            else
            {
                var enumName = enumNames.FirstOrDefault(x => x.ToLower() == key.ToLower());
                if (enumName != null)
                {
                    Win32.KeyboardInterface.SendKey((Win32.Keys)Enum.Parse(typeof(Win32.Keys), enumName));
                }
            }
            if (delayTime > 0) await Task.Delay(delayTime);
        }
    }

    /// <summary>
    /// 激活英文键盘
    /// </summary>
    /// <returns></returns>
    public static async Task ActiveEnglishKeyboard()
    {
        await Task.CompletedTask;
        Win32.WindowInterface.ActiveEnglishKeyboard();
    }

    /// <summary>
    /// Window状态机
    /// </summary>
    /// <param name="inputPath"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task MatchWindow(
        [ArgsIndex]string inputPath,
        [ArgsIndex]string? outputPath=null)
    {
        WindowStateMatch stateMachine = Json.Load(inputPath);
        Cache cache = new();
        cache.TopWindows = Win32.WindowInterface.GetWindows();
        Json result = Json.NewObject();
        var hitKeys = await stateMachine.Match(cache);
        foreach (var key in hitKeys)
        {
            result[key] = stateMachine.Target[key];
        }
        if (outputPath != null)
        {
            result.Save(outputPath);
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    }

    /// <summary>
    /// 关闭Window
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    public static async Task CloseWindow(
        [ArgsIndex]string hWnd)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        window.Close();
    }

    /// <summary>
    /// 鼠标移动
    /// </summary>
    /// <param name="deltaX"></param>
    /// <param name="deltaY"></param>
    /// <returns></returns>
    public static async Task MouseMove(
        [ArgsIndex]string deltaX,
        [ArgsIndex]string deltaY)
    {
        await Task.CompletedTask;
        Win32.MouseInterface.Move(int.Parse(deltaX), int.Parse(deltaY));
    }

    /// <summary>
    /// 鼠标移动
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static async Task MouseMoveTo(
        [ArgsIndex] string x,
        [ArgsIndex] string y)
    {
        await Task.CompletedTask;
        Win32.MouseInterface.MoveTo(int.Parse(x), int.Parse(y));
    }

    /// <summary>
    /// 鼠标移动
    /// </summary>
    /// <returns></returns>
    public static async Task MouseClick()
    {
        await Task.CompletedTask;
        Win32.MouseInterface.Click();
    }

    /// <summary>
    /// 鼠标点击指定窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    public static async Task MouseClickWindow(
        [ArgsIndex] string hWnd)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        var location = window.Location;
        var size = window.Size;
        Win32.MouseInterface.MoveTo(location.X + size.Width / 2, location.Y + size.Height / 2);
        Win32.MouseInterface.Click();
    }

    /// <summary>
    /// 鼠标点击指定窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="xRatio"></param>
    /// <param name="yRatio"></param>
    /// <param name="delay"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static async Task MouseClickWindowAtRatio(
        [ArgsIndex] string hWnd,
        [ArgsIndex]string xRatio,
        [ArgsIndex]string yRatio,
        [ArgsAliases("--delay")] string delay = "",
        [ArgsAliases("--method")] string method="")
    {
        await Task.CompletedTask;
        int delayTime = 0;
        if (delay != "")
        {
            delayTime = int.Parse(delay);
        }
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        var location = window.Location;
        var size = window.Size;
        var x = location.X + size.Width * double.Parse(xRatio);
        var y = location.Y + size.Height * double.Parse(yRatio);
        window.SetForeground();
        if (delayTime > 0) await Task.Delay(delayTime);
        Win32.MouseInterface.MoveTo((int)x, (int)y);
        if (delayTime > 0) await Task.Delay(delayTime);
        window.Focus();
        if (delayTime > 0) await Task.Delay(delayTime);
        if (method == "" || method == "send-input" || method == "0")
        {
            Win32.MouseInterface.Click();
        }
        else if(method == "1" || method == "mouse-event")
        {
            Win32.MouseInterface.Click2();
        }
    }

    /// <summary>
    /// 鼠标点击指定窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="xDelta"></param>
    /// <param name="yDelta"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static async Task MouseClickWindowAt(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string xDelta,
        [ArgsIndex] string yDelta,
        [ArgsAliases("--delay")] string delay="")
    {
        int delayTime = 0;
        if (delay != "")
        {
            delayTime = int.Parse(delay);
        }
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        var location = window.Location;
        var size = window.Size;
        var x = location.X + int.Parse(xDelta);
        var y = location.Y + int.Parse(yDelta);
        window.SetForeground();
        if(delayTime > 0) await Task.Delay(delayTime);
        Win32.MouseInterface.MoveTo((int)x, (int)y);
        if (delayTime > 0) await Task.Delay(delayTime);
        Win32.MouseInterface.Click();
    }

    /// <summary>
    /// 鼠标点击指定窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    public static async Task FocusWindow(
        [ArgsIndex] string hWnd)
    {
        await Task.CompletedTask;
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        window.Focus();
    }

    /// <summary>
    /// OCR窗口
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task OcrWindow(
        [ArgsIndex] string hWnd,
        [ArgsIndex] string? outputPath=null)
    {
        Win32.WindowInterface window = Util.ConvertStringToIntptr(hWnd);
        using var bmp = window.CaptureForeground();
        var imagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
        bmp.Save(imagePath,ImageFormat.Png);
        var tesseractPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "data", "tesseract", "tesseract.exe");
        if (File.Exists(tesseractPath) == false)
        {
            var archivePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "data", "tesseract.7z");
            var extractPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "data", "tesseract");
            if(File.Exists(archivePath))
            {
                await CompressCommands.Extract(archivePath, extractPath);
            }
        }
        if (File.Exists(tesseractPath) == false)
        {
            throw new Exception("tesseract.exe 不存在");
        }
        var process = new Process();
        var tempPath = Path.GetTempFileName();
        process.StartInfo.FileName = tesseractPath;
        process.StartInfo.ArgumentList.Add(imagePath);
        process.StartInfo.ArgumentList.Add(tempPath);
        process.StartInfo.ArgumentList.Add("-l");
        process.StartInfo.ArgumentList.Add("eng+chi_sim");
        process.StartInfo.ArgumentList.Add("tsv");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.OutputDataReceived += (sender, e) =>
        {
            //Console.WriteLine(e.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        var tsvLines = File.ReadAllLines(tempPath, Util.UTF8);
        Json result = Json.NewObject();
        var blocks = result.GetOrCreateArray("blocks");
        foreach (var line in tsvLines)
        {
            var items = line.Split('\t');
            if (items.Length == 12)
            {
                var block = Json.NewObject();
                block["level"] = items[0];
                block["page_num"] = items[1];
                block["block_num"] = items[2];
                block["par_num"] = items[3];
                block["line_num"] = items[4];
                block["word_num"] = items[5];
                block["left"] = items[6];
                block["top"] = items[7];
                block["width"] = items[8];
                block["height"] = items[9];
                block["conf"] = items[10];
                block["text"] = items[11];
                blocks.Add(block);
            }
        }
        if (outputPath != null)
        {
            result.Save(outputPath);
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    } 
}
