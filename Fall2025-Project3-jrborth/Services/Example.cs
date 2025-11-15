using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using VaderSharp2;

namespace AiConsoleApp;

internal class Program
{
    private static readonly Uri ApiEndpoint = new("https://<ENDPOINT>.cognitiveservices.azure.com/");
    private static readonly ApiKeyCredential ApiCredential = new("<API_KEY>");
    private const string AiDeployment = "<MODEL_DEPLOYMENT_NAME>";

    private const string MovieYear = "1997";
    private const string MovieName = "Titanic";
    private const string MovieDirector = "James Cameron";

    private static async Task Main()
    {
        await MovieReviewSimple();
        //await MovieReview();
        //await MovieReviewsMultipleCalls();
        //await MovieReviewsSingleCallParsed();
        //await TwitterApiSim();
        //await TwitterApiSimJson();
    }

    private static async Task MovieReviewSimple()
    {
        Console.WriteLine("Asking reviewer...");

        ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage($"You are a harsh film critic."),
            new UserChatMessage($"How would you rate the movie {MovieName} released in {MovieYear} directed by {MovieDirector} out of 10?")
        };
        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
        string review = result.Value.Content[0].Text;

        var analyzer = new SentimentIntensityAnalyzer();
        SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);

        Console.WriteLine($"{review}\n\nSentiment Analysis: {sentiment}");
    }

    private static async Task MovieReview()
    {
        Console.WriteLine("Asking reviewer...");

        var clientOptions = new AzureOpenAIClientOptions
        {

        };
        var baseClient = new AzureOpenAIClient(ApiEndpoint, ApiCredential, clientOptions);
        ChatClient chatClient = baseClient.GetChatClient(AiDeployment);

        var messages = new List<ChatMessage>()
        {
            new SystemChatMessage($"You are a harsh film critic."),
            new UserChatMessage($"How would you rate the movie {MovieName} released in {MovieYear} directed by {MovieDirector} out of 10?")
        };
        var chatCompletionOptions = new ChatCompletionOptions
        {

        };
        ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
        string review = result.Value.Content[0].Text;

        var analyzer = new SentimentIntensityAnalyzer();
        SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);

        Console.WriteLine($"{review}\n\nSentiment Analysis: {sentiment}");
    }

    private static async Task MovieReviewsMultipleCalls()
    {
        Console.WriteLine("Asking reviewers...");

        ChatClient chatClient = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

        string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy" };
        var reviews = new List<string>();
        foreach (string persona in personas)
        {
            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You are a film reviewer and film critic who {persona}."),
                new UserChatMessage($"How would you rate the movie {MovieName} released in {MovieYear} directed by {MovieDirector} out of 10 in less than 175 words?")
            };
            var chatCompletionOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 200,
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            reviews.Add(result.Value.Content[0].Text);
            Thread.Sleep(TimeSpan.FromSeconds(10)); // Request throttle due to rate limit
        }

        var analyzer = new SentimentIntensityAnalyzer();
        double sentimentTotal = 0;
        for (int i = 0; i < reviews.Count; i++)
        {
            string review = reviews[i];
            SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);
            sentimentTotal += sentiment.Compound;

            Console.WriteLine($"Review {i + 1} (sentiment {sentiment.Compound})");
            Console.WriteLine(review);
            Console.WriteLine();
        }

        double sentimentAverage = sentimentTotal / reviews.Count;
        Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");
    }

    private static async Task MovieReviewsSingleCallParsed()
    {
        Console.WriteLine("Asking reviewers...");

        ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

        string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy" };
        var messages = new ChatMessage[]
        {
            new SystemChatMessage($"You represent a group of {personas.Length} film critics who have the following personalities: {string.Join(",", personas)}. When you receive a question, respond as each member of the group with each response separated by a '|', but don't indicate which member you are."),
            new UserChatMessage($"How would you rate the movie {MovieName} released in {MovieYear} directed by {MovieDirector} out of 10 in 150 words or less?")
        };
        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
        string[] reviews = result.Value.Content[0].Text.Split('|').Select(s => s.Trim()).ToArray();

        var analyzer = new SentimentIntensityAnalyzer();
        double sentimentTotal = 0;
        for (int i = 0; i < reviews.Length; i++)
        {
            string review = reviews[i];
            SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);
            sentimentTotal += sentiment.Compound;

            Console.WriteLine($"Review {i + 1} (sentiment {sentiment.Compound})");
            Console.WriteLine(review);
            Console.WriteLine();
        }

        double sentimentAverage = sentimentTotal / reviews.Length;
        Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");
    }

    private static async Task TwitterApiSim()
    {
        Console.WriteLine("Polling Twitter...");

        ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage($"You represent the Twitter social media platform. Generate an answer with a valid JSON formatted array of objects containing the tweet and username. The response should start with [."),
            new UserChatMessage($"Generate 20 tweets from a variety of users about the movie {MovieName} released in {MovieYear} directed by {MovieDirector}.")
        };
        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);

        string tweetsJsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";
        Console.WriteLine(tweetsJsonString);
        JsonArray json = JsonNode.Parse(tweetsJsonString)!.AsArray();

        var analyzer = new SentimentIntensityAnalyzer();
        double sentimentTotal = 0;

        var tweets = json.Select(t => new { Username = t!["username"]?.ToString() ?? "", Text = t!["tweet"]?.ToString() ?? "" }).ToArray();
        foreach (var tweet in tweets)
        {
            SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
            sentimentTotal += sentiment.Compound;

            Console.WriteLine($"{tweet.Username}: \"{tweet.Text}\" (sentiment {sentiment.Compound})\n");
        }

        double sentimentAverage = sentimentTotal / tweets.Length;
        Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");
    }

    private record class Tweet(string Username, string Text);
    private record class Tweets(Tweet[] Items);
    private static async Task TwitterApiSimJson()
    {
        Console.WriteLine("Polling Twitter...");

        ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        JsonNode schema = options.GetJsonSchemaAsNode(typeof(Tweets), new()
        {
            TreatNullObliviousAsNonNullable = true,
        });

        // code above generates this schema dynamically from the classes
        string responseSchema = @"
        {
            ""type"": ""object"",
            ""properties"": {
                ""Items"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""Username"": {
                                ""type"": ""string"",
                                ""description"": ""The username of the tweeter""
                            },
                            ""Text"": {
                                ""type"": ""string"",
                                ""description"": ""The content of the tweet""
                            }
                        },
                        ""required"": [""Username"", ""Text""],
                        ""additionalProperties"": false
                    }
                }
            },
            ""required"": [""Items""],
            ""additionalProperties"": false
        }";

        var chatCompletionOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("XTwitterApiJson", BinaryData.FromString(schema.ToString()), jsonSchemaIsStrict: true),
        };
        var messages = new ChatMessage[]
        {
            new SystemChatMessage($"You represent the X/Twitter social media platform API that returns JSON data."),
            new UserChatMessage($"Generate 20 tweets from a variety of users about the movie {MovieName} released in {MovieYear} directed by {MovieDirector}.")
        };
        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages, chatCompletionOptions);

        string jsonString = result.Value.Content.FirstOrDefault()?.Text ?? @"{""Items"":[]}";
        Tweets tweets = JsonSerializer.Deserialize<Tweets>(jsonString) ?? new([]);

        Console.WriteLine(schema.ToString());
        Console.WriteLine();
        Console.WriteLine(jsonString);
        Console.WriteLine();

        var analyzer = new SentimentIntensityAnalyzer();
        double sentimentTotal = 0;
        foreach (var tweet in tweets.Items)
        {
            SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
            sentimentTotal += sentiment.Compound;

            Console.WriteLine($"{tweet.Username}: \"{tweet.Text}\" (sentiment {sentiment.Compound})\n");
        }

        double sentimentAverage = sentimentTotal / tweets.Items.Length;
        Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");
    }
}
