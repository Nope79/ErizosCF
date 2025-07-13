using ErizosCF.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
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
}