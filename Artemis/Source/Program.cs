using System.Diagnostics;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var config = Config.Load();

var builder = Host.CreateApplicationBuilder();

builder.Services
    .AddSerilog()
    .AddSingleton<Stopwatch>()
    .AddHangfire(x => x.UseLiteDbStorage())
    .AddHangfireServer()
    .AddSingleton(config)
    .AddSingleton<Cache.MessageCache>()
    .AddDiscordGateway(options => {
        options.Token = config.Token;
        options.Intents = GatewayIntents.All;
    })
    .AddGatewayEventHandlers(typeof(Program).Assembly)
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext, AutocompleteInteractionContext>(options => {
        options.DefaultContexts = [InteractionContextType.Guild];
        options.ResultHandler = new Handlers.ApplicationCommandResultHandler<ApplicationCommandContext>();
    })
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
    .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
    .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
    .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
    .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>();

var host = builder.Build();

// Add command modules.
host.AddModules(typeof(Program).Assembly);

// Add event handlers.
host.UseGatewayEventHandlers();

// Start
await host.RunAsync();
