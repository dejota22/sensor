﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class CompanyTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
