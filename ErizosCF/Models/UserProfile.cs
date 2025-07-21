using CommunityToolkit.Mvvm.ComponentModel;
using ErizosCF.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;

namespace ErizosCF.Models
{
    public partial class UserProfile : ObservableObject
    {
        // API CF
        public string Handle { get; set; }
        public string FirstName { get; set; } = "Sin nombre";
        public string LastName { get; set; } = "Sin apellido";
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int CurrentRating { get; set; }
        public int MaxRating { get; set; }
        public DateTime FechaRegistroCF { get; set; }
        public List<ProblemStats> Problemas { get; set; } = new();

        // BD local
        public string Estado { get; set; } // "ICPC", "Excelente", "Normal", "Riesto".
        public string Sexo { get; set; } // "M" o "F"
        public int IdEscuela { get; set; }
        public string NombreEscuela { get; set; }
        public int Curso { get; set; } // 1, 2, 3

        // Estadisticas calculadas
        [ObservableProperty]
        private Dictionary<int, int> _problemasPorDificultad = new();
        public ObservableCollection<int> TODOSProblemasPorSemana { get; set; } = new();
        public ObservableCollection<int> ProblemasPorSemana { get; set; } = new();
        public int TotalSolved { get; set; }
        public int Individual { get; set; }
        public int Team { get; set; }

        // Metodos
        public async Task ActualizarDatosCodeforces(UserProfile user, List<ProblemStats> problemas, int id)
        {
            CurrentRating = user.CurrentRating; // NO SÉ POR QUE NO PUEDO HACER ESTO AL LLAMAR LA API JEJE... SIMPLEMENTE NO CARGA.
            if (NombreEscuela == null) NombreEscuela = await ObtenerEscuela(id);
            TotalSolved = problemas.Count();
            ProblemasPorDificultad.Clear();
            Team = 0;
            Individual = 0;

            /* con esto el usuario ve 0's, de momento no vamos a hacerlo visible, pero lo dejo por si algún día añado una función para que 
             * el usuario pueda elegir entre ver o no 0's
            ProblemasPorDificultad  = new Dictionary<int, int>
            {
                {-1, 0 },
                { 800, 0 },
                { 900, 0 },
                { 1000, 0 },
                { 1100, 0 },
                { 1200, 0 },
                { 1300, 0 },
                { 1400, 0 },
                { 1500, 0 },
                { 1600, 0 },
                { 1700, 0 },
                { 1800, 0 },
                { 1900, 0 },
                { 2000, 0 },
                { 2100, 0 },
                { 2200, 0 },
                { 2300, 0 },
                { 2400, 0 },
                { 2500, 0 }
            };
            */
            /// actualización de lo anterior: modifiqué el código y no sé si lo anterior sigue funcionando, yo diría que no porque se crea un nuevo diccionario, entonces esto iría en el nuevo 
            /// diccionario y no atrás. solo para que lo tengan en cuenta, pero como solo es una rápida intuición, igual y me equivoco. solo tenganlo en cuenta.

            if (problemas != null)
            {
                var nuevoDiccionario = new Dictionary<int, int>();

                foreach (var p in problemas)
                {
                    int dificultadKey = (p.Dificultad > 0) ? p.Dificultad : -1;
                    nuevoDiccionario[dificultadKey] = nuevoDiccionario.TryGetValue(dificultadKey, out var count) ? count + 1 : 1;

                    if (p.TeamId == null) Individual++;
                }

                Team = TotalSolved - Individual;
                ProblemasPorDificultad = nuevoDiccionario;
            }
        }

        public static async Task<List<UserProfile>> ObtenerTodosUsuariosAsync()
        {
            var db = new DbService();
            var usuarios = new List<UserProfile>();

            try
            {
                db.OpenConnectionAsync();

                var query = "SELECT handle, nombres, apellidos, estado, sexo, id_escuela, curso FROM usuarios";
                using var cmd = new MySqlCommand(query, db.Connection);
                using var reader = await cmd.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var usuario = new UserProfile
                    {
                        Handle = reader.GetString("handle"),
                        Estado = reader.GetString("estado").ToUpper(),
                        FirstName = reader.GetString("nombres"),
                        LastName = reader.GetString("apellidos"),
                        Sexo = reader.GetString("sexo").ToUpper(),
                        IdEscuela = reader.GetInt32("id_escuela"),
                        Curso = reader.GetInt32("curso")
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
        public static async Task<string> ObtenerEscuela(int id)
        {
            if (id <= 0) return "Desconocida";

            var db = new DbService();
            string nombreEscuela = "Desconocida";

            try
            {
                db.OpenConnectionAsync();

                var query = "select nombre from escuelas where id = @id";

                using var cmd = new MySqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@id", id);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    nombreEscuela = result.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener escuela: {ex.Message}");
            }
            finally
            {
                db.CloseConnection();
            }

            return nombreEscuela;
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

        public static int ObtenerRangoDesdeRating(int rating)
        {
            if (rating < 1200) return 0;
            if (rating < 1400) return 1;
            if (rating < 1600) return 2;
            if (rating < 1900) return 3;
            if (rating < 2100) return 4;
            if (rating < 2300) return 5;
            if (rating < 2400) return 6;
            if (rating < 2600) return 7;
            if (rating < 3000) return 8;
            return 9;
        }
    }
}
