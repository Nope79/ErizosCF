using System.Text.Json;
using ErizosCF.Models;

namespace ErizosCF.Services
{
    public class CFService
    {
        private readonly HttpClient _httpClient;

        public CFService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<UserProfile?> GetUserInfoAsync(string handle)
        {
            try
            {
                string url = $"https://codeforces.com/api/user.info?handles={handle}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                var user = doc.RootElement.GetProperty("result")[0];

                int? currentRating = null;
                if (user.TryGetProperty("rating", out var r))
                    currentRating = r.GetInt32();

                int? maxRating = null;
                if (user.TryGetProperty("maxRating", out var mr))
                    maxRating = mr.GetInt32();

                return new UserProfile
                {
                    Handle = user.GetProperty("handle").GetString(),
                    FirstName = user.TryGetProperty("firstName", out var fn) ? fn.GetString() : "",
                    LastName = user.TryGetProperty("lastName", out var ln) ? ln.GetString() : "",
                    CurrentRating = (int)currentRating,
                    MaxRating = (int)maxRating
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ContestStat>> GetUserRatingAsync(string handle)
        {
            try
            {
                string url = $"https://codeforces.com/api/user.rating?handle={handle}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return new List<ContestStat>();

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                var list = new List<ContestStat>();
                foreach (var entry in doc.RootElement.GetProperty("result").EnumerateArray())
                {
                    list.Add(new ContestStat
                    {
                        ContestName = entry.GetProperty("contestName").GetString(),
                        Rank = entry.GetProperty("rank").GetInt32(),
                        NewRating = entry.GetProperty("newRating").GetInt32(),
                        OldRating = entry.GetProperty("oldRating").GetInt32(),
                        ContestDate = DateTimeOffset.FromUnixTimeSeconds(entry.GetProperty("ratingUpdateTimeSeconds").GetInt64()).DateTime
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ProblemStat>> GetUserStatusAsync(string handle)
        {
            try
            {
                string url = $"https://codeforces.com/api/user.status?handle={handle}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return new List<ProblemStat>();

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                var list = new List<ProblemStat>();
                foreach (var entry in doc.RootElement.GetProperty("result").EnumerateArray())
                {
                    list.Add(new ProblemStat
                    {
                        ProblemName = $"{entry.GetProperty("problem").GetProperty("contestId").GetInt32()}-{entry.GetProperty("problem").GetProperty("index").GetString()}",
                        Verdict = entry.TryGetProperty("verdict", out var v) ? v.GetString() ?? "UNKNOWN" : "UNKNOWN",
                        Date = DateTimeOffset.FromUnixTimeSeconds(entry.GetProperty("creationTimeSeconds").GetInt64()).DateTime
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
