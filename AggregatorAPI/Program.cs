using AggregatorAPI.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddOptions();
builder.Services.AddHttpClient();
builder.Services.AddServices();
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//builder.Services.AddOutputCache();
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
//app.UseOutputCache();

app.Run();
