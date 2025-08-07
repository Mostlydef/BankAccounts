namespace BankAccounts.Configurations
{
    /// <summary>
    /// Представляет настройки Swagger, используемые для конфигурации
    /// авторизации через OpenID Connect в Swagger UI.
    /// </summary>
    public class SwaggerSettings
    {
        /// <summary>
        /// URL-адрес для авторизации пользователей через OpenID-провайдер (например, Keycloak).
        /// Используется в Swagger UI для начала аутентификации.
        /// </summary>
        public string AuthorizationUrl { get; init; } = string.Empty;
        /// <summary>
        /// Название scope для OpenID, обычно это "openid".
        /// </summary>
        public string OpenIdScope {  get; init; } = string.Empty;
        /// <summary>
        /// Название scope для профиля пользователя, обычно это "profile".
        /// </summary>
        public string ProfileScope {  get; init; } = string.Empty;
        /// <summary>
        /// Описание схемы безопасности, отображаемое в Swagger UI.
        /// </summary>
        public string Description { get; init; } = string.Empty;
        /// <summary>
        /// Название схемы безопасности, например "Bearer".
        /// Используется для указания типа авторизации в Swagger.
        /// </summary>
        public string SecurityScheme { get; init; } = string.Empty;
    }
}
