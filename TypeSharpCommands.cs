using System.Text;
using TidyHPC.LiteJson;
using TidyHPC.Routers.Args;
using TypeSharp;
using TypeSharp.System;

namespace WindowsCommonCLI;

/// <summary>
/// TypeSharp commands
/// </summary>
public class TypeSharpCommands
{
    /// <summary>
    /// 运行脚本
    /// </summary>
    /// <param name="scriptath"></param>
    /// <returns></returns>
    public static async Task Run(
    [ArgsIndex] string scriptath)
    {
        if (Path.IsPathRooted(scriptath) == false)
        {
            scriptath = Path.GetFullPath(scriptath);
        }
        Directory.SetCurrentDirectory(Path.GetDirectoryName(scriptath) ?? "");
        context.args = Self.args.Skip(2).ToArray();
        context.script_path = scriptath;
        TSScriptEngine.Run(File.ReadAllText(scriptath, Util.UTF8));
        await Task.CompletedTask;
    }

    /// <summary>
    /// 批处理
    /// </summary>
    /// <param name="scriptath"></param>
    /// <returns></returns>
    public static async Task Batch([ArgsIndex] string scriptath)
    {
        try
        {
            await Task.CompletedTask;
            if (Path.IsPathRooted(scriptath) == false)
            {
                scriptath = Path.GetFullPath(scriptath);
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(scriptath) ?? "");
            context.args = Self.args.Skip(2).ToArray();
            context.script_path = scriptath;
            var lines = File.ReadAllLines(scriptath, Util.UTF8);
            StringBuilder base64Temp = new();
            bool isContainsExit = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "exit")
                {
                    isContainsExit = true;
                }
                else if (isContainsExit)
                {
                    base64Temp.Append(line);
                }
            }
            string base64 = base64Temp.ToString();
            int scriptType = 0; // 0: base64, 1: aes
            if(base64.ToString().Contains(' '))
            {
                var key = base64[..base64.IndexOf(' ')];
                if (key?.ToLower() == "aes")
                {
                    scriptType = 1;
                    base64 = base64[(base64.IndexOf(' ') + 1)..];
                }
                else if (key?.ToLower() == "base64")
                {
                    scriptType = 0;
                    base64 = base64[(base64.IndexOf(' ') + 1)..];
                }
                else
                {
                    throw new Exception("未知的脚本类型");
                }
            }
            string manifestString = string.Empty;
            if (scriptType == 1)
            {
                var scriptSecretPath = Path.Combine(Path.GetDirectoryName(scriptath) ?? "", Path.GetFileNameWithoutExtension(scriptath) + ".secret");
                string? password = null;
                if (File.Exists(scriptSecretPath))
                {
                    password = File.ReadAllText(scriptSecretPath, Util.UTF8);
                }
                else
                {
                    Console.WriteLine("请输入密码：");
                    password = Console.ReadLine();
                }
                if (password == null)
                {
                    Console.WriteLine("密码不能为空");
                    return;
                }

                manifestString = Util.AesDecrypt(base64.ToString(), password, Self.aesKey);
                if (Json.TryParse(manifestString, out var manifest))
                {
                    string script = string.Empty;
                    manifest.ForeachObject((key, value) =>
                    {
                        var content = Util.UTF8.GetString(Convert.FromBase64String(value.AsString));
                        if (key == "main.ts" || key == "index.ts")
                        {
                            script = content;
                        }
                        context.manifest.Add(key, content);
                    });
                    TSScriptEngine.Run(script);
                }
                else
                {
                    Console.WriteLine("密码错误(2)");
                }
            }
            else if (scriptType==0)
            {
                manifestString = Util.UTF8.GetString(Convert.FromBase64String(base64));
                if (Json.TryParse(manifestString, out var manifest))
                {
                    string script = string.Empty;
                    manifest.ForeachObject((key, value) =>
                    {
                        var content = Util.UTF8.GetString(Convert.FromBase64String(value.AsString));
                        if (key == "main.ts" || key == "index.ts")
                        {
                            script = content;
                        }
                        context.manifest.Add(key, content);
                    });
                    TSScriptEngine.Run(script);
                }
                else
                {
                    Console.WriteLine("文件错误");
                }
            }
            
        }
        catch (Exception e)
        {
            var loggerPath = Path.Combine(Path.GetDirectoryName(scriptath) ?? "", Path.GetFileNameWithoutExtension(scriptath) + ".error");
            File.WriteAllText(loggerPath, e.ToString(), Util.UTF8);
            throw;
        }
    }

    /// <summary>
    /// 打包
    /// </summary>
    /// <param name="inputDirectory"></param>
    /// <returns></returns>
    public static async Task Package([ArgsIndex] string inputDirectory)
    {
        await Task.CompletedTask;
        string? isEncrypt = null;
        while (isEncrypt != "Y" && isEncrypt != "N")
        {
            Console.WriteLine("是否需要加密？(Y/N)");
            isEncrypt = Console.ReadLine();
        }
        Json manifest = Json.NewObject();
        var files = Directory.GetFiles(inputDirectory);
        foreach (var file in files)
        {
            manifest.Set(Path.GetFileName(file), Convert.ToBase64String(File.ReadAllBytes(file)));
        }
        var manifestString = manifest.ToString();
        if (isEncrypt == "Y")
        {
            var encryptResult = Util.AesEncrypt(manifestString, Self.aesKey);
            var script = $"""
            "%~dp0wcl.exe" bat "%~f0" %*
            pause
            exit
            AES {encryptResult.Base64EncryptedContent}
            """;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(inputDirectory) ?? "", Path.GetFileName(inputDirectory) + ".bat"), script, Util.UTF8);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(inputDirectory) ?? "", Path.GetFileName(inputDirectory) + ".secret"), encryptResult.Base64Iv, Util.UTF8);
            Console.WriteLine($"密码是：{encryptResult.Base64Iv}");
        }
        else
        {
            var encode = Convert.ToBase64String(Util.UTF8.GetBytes(manifestString));
            var script = $"""
            "%~dp0wcl.exe" bat "%~f0" %*
            pause
            exit
            Base64 {encode}
            """;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(inputDirectory) ?? "", Path.GetFileName(inputDirectory) + ".bat"), script, Util.UTF8);
        }
    }
}
