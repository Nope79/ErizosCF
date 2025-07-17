using System.Collections.Generic;

namespace ErizosCF.Models
{
    public class UserStats
    {
        public UserProfile Profile { get; set; }
        public List<ProblemStats> ProblemHistory { get; set; }
        public List<ContestStat> ContestHistory { get; set; }

        public UserStats()
        {
            ProblemHistory = new List<ProblemStats>();
            ContestHistory = new List<ContestStat>();
        }
    }
}
