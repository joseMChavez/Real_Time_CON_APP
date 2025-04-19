using testChannel.Services;
using testChannel.Workerd;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add services to the container.
//asi agrego los servicios que son tipos genericos
builder.Services.AddSingleton(typeof(QueueTask<>));
//Confuracion de los worker
builder.Services.AddHostedService<Consumer>();
var app = builder.Build();
// Habilitar soporte para WebSockets
app.UseWebSockets();

// Registrar el middleware de WebSocketServer
app.UseMiddleware<WebSocketServer>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/message", async (HttpRequest request, QueueTask<string> queue) =>
{
    using var reader = new StreamReader(request.Body);
    var message = await reader.ReadToEndAsync();

   await queue.EnqueueAsync(message);

    return Results.Ok(new { status = "Message queued", message });
})
.WithName("SendMessage");


app.Run();


