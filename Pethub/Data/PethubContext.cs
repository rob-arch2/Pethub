using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pethub.Models;

namespace Pethub.Data
{
    public class PethubContext : DbContext
    {
        public PethubContext (DbContextOptions<PethubContext> options)
            : base(options)
        {
        }

        public DbSet<Pethub.Models.Account> Account { get; set; } = default!;
    }
}
