using DogTrack.DataAccess.DogTrackDataAccess;
using DogTrack.DataAccess.IDogTrackDataAccess;
using DogTrack.Helper;
using DogTrack.Models;
using DogTrack.Service.DogTrackService;
using DogTrack.Service.IDogTrackService;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
Log.Logger = logger;

var assembly = Assembly.GetExecutingAssembly().GetName();
Log.Information("Starting {name} application version {version}", assembly.Name, assembly.Version);

builder.Services.AddSingleton
  (
     service => new UserContext("DogTrack", "0.0.1")
  );


builder.Services.AddControllers();


builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddScoped<IDogTrackDataAccess, DogTrackDataAccess>();
builder.Services.AddScoped<IDogTrackService, DogTrackService>();
builder.Services.AddHostedService<TrackGenerationService>();
builder.Services.AddHostedService<TicketValidationService>();
builder.Services.AddHostedService<RaceResolvingService>();

builder.Services
    .AddOpenIddict()
    .AddServer(options =>
    {
        options.EnableDegradedMode()
               .DisableTokenStorage()
               .UseAspNetCore();

            options.AddEphemeralEncryptionKey();
            options.AddEphemeralSigningKey();
        

        options.SetTokenEndpointUris("token")
               .AllowPasswordFlow()
               .AllowRefreshTokenFlow()
               .DisableSlidingRefreshTokenExpiration()
               .RegisterScopes
                (
                    [
                        OpenIddictConstants.Scopes.OfflineAccess,
                            "read",
                            "write"
                    ]
                )
               .AddEventHandler<OpenIddictServerEvents.ValidateTokenRequestContext>
                (
                    b => b.UseScopedHandler<TokenRequestHandler>()
                )
                .AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>
                (
                    b => b.UseScopedHandler<HandleTokenRequestHandler>()
                )
                .AddEventHandler<OpenIddictServerEvents.ApplyTokenResponseContext>
                (
                    b => b.UseScopedHandler<ApplyTokenResponseHandler>()
                );

            options
               .SetAccessTokenLifetime(new TimeSpan(0, 0, 15000))
               .SetRefreshTokenLifetime(new TimeSpan(0, 0, 15000));
        

    })
    .AddValidation
    (
        options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        }
    );

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);


builder.Services.AddMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
        options =>
        {

            options.SwaggerDoc
            (
                "v1",
                new OpenApiInfo
                {
                    Title = "Dog Track API",
                    Version = "v1",
                    Description = $"{assembly.Name} v{assembly.Version}"
                }
            );

            options.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });

            options.SwaggerGeneratorOptions.SortKeySelector = (apiDesc) => apiDesc.GroupName;

            options.CustomSchemaIds(x => x.FullName);

            options.AddSecurityDefinition
            (
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/token", UriKind.Relative),
                        }
                    },
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    }
                }
            );

            options.AddSecurityRequirement
            (
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            );
        }
    );


var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
