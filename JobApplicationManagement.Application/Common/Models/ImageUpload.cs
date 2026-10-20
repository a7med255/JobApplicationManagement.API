using JobApplicationManagement.Application.Common.Exceptions;

namespace JobApplicationManagement.Application.Common.Models;

public sealed record ImageUpload(byte[] Content, string FileName, string ContentType);

public static class ImageUploadValidation
{
    public static void Validate(ImageUpload upload, long maximumBytes)
    {
        var errors = new Dictionary<string, string[]>();
        if (upload.Content.Length == 0) errors["File"] = ["Choose a non-empty image."];
        else if (upload.Content.LongLength > maximumBytes) errors["File"] = [$"The image cannot exceed {maximumBytes / 1024 / 1024} MB."];

        var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
        var allowedExtension = extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        var allowedMime = upload.ContentType.ToLowerInvariant() is "image/jpeg" or "image/png" or "image/webp";
        if (!allowedExtension || !allowedMime) errors["File"] = ["Only JPEG, PNG, and WebP images are supported."];
        if (upload.Content.Length < 12 || !HasValidSignature(upload.Content, extension)) errors["File"] = ["The file content does not match a supported image format."];
        if (errors.Count > 0) throw new ValidationException(errors);
    }

    private static bool HasValidSignature(byte[] content, string extension) => extension switch
    {
        ".jpg" or ".jpeg" => content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF,
        ".png" => content.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        ".webp" => content.AsSpan(0, 4).SequenceEqual("RIFF"u8) && content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };
}
