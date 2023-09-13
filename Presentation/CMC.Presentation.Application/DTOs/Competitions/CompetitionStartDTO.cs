using CMC.Kernel.Infrastructure.Caching.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class CompetitionStartDTO
    {
        public int Id { get; set; }
        public List<CompetitionsPlayerDTO> TeamCityMall { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<CompetitionsPlayerDTO> OtherTeam { get; set; } = new List<CompetitionsPlayerDTO>();
        public List<LookupModel> Categories { get; set; } = new List<LookupModel>();
    }

    public class CompetitionsPlayerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public bool IsVSPlayer { get; set; }
        public List<CompetitonQuestions> competitonQuestions { get; set; } = new List<CompetitonQuestions>();
    }

    public class CompetitonQuestions
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public int Timer { get; set; }
        public int Points { get; set; }
    }
}
