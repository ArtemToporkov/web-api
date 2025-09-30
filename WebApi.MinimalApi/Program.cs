using Microsoft.AspNetCore.Mvc.Formatters;
using WebApi.MinimalApi.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddControllers(options =>
    {
        options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
        options.ReturnHttpNotAcceptable = true;
        options.RespectBrowserAcceptHeader = true;
    })
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    });
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.MapControllers();

app.Run();