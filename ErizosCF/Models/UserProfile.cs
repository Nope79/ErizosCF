using ErizosCF.Services;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;

namespace ErizosCF.Models
{
    public class UserProfile
    {
        public string Handle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int CurrentRating { get; set; }
        public int MaxRating { get; set; }
        public int Escuela { get; set; }
        public int TotalSolved { get; set; }
        public List<ProblemStat> Problemas { get; set; } = new();
        public Dictionary<int, int> ProblemasPorDificultad { get; set; } = new();

        public void ActualizarDatosCodeforces(UserProfile userInfo, List<ProblemStat> problemas)
        {
            FirstName = userInfo.FirstName;
            LastName = userInfo.LastName;
            CurrentRating = userInfo.CurrentRating;
            MaxRating = userInfo.MaxRating;

            var problemasOk = problemas
                .Where(p => p.Verdict == "OK")
                .DistinctBy(p => p.ProblemName)
                .ToList();

            TotalSolved = problemasOk.Count;
            Problemas = problemasOk;

            // Versión 1: Si Rating es int y 0 significa "no rating"
            ProblemasPorDificultad = problemasOk
                .Where(p => p.Rating != 0) // Asumiendo que 0 significa no tiene rating
                .GroupBy(p => p.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Versión 2: Si Rating siempre tiene valor (sin filtrar)
            /*
            ProblemasPorDificultad = problemasOk
                .GroupBy(p => p.Rating)
                .ToDictionary(g => g.Key, g => g.Count());
            */
        }
        public static async Task<List<UserProfile>> ObtenerTodosAsync()
        {
            var db = new DbService();
            var usuarios = new List<UserProfile>();

            try
            {
                db.OpenConnectionAsync();

                var query = "SELECT handle FROM usuarios";
                using var cmd = new MySqlCommand(query, db.Connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var usuario = new UserProfile
                    {
                        Handle = reader.GetString("handle")
                    };

                    usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener usuarios: {ex.Message}");
            }
            finally
            {
                db.CloseConnection();
            }

            return usuarios;
        }


        public async Task GuardarAsync()
        {
            var db = new DbService();
            try
            {
                db.OpenConnectionAsync();
                // Implementar INSERT/UPDATE en la base de datos
            }
            finally
            {
                db.CloseConnection();
            }
        }

        public async Task EliminarAsync()
        {
            var db = new DbService();
            try
            {
                db.OpenConnectionAsync();
                // Implementar DELETE en la base de datos
            }
            finally
            {
                db.CloseConnection();
            }
        }
    }
}
