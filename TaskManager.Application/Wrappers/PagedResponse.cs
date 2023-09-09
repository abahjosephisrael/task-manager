using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TaskManager.Application.Wrappers
{
    public class PagedResponse<T> : Response<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public PagedResponse(T data, int pageNumber, int pageSize, int total)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Total = total;
            Data = data;
            Message = null;
            Succeeded = true;
            Errors = null;
        }
    }
}
