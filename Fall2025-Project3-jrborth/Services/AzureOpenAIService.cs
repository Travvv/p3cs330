using System.Text.Json.Nodes;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VaderSharp2;

namespace Fall2025_Project3_jrborth.Services
{
    // kept API surface identical to your working file but moved into the web project's namespace
    public interface IAzureOpenAIService
    {
        Task<(IReadOnlyList<ReviewResult> Reviews, double AverageSentiment)> GetThreeAiReviewsAsync(string movieTitle, string year, string director);
        Task<(IReadOnlyList<TweetResult> Tweets, double AverageSentiment)> GetFiveFakeTweetsAsync(string actorName);
    }

    public record ReviewResult(string Text, double Compound);
    public record TweetResult(string Username, string Text, double Compound);

    public sealed class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deployment;
        private readonly SentimentIntensityAnalyzer _analyzer = new();

        public AzureOpenAIService(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentException("AzureOpenAI:Endpoint missing in configuration");
            string apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new ArgumentException("AzureOpenAI:ApiKey missing in configuration");
            _deployment = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentException("AzureOpenAI:DeploymentName missing in configuration");

            var credential = new AzureKeyCredential(apiKey);
            _client = new AzureOpenAIClient(new Uri(endpoint), credential);
        }

        public async Task<(IReadOnlyList<ReviewResult> Reviews, double AverageSentiment)> GetThreeAiReviewsAsync(string movieTitle, string year, string director)
        {
            ChatClient chatClient = _client.GetChatClient(_deployment);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage("You are a group of 3 distinct film critics. Produce exactly three short reviews separated by '|' and nothing else."),
                new UserChatMessage($"Write 3 short reviews for \"{movieTitle}\" ({year}) directed by {director}.")
            };

            try
            {
                var result = await chatClient.CompleteChatAsync(messages);
                string raw = result.Value.Content.FirstOrDefault()?.Text ?? string.Empty;

                var parts = raw.Split('|', StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .Take(3)
                               .ToArray();

                var reviews = new List<ReviewResult>();
                double total = 0;
                foreach (var text in parts)
                {
                    var sentiment = _analyzer.PolarityScores(text);
                    reviews.Add(new ReviewResult(text, sentiment.Compound));
                    total += sentiment.Compound;
                }

                double average = reviews.Count > 0 ? total / reviews.Count : 0.0;
                return (reviews, average);
            }
            catch
            {
                return (Array.Empty<ReviewResult>(), 0.0);
            }
        }

        public async Task<(IReadOnlyList<TweetResult> Tweets, double AverageSentiment)> GetFiveFakeTweetsAsync(string actorName)
        {
            ChatClient chatClient = _client.GetChatClient(_deployment);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage("Return a valid JSON array of objects with 'username' and 'tweet'. The response must be pure JSON and start with '['."),
                new UserChatMessage($"Generate 5 short tweets about the actor \"{actorName}\". Each object must have 'username' and 'tweet'.")
            };

            try
            {
                var result = await chatClient.CompleteChatAsync(messages);
                string jsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";

                JsonNode? parsed = null;
                try { parsed = JsonNode.Parse(jsonString); }
                catch
                {
                    int start = jsonString.IndexOf('[');
                    int end = jsonString.LastIndexOf(']');
                    if (start >= 0 && end > start)
                    {
                        string slice = jsonString[start..(end + 1)];
                        try { parsed = JsonNode.Parse(slice); } catch { parsed = null; }
                    }
                }

                var tweets = new List<TweetResult>();
                double total = 0;
                if (parsed is JsonArray arr)
                {
                    foreach (JsonNode? item in arr.Take(5))
                    {
                        if (item is JsonObject obj)
                        {
                            string username = obj["username"]?.ToString() ?? string.Empty;
                            string tweet = obj["tweet"]?.ToString() ?? string.Empty;
                            var sentiment = _analyzer.PolarityScores(tweet);
                            tweets.Add(new TweetResult(username, tweet, sentiment.Compound));
                            total += sentiment.Compound;
                        }
                    }
                }

                double average = tweets.Count > 0 ? total / tweets.Count : 0.0;
                return (tweets, average);
            }
            catch
            {
                return (Array.Empty<TweetResult>(), 0.0);
            }
        }
    }
}