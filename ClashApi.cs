using System.Net.Http;
using System.Text;
using System;
using KyodaiBot.Models;
using Newtonsoft.Json;

namespace KyodaiBot;

public static class ClashApi
{
    public static readonly HttpClient Client = new HttpClient();
    public static string BaseUrl = "https://cocproxy.royaleapi.dev/v1";

    private static readonly string _token = File.ReadAllText(
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "token.txt")
    );

    public static async Task<ValidateToken?> ValidatePlayer(string playerTag, string playerToken)
    {
        string encodedTag = Uri.EscapeDataString(playerTag);
        return await Post<ValidateToken>($"{BaseUrl}/players/{encodedTag}/verifytoken", new { token = playerToken });
    }

    public static async Task<Members?> GetMembers(string clanTag)
    {
        string encodedTag = Uri.EscapeDataString(clanTag);
        return await Get<Members>($"{BaseUrl}/clans/{encodedTag}/members");
    }

    public static async Task<Clan?> GetClan(string clanTag)
    {
        string encodedTag = Uri.EscapeDataString(clanTag);
        return await Get<Clan>($"{BaseUrl}/clans/{encodedTag}");
    }

    public static async Task<Player?> GetPlayer(string playerTag)
    {
        string encodedTag = Uri.EscapeDataString(playerTag);
        return await Get<Player>($"{BaseUrl}/players/{encodedTag}");
    }


    private static async Task<T?> Get<T>(string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("Authorization", $"Bearer {_token}");

        using var response = await Client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(json);
    }

    public static async Task<T?> Post<T>(string endpoint, object body)
    {
        string json = JsonConvert.SerializeObject(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Bearer {_token}");

        using var response = await Client.SendAsync(request);
        string responseJson = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(responseJson);
    }

    public static bool ValidateTag (string tag)
    {
        return tag.StartsWith("#") && tag.Length <= 10;
    }
}
