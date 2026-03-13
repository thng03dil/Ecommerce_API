using Ecommerce_API.Data;
using Ecommerce_API.Middleware;
using Ecommerce_API.Services.Implementations;
using Ecommerce_API.Services.Interfaces;
using FluentValidation;
using Ecommerce_API.Repositories.Implementations;
using Ecommerce_API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ecommerce_API.Helpers.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce_API.Data.Configurations;
using Ecommerce_API.Data.Seed;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Register Repository
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();

// Register Service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

//configure validation error response format
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value!.Errors.Count > 0)
            .Select(x => new
            {
                field = x.Key,
                messages = x.Value!.Errors.Select(e => e.ErrorMessage)
            });

        var response = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Success = false,
            ErrorCode = "VALIDATION_ERROR",
            Message = "Validation failed",
            Path = context.HttpContext.Request.Path,
            TraceId = context.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();

//register JwtSettings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

//configure JWT in swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token Input : Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

//configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var secret = builder.Configuration["Jwt:Key"]
     ?? throw new Exception("JWT Key not configured");

    var key = Encoding.UTF8.GetBytes(secret);

    options.TokenValidationParameters =
    new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Ngăn mã nguồn gốc trả về 401 trống rỗng
            context.HandleResponse();

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                StatusCode = 401,
                Success = false,
                ErrorCode = "UNAUTHORIZED",
                Message = "Authentication failed: You are not authorized to access this resource.",
                Path = context.Request.Path,
                TraceId = context.HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    };
});

var app = builder.Build();
//Console.WriteLine("--------------------------------------------------");
//Console.WriteLine("BCrypt Hash cho '123456' là: " + BCrypt.Net.BCrypt.HashPassword("123456"));
//Console.WriteLine("--------------------------------------------------");




app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  
app.UseAuthorization();
app.MapControllers();

// call seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DataSeeder.SeedAdminAsync(context);
}

app.Run();
