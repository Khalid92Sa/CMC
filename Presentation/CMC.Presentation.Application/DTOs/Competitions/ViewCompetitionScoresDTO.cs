using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class ViewCompetitionScoresDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string WinningTeamName { get; set; }
        public string WinningPlayerName { get; set; }
        public int TotalWinningPlayerScore { get; set; }
        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        public List<CompetitionsPlayerDTO> TeamCityMall { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<CompetitionsPlayerDTO> OtherTeam { get; set; } = new List<CompetitionsPlayerDTO>();
    }
}
