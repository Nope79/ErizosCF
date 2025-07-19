using System.Diagnostics;

namespace ErizosCF.Models
{
    public class ProblemStats
    {
        public DateTime SolvedDate { get; set; }
        public string ProblemName { get; set; }
        public string ContestId { get; set; }
        public string Index { get; set; }
        public string Verdict { get; set; }
        public int Dificultad { get; set; }
        public int? TeamId { get; set; }

        public static List<int> ProblemasPorSemana(List<ProblemStats> problemas, DateTime FechaInicio, DateTime FechaFin)
        {
            TimeSpan diferencia = FechaFin.Subtract(FechaInicio);
            int diasDiferencia = diferencia.Days;

            diasDiferencia = (int)Math.Ceiling(diasDiferencia / 7.0);

            List<int> semanas = Enumerable.Repeat(0, diasDiferencia).ToList();

            try
            {
                var x = FechaInicio.Date;
                int y = 0;

                foreach (var p in problemas)
                {
                    if (p.SolvedDate >= x.Date && p.SolvedDate <= x.Date.AddDays(7).AddSeconds(-1)) semanas[y]++;
                    else
                    {
                        x = x.Date.AddDays(7);
                        semanas[++y]++;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Error al generar los porblemas por semana. {e}");
            }
            return semanas;
        }
    }
}
