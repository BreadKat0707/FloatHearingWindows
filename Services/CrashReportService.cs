using System.Text;
using Windows.ApplicationModel.DataTransfer;

namespace FloatHearing.Services;

/// <summary>
/// 崩溃报告服务：保存未处理异常信息，并在下次启动时展示。
/// </summary>
public static class CrashReportService
{
    private static string CrashFilePath => Path.Combine(
        Windows.Storage.ApplicationData.Current.LocalFolder.Path,
        "crash.log");

    /// <summary>
    /// 保存崩溃信息到本地文件。
    /// </summary>
    public static void SaveCrashInfo(Exception exception)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"崩溃时间: {DateTime.UtcNow:O}");
            sb.AppendLine($"异常类型: {exception.GetType().FullName}");
            sb.AppendLine($"异常消息: {exception.Message}");
            sb.AppendLine($"堆栈跟踪:");
            sb.AppendLine(exception.StackTrace);

            if (exception.InnerException is not null)
            {
                sb.AppendLine();
                sb.AppendLine("--- 内部异常 ---");
                sb.AppendLine($"异常类型: {exception.InnerException.GetType().FullName}");
                sb.AppendLine($"异常消息: {exception.InnerException.Message}");
                sb.AppendLine($"堆栈跟踪:");
                sb.AppendLine(exception.InnerException.StackTrace);
            }

            File.WriteAllText(CrashFilePath, sb.ToString());
        }
        catch
        {
            // 忽略保存失败
        }
    }

    /// <summary>
    /// 检查是否存在待处理的崩溃报告。
    /// </summary>
    public static bool HasPendingReport()
    {
        return File.Exists(CrashFilePath);
    }

    /// <summary>
    /// 读取并删除崩溃报告文件。
    /// </summary>
    public static string? ReadAndClearReport()
    {
        try
        {
            var path = CrashFilePath;
            if (!File.Exists(path))
            {
                return null;
            }

            var content = File.ReadAllText(path);
            File.Delete(path);
            return content;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 将文本复制到剪贴板。
    /// </summary>
    public static void CopyToClipboard(string text)
    {
        try
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
        }
        catch
        {
            // 忽略复制失败
        }
    }
}
