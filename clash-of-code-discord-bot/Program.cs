using clash_of_code_bot.Exceptions;
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
    private readonly List<string> _validLanguages = new() { "Any" };
    private readonly List<string> _validModes = new() { "RANDOM" };

    private Program()
    {
        _client = new DiscordSocketClient();
        _codinGame = new CodinGame(Config["ClashOfCode:Cookie"]);
        _validModes.AddRange(Config["ClashOfCode:Modes"].Split(',').ToList());
    }

    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private async Task MainAsync()
    {
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.AutocompleteExecuted += GenerateSuggestionsAsync;

        var token = Config["Discord:Token"];
        if (string.IsNullOrEmpty(token))
            throw new MissingTokenException("Discord token not found, check appsettings.json");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        _validLanguages.AddRange(await _codinGame.GetLanguageIds());

        await Task.Delay(-1);
    }

    private async Task Client_Ready()
    {
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
                "mode", ApplicationCommandOptionType.String,
                "fastest, reverse, shortest or random", true,
                choices: modes.ToArray())
            .AddOption(
                "language", ApplicationCommandOptionType.String,
                "name of programming language or \"any\"", true, isAutocomplete: true);

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
        await command.RespondAsync(":hourglass: Creating a lobby...");

        var modeInput = command.Data.Options.First().Value.ToString();
        var languageInput = command.Data.Options.ElementAt(1).Value.ToString();

        if (string.IsNullOrEmpty(modeInput) || string.IsNullOrEmpty(languageInput)) return;

        var mode = _validModes.Find(
            x => x.ToLower().Contains(modeInput.ToLower()));
        var language = _validLanguages.Find(
            x => x.ToLower().Contains(languageInput.ToLower()));

        if (mode == null || language == null) return;

        var modeArr = mode == "RANDOM" ? _validModes.Where(x => x != "RANDOM").ToArray() : new[] { mode };
        var handle = await _codinGame.CreateClash(modeArr, language);

        mode = mode switch
        {
            "FASTEST" => ":rocket: Fastest",
            "REVERSE" => ":brain: Reverse",
            "SHORTEST" => ":scroll: Shortest",
            "RANDOM" => ":game_die: Random",
            _ => mode
        };
        await command.ModifyOriginalResponseAsync(x =>
            x.Content = $"{mode}  -  {language}  -  started by {command.User.Mention}\n" +
                        $"https://www.codingame.com/clashofcode/clash/{handle}");
        LeaveClash(handle);
        DeleteResponseAfterDelay(command);
    }

    private async Task LeaveClash(string handle)
    {
        var playerCount = 1;
        while (playerCount == 1)
        {
            playerCount = await _codinGame.GetPlayerCount(handle);
            await Task.Delay(500);
        }

        await _codinGame.LeaveClash(handle);
    }

    private async Task DeleteResponseAfterDelay(SocketSlashCommand command)
    {
        await Task.Delay(300000);
        await command.DeleteOriginalResponseAsync();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task GenerateSuggestionsAsync(SocketAutocompleteInteraction interaction)
    {
        var input = interaction.Data.Options.ElementAt(1).Value.ToString();
        if (input == null) return;

        var results = _validLanguages.Where(
            x => x.ToLower().Contains(input.ToLower())).Select(x => new AutocompleteResult(x, x)).ToArray();

        await interaction.RespondAsync(results.Take(25));
    }
}