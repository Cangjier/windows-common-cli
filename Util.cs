using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TidyConsole;
using TidyWin32;

namespace WindowsCommonCLI;
internal class Util
{
    /// <summary>
    /// AES加密结果
    /// </summary>
    public struct AesEncryptResult
    {
        /// <summary>
        /// Base64且加密后的内容
        /// </summary>
        public string Base64EncryptedContent { get; set; }

        /// <summary>
        /// Base64后的IV
        /// </summary>
        public string Base64Iv { get; set; }
    }

    public static AesEncryptResult AesEncrypt(string value,Guid key)
    {
        using var aes = Aes.Create();
        aes.Key = key.ToByteArray();
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        byte[] data = Encoding.UTF8.GetBytes(value);
        byte[] result = encryptor.TransformFinalBlock(data, 0, data.Length);
        return new AesEncryptResult
        {
            Base64EncryptedContent = Convert.ToBase64String(result),
            Base64Iv = Convert.ToBase64String(aes.IV)
        };
    }

    public static AesEncryptResult AesEncrypt(byte[] value, Guid key)
    {
        using var aes = Aes.Create();
        aes.Key = key.ToByteArray();
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        byte[] result = encryptor.TransformFinalBlock(value, 0, value.Length);
        return new AesEncryptResult
        {
            Base64EncryptedContent = Convert.ToBase64String(result),
            Base64Iv = Convert.ToBase64String(aes.IV)
        };
    }

    public static string AesDecrypt(string base64EncryptedContent, string base64Iv, Guid key)
    {
        using var aes = Aes.Create();
        aes.Key = key.ToByteArray();
        aes.IV = Convert.FromBase64String(base64Iv);
        using var decryptor = aes.CreateDecryptor();
        byte[] data = Convert.FromBase64String(base64EncryptedContent);
        byte[] result = decryptor.TransformFinalBlock(data, 0, data.Length);
        return Encoding.UTF8.GetString(result);
    }

    public static bool IsBase64(string value)
    {
        var invalidChars = value.Where(c => !char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '=').ToArray();
        return value.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=');
    }

    public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

    public static async Task<string> GetClipboardText()
    {
        var path = Path.GetTempFileName();
        var ps = $"""
            Add-Type -AssemblyName System.Windows.Forms
            $clipboardText = [System.Windows.Forms.Clipboard]::GetText()
            $filePath = "{path}"
            $clipboardText | Out-File -FilePath $filePath -Encoding utf8
            """;
        await Consoles.ExecutePowserShell(ps);
        var result = await File.ReadAllTextAsync(path, UTF8);
        File.Delete(path);
        return result;
    }

    public static async Task SetClipboardText(string value)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, value, UTF8);
        var ps = $"""
            Add-Type -AssemblyName System.Windows.Forms
            $filePath = "{path}"
            Write-Host $filePath 
            $fileContent = Get-Content -Path $filePath -Encoding UTF8 -Raw
            [System.Windows.Forms.Clipboard]::SetText($fileContent)
            """;
        await Consoles.ExecutePowserShell(ps);
        File.Delete(path);
    }

    public static IntPtr ConvertStringToIntptr(string value)
    {
        // 0x开头的16进制
        if (value.ToLower().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return new IntPtr(Convert.ToInt64(value[2..], 16));
        }
        else if(value.StartsWith("g/", StringComparison.OrdinalIgnoreCase))
        {
            var regex = new Regex(value[2..]);
            var windows = Win32.WindowInterface.GetWindows();
            foreach (var window in windows)
            {
                if (regex.IsMatch(window.WindowText)|| regex.IsMatch(window.Text)|| regex.IsMatch(window.ClassName))
                {
                    return window.hWnd;
                }
            }
            throw new Exception("未找到窗口");
        }
        // 10进制
        return new IntPtr(Convert.ToInt64(value));
    }
}
