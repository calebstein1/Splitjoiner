namespace Splitloader.UploadServices.Common;

public interface IVideoUploader<T>
{
    ObservableString? UIStatus { get; set; }
    Task<T> GetAuthorizationAsync();
    Task UploadVideoAsync();
}