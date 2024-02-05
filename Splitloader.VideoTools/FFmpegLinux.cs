using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Splitloader.VideoTools;

internal static class FFmpegLinux
{
    private static readonly string InternalLibPath = Environment.GetEnvironmentVariable("HOME") + "/.local/share/splitloader/lib";
    private static readonly List<string> LinuxPaths =
    [
        "/usr/bin/ffmpeg",
        "/usr/local/bin/ffmpeg",
        $"{InternalLibPath}/ffmpeg"
    ];

    internal static async Task<string> FindOrDownloadLinuxAsync()
    {
        var ffmpegPath = LinuxPaths.FirstOrDefault(File.Exists);
        /*
         * TODO: Refactor this into a universal download method that picks download url based on OS and takes _internalLibPath as an argument
         */
        if (ffmpegPath == null)
        {
            FFmpegTools.FfStatus.Value = "Downloading FFmpeg...";
            Directory.CreateDirectory(InternalLibPath);

            var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(new Uri("https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz"));
            await using var fileStream = new FileStream($"{InternalLibPath}/ffmpeg.tar.xz", FileMode.CreateNew);
            await stream.CopyToAsync(fileStream);

            FFmpegTools.FfStatus.Value = "Extracting FFmpeg...";
            await using Stream archiveStream = File.OpenRead($"{InternalLibPath}/ffmpeg.tar.xz");
            using var reader = ReaderFactory.Open(archiveStream);
            reader.WriteAllToDirectory(InternalLibPath, new ExtractionOptions
            {
                ExtractFullPath = false,
                Overwrite = true
            });

            FFmpegTools.FfStatus.Value = "Cleaning up...";
            File.Delete($"{InternalLibPath}/ffmpeg.tar.xz");

            ffmpegPath = $"{InternalLibPath}/ffmpeg";
            File.SetUnixFileMode(ffmpegPath, UnixFileMode.UserExecute);
        }

        try
        {
            var ffmpegProcInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Arguments = "-version",
                FileName = ffmpegPath
            };
            var ffmpegProc = new Process();
            ffmpegProc.StartInfo = ffmpegProcInfo;
            ffmpegProc.Start();
            var ffmpegVer = await ffmpegProc.StandardOutput.ReadLineAsync();
            return ffmpegVer;
        }
        catch (Exception e)
        {
            return "Something's broken with your FFmpeg. Unable to continue.";
        }
    }
}