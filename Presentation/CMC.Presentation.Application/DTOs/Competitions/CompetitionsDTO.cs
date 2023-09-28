using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class CompetitionsDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public TeamDTO Team1 { get; set; } = new TeamDTO();
        public TeamDTO Team2 { get; set; } = new TeamDTO();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? HostID { get; set; }
        public PlayerDTO WinningPlayer { get; set; } = new PlayerDTO();
        public TeamDTO WinningTeam { get; set; } = new TeamDTO();
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

        public int? ParentId { get; set; }
        public List<int> CategoriesIds { get; set; }
        public List<LookupModel> Categories { get; set; } = new List<LookupModel>();
        public List<LookupModel> ParentCompetition { get; set; } = new List<LookupModel>();
        public List<LookupModel> CityMallTeam { get; set; } = new List<LookupModel>();
        public List<LookupModel> OtherTeam { get; set; } = new List<LookupModel>();
        public List<LookupModel> Hosts { get; set; } = new List<LookupModel>();
        public List<LatestCompeitionsScore> LatestScores { get; set; } = new List<LatestCompeitionsScore>();
    }
}
