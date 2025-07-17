namespace ErizosCF.Models
{
    public class ProblemStats
    {
        public DateTime SolvedDate { get; set; }
        public string ProblemName { get; set; }
        public string ContestId { get; set; }
        public string Index { get; set; }
        public string Verdict { get; set; }
        public DateTime FechaResolucion { get; set; }
        public int Dificultad { get; set; }
        public int? TeamId { get; set; } 
    }
}
