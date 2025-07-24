using AutoMapper;
using DUC_DCC_LogAPI;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models.ApiSetting;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Service.AuthenService;
using DUC_DCC_LogAPI.Service.DccService;
using DUC_DCC_LogAPI.Service.Duc_DccLog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(optios =>
{
    optios.RequireHttpsMetadata = false;
    optios.SaveToken = true;
    optios.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["jwt:Audience"],
        ValidIssuer = builder.Configuration["jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"]!))
    };
});

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));



builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSetting")); 
builder.Services.AddScoped<IDUC_DCC_Log, DUC_DCC_LogService>();
builder.Services.AddScoped<IAuthenService, AuthenService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();


IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

