﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? Fullname { get; set; }
        public ICollection<Address> Addresses { get; } = new HashSet<Address>();
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
