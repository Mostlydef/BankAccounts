using BankAccounts.Features.Accounts.Events;
using BankAccounts.Features.Transactions.Events;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using JetBrains.Annotations;

namespace BankAccounts
{
    /// <summary>
    /// Документальный фильтр для Swagger, который регистрирует схемы событий.
    /// </summary>
    [UsedImplicitly]
    public class EventDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Добавляет схемы всех событий в Swagger-документацию.
        /// </summary>
        /// <param name="swaggerDoc">Объект Swagger документа</param>
        /// <param name="context">Контекст фильтра документа</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            context.SchemaGenerator.GenerateSchema(typeof(MoneyCreditedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(MoneyDebitedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(TransferCompletedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(AccountOpenedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(ClientBlockedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(ClientUnblockedEvent), context.SchemaRepository);
            context.SchemaGenerator.GenerateSchema(typeof(InterestAccruedEvent), context.SchemaRepository);
        }
    }

}
