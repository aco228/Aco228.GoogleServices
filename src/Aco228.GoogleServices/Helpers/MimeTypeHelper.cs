using System.ComponentModel;
using Aco228.Common.Extensions;
using Aco228.GoogleServices.Models;

namespace Aco228.GoogleServices.Helpers;

public static class MimeTypeHelper
{
    public static string? GetMimeTypeFromUrl(string url)
    {
        if(string.IsNullOrEmpty(url))
            return null;
        
        var extension = url.Split('?').First().Split('.').Last();
        return GetMimeStringType(extension);
    }
    
    public static string? GetMimeStringType(string extension)
    {
        var mimeType = GetMimeType(extension);
        if (mimeType is null or MimeTypes.unknown)
            return null;

        return mimeType.Value.GetMimeType();
    }
    
    public static string? GetExtension(string? mimeType)
    {
        if (string.IsNullOrEmpty(mimeType))
            return null;

        // Find the enum that has a Description matching the mimeType string
        var match = Enum.GetValues<MimeTypes>()
            .FirstOrDefault(m => m != MimeTypes.unknown && 
                                 m.GetAttribute<DescriptionAttribute>()?.Description == mimeType);

        return match == MimeTypes.unknown ? null : match.ToString();
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