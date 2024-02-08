namespace Splitloader.UploadServices.Common;

public class VideoUpload(string name, string description, string videoPath)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string VideoPath { get; } = videoPath;
}