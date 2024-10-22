using Microsoft.EntityFrameworkCore;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Interfaces;
using UberSystem.Infrastructure;
using UberSystem.Service;
using UberSytem.Dto;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<LocationUpdateService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Đăng ký UberSystemDbContext với DI container
builder.Services.AddDbContext<UberSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Đăng ký một Func<UberSystemDbContext> để phục vụ DbFactory
builder.Services.AddScoped<Func<UberSystemDbContext>>(provider =>
    () => provider.GetService<UberSystemDbContext>());

builder.Services.AddScoped<DbFactory>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfileExtension));


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDriverService, DriverService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

