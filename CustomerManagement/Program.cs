using BallCore.RabbitMq;
using CustomerManagement;
using CustomerManagement.DataAccess;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add DBContext
var mariaDbConnectionString = builder.Configuration.GetConnectionString("MariaDbConnectionString");
builder.Services.AddDbContext<CustomerManagementDbContext>(options =>
    options.UseMySql(mariaDbConnectionString, ServerVersion.AutoDetect(mariaDbConnectionString)));

//Create connection
var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

if (!isDevelopment)
{
    var connection = new ConnectionFactory
    {
        HostName = "rabbitmq",
        Port = 5672,
        UserName = "Rathalos",
        Password = "1234",
        DispatchConsumersAsync = true
    }.CreateConnection();

    builder.Services.AddSingleton(connection);

//Inject ExchangeDeclarator
    var exchanges = new Dictionary<string, IEnumerable<string>>
    {
        { "customer_exchange", new[] { "payment", "servicedesk", "notifications", "order_management" } }
    };

    builder.Services.AddHostedService(_ => new ExchangeDeclarator(connection, exchanges));

//Inject receiver
    builder.Services.AddHostedService<CustomerMessageReceiver>();

//Inject sender
    builder.Services.AddTransient<IMessageSender, MessageSender>();
}

// Add framework services
builder.Services
    .AddMvc(options => options.EnableEndpointRouting = false);

// setup MVC
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseMvc();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.MapGet("/", () => "Hello World suppliermanagement!");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<CustomerManagementDbContext>();
    if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();
}

Console.WriteLine("Starting application");
app.Run();