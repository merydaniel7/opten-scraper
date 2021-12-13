using CompanyRepository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace CompanyRepository.DbService
{
    class OptenCompanyRepository : DbContext
    {
        // server connection string:  Data Source=VMI602353\\SQLEXPRESS;Database=Product;Integrated Security=true;ConnectRetryCount=0
        // this pc connection string: Data Source=LENOVO-M82-PC3\\SQLEXPRESS;Database=Product;Integrated Security=true;ConnectRetryCount=0
        public static string _connectionString = "Data Source=VMI602353\\SQLEXPRESS;Database=Product;Integrated Security=true;ConnectRetryCount=0";

        public OptenCompanyRepository()
        {
        }

        public OptenCompanyRepository(DbContextOptions<OptenCompanyRepository> options)
        {
            Database.Migrate();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>().ToTable("OptenCompanies");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public virtual DbSet<Company> Company { get; set; }
    }
}
