using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using ToDoList.Models.EFModel;

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