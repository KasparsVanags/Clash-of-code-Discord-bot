using System.Net.Http.Headers;
using System.Net.Http.Json;
using clash_of_code_bot.Exceptions;

namespace clash_of_code_bot;

public class CodinGame
{
    private readonly HttpClient _client = new();
    private readonly string _rememberMeCookie;
    private readonly string _userId;

    public CodinGame(string rememberMeCookie)
    {
        //Cookie expires after 1 year
        if (string.IsNullOrEmpty(rememberMeCookie))
            throw new MissingCookieException("Codingame cookie not found, check appsettings.json");

        _rememberMeCookie = rememberMeCookie;
        _userId = rememberMeCookie[..7];
        _client.BaseAddress = new Uri("https://www.codingame.com/services/");
    }

    public async Task<string> CreateClash(string[] mode, string language)
    {
        var languageArr = language == "Any" ? Array.Empty<string>() : new[] { language };

        var response = await Request("ClashOfCode/CreatePrivateClash",
            new object[] { _userId, languageArr, mode });
        var result = await response.Content.ReadFromJsonAsync<Lobby>();

        return result == null ? "something broke, bot cookie expired?" : result.publicHandle;
    }

    public async Task LeaveClash(string publicHandle)
    {
        await Request("ClashOfCode/LeaveClashByHandle", new object[] { _userId, publicHandle });
    }

    public async Task<int> GetPlayerCount(string handle)
    {
        var response = await Request("ClashOfCode/FindClashByHandle", new object[] { handle });
        var result = await response.Content.ReadFromJsonAsync<Lobby>();
        return result == null ? 0 : result.players.Count;
    }

    public async Task<List<string>> GetLanguageIds()
    {
        List<string>? result = null;
        while (result == null)
        {
            var response = await Request("ProgrammingLanguage/FindAllIds", Array.Empty<object>());
            result = await response.Content.ReadFromJsonAsync<List<string>>();
            await Task.Delay(500);
        }

        return result;
    }

    private async Task<HttpResponseMessage> Request(string request, object[] parameters)
    {
        using var requestMessage = new HttpRequestMessage(new HttpMethod("POST"), request);
        requestMessage.Headers.TryAddWithoutValidation("cookie", $"rememberMe={_rememberMeCookie}");
        requestMessage.Content = JsonContent.Create(parameters);
        requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;charset=UTF-8");

        return await _client
            .SendAsync(requestMessage);
    }
}