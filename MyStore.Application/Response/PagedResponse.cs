﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Response
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
