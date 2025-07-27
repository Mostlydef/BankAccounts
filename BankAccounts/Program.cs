using BankAccounts.Abstractions.Services;
using BankAccounts.Database.Interfaces;
using BankAccounts.Database.Repository;
using BankAccounts.Features.Accounts;
using BankAccounts.Features.Transactions;
using BankAccounts.Infrastructure.CurrencyService;
using BankAccounts.Infrastructure.VerificationService;
using BankAccounts.Middlewares;
using BankAccounts.PipelineBehaviors;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace BankAccounts
{
    /// <summary>
    /// Главная точка входа в приложение BankAccounts API.
    /// Настраивает зависимости, middleware и запускает приложение.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Точка входа в приложение.
        /// Настраивает все зависимости, middleware и запускает ASP.NET Core приложение.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IAccountRepository, AccountRepositoryStub>();
            builder.Services.AddSingleton<ITransactionRepository, TransactionRepositoryStub>();
            builder.Services.AddSingleton<ICurrencyService, CurrencyServiceStub>();
            builder.Services.AddSingleton<ICustomerVerificationService, CustomerVerificationServiceStub>();
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AccountMappingProfile>();
                cfg.AddProfile<TransactionMappingProfile>();
            });
            builder.Services.AddMediatR(config =>
                config.RegisterServicesFromAssembly(typeof(Program).Assembly));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });


            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
