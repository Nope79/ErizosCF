namespace ErizosCF.Models
{
    public class UserProfile
    {
        public string Handle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CurrentRating { get; set; }
        public int MaxRating { get; set; }
        public string Estado { get; set; }  // progreso normal, bueno, mal, etc
        public string Sexo { get; set; }
        public string Escuela { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalSolved { get; set; }
        public List<ProblemStat> Problemas { get; set; } = new();
        public double AvgPositionLastNContests { get; set; }
        public DateTime LastContestDate { get; set; }
        public List<ContestStat> Concursos { get; set; } = new();
    }
}
