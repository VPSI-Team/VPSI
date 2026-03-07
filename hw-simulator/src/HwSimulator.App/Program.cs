using HwSimulator.App;
using HwSimulator.App.Models;
using HwSimulator.App.Transport;

var builder = Host.CreateApplicationBuilder(args);

// Konfigurace simulátoru z appsettings + env proměnných
builder.Services.Configure<SimulatorOptions>(builder.Configuration.GetSection(SimulatorOptions.SectionName));

// Env proměnné z docker-compose přepisují appsettings
var envApiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
if (!string.IsNullOrEmpty(envApiBaseUrl))
    builder.Configuration[$"{SimulatorOptions.SectionName}:ApiBaseUrl"] = envApiBaseUrl;

var envScenario = Environment.GetEnvironmentVariable("SIMULATOR_SCENARIO");
if (!string.IsNullOrEmpty(envScenario))
    builder.Configuration[$"{SimulatorOptions.SectionName}:Scenario"] = envScenario;

// HTTP klient pro odesílání eventů
builder.Services.AddHttpClient<IEventSender, HttpEventSender>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
