using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CompanyRepository.Model
{
    [Serializable()]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string CompanyName { get; set; }

        public string CompanyRegistrationNumber { get; set; }

        public string CompanyTaxNumber { get; set; }

        public string MainActivity { get; set; }

        public string Activity { get; set; }

        public string CompanyStatus { get; set; }

        public string TaxNumberStatus { get; set; }

        public string BannedContactStatus { get; set; }

        public string NumberOfEmployees { get; set; }

        public string DateOfFoundation { get; set; }

        public string LastNetIncome { get; set; }

        public string LastProfitBeforeTax { get; set; }

        public string CompanyPage { get; set; }

        public string CompanyEmail { get; set; }

        public string CompanyPhone { get; set; }

        public string CompanyLink { get; set; }
    }
}
