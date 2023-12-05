using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiMyNote.Data;
using WebApiMyNote.Entities;
using WebApiMyNote.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NoteBookDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Develop"));
});

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authentication for Minimal API"
};

var securityRequirements = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

var contactInfo = new OpenApiContact()
{
    Name = "Suttiporn Srisawad",
    Email = "suttiporn.s2540@gmail.com",
    Url = new Uri("https://github.com/yee-suttiporn")
};

var license = new OpenApiLicense()
{
    Name = "Free License",
    Url = new Uri("https://github.com/yee-suttiporn")
};

var info = new OpenApiInfo()
{
    Version = "v1",
    Title = "My Note API",
    //Description = "My Note API",
    TermsOfService = new Uri("https://github.com/yee-suttiporn"),
    Contact = contactInfo,
    License = license
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityRequirements);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region User
var userEndPoint = app.MapGroup("/Users").WithTags("Users");

userEndPoint.MapPost("/Insert", [Authorize] async (User user, NoteBookDbContext db) =>
{
    if (user == null)
        return Results.BadRequest();

    try
    {
        var CheckExistUsername = await db.Users.FirstOrDefaultAsync(x => x.Username == user.Username);
        if (CheckExistUsername != null) return Results.BadRequest(new ResponseMessage<User>(false, "ชื่อผู้ใช้งานถูกใช้แล้ว!", user));

        user.Password = PasswordHasher.HashPassword(user.Password);
        user.UserId = 0;
        user.CreateDate = DateTime.Now;
        user.LastUpdate = DateTime.Now;

        await db.Users.AddAsync(user);
        int status = await db.SaveChangesAsync();

        return status == 0 ? Results.BadRequest(new ResponseMessage<User>(false, "เพิ่มผู้ใช้งานไม่สำเร็จ!", user))
        : Results.Ok(new ResponseMessage<User>(true, "เพิ่มผู้ใช้งานเรียบร้อยแล้ว", user));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<User>(false, ex.Message, user));
    }
});

userEndPoint.MapDelete("/Delete", [Authorize] async (int id, NoteBookDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
    if (user == null)
        return Results.NotFound(new ResponseMessage<User>(false, "ไม่พบผู้ใช้งานที่ต้องการลบ", user));

    try
    {
        db.Users.Remove(user);
        int status = await db.SaveChangesAsync();
        return status == 0 ? Results.BadRequest(new ResponseMessage<User>(false, "ลบผู้ใช้งานไม่สำเร็จ!", user))
        : Results.Ok(new ResponseMessage<User>(true, "ลบผู้ใช้งานเรียบร้อยแล้ว", user));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<User>(false, ex.Message, user));
    }
});

userEndPoint.MapPut("/Update", [Authorize] async (User obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == obj.UserId);

    if (user == null)
        return Results.NotFound(new ResponseMessage<User>(false, "ไม่พบผู้ใช้งาน", user));

    user.FullName = obj.FullName;
    user.LastUpdate = DateTime.Now;

    try
    {
        db.Users.Update(user);
        int status = await db.SaveChangesAsync();

        return status == 0 ? Results.BadRequest(new ResponseMessage<User>(false, "แก้ไขผู้ใช้งานไม่สำเร็จ!", user))
        : Results.Ok(new ResponseMessage<User>(true, "แก้ไขผู้ใช้งานเรียบร้อยแล้ว", user));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<User>(false, ex.Message, user));
    }
});

userEndPoint.MapPut("/ChangePassword", [Authorize] async (User obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == obj.UserId);

    if (user == null)
        return Results.NotFound(new ResponseMessage<User>(false, "ไม่พบผู้ใช้งาน", user));

    user.Password = PasswordHasher.HashPassword(obj.Password);
    user.LastUpdate = DateTime.Now;

    try
    {
        db.Users.Update(user);
        int status = await db.SaveChangesAsync();

        return status == 0 ? Results.BadRequest(new ResponseMessage<User>(false, "เปลี่ยนรหัสผ่านผู้ใช้งานไม่สำเร็จ!", user))
        : Results.Ok(new ResponseMessage<User>(true, "เปลี่ยนรหัสผ่านผู้ใช้งานเรียบร้อยแล้ว", user));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<User>(false, ex.Message, user));
    }
});

