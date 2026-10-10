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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "BankApp API";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
