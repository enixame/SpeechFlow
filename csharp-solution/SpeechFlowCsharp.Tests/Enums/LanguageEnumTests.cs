using SpeechFlowCsharp.Enums;

namespace SpeechFlowCsharp.Tests
{
    public class LanguageEnumTests
    {
        [Fact]
        public void LanguageEnum_ShouldContainOnlyFrenchAndEnglish()
        {
            // Convertir le résultat en un tableau typé de Language
            var values = Enum.GetValues(typeof(Language)).Cast<Language>().ToArray();

            // Vérifie que l'enum contient uniquement les valeurs French et English
            Assert.Contains(Language.French, values);
            Assert.Contains(Language.English, values);

            // Vérifie qu'il n'y a pas d'autres valeurs dans l'enum
            Assert.Equal(2, values.Length); // Seulement 2 valeurs sont attendues (French, English)
        }

        [Fact]
        public void InvalidEnumValue_ShouldReturnFalse_WhenCheckedWithEnumIsDefined()
        {
            // Vérifier qu'une valeur arbitraire comme 999 n'est pas définie dans l'enum
            bool isDefined = Enum.IsDefined(typeof(Language), 999);

            // Assert que 999 n'est pas une valeur valide de l'enum
            Assert.False(isDefined);
        }
    }
}
