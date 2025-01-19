using Microsoft.EntityFrameworkCore;
using CodeGenie.Data;
using CodeGenie.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container for Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sample API endpoint to retrieve all users from the database
app.MapGet("/users", async (ApplicationDbContext db) =>
{
    return await db.Users.ToListAsync();
})
.WithName("GetUsers")
.WithOpenApi();

// Sample API endpoint to create a new user
app.MapPost("/users", async (User user, ApplicationDbContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.WithOpenApi();

app.Run();
