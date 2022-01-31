using CsvHelper.Configuration;
using CsvHelper;
using System;
using CompanyRepository.Model;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Utility
{
    public class Util
    {
        public static void WriteToFile(List<Company> elements, string name)
        {
           
            bool done = false;
            while (!done)
            {
                try
                {
                    using var writer = new StreamWriter(name + ".csv", false, Encoding.UTF8);
                    using var csv = new CsvWriter(writer);
                    csv.Configuration.RegisterClassMap(new SetCsvHeaders());
                    csv.WriteRecords(elements);
                    done = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(2000);
                }
            }
        }

        class SetCsvHeaders : ClassMap<Company>
        {
            public SetCsvHeaders()
            {
                Map(m => m.Id).Index(0).Name("ID");
                Map(m => m.CompanyName).Index(1).Name("Cég név");
                Map(m => m.CompanyRegistrationNumber).Index(2).Name("Cégjegyzék szám");
                Map(m => m.CompanyTaxNumber).Index(3).Name("Adószám");
                Map(m => m.MainActivity).Index(4).Name("Főtevékenység");
                Map(m => m.Activity).Index(5).Name("Tevékenység");
                Map(m => m.CompanyStatus).Index(6).Name("Cég státusz");
                Map(m => m.TaxNumberStatus).Index(7).Name("Cég adószám státusz");
                Map(m => m.BannedContactStatus).Index(8).Name("Eltiltott személy státusz");
                Map(m => m.NumberOfEmployees).Index(9).Name("Alkalmazottak száma");
                Map(m => m.DateOfFoundation).Index(10).Name("Alapítás dátuma");
                Map(m => m.LastNetIncome).Index(11).Name("Utolsó nettó bevétel");
                Map(m => m.LastProfitBeforeTax).Index(12).Name("Utolsó adózás előtti profit");
                Map(m => m.CompanyPage).Index(13).Name("Cég oldal");
                Map(m => m.CompanyEmail).Index(14).Name("Cég elérhetőség");
                Map(m => m.CompanyLink).Index(15).Name("Cég link");
            }
        }
    }
}
