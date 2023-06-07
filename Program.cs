using SAT_Socket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.SetIsOriginAllowed(origin => true)
.AllowCredentials());


app.UseHttpsRedirection();

app.MapHub<ChatHub>("/hub");

app.Run();