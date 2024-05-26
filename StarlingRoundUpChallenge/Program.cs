using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StarlingRoundUpChallenge;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

// add custom services
builder.Services.AddScoped<IApiHelper, ApiHelper>();
builder.Services.AddScoped<IAccountService, AccountService>();

// set up api client
builder.Services.Configure<StarlingApiSettings>(configuration.GetSection("StarlingApiSettings"));
builder.Services.AddHttpClient<IApiHelper, ApiHelper>((provider, client) =>
{
    var settings = provider.GetRequiredService<IOptions<StarlingApiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUri);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(settings.MediaType));

    client.DefaultRequestHeaders.Add("Authorization", settings.Authorization);
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15)
}).SetHandlerLifetime(Timeout.InfiniteTimeSpan);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(); 
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();