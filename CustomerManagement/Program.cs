using System.Text.Json;
using BallCore.RabbitMq;
using CustomerManagement.DataAccess;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// add DBContext
var mariaDbConnectionString = builder.Configuration.GetConnectionString("MariaDbConnectionString");
builder.Services.AddDbContext<CustomerManagementDbContext>(options =>
    options.UseMySql(mariaDbConnectionString, ServerVersion.AutoDetect(mariaDbConnectionString)));

builder.Services.AddSingleton<IMessageSender>(new MessageSender("general"));

// Add framework services
builder.Services
    .AddMvc(options => options.EnableEndpointRouting = false);

// setup MVC
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMvc();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapGet("/", () => "Hello World from customermanagement!!!!!");
app.MapGet("/send", (IMessageSender rmq) =>
{
    var message = new
    {
        Msg = "Hello world",
        Time = DateTime.Now.ToLongTimeString()
    };
    
    rmq.Send("general", message);
    return Results.Ok($"Sent message: {JsonSerializer.Serialize(message)}");
});

Console.WriteLine("Starting application");
app.Run();