using System.Text;
using KyodaiBot.Models;
using Newtonsoft.Json;

namespace KyodaiBot;

public sealed class ClashApi(string token)
{
    public readonly HttpClient Client = new HttpClient();
    public string BaseUrl = "https://cocproxy.royaleapi.dev/v1";

    public async Task<ValidateToken?> ValidatePlayer(string playerTag, string playerToken)
    {
        string encodedTag = Uri.EscapeDataString(playerTag);
        return await Post<ValidateToken>($"{BaseUrl}/players/{encodedTag}/verifytoken", new { token = playerToken });
    }

    public async Task<Members?> GetMembers(string clanTag)
    {
        string encodedTag = Uri.EscapeDataString(clanTag);
        return await Get<Members>($"{BaseUrl}/clans/{encodedTag}/members");
    }

    public async Task<Clan?> GetClan(string clanTag)
    {
        string encodedTag = Uri.EscapeDataString(clanTag);
        return await Get<Clan>($"{BaseUrl}/clans/{encodedTag}");
    }

    public async Task<Player?> GetPlayer(string playerTag)
    {
        string encodedTag = Uri.EscapeDataString(playerTag);
        return await Get<Player>($"{BaseUrl}/players/{encodedTag}");
    }


    private async Task<T?> Get<T>(string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("Authorization", $"Bearer {token}");

        using var response = await Client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<T?> Post<T>(string endpoint, object body)
    {
        string json = JsonConvert.SerializeObject(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Bearer {token}");

        using var response = await Client.SendAsync(request);
        string responseJson = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(responseJson);
    }

    public bool ValidateTag (string tag)
    {
        return tag.StartsWith("#") && tag.Length <= 10;
    }
}
