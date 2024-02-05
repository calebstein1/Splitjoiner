﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Splitloader.VideoTools;

public class FFmpegTools
{
    public readonly ObservableString FfStatus = new();
    private readonly string _internalLibPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
        Environment.GetEnvironmentVariable("HOME") + "/.local/share/splitloader/lib" :
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/splitloader/lib";
    private readonly List<string> _linuxPaths;
    private readonly List<string> _windowsPaths;
    private string? _ffmpegPath;
    private Task? _initTask;

    public FFmpegTools()
    {
        _linuxPaths =
        [
            "/usr/bin/ffmpeg",
            "/usr/local/bin/ffmpeg",
            $"{_internalLibPath}/ffmpeg"
        ];

        _windowsPaths =
        [
            $"{_internalLibPath}/ffmpeg"
        ];
    }

    public Task FindOrDownloadAsync()
    {
        return _initTask ??= InitFfmpegAsync();
    }

    private async Task InitFfmpegAsync()
    {
        _ffmpegPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
            _linuxPaths.FirstOrDefault(File.Exists) :
            _windowsPaths.FirstOrDefault(File.Exists);
        if (_ffmpegPath == null)
        {
            FfStatus.Value = "Downloading FFmpeg...";
            var downloadUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new Uri("https://johnvansickle/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz")
                : new Uri("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z");
                
            Directory.CreateDirectory(_internalLibPath);

            var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(downloadUrl);
            await using var fileStream = new FileStream($"{_internalLibPath}/ffmpeg.tar.xz", FileMode.CreateNew);
            await stream.CopyToAsync(fileStream);

            FfStatus.Value = "Extracting FFmpeg...";
            await using Stream archiveStream = File.OpenRead($"{_internalLibPath}/ffmpeg.tar.xz");
            using var reader = ReaderFactory.Open(archiveStream);
            reader.WriteAllToDirectory(_internalLibPath, new ExtractionOptions
            {
                ExtractFullPath = false,
                Overwrite = true
            });

            FfStatus.Value = "Cleaning up...";
            File.Delete($"{_internalLibPath}/ffmpeg.tar.xz");

            _ffmpegPath = $"{_internalLibPath}/ffmpeg";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                File.SetUnixFileMode(_ffmpegPath, UnixFileMode.UserExecute);
        }

        try
        {
            var ffmpegProcInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Arguments = "-version",
                FileName = _ffmpegPath
            };
            var ffmpegProc = new Process();
            ffmpegProc.StartInfo = ffmpegProcInfo;
            ffmpegProc.Start();
            var ffmpegVer = await ffmpegProc.StandardOutput.ReadLineAsync();
            FfStatus.Value = $"Using {ffmpegVer}";
        }
        catch (Exception e)
        {
            FfStatus.Value = "Something's broken with your FFmpeg. Unable to continue.";
            throw;
        }
    }
    
    {
        FfStatus.Value = $"Using {await FFmpegLinux.FindOrDownloadLinuxAsync()}";
    }
}