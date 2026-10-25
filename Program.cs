using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var endpoint = (config["AZURE_OPENAI_ENDPOINT"]
    ?? throw new InvalidOperationException(
        "Set AZURE_OPENAI_ENDPOINT with dotnet user-secrets, for example https://YOUR-RESOURCE-NAME.openai.azure.com"))
    .TrimEnd('/');
var deployment = config["AZURE_OPENAI_DEPLOYMENT"] ?? "gpt-6-luna";
var apiKey = config["AZURE_OPENAI_API_KEY"];

var ticket = "I was charged twice for my order. Please return the extra charge.";

var body = new JsonObject
{
    ["model"] = deployment,
    ["input"] = ticket,
    ["questions"] = new JsonArray
    {
        new JsonObject
        {
            ["type"] = "predicate",
            ["name"] = "refund_requested",
            ["instructions"] = "Does the customer ask for a refund?"
        },
        new JsonObject
        {
            ["type"] = "choice",
            ["name"] = "department",
            ["instructions"] = "Which department should handle this complaint?",
            ["choices"] = new JsonArray
            {
                new JsonObject { ["value"] = "billing", ["description"] = "Payments and refunds." },
                new JsonObject { ["value"] = "technical", ["description"] = "Product problems." },
                new JsonObject { ["value"] = "shipping", ["description"] = "Delivery and tracking." },
                new JsonObject { ["value"] = "other", ["description"] = "All other requests." }
            }
        },
        new JsonObject
        {
            ["type"] = "score",
            ["name"] = "severity",
            ["instructions"] = "How severe is this issue?",
            ["levels"] = new JsonArray
            {
                new JsonObject { ["label"] = "Cosmetic", ["description"] = "No lost functionality." },
                new JsonObject { ["label"] = "Workaround available", ["description"] = "A task fails, but another way works." },
                new JsonObject { ["label"] = "Fully blocked", ["description"] = "A task fails with no workaround." }
            }
        }
    }
};

using var decision = await PostDecisionAsync(endpoint, apiKey, body);

foreach (var answer in decision.RootElement.GetProperty("answers").EnumerateArray())
{
    PrintAnswer(answer);
}

static async Task<JsonDocument> PostDecisionAsync(string endpoint, string? apiKey, JsonObject body)
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
    using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/openai/v1/decisions");

    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        request.Headers.Add("api-key", apiKey);
    }
    else
    {
        var credential = new DefaultAzureCredential();
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]),
            CancellationToken.None);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    // StringContent sets a fixed Content-Length; JsonContent uses chunked encoding.
    request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

    using var response = await http.SendAsync(request);
    var text = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Decisions API returned {(int)response.StatusCode}: {text}");
    }

    return JsonDocument.Parse(text);
}

static void PrintAnswer(JsonElement answer)
{
    var type = answer.GetProperty("type").GetString();
    var name = answer.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;

    switch (type)
    {
        case "predicate":
            var probability = answer.GetProperty("probability").GetDouble();
            Console.WriteLine($"{name}: {probability:P0} true");
            break;

        case "choice":
            var choice = answer.GetProperty("choice").GetString();
            var choiceConfidence = answer.GetProperty("confidence").GetDouble();
            Console.WriteLine($"{name}: {choice} (confidence {choiceConfidence:P0})");
            break;

        case "score":
            var score = answer.GetProperty("score").GetDouble();
            var scoreConfidence = answer.GetProperty("confidence").GetDouble();
            Console.WriteLine($"{name}: {score:0.00} (confidence {scoreConfidence:P0})");
            break;

        case "refusal":
            // A refusal is not a low probability. Send this one to a person.
            Console.WriteLine($"{name ?? "(unnamed)"}: refused, send to a person");
            break;

        default:
            Console.WriteLine($"Unexpected answer: {answer.GetRawText()}");
            break;
    }
}
