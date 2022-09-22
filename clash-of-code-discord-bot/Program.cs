using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace clash_of_code_bot;

public class Program : InteractionModuleBase
{
    private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json").Build();

    private readonly DiscordSocketClient _client;
    private readonly CodinGame _codinGame;
    private readonly List<string> _validModes;
    private List<string> _validLanguages;

    private Program()
    {
        _client = new DiscordSocketClient();
        _codinGame = new CodinGame(Config["ClashOfCode:Cookie"]);
        _validModes = Config["ClashOfCode:Modes"].Split(',').ToList();
    }

    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        _client.Log += Log;
        await _client.LoginAsync(TokenType.Bot, Config["Discord:Token"]);
        await _client.StartAsync();
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        await Task.Delay(-1);
    }

    private async Task Client_Ready()
    {
        _validLanguages = await _codinGame.GetLanguageIds();
        _validLanguages.Add("Any");
        var modes = new List<ApplicationCommandOptionChoiceProperties>();
        _validModes.ForEach(x => modes.Add(new ApplicationCommandOptionChoiceProperties
        {
            Name = x.ToLower(),
            Value = x
        }));

        var globalCommand = new SlashCommandBuilder()
            .WithName("clash")
            .WithDescription("starts a new clash of code lobby")
            .AddOption(
                "mode", ApplicationCommandOptionType.String, "fastest, reverse, shortest or random", true,
                choices: modes.ToArray())
            .AddOption(
                "language", ApplicationCommandOptionType.String, "name of programming language or \"any\"", true);
        try
        {
            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "clash":
                await HandleClashCommand(command);
                break;
        }
    }

    private async Task HandleClashCommand(SocketSlashCommand command)
    {
        var mode = _validModes.Find(
            x => x.ToLower().Contains(command.Data.Options.First().Value.ToString().ToLower()));
        var language = _validLanguages.Find(
            x => x.ToLower().Contains(command.Data.Options.ElementAt(1).Value.ToString().ToLower()));

        if (mode == null || language == null) return;
        var handle = await _codinGame.CreateClash(mode, language);
        mode = mode switch
        {
            "RANDOM" => ":game_die: Random",
            "FASTEST" => ":rocket: Fastest",
            "REVERSE" => ":brain: Reverse",
            _ => ":scroll: Shortest"
        };
        await command.RespondAsync($"{mode}  -  {language}  -  started by {command.User.Mention}\nhttps://www.codingame.com/clashofcode/clash/{handle}");
        LeaveClash(handle);
        DeleteResponse(command);
    }

    private async Task LeaveClash(string handle)
    {
        while (await _codinGame.GetPlayerCount(handle) < 2) await Task.Delay(1000);

        await _codinGame.LeaveClash(handle);
    }

    private async Task DeleteResponse(SocketSlashCommand command)
    {
        await Task.Delay(300000);
        await command.DeleteOriginalResponseAsync();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}