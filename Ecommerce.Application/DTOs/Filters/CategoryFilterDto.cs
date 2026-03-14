using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Filters
{
    public class CategoryFilterDto
    {
        public string? Keyword { get; set; }

        public string? Slug { get; set; }


        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }
}
