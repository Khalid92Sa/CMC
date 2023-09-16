using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class LatestCompeitionsScore
    {
        public string CompeititonName { get; set; }
        public DateTime? EndDate { get; set; }
        public string WinningTeamName { get; set; }
        public string WinningCityMallPlayerName { get; set; }
        public int CityMallPlayerPoints { get; set; }
        public string WinningOtherPlayerName { get; set; }
        public int OtherPlayerPoints { get; set;}
    }
}
