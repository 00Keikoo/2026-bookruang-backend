using Microsoft.EntityFrameworkCore;
using BookRuangApi;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=bookruang.db"));

// CORS - PENTING! Harus sebelum Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// USE CORS - HARUS SEBELUM UseAuthorization!
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();