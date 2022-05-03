using Generator.API.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reusable.Utils;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<GeneratorContext>();

using(var ctx = new GeneratorContext(configuration))
{
    ctx.Database.Migrate();
}
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ApplicationLogic>(); 
builder.Services.AddScoped<Log<ApplicationLogic>>(); 
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
