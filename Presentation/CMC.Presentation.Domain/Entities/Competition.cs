using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Domain.Entities
{
    public class Competition : FullAuditableEntity<int>
    {
        public string Name { get; set; }
        public virtual int Team1Id { get; set; }
        public virtual Team Team1 { get; set; } // CityMall

        public virtual int Team2Id { get; set; }
        public virtual Team Team2 { get; set; } // Other team

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? HostID { get; set; }
        public virtual User Host { get; set; }

        public virtual int? WinningPlayerId { get; set; }
        public virtual Player WinningPlayer { get; set; }

        public virtual int? WinningTeamId { get; set; }
        public virtual Team WinningTeam { get; set; }
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }

        public int RoundCount { get; set; }
        public int? Round1Points { get; set; }
        public int? Round1Time { get; set; }
        public int? Round2Points { get; set; }
        public int? Round2Time { get; set; }
        public int? Round3Points { get; set; }
        public int? Round3Time { get; set; }
        public int? Round4Points { get; set; }
        public int? Round4Time { get; set; }

        public string CategoriesIds { get; set; }
        public virtual int? ParentId { get; set; }
        public virtual Competition Parent { get; set; }

        public virtual ICollection<CompetitionQuestion> CompetitionQuestions { get; set; }
    }
}
