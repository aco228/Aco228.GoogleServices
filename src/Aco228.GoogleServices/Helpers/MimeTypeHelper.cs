using System.ComponentModel;
using Aco228.Common.Extensions;
using Aco228.GoogleServices.Models;

namespace Aco228.GoogleServices.Helpers;

public static class MimeTypeHelper
{
    public static string? GetMimeStringType(string extension)
    {
        var mimeType = GetMimeType(extension);
        if (mimeType is null or MimeTypes.unknown)
            return null;

        return mimeType.Value.GetMimeType();
    }
    
    public static string GetMimeType(this MimeTypes mimeType) 
        => mimeType.GetAttribute<DescriptionAttribute>()!.Description;
    
    public static MimeTypes? GetMimeType(string extension)
    {
        extension = extension.Remove(".");
        var mimeType = extension.ToEnum(MimeTypes.unknown);
        if (mimeType == MimeTypes.unknown)
            return null;
        
        return mimeType;       
    }
}