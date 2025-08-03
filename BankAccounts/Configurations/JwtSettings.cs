using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BankAccounts.Configurations
{
    /// <summary>
    /// Представляет настройки JWT-аутентификации,
    /// используемые для конфигурации <see cref="JwtBearerOptions"/>.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Эмитент токена, который считается допустимым (например, адрес Keycloak).
        /// Используется для валидации поля "iss" в JWT.
        /// </summary>
        public string ValidIssuer { get; init; } = string.Empty;
        /// <summary>
        /// Идентификатор клиента (аудитория), которому предназначен токен.
        /// Используется для валидации поля "aud" в JWT.
        /// </summary>
        public string Audience { get; init; } = string.Empty;
        /// <summary>
        /// URL-адрес, по которому OpenID-провайдер (например, Keycloak)
        /// предоставляет метаданные конфигурации, включая JWKS.
        /// </summary>
        public string MetadataAddress { get; init; } = string.Empty;
    }
}
