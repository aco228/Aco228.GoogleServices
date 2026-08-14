using Aco228.Common.Models;
using Google.Cloud.Translation.V2;

namespace Aco228.GoogleServices.Services;

public interface IGoogleTranslateService : ITransient
{
    Task<string?> Translate(string? text, string sourceLanguage, string targetLanguage, bool importantText = false);
}

public class GoogleTranslateService : IGoogleTranslateService
{
    private readonly IGoogleClientProvider _googleClientProvider;

    public GoogleTranslateService(IGoogleClientProvider googleClientProvider)
    {
        _googleClientProvider = googleClientProvider;
    }

    public async Task<string?> Translate(string? text, string sourceLanguage, string targetLanguage, bool importantText = false)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return null;
            
            if (sourceLanguage.Equals(targetLanguage, StringComparison.InvariantCultureIgnoreCase))
                return text;
            
            var client = await _googleClientProvider.CreateTranslationClient();
            var split = targetLanguage.Split("-");
            var targetLanguageCode = split.First().ToLower() + (split.Length == 1 ? "" : $"-{split[1]}");
            var translationResult = importantText 
                ? await client.TranslateTextAsync(text, targetLanguageCode, sourceLanguage.ToLower(), model: TranslationModel.NeuralMachineTranslation)
                : await client.TranslateTextAsync(text, targetLanguageCode, sourceLanguage.ToLower(), model: TranslationModel.Base);

            return translationResult?.TranslatedText;
        }
        catch
        {
            return null;
        }
    }
}