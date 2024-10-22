using UberSystem.Api.Customer.Extensions;
using UberSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Service;
using UberSystem.Domain.Interfaces;
using UberSytem.Dto; // Thêm namespace cho DbContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Register(builder.Configuration);

// Đăng ký UberSystemDbContext với DI container
builder.Services.AddDbContext<UberSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký một Func<UberSystemDbContext> để phục vụ DbFactory
builder.Services.AddScoped<Func<UberSystemDbContext>>(provider =>
    () => provider.GetService<UberSystemDbContext>());

builder.Services.AddScoped<DbFactory>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfileExtension));


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
