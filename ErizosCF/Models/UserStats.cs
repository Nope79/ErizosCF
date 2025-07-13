using System.Collections.Generic;

namespace ErizosCF.Models
{
    public class UserStats
    {
        public UserProfile Profile { get; set; }
        public List<ProblemStat> ProblemHistory { get; set; }
        public List<ContestStat> ContestHistory { get; set; }

        public UserStats()
        {
            ProblemHistory = new List<ProblemStat>();
            ContestHistory = new List<ContestStat>();
        }
    }
}
