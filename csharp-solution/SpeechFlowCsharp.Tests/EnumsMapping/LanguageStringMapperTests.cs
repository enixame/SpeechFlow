using SpeechFlowCsharp.Enums;

namespace SpeechFlowCsharp.Tests.EnumsMapping
{
    public class LanguageStringMapperTests
    {
        [Fact]
        public void ToLanguageString_ShouldReturnFrenchCode_WhenLanguageIsFrench()
        {
            // Act
            var result = LanguageStringMapper.ToLanguageString(Language.French);

            // Assert
            Assert.Equal("fr", result);
        }

        [Fact]
        public void ToLanguageString_ShouldReturnEnglishCode_WhenLanguageIsEnglish()
        {
            // Act
            var result = LanguageStringMapper.ToLanguageString(Language.English);

            // Assert
            Assert.Equal("en", result);
        }

        [Fact]
        public void ToLanguageString_ShouldThrowArgumentOutOfRangeException_WhenLanguageIsNotSupported()
        {
            // Arrange
            var invalidLanguage = (Language)999; // Langue qui n'est pas dans l'enum

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LanguageStringMapper.ToLanguageString(invalidLanguage));

            // Vérifie que le message d'erreur contient le nom du paramètre attendu
            Assert.Equal("language", exception.ParamName);
        }
    }
}