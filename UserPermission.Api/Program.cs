using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserPermission.Core.Interfaces;
using UserPermission.Core.Services;
using UserPermission.Infrastructure.Data;
using UserPermission.Infrastructure.Repositories;
using UserPermission.Infrastructure.Security;

using IUserRepository = UserPermission.Core.Interfaces.IUserRepository;
using IRoleRepository = UserPermission.Core.Interfaces.IRoleRepository;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UserPermissionDb"));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();

// Security
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom 400 response for invalid models
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value?.Errors.Select(er => er.ErrorMessage).ToArray()
            });

        return new BadRequestObjectResult(new { Message = "Validation failed", Errors = errors });
    };
});

var app = builder.Build();

// Swagger is always enabled
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
