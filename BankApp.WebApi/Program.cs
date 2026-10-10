using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using BankApp.Application.Common;
using BankApp.Infrastructure.Identity;
using BankApp.Infrastructure.Data;
using BankApp.Application.Users;
using BankApp.Application.Loans;
using BankApp.Application.Wallets;
using BankApp.Infrastructure.Messaging;
using BankApp.WebApi.OpenApi;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Data layer config...(PostgreSQL via ConnectionStrings:DefaultConnection)
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBankDbContext>(sp => sp.GetRequiredService<BankDbContext>());

// 2. Security Infrastructure Mapping Configguration (Jwt section; the key comes from user-secrets or Jwt__Key)
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
jwtSettings.EnsureValid();
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<LoanApplicationService>();
builder.Services.AddScoped<WalletService>();

// 3. Kafka messaging: the dispatcher publishes OutboxMessages rows, the consumer reads loan and webhook events (Kafka:BootstrapServers)
builder.Services.AddSingleton<IKafkaClientConnectionFactory, KafkaClientConnectionFactory>();
builder.Services.AddHostedService<HighPerformanceOutboxDispatcher>();
builder.Services.AddHostedService<WebhookProcessingEventConsumer>();

// Enums are sent and received as names ("Simple", "Reducing"); reading ignores case, so "SIMPLE" still works.
// Numbers are rejected so a client can't send 1 and silently get "Reducing".
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
// The OpenAPI generator reads these options (not the MVC ones above), so enums show as names in Scalar.
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});

// 4. Mount the authentcation middleware
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(jwtSettings.KeyBytes)
    };

    // Default 401s have an empty body; return the reason so clients (and Scalar) can see what went wrong.
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            string message = context.AuthenticateFailure switch
            {
                SecurityTokenExpiredException => "Token has expired. Log in again.",
                not null => "Invalid token.",
                null => "Missing token. Send the header: Authorization: Bearer <token>"
            };
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Message = message });
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "BankApp API";
        opt.WithPreferredScheme("Bearer");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
