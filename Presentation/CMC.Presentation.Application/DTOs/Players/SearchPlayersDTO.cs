using CMC.Kernel.Core.Models;
using CMC.Presentation.Application.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.DTOs.Players
{
    public class SearchPlayersDTO : PagedRequest
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public PlayerSearchTypes PlayerType { get; set; }
    }
}
