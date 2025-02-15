using CMS.Application.Mapper;
using CMS.Application.Services.Category;
using CMS.Application.Services.Content;
using CMS.Application.Services.User;
using CMS.Domain.Models.User;
using CMS.Domain.Repositories.Category;
using CMS.Domain.Repositories.Content;
using CMS.Domain.Repositories.Generic;
using CMS.Domain.Repositories.User;
using CMS.Domain.Services.Category;
using CMS.Domain.Services.Content;
using CMS.Domain.Services.User;
using CMS.Domain.UnitOfWork;
using CMS.Infrastructure.Context;
using CMS.Infrastructure.Repositories.Category;
using CMS.Infrastructure.Repositories.Content;
using CMS.Infrastructure.Repositories.Generic;
using CMS.Infrastructure.Repositories.User;
using CMS.Infrastructure.UnitOfWork;
using CMS.Shared.Exceptions;
using CMS.Shared.Helper.Cache;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"),
        sqlServerOptionsAction =>
        {
            sqlServerOptionsAction.MigrationsAssembly("CMS.Infrastructure");
        });
});

builder.Services.AddMemoryCache();

MappingConfig.ConfigureMappings();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddSingleton<CacheHelper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IUserContentRepository, UserContentRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));



var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();


app.UseCustomException();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