userEndPoint.MapPost("/UserLogin", [Authorize] async (User obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    try
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == obj.Username);
        if (user == null)
            return Results.NotFound(new ResponseMessage<User>(false, "ไม่พบผู้ใช้งาน", obj));

        bool checkLogin = user.Username == obj.Username && PasswordHasher.VerifyPassword(obj.Password, user.Password);

        if (checkLogin)
            return Results.Ok(new ResponseMessage<User>(true, "เข้าสู่ระบบเรียบร้อยแล้ว", obj));

        return Results.BadRequest(new ResponseMessage<User>(false, "เข้าสู่ระบบไม่สำเร็จ!", obj));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<User>(false, ex.Message, obj));
    }
});

userEndPoint.MapGet("/GetById", [Authorize] async (int id, NoteBookDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

userEndPoint.MapGet("/GetAll", [Authorize] async (NoteBookDbContext db) => await db.Users.ToListAsync());

#endregion User

#region Categories

var categoryEndPoint = app.MapGroup("/Categories").WithTags("Categories");

categoryEndPoint.MapPost("/Insert", [Authorize] async (Category obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    try
    {
        obj.CategoryId = 0;
        await db.Categories.AddAsync(obj);
        int status = await db.SaveChangesAsync();
        return status == 0 ? Results.BadRequest(new ResponseMessage<Category>(false, "เพิ่มหมวดหมู่ไม่สำเร็จ!", obj))
        : Results.Ok(new ResponseMessage<Category>(true, "เพิ่มหมวดหมู่เรียบร้อยแล้ว", obj));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<Category>(false, ex.Message, obj));
    }
});

categoryEndPoint.MapDelete("/Delete", [Authorize] async (int id, NoteBookDbContext db) =>
{
    var category = await db.Categories.FirstOrDefaultAsync(x => x.CategoryId == id);
    if (category == null)
        return Results.NotFound(new ResponseMessage<Category>(false, "ไม่พบหมวดหมู่ที่ต้องการลบ", category));

    try
    {
        db.Categories.Remove(category);
        int status = await db.SaveChangesAsync();
        return status == 0 ? Results.BadRequest(new ResponseMessage<Category>(false, "ลบหมวดหมู่ไม่สำเร็จ!", category))
        : Results.Ok(new ResponseMessage<Category>(false, "ลบหมวดหมู่เรียบร้อยแล้ว", category));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<Category>(false, ex.Message, category));
    }
});

categoryEndPoint.MapPut("/Update", [Authorize] async (Category obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    var category = await db.Categories.FindAsync(obj.CategoryId);
    if (category == null)
        return Results.NotFound(new ResponseMessage<Category>(false, "ไม่พบหมวดหมู่ที่ต้องการแก้ไข!", obj));

    category.CategoryName = obj.CategoryName;
    category.CategoryDescription = obj.CategoryDescription;
    category.LastUpdate = DateTime.Now;

    try
    {
        db.Categories.Update(category);
        int status = await db.SaveChangesAsync();
        return status == 0 ? Results.BadRequest(new ResponseMessage<Category>(false, "แก้ไขหมวดหมู่ไม่สำเร็จ!", category))
        : Results.Ok(new ResponseMessage<Category>(true, "แก้ไขหมวดหมู่เรียบร้อยแล้ว", category));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<Category>(false, ex.Message, obj));
    }
});

categoryEndPoint.MapGet("/GetById", [Authorize] async (int categoryId, int userId, NoteBookDbContext db) =>
{
    var category = await db.Categories.FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.UserId == userId);
    return category == null ? Results.NotFound() : Results.Ok(category);
});

