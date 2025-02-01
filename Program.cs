using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Filters;
using System.Text;
using ToDoList.Models.EFModel;
using ToDoList.Models.Filters;
using ToDoList.Models.Helpers.Implement;
using ToDoList.Models.Helpers.Interface;
using ToDoList.Models.Service.Implement;
using ToDoList.Models.Service.Interface;

//取得組態設定檔
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

//Log初始化 (捕捉host啟動的錯誤)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()//讓log訊息多顯示一點
    .ReadFrom.Configuration(configuration)// 從設定檔中讀取
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog 兩階段初始化
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)// 從設定檔中讀取
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Filter.ByExcluding(Matching.FromSource("Microsoft"))
    );

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 註冊 DbContext
    builder.Services.AddDbContext<ToDoListContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 註冊 TokenValidationParameters
    var tokenValidationParameters = new TokenValidationParameters
    {
        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        // 透過這項宣告，就可以從 "roles" 取值，並允許讓 [Authorize] 判斷角色
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        ClockSkew = TimeSpan.Zero,
        // 驗證 Issuer
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

        // 通常不太需要驗證 Audience
        ValidateAudience = false,

        // 驗證 Token 的有效期間
        ValidateLifetime = true,

        // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
        ValidateIssuerSigningKey = true,

        // Key應該從 IConfiguration 取得
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")!))
    };

    // 註冊 TokenValidationParameters
    builder.Services.AddSingleton(tokenValidationParameters);

    // 註冊 JWT 設定
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
            options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

            options.TokenValidationParameters = tokenValidationParameters;
        });

    //JWT Helpers 需要用到 tokenValidationParameters 注入
    builder.Services.AddSingleton(tokenValidationParameters);

    // 註冊授權服務
    builder.Services.AddAuthorization();

    // 註冊 JwtHelpers
    builder.Services.AddSingleton<IJwtHelpers, JwtHelpers>();

    // 註冊 EncryptionService
    builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

    // 註冊 全域 API 輸出格式 Filter
    builder.Services.AddMvc(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
    });

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