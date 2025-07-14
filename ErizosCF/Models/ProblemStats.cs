namespace ErizosCF.Models
{
    public class ProblemStat
    {
        public DateTime SolvedDate { get; set; }
        public string ProblemName { get; set; }
        public string ContestId { get; set; }
        public string Index { get; set; }
        public string Verdict { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
    }
}
