using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApiMyNote.Data;
using WebApiMyNote.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Minimal API My NoteBooks",
        //Description = "Minimal API My Notebooks",
        Version = "v1",
        Contact = new OpenApiContact()
        {
            Name = "Suttiporn Srisawad",
            Url = new Uri("https://github.com/yee-suttiporn")
        }
    });
});

builder.Services.AddDbContext<NoteBookDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Develop"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region User
var userEndPoint = app.MapGroup("/Users").WithTags("Users");

userEndPoint.MapPost("/Insert", async (User user, NoteBookDbContext db) =>
{
    if (user == null)
        return Results.BadRequest();

    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return Results.Created();
});

userEndPoint.MapDelete("/Delete", async (int id, NoteBookDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
    if (user == null)
        return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok();
});

userEndPoint.MapPut("/Update", async (User obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == obj.UserId);

    if (user == null)
        return Results.NotFound(obj);

    user.Username = obj.Username;
    user.FullName = obj.FullName;
    user.Password = obj.Password;
    user.LastUpdate = DateTime.Now;
    db.Users.Update(user);
    await db.SaveChangesAsync();

    return Results.Ok(obj);
});

userEndPoint.MapGet("/GetById", async (int id, NoteBookDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

userEndPoint.MapGet("/GetAll", async (NoteBookDbContext db) => await db.Users.ToListAsync());

#endregion User

#region Categories

var categoryEndPoint = app.MapGroup("/Categories").WithTags("Categories");

categoryEndPoint.MapPost("/Insert", async (Category obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest(obj);

    db.Categories.Add(obj);
    db.SaveChanges();
    return Results.Ok(obj);
});

categoryEndPoint.MapDelete("/Delete", async (int id, NoteBookDbContext db) =>
{
    var category = await db.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
    if (category == null)
        return Results.NotFound();

    db.Categories.Remove(category);
    db.SaveChanges();
    return Results.Ok();
});

categoryEndPoint.MapPut("/Update", async (Category obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest(obj);

    var category = await db.Categories.FindAsync(obj.CategoryId);
    if (category == null)
        return Results.NotFound();

    category.CategoryName = obj.CategoryName;
    category.CategoryDescription = obj.CategoryDescription;
    category.LastUpdate = DateTime.Now;

    db.Categories.Update(category);
    await db.SaveChangesAsync();
    return Results.Ok(obj);
});

categoryEndPoint.MapGet("/GetById", async (int id, NoteBookDbContext db) =>
{
    var category = await db.Categories.FindAsync(id);
    return category == null ? Results.NotFound() : Results.Ok(category);
});

categoryEndPoint.MapGet("/GetAll", async (NoteBookDbContext db) => await db.Categories.ToListAsync());

#endregion Categories

#region NoteBook
var noteBookEndPoint = app.MapGroup("/NoteBooks").WithTags("Notebooks");
noteBookEndPoint.MapPost("/Insert", async (NoteBook obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest(obj);

    await db.Notes.AddAsync(obj);
    return await db.SaveChangesAsync() == 0 ? Results.BadRequest(obj) : Results.Created();
});

#endregion NoteBook

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
