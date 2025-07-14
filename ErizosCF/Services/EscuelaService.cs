using ErizosCF.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using ErizosCF.Services;
public class EscuelaService
{

    private const string FileName = "escuelas.json";
    public string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);

    public ObservableCollection<Escuela> LoadEscuelas()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var escuelas = JsonSerializer.Deserialize<ObservableCollection<Escuela>>(json);

                if (escuelas != null && escuelas.Count > 0)
                {
                    return escuelas;
                }
            }
        }
        catch (Exception)
        {
        }

        return new ObservableCollection<Escuela>
        {
            new Escuela { NombreEscuela = "ITSUR", Seleccionada = true },
            new Escuela { NombreEscuela = "CBTis 217", Seleccionada = false }
        };
    }

    public void SaveEscuelas(ObservableCollection<Escuela> escuelas)
    {
        try
        {
            var json = JsonSerializer.Serialize(escuelas);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving escuelas: {ex.Message}");
        }
    }

    private readonly DbService _dbService = new DbService();

    public List<Escuela> ObtenerTodas()
    {
        try
        {
            _dbService.OpenConnectionAsync();
            // Implementar consulta SQL para obtener todas las escuelas
            return new List<Escuela>(); // Datos reales de DB
        }
        finally
        {
            _dbService.CloseConnection();
        }
    }

    public void Agregar(Escuela escuela)
    {
        try
        {
            _dbService.OpenConnectionAsync();
            // Implementar INSERT en la base de datos
        }
        finally
        {
            _dbService.CloseConnection();
        }
    }

    public void Actualizar(int id, string nuevoNombre)
    {
        try
        {
            _dbService.OpenConnectionAsync();
            // Implementar UPDATE en la base de datos
        }
        finally
        {
            _dbService.CloseConnection();
        }
    }

    public void Eliminar(int id)
    {
        try
        {
            _dbService.OpenConnectionAsync();
            // Implementar DELETE en la base de datos
        }
        finally
        {
            _dbService.CloseConnection();
        }
    }
}