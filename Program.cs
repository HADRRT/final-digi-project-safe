// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using System.Text;
// using ExpensesSystem.Data;

// var builder = WebApplication.CreateBuilder(args);

// // ---------------- CONTROLLERS ----------------
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // ---------------- SWAGGER ----------------
// builder.Services.AddSwaggerGen(c =>
// {
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.ApiKey,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header
//     });

//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             new string[] {}
//         }
//     });
// });

// // ---------------- DATABASE ----------------
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
// );

// // ---------------- CORS (IMPORTANT FIX) ----------------
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend",
//         policy =>
//         {
//             policy
//                 .WithOrigins("http://localhost:3001")
//                 .AllowAnyHeader()
//                 .AllowAnyMethod();
//         });
// });

// // ---------------- AUTH ----------------
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = false,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//         };
//     });

// builder.Services.AddAuthorization();

// var app = builder.Build();

// // ---------------- PIPELINE ORDER (VERY IMPORTANT) ----------------
// app.UseSwagger();
// app.UseSwaggerUI();

// app.UseHttpsRedirection();

// // ✅ CORS MUST BE HERE (before auth)
// app.UseCors("AllowFrontend");

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ExpensesSystem.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();