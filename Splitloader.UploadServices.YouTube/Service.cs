using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Splitloader.UploadServices.Common;

namespace Splitloader.UploadServices.YouTube;

public static class Service
{
    public static readonly ObservableString UiStatus = new();

    private static async Task<UserCredential> GetAuthorizationAsync()
    {
        await using var stream = new FileStream(
            Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config/Splitloader/youtube-keys.json"),
            FileMode.Open, FileAccess.Read);
        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { YouTubeService.Scope.YoutubeUpload },
            "user",
            CancellationToken.None
        );
    }

    public static async Task UploadVideoAsync(VideoUpload videoUpload)
    {
        var credential = await GetAuthorizationAsync();

        var youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
        });

        var video = new Video
        {
            Snippet = new VideoSnippet
            {
                Title = videoUpload.Name,
                Description = videoUpload.Description
            },
            Status = new VideoStatus
            {
                PrivacyStatus = "public",
                MadeForKids = false
            }
            
        };

        await using var fileStream = new FileStream(videoUpload.VideoPath, FileMode.Open);
        var videoUploadRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
        videoUploadRequest.ProgressChanged += videoUploadRequest_ProgressChanged;
        videoUploadRequest.ResponseReceived += videoUploadRequest_ResponseReceived;

        await videoUploadRequest.UploadAsync();
    }

    private static void videoUploadRequest_ProgressChanged(IUploadProgress progress)
    {
        UiStatus.Value = progress.Status switch
        {
            UploadStatus.Starting => "Starting upload...",
            UploadStatus.Uploading => $"{progress.BytesSent.ToString()} bytes sent...",
            UploadStatus.Failed => "Upload failed.",
            _ => UiStatus.Value
        };
    }
    
    private static void videoUploadRequest_ResponseReceived(Video video)
    {
        UiStatus.Value = "Video successfully uploaded.";
    }
}
