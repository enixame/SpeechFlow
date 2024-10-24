using SpeechFlowCsharp.Enums;

public static class LanguageStringMapper 
{
    public static string ToLanguageString(Language language)
    {
        return language switch
        {
            Language.French => "fr",
            Language.English => "en",
            _ => throw new ArgumentOutOfRangeException(nameof(language)),
        };
    }
}