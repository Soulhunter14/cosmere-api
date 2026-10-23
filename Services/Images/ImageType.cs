namespace Services.Images;

/// <summary>Detection of the image types the app stores (WebP, PNG and JPEG), shared by every upload.</summary>
public static class ImageType
{
    /// <summary>The type of an image by its first bytes (the client's Content-Type is not trusted); null = not WebP, PNG or JPEG.</summary>
    public static string? Detect(byte[] d) =>
        d.Length >= 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F' && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P' ? "image/webp"
        : d.Length >= 8 && d[0] == 0x89 && d[1] == 'P' && d[2] == 'N' && d[3] == 'G' ? "image/png"
        : d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF ? "image/jpeg"
        : null;
}
