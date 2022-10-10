var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddDbContext<GeneratorContext>();

using (var ctx = new GeneratorContext(configuration))
{
    ctx.Database.Migrate();
}
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Utils
builder.Services.AddScoped<ILog, Log<Generator.API.Generator>>();
#endregion

#region Logic
builder.Services.AddScoped<ApplicationLogic>();
builder.Services.AddScoped<GeneratorLogic>();
builder.Services.AddScoped<ConflictLogic>();
#endregion

#region Generators
builder.Services.AddScoped<GatewayGenerator>();
builder.Services.AddScoped<EntityGenerator>();
builder.Services.AddScoped<BackendGenerator>();
builder.Services.AddScoped<WorkspaceGenerator>();
builder.Services.AddScoped<FrontendGenerator>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseFileServer();

app.UseAuthorization();

app.MapControllers();

app.Run();
