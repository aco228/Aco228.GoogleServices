using System.ComponentModel;
using Aco228.Common.Extensions;
using Aco228.GoogleServices.Models;

namespace Aco228.GoogleServices.Helpers;

public static class MimeTypeHelper
{
    public static string? GetMimeType(string extension)
    {
        extension = extension.Remove(".");
        var mimeType = extension.ToEnum(MimeTypes.unknown);
        if (mimeType == MimeTypes.unknown)
            return null;

        return mimeType.GetAttribute<DescriptionAttribute>()!.Description;
    }
}