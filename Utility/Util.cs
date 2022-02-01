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


        public static Dictionary<string, string> GetMainActivities()
        {
            return new Dictionary<string, string>() {
            {"01", "01 Növénytermesztés, állattenyésztés, vadgazdálkodás és kapcsolódó szolgáltatások"},
            {"02", "02 Erdőgazdálkodás"},
            {"03", "03 Halászat, halgazdálkodás"},
            {"05", "05 Szénbányászat"},
            {"06", "06 Kőolaj -, földgázkitermelés"},
            {"07", "07 Fémtartalmú érc bányászata"},
            {"08", "08 Egyéb bányászat"},
            {"09", "09 Bányászati szolgáltatás"},
            {"10", "10 Élelmiszergyártás"},
            {"11", "11 Italgyártás"},
            {"12", "12 Dohánytermék gyártása"},
            {"13", "13 Textília gyártása"},
            {"14", "14 Ruházati termék gyártása"},
            {"15", "15 Bőr, bőrtermék, lábbeli gyártása"},
            {"16", "16 Fafeldolgozás(kivéve: bútor), fonottáru gyártása"},
            {"17", "17 Papír, papírtermék gyártása"},
            {"18", "18 Nyomdai és egyéb sokszorosítási tevékenység"},
            {"19", "19 Kokszgyártás, kőolaj - feldolgozás"},
            {"20", "20 Vegyi anyag, termék gyártása"},
            {"21", "21 Gyógyszergyártás"},
            {"22", "22 Gumi -, műanyag termék gyártása"},
            {"23", "23 Nemfém ásványi termék gyártása"},
            {"24", "24 Fémalapanyag gyártása"},
            {"25", "25 Fémfeldolgozási termék gyártása"},
            {"26", "26 Számítógép, elektronikai, optikai termék gyártása"},
            {"27", "27 Villamos berendezés gyártása"},
            {"28", "28 Gép, gépi berendezés gyártása"},
            {"29", "29 Közúti jármű gyártása"},
            {"30", "30 Egyéb jármű gyártása"},
            {"31", "31 Bútorgyártás"},
            {"32", "32 Egyéb feldolgozóipari tevékenység"},
            {"33", "33 Ipari gép, berendezés, eszköz javítása"},
            {"35", "35 Villamosenergia -, gáz -, gőzellátás, légkondicionálás"},
            {"36", "36 Víztermelés, -kezelés, -ellátás"},
            {"37", "37 Szennyvíz gyűjtése, kezelése"},
            {"38", "38 Hulladékgazdálkodás"},
            {"39", "39 Szennyeződésmentesítés, egyéb hulladékkezelés"},
            {"41", "41 Épületek építése"},
            {"42", "42 Egyéb építmény építése"},
            {"43", "43 Speciális szaképítés"},
            {"45", "45 Gépjármű, motorkerékpár kereskedelme, javítása"},
            {"46", "46 Nagykereskedelem(kivéve: jármű, motorkerékpár)"},
            {"47", "47 Kiskereskedelem(kivéve: gépjármű, motorkerékpár)"},
            {"49", "49 Szárazföldi, csővezetékes szállítás"},
            {"50", "50 Vízi szállítás"},
            {"51", "51 Légi szállítás"},
            {"52", "52 Raktározás, szállítást kiegészítő tevékenység"},
            {"53", "53 Postai, futárpostai tevékenység"},
            {"55", "55 Szálláshely - szolgáltatás"},
            {"56", "56 Vendéglátás"},
            {"58", "58 Kiadói tevékenység"},
            {"59", "59 Film, video, televízióműsor gyártása, hangfelvétel-kiadás"},
            {"60", "60 Műsorösszeállítás, műsorszolgáltatás"},
            {"61", "61 Távközlés"},
            {"62", "62 Információ - technológiai szolgáltatás"},
            {"63", "63 Információs szolgáltatás"},
            {"64", "64 Pénzügyi közvetítés(kivéve: biztosítási, nyugdíjpénztári tevékenység)"},
            {"65", "65 Biztosítás, viszontbiztosítás, nyugdíjalapok(kivéve: kötelező társadalombiztosítás)"},
            {"66", "66 Egyéb pénzügyi tevékenység"},
            {"68", "68 INGATLANÜGYLETEK"},
            {"69", "69 Jogi, számviteli, adószakértői tevékenység"},
            {"70", "70 Üzletvezetési, vezetői tanácsadás"},
            {"71", "71 Építészmérnöki tevékenység"},
            {"72", "72 Tudományos kutatás, fejlesztés"},
            {"73", "73 Reklám, piackutatás"},
            {"74", "74 Egyéb szakmai, tudományos, műszaki tevékenység"},
            {"75", "75 Állat - egészségügyi ellátás"},
            {"77", "77 Kölcsönzés, operatív lízing"},
            {"78", "78 Munkaerőpiaci szolgáltatás"},
            {"79", "79 Utazásközvetítés, utazásszervezés, egyéb foglalás"},
            {"80", "80 Biztonsági, nyomozói tevékenység"},
            {"81", "81 Építményüzemeltetés, zöldterület - kezelés"},
            {"82", "82 Adminisztratív, kiegészítő egyéb üzleti szolgáltatás"},
            {"84", "84 Közigazgatás, védelem"},
            {"85", "85 Oktatás"},
            {"86", "86 Humán - egészségügyi ellátás"},
            {"87", "87 Bentlakásos, nem kórházi ápolás"},
            {"88", "88 Szociális ellátás bentlakás nélkül"},
            {"90", "90 Alkotó -, művészeti, szórakoztató tevékenység"},
            {"91", "91 Könyvtári, levéltári, múzeumi, egyéb kulturális tevékenység"},
            {"92", "92 Szerencsejáték, fogadás"},
            {"93", "93 Sport -, szórakoztató, szabadidős tevékenység"},
            {"94", "94 Érdekképviselet"},
            {"95", "95 Számítógép, személyi, háztartási cikk javítása"},
            {"96", "96 Egyéb személyi szolgáltatás"},
            {"97", "97 Háztartási alkalmazottat foglalkoztató magánháztartás"},
            {"98", "98 Háztartás termék-előállítása, szolgáltatása saját fogyasztásra"},
            {"99", "99 Területen kívüli szervezet"}
            };
        }

        class SetCsvHeaders : ClassMap<Company>
        {
            public SetCsvHeaders()
            {
                Map(m => m.Id).Index(0).Name("Azonosító");
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
                Map(m => m.CompanyEmail).Index(14).Name("Cég email");
                Map(m => m.CompanyPhone).Index(15).Name("Cég telefon");
                Map(m => m.CompanyLink).Index(16).Name("Cég link");
            }
        }
    }
}
