namespace Tapex_Project.Models;

public sealed class ImageModel
{
    public string FilePath { get; init; } = string.Empty;
    public static ImageModel FromFile(string path) => new() { FilePath = path };
}
