using Microsoft.EntityFrameworkCore;
using BookRuangApi;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database(SQLite)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=bookruang.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",policy =>
    {
       policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader(); 
    });
});
var app = builder.Build();

app.UseCors("AllowAll");

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

