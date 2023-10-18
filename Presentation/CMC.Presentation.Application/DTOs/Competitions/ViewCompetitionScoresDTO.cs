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
        public string HostName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        public int CompettionQuestionTypeId { get; set; }
        public string CompettionQuestionType { get; set; }
        public int? QuestionForEachPlayer { get; set; }
        public bool IsFinalCompetition { get; set; }

        public int RoundCount { get; set; }
        public int? Round1Points { get; set; }
        public int? Round1Time { get; set; }
        public int? Round2Points { get; set; }
        public int? Round2Time { get; set; }
        public int? Round3Points { get; set; }
        public int? Round3Time { get; set; }
        public int? Round4Points { get; set; }
        public int? Round4Time { get; set; }


        public string WinningTeamName { get; set; }
        public string WinningPlayerName { get; set; }
        public int TotalWinningPlayerScore { get; set; }


        public string Team1Name { get; set; }
        public string Team2Name { get; set; }
        
        public string ParentCompetitionName { get; set; }

        public List<int> CategoriesIds { get; set; }
        public List<LookupModel> Categories { get; set; }
        public List<LookupModel> CompetitionQuestionTypes { get; set; } = new List<LookupModel>();
        public List<CompetitionsPlayerDTO> TeamCityMall { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<CompetitionsPlayerDTO> OtherTeam { get; set; } = new List<CompetitionsPlayerDTO>();

    }
}
