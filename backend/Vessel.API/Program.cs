using System.Reflection;
using Microsoft.OpenApi;
using Vessel.API.Extensions;
using Vessel.API.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(
    options => {options.Filters.Add<ValidationFilter>();
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("VesselFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Vessel API",
        Description = "API for Vessel",
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer: {token}"
    });
    
    var securityRequirement = new OpenApiSecurityRequirement();
    securityRequirement.Add(new OpenApiSecuritySchemeReference("Bearer"), new List<string>());
    options.AddSecurityRequirement(_ => securityRequirement);
});
builder.Services.AddApplicationServices(builder.Configuration);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<Vessel.Infrastructure.Data.DbInitializer>();
        await seeder.InitializeAsync();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("VesselFrontend");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