categoryEndPoint.MapGet("/GetAll", [Authorize] async (int userId, NoteBookDbContext db) =>
{
    return await db.Categories.Where(x => x.UserId == userId).ToListAsync();
});

#endregion Categories

#region NoteBook
var noteBookEndPoint = app.MapGroup("/NoteBooks").WithTags("Notebooks");
noteBookEndPoint.MapPost("/Insert", [Authorize] async (NoteBook obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    try
    {
        obj.NoteId = 0;
        await db.Notes.AddAsync(obj);
        return await db.SaveChangesAsync() == 0 ? Results.BadRequest(new ResponseMessage<NoteBook>(false, "เพิ่มโน๊ตไม่สำเร็จ!", obj))
        : Results.Ok(new ResponseMessage<NoteBook>(true, "เพิ่มโน๊ตเรียบร้อยแล้ว", obj));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<NoteBook>(false, ex.Message, obj));
    }
});

noteBookEndPoint.MapDelete("/Delete", [Authorize] async (int id, NoteBookDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note == null)
        return Results.NotFound(new ResponseMessage<NoteBook>(false, "ไม่พบโน๊ตที่ต้องการลบ!", note));

    try
    {
        db.Notes.Remove(note);
        return await db.SaveChangesAsync() == 0 ? Results.BadRequest(new ResponseMessage<NoteBook>(false, "ลบโน๊ตไม่สำเร็จ!", note))
        : Results.Ok(new ResponseMessage<NoteBook>(true, "ลบโน๊ตเรียบร้อยแล้ว", note));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<NoteBook>(false, ex.Message, note));
    }
});

noteBookEndPoint.MapPut("/Update", [Authorize] async (NoteBook obj, NoteBookDbContext db) =>
{
    if (obj == null)
        return Results.BadRequest();

    var note = await db.Notes.FindAsync(obj.NoteId);
    if (note == null)
        return Results.NotFound(new ResponseMessage<NoteBook>(false, "ไม่พบโน๊ตที่ต้องการแก้ไข!", obj));

    note.NoteTitle = obj.NoteTitle;
    note.NoteDescription = obj.NoteDescription;
    note.CategoryId = obj.CategoryId;
    note.LastUpdate = DateTime.Now;

    try
    {
        db.Notes.Update(note);
        return await db.SaveChangesAsync() == 0 ? Results.BadRequest(new ResponseMessage<NoteBook>(false, "แก้ไขโน๊ตไม่สำเร็จ!", obj))
        : Results.Ok(new ResponseMessage<NoteBook>(true, "แก้ไขโน๊ตเรียบร้อยแล้ว", obj));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ResponseMessage<NoteBook>(false, ex.Message, obj));
    }
});

noteBookEndPoint.MapGet("/GetById", [Authorize] async (int noteId, int userId, NoteBookDbContext db) =>
{
    var note = db.Notes.FirstOrDefault(x => x.NoteId == noteId && x.UserId == userId);
    return note == null ? Results.NotFound() : Results.Ok(note);
});

noteBookEndPoint.MapGet("/GetAll", [Authorize] async (int userId, NoteBookDbContext db) =>
{
    return await db.Notes.Where(x => x.UserId == userId).ToListAsync();
});

#endregion NoteBook

#region Authentication
var authEndPoint = app.MapGroup("/Authentication");

authEndPoint.MapPost("/GetToken", [AllowAnonymous] (UserAuth user) =>
{
    if (user.Username == "admin@suttiporn.com" && user.Password == "@ApiMyNote2023")
    {
        var secureKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var securityKey = new SymmetricSecurityKey(secureKey);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id","1"),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),

            Expires = DateTime.Now.AddMinutes(5),
            Audience = audience,
            Issuer = issuer,
            SigningCredentials = credentials
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);
        return Results.Ok(jwtToken);

    }
    return Results.Unauthorized();
});

#endregion Authentication

app.Run();

internal record ResponseMessage<T>(bool Success, string Message, T? Data);
internal record UserAuth(string Username, string Password);