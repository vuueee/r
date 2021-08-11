using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Aur.Models;

namespace Aur.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

     

        public DbSet<Group> Groups { get; set; }
        public DbSet<Manga> Mangas { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Image> Images { get; set; }

    }
}
