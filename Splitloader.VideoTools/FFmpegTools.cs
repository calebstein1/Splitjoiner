using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Splitloader.VideoTools;

public class FFmpegTools
{
    public readonly ObservableString FfStatus = new();
    public readonly ObservableString ConcatStatus = new();
    private readonly string _internalLibPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
        Path.Join(Environment.GetEnvironmentVariable("HOME"), ".local/share/splitloader/lib") :
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "splitloader/lib");
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
            Path.Join(_internalLibPath, "ffmpeg")
        ];

        _windowsPaths =
        [
            Path.Join(_internalLibPath, "ffmpeg.exe")
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
            var downloadUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                new Uri("https://johnvansickle/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz") :
                new Uri("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z");
            var outFile = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "ffmpeg.tar.xz" : "ffmpeg.7z";
                
            Directory.CreateDirectory(_internalLibPath);

            var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(downloadUrl);
            await using var fileStream = new FileStream(Path.Join(_internalLibPath, outFile), FileMode.CreateNew);
            await stream.CopyToAsync(fileStream);

            FfStatus.Value = "Extracting FFmpeg...";
            await using Stream archiveStream = File.OpenRead(Path.Join(_internalLibPath, outFile));
            using var reader = ReaderFactory.Open(archiveStream);
            reader.WriteAllToDirectory(_internalLibPath, new ExtractionOptions
            {
                ExtractFullPath = false,
                Overwrite = true
            });

            FfStatus.Value = "Cleaning up...";
            File.Delete(Path.Join(_internalLibPath, outFile));

            _ffmpegPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                Path.Join(_internalLibPath, "ffmpeg") :
                Path.Join(_internalLibPath, "ffmpeg.exe");
            
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
            FfStatus.Value = e.Message;
            throw;
        }
    }
    
    public async Task ConcatVideoParts(IEnumerable<string> vidParts)
    {
        await FindOrDownloadAsync();

        var ffmpegFileList = Path.GetTempFileName();
        var concatVideoOutput = $"{ffmpegFileList}.mp4";

        await using StreamWriter ffmpegFileListStream = new(ffmpegFileList);
        foreach (var path in vidParts)
        {
            if (vidParts == null) continue;
            await ffmpegFileListStream.WriteLineAsync($"file '{path}'");
        }

        try
        {
            var ffmpegProcInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                Arguments = $"-f concat -safe 0 -i {ffmpegFileList} -c copy {concatVideoOutput}",
                FileName = _ffmpegPath
            };
            var ffmpegProc = new Process();
            ffmpegProc.Exited += (sender, e) =>
            {
                File.Delete(ffmpegFileList);
                ConcatStatus.Value = ffmpegProc.ExitCode == 0 ?
                    concatVideoOutput :
                    "failed";
            };
            ffmpegProc.ErrorDataReceived += (sender, e) =>
            {
                FfStatus.Value = e.Data;
            };
            ffmpegProc.StartInfo = ffmpegProcInfo;
            FfStatus.Value = "Combining video files...";
            ffmpegProc.EnableRaisingEvents = true;
            ffmpegProc.Start();
            ffmpegProc.BeginErrorReadLine();
        }
        catch (Exception e)
        {
            FfStatus.Value = e.Message;
            throw;
        }
    }
}