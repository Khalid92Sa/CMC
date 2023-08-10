using Elastic.Apm.Api;
using CMC.Kernel.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Wrappers
{
    public class PagedResult<T> : Response
    {
        public PagedResult()
        {

        }
        public PagedResult(List<T> data)
        {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public List<string> Messages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PagedResult(bool succeeded, IEnumerable<T> data = default, List<string> messages = null, int count = 0, int page = 1, int pageSize = 50, HttpStatusCode StatusCode = HttpStatusCode.Ok)
        {
            Data = data;
            CurrentPage = page;
            Succeeded = succeeded;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Messages = messages;
            base.StatusCode = (int)StatusCode;
        }

        public static PagedResult<T> Success(IEnumerable<T> data, int count, int page, int pageSize)
        {
            return new PagedResult<T>(true, data, null, count, page, pageSize);
        }

        public static PagedResult<T> Failure(List<string> messages, HttpStatusCode StatusCode = HttpStatusCode.BadRequest)
        {
            return new PagedResult<T>(false, default, messages, StatusCode: StatusCode);
        }

        public dynamic List()
        {
            throw new NotImplementedException();
        }
    }
}
