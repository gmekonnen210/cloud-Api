using CloudStorage.Api.Middleware;
using CloudStorage.DomainService.Implementations;
using CloudStorage.DomainService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Virtual Storage API",
        Version = "v1",
        Description = "API for abstracting cloud storage providers (Azure Blob Storage, AWS S3)"
    });
}); 
// register Logging
builder.Services.AddLogging( logging => 
{     logging.AddConsole();
     logging.AddDebug();
   logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

//Register storage services with proper lifetime management
builder.Services.AddScoped<IStorageProvideFactory, StorageProviderFactory>();
//builder.Services.AddScoped<AzureStorageProvider>();
//builder.Services.AddScoped<AwsS3StorageProvider>();

//Register HttpClient for AWS if needed
//builder.Services.AddHttpClient<AwsS3StorageProvider>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
