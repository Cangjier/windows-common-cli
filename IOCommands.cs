using TidyHPC.Routers.Args;

namespace WindowsCommonCLI;

/// <summary>
/// This class contains the commands for the IO operations.
/// </summary>
public class IOCommands
{
    /// <summary>
    /// 拷贝文件夹
    /// </summary>
    /// <param name="sourceDirectory"></param>
    /// <param name="destinationDirectory"></param>
    /// <returns></returns>
    public static async Task CopyDirectory(
        [ArgsIndex]string sourceDirectory, 
        [ArgsIndex]string destinationDirectory)
    {
        await Task.CompletedTask;

        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine("Source directory does not exist.");
            return;
        }

        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        foreach (string item in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, item);
            var destination = Path.Combine(destinationDirectory, relativePath);
            if (Directory.Exists(destination) == false)
            {
                Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
            }
            else
            {
                // 如果目录是只读的，先取消只读
                var di = new DirectoryInfo(destination);
                if ((di.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        foreach (string item in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, item);
            var destination = Path.Combine(destinationDirectory, relativePath);
            // 如果文件是只读的，先取消只读
            if (File.Exists(destination))
            {
                var fi = new FileInfo(destination);
                if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fi.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
            File.Copy(item, Path.Combine(destinationDirectory, relativePath), true);
        }
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static async Task DeleteDirectory(
               [ArgsIndex]string directory)
    {
        await Task.CompletedTask;

        if (!Directory.Exists(directory))
        {
            Console.WriteLine("Directory does not exist.");
            return;
        }
        void deleteFiles()
        {
            foreach (string item in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                // 文件可能没有权限
                try
                {
                    File.Delete(item);
                }
                catch
                {
                    // ignored
                }
            }
        }

        try
        {
            Directory.Delete(directory, true);
        }
        catch
        {
            // ignored
        }

        if (Directory.Exists(directory) == false) return;
        // 先删除一遍文件
        deleteFiles();
        // 再删除一遍文件夹
        try
        {
            Directory.Delete(directory, true);
        }
        catch
        {
            // ignored
        }
    }
}
