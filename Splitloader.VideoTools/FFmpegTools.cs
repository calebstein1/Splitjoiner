using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Splitloader.VideoTools;

internal enum OperatingSystem
{
    Windows,
    Mac,
    Linux
}

public class FFmpegTools
{
    private OperatingSystem _os;
    public readonly ObservableString FfStatus = new();
    public readonly ObservableString ConcatStatus = new();
    private string _internalBinPath;
    private readonly List<string> _windowsPaths;
    private readonly List<string> _macPaths;
    private readonly List<string> _linuxPaths;
    private string? _ffmpegPath;
    private Task? _initTask;

    public FFmpegTools()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _os = OperatingSystem.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _os = OperatingSystem.Mac;
        }
        else
        {
            _os = OperatingSystem.Linux;
        }

        _internalBinPath = _os switch
        {
            OperatingSystem.Windows =>
                Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "splitloader/bin"),
            OperatingSystem.Mac =>
                Path.Join(Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/splitloader/bin"),
            _ =>
                Path.Join(Environment.GetEnvironmentVariable("HOME"), ".local/share/splitloader/bin")
        };
        
        _windowsPaths =
        [
            Path.Join(_internalBinPath, "ffmpeg.exe")
        ];

        _macPaths =
        [
            "/opt/local/bin/ffmpeg",
            "/usr/local/bin/ffmpeg",
            Path.Join(_internalBinPath, "ffmpeg")
        ];
        
        _linuxPaths =
        [
            "/usr/bin/ffmpeg",
            "/usr/local/bin/ffmpeg",
            Path.Join(_internalBinPath, "ffmpeg")
        ];
    }

    public Task FindOrDownloadAsync()
    {
        return _initTask ??= InitFfmpegAsync();
    }

    private async Task InitFfmpegAsync()
    {
        _ffmpegPath = _os switch
        {
            OperatingSystem.Windows => _windowsPaths.FirstOrDefault(File.Exists),
            OperatingSystem.Mac => _macPaths.FirstOrDefault(File.Exists),
            _ => _linuxPaths.FirstOrDefault(File.Exists)
        };
        
        if (_ffmpegPath == null)
        {
            FfStatus.Value = "Downloading FFmpeg...";
            var downloadUrl = _os switch
            {
                OperatingSystem.Windows => new Uri("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z"),
                OperatingSystem.Mac => new Uri("https://evermeet.cx/ffmpeg/getrelease/zip"),
                _ => new Uri("https://johnvansickle/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz")
            };
            var outFile = _os switch
            {
                OperatingSystem.Windows => "ffmpeg.7z",
                OperatingSystem.Mac => "ffmpeg.zip",
                _ => "ffmpeg.tar.xz"
            };
                
            Directory.CreateDirectory(_internalBinPath);

            var httpClient = new HttpClient();
            await using var stream = await httpClient.GetStreamAsync(downloadUrl);
            await using var fileStream = new FileStream(Path.Join(_internalBinPath, outFile), FileMode.CreateNew);
            await stream.CopyToAsync(fileStream);

            FfStatus.Value = "Extracting FFmpeg...";
            await using Stream archiveStream = File.OpenRead(Path.Join(_internalBinPath, outFile));
            using var reader = ReaderFactory.Open(archiveStream);
            reader.WriteAllToDirectory(_internalBinPath, new ExtractionOptions
            {
                ExtractFullPath = false,
                Overwrite = true
            });

            FfStatus.Value = "Cleaning up...";
            File.Delete(Path.Join(_internalBinPath, outFile));

            _ffmpegPath = _os switch
            {
                OperatingSystem.Windows => Path.Join(_internalBinPath, "ffmpeg.exe"),
                _ => Path.Join(_internalBinPath, "ffmpeg")
            };

            #pragma warning disable CA1416
            if (_os != OperatingSystem.Windows)
            {
                File.SetUnixFileMode(_ffmpegPath, UnixFileMode.UserExecute);
            }
            #pragma warning restore CA1416
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
    
    public async Task ConcatVideoParts(IEnumerable<string?> vidParts, string? outputName)
    {
        await FindOrDownloadAsync();
        if (outputName is null)
        {
            FfStatus.Value = "Must specify an output file name";
            return;
        }

        var ffmpegFileList = Path.GetTempFileName();
        var concatVideoOutput = Path.Join(Environment.GetEnvironmentVariable("HOME"), outputName);

        await using StreamWriter ffmpegFileListStream = new(ffmpegFileList);
        foreach (var path in vidParts)
        {
            if (path is null) continue;
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