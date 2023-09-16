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
        public TeamDTO Team1 { get; set; } = new TeamDTO();
        public TeamDTO Team2 { get; set; } = new TeamDTO();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? HostID { get; set; }
        public PlayerDTO WinningPlayer { get; set; } = new PlayerDTO();
        public TeamDTO WinningTeam { get; set; } = new TeamDTO();
        public int? Team1Score { get; set; }
        public int? Team2Score { get; set; }
        public int QuestionCount { get; set; }
        public List<LookupModel> CityMallTeam { get; set; } = new List<LookupModel>();
        public List<LookupModel> OtherTeam { get; set; } = new List<LookupModel>();
        public List<LookupModel> Hosts { get; set; } = new List<LookupModel>();
        public List<LatestCompeitionsScore> LatestScores { get; set; } = new List<LatestCompeitionsScore>();
    }
}
