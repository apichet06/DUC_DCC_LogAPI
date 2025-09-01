using AutoMapper;
using DUC_DCC_LogAPI;
using DUC_DCC_LogAPI.Data;
using DUC_DCC_LogAPI.Models.ApiSetting;
using DUC_DCC_LogAPI.Models.Dto;
using DUC_DCC_LogAPI.Service.AuthenService;
using DUC_DCC_LogAPI.Service.Chart;
using DUC_DCC_LogAPI.Service.DccService;
using DUC_DCC_LogAPI.Service.Duc_DccLog;
using DUC_DCC_LogAPI.Service.History;
using DUC_DCC_LogAPI.Service.UserPermissionService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
       
    };
});

  

builder.Services.Configure<ApiSettings>(
builder.Configuration.GetSection("ApiSettings"));



builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSetting")); 
builder.Services.AddScoped<IDUC_DCC_Log, DUC_DCC_LogService>();
builder.Services.AddScoped<IAuthenService, AuthenService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IChart, ChartService>();
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "กรอก JWT token ตรงนี้ (ไม่ต้องพิมพ์ Bearer เอง)"
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
            Array.Empty<string>()
        }
    });
});
 
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePathBase("/CRUDLogs/dccduc_Api_new");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/CRUDLogs/dccduc_Api_new/swagger/v1/swagger.json", "DUC_DCC_LogAPI API"); 
    c.RoutePrefix = string.Empty;
   
});


app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

