using CMC.Kernel.Core.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Models
{
    public class PagedRequest
    {
        public int PageSize { get; set; } = PaginationSettings.PageSize;
        public int PageNumber { get; set; } = 1;
    }
}
