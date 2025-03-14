using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Quartz;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Configuration"));

var appSettings = builder.Configuration.GetSection("Configuration").Get<AppSettings>() ?? new AppSettings();

ConfigureServices(builder, appSettings);

ConfigureJob(builder, appSettings.CronConfiguration);

var app = builder.Build();

ConfigureApp(app);

app.MapConsolidationEndpoints();

app.Run();


static void ConfigureServices(WebApplicationBuilder builder, AppSettings appSettings)
{
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });

    builder.Services.AddAutoMapper(typeof(Program));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        var key = appSettings.JwtToken;
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? "")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(appSettings.LedgerConnectionString));

    builder.Services.AddSingleton(sp => new MongoClient(appSettings.MongoSettings.ConnectionString));

    builder.Services.AddScoped(sp =>
    {
        var client = sp.GetRequiredService<MongoClient>();
        return client.GetDatabase(appSettings.MongoSettings.DatabaseName);
    });

    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IConsolidationRepository, ConsolidationRepository>();
    builder.Services.AddScoped<IConsolidationService, ConsolidationService>();
}

static void ConfigureApp(WebApplication app)
{
    app.UseAuthentication();
    app.UseAuthorization();

    //this can be done with migrations, to keep it simple i'm creating the tables here
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/openapi/v1.json", "Balance Ledger API"));
    }

    app.UseHttpsRedirection();
}

static void ConfigureJob(WebApplicationBuilder builder, string cronConfiguration)
{
    builder.Services.AddQuartz(q =>
    {
        var job = new JobKey("ConsolidationJob");

        q.AddJob<ConsolidationJob>(opt =>
        opt.WithIdentity(job).RequestRecovery());
        q.AddTrigger(opt => opt
            .ForJob(job)
            .WithIdentity("ConsolidationJobTrigger")
            .WithCronSchedule(cronConfiguration));
    });

    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });
}

//tudo isso para adicionar o esquema de segurança no swagger
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}

// para os testes conseguirem acessar, preciso deixar implicito aqui 
public partial class Program { }