using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Entities.Models;

namespace backend.Entities
{
    public class backendContext : DbContext
    {
        public backendContext(DbContextOptions options): base(options)
        {

        }
        public DbSet<User> Users { get; set; }

    }
}
