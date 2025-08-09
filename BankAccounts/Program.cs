using BankAccounts.Abstractions.Services;
using BankAccounts.Configurations;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using BankAccounts.Database;
using Microsoft.EntityFrameworkCore;

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
            IdentityModelEventSource.ShowPII = true;
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);
            builder.Services.AddControllers();

            var allowSpecificOrigin = "AllowAll";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(allowSpecificOrigin, policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
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
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                var swaggerSettings = builder.Configuration
                    .GetSection(nameof(SwaggerSettings))
                    .Get<SwaggerSettings>();
                options.IncludeXmlComments(xmlPath);

                if (swaggerSettings == null)
                    throw new InvalidOperationException("Настройки swagger настроены неправильно.");
                options.AddSecurityDefinition(swaggerSettings.OpenIdScope, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(swaggerSettings.AuthorizationUrl),
                            Scopes = new Dictionary<string, string>
                            {
                                { swaggerSettings.OpenIdScope, swaggerSettings.OpenIdScope },
                                { swaggerSettings.ProfileScope, swaggerSettings.ProfileScope }
                            }
                        }
                    },
                    Description = swaggerSettings.Description
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = swaggerSettings.OpenIdScope
                            },
                            In = ParameterLocation.Header,
                            Name = swaggerSettings.SecurityScheme,
                            Scheme = swaggerSettings.SecurityScheme
                        },
                        []
                    }
                });
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSettings = builder.Configuration
                        .GetSection(nameof(JwtSettings))
                        .Get<JwtSettings>();
                    if (jwtSettings == null)
                        throw new InvalidOperationException("Настройки JWT настроены неправильно.");
                    options.RequireHttpsMetadata = false;
                    options.Audience = jwtSettings.Audience;
                    options.MetadataAddress = jwtSettings.MetadataAddress;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtSettings.ValidIssuer,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"JWT error: {context.Exception.Message}");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            // Выполнение миграции при старте контейнера
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();


            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseCors(allowSpecificOrigin);
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

    }
}
