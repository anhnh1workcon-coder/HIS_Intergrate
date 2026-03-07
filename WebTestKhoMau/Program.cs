using WebTestKhoMau.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure controllers and JSON serializer to use PascalCase (preserve C# property names)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Ensure returned JSON preserves PascalCase property names (default C# naming)
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Keep dictionary keys as defined as well
        o.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Blood Inventory Management API", 
        Version = "v1",
        Description = "Mock backend API for Blood Bank (Kho Máu) Management System - Minh Tâm Hospital"
    });
});

// Add services
builder.Services.AddScoped<IMockDatabaseService, MockDatabaseService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<ILogService, LogService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blood Inventory API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
