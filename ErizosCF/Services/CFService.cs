using ErizosCF.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

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

        // método para filtrar los problemas de la primera tabla jeje
        public async Task<List<ProblemStats>> GetUserStatusAsync(string handle, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var problemasUnicos = new HashSet<string>();
            var listaFiltrada = new List<ProblemStats>();

            // esto es necesario para que los límites de las fechas, en la fecha final, se cuente el día del límite también
            fechaFin = fechaFin.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            try
            {
                var response = await _httpClient.GetStringAsync($"https://codeforces.com/api/user.status?handle={handle}");
                using JsonDocument doc = JsonDocument.Parse(response);

                // Convertir a lista y ordenar por fecha (más antiguo primero)
                var submissions = doc.RootElement.GetProperty("result").EnumerateArray()
                    .Select(entry => new {
                        Entry = entry,
                        Time = entry.GetProperty("creationTimeSeconds").GetInt64()
                    })
                    .OrderBy(x => x.Time)  // Orden cronológico ascendente
                    .ToList();

                foreach (var sub in submissions)
                {
                    var entry = sub.Entry;

                    // no hay veredicto o es diferente de OK
                    if (!entry.TryGetProperty("verdict", out var verdict) || verdict.GetString() != "OK")
                        continue;

                    // no hay fecha
                    if (!entry.TryGetProperty("creationTimeSeconds", out var time))
                        continue;

                    var fechaResolucion = DateTimeOffset.FromUnixTimeSeconds(time.GetInt64()).ToLocalTime().DateTime;

                    // fecha fuera de rango
                    if (fechaInicio != null && fechaResolucion < fechaInicio.Value) continue;
                    if (fechaFin != null && fechaResolucion > fechaFin.Value) continue;

                    // dificultad
                    int dificultad = -1; // no hay rating
                    if (entry.GetProperty("problem").TryGetProperty("rating", out var rating))
                    {
                        dificultad = rating.GetInt32();
                        if (dificultad > 2500) continue; // quitar esta línea si se quiere incluir todos los ratings
                    }

                    // id, no sé como funciona, preguntar al chat y no a mí. Basicamente sirve para añadirlo al hashset
                    // si hay problemas de conteo, muy probablemente se deba a esto. inventarse otra fórmula y revisar si persisten dichos problemas...
                    var problemId = $"{entry.GetProperty("problem").GetProperty("contestId")}-{entry.GetProperty("problem").GetProperty("index")}";

                    // si no se añade al hash
                    if (!problemasUnicos.Add(problemId))
                        continue;

                    var author = entry.GetProperty("author");

                    bool isTeam = author.TryGetProperty("teamId", out _);
                    int? teamId = isTeam ? author.GetProperty("teamId").GetInt32() : null;

                    // agregar a lista ya filtrada
                    listaFiltrada.Add(new ProblemStats
                    {
                        ProblemName = problemId,
                        Verdict = "OK",
                        FechaResolucion = fechaResolucion,
                        Dificultad = dificultad,
                        TeamId = teamId
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener problemas: {ex.Message}");
            }

            return listaFiltrada;
        }
    }
}
