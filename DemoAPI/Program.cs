using AggregatorAPI.Configuration;
using AggregatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddOptions();
builder.Services.AddHttpClient();


builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<RedditService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<AggregationService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

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
