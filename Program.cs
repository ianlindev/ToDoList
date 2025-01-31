using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using ToDoList.Models.EFModel;

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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}