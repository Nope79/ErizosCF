using CommunityToolkit.Mvvm.ComponentModel;
using ErizosCF.Services;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace ErizosCF.Models
{
    public partial class Escuela : ObservableObject
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        [ObservableProperty]
        bool estaSeleccionada;

        public event Action FiltrosCambiaron;
        partial void OnEstaSeleccionadaChanged(bool oldValue, bool newValue)
        {
            FiltrosCambiaron?.Invoke(); 
        }
        public static async Task<List<Escuela>> ObtenerEscuelasAsync()
        {
            var db = new DbService();
            var escuelas = new List<Escuela>();

            try
            {
                db.OpenConnectionAsync();

                var query = "SELECT id, nombre FROM escuelas";
                using var cmd = new MySqlCommand(query, db.Connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var escuela = new Escuela
                    {
                        Id = reader.GetInt32("id"),
                        Nombre = reader.GetString("nombre").ToUpper()
                    };

                    escuelas.Add(escuela);
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

            return escuelas;
        }
    }
}