using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Competitions
{
    public class GetCategoryByPlayerDTO
    {
        public int playerId { get; set; }
        public bool IsCityMallTeam { get; set; }
    }
}
