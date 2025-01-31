using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Filters;
using System.Text;
using ToDoList.Models.EFModel;
using ToDoList.Models.Helpers;

//���o�պA�]�w��
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

//Log��l�� (����host�Ұʪ����~)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()//��log�T���h��ܤ@�I
    .ReadFrom.Configuration(configuration)// �q�]�w�ɤ�Ū��
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog �ⶥ�q��l��
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)// �q�]�w�ɤ�Ū��
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Filter.ByExcluding(Matching.FromSource("Microsoft"))
    );

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ���U DbContext
    builder.Services.AddDbContext<ToDoListContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ���U JwtHelpers
    builder.Services.AddSingleton<JwtHelpers>();

    // ���U JWT �]�w
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
            options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
                NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�ä��\�� [Authorize] �P�_����
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                // ���� Issuer
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

                // �q�`���ӻݭn���� Audience
                ValidateAudience = false,

                // ���� Token �����Ĵ���
                ValidateLifetime = true,

                // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
                ValidateIssuerSigningKey = false,

                // Key���ӱq IConfiguration ���o
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")!))
            };
        });

    // ���U���v�A��
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}