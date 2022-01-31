using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using CompanyRepository.Model;

namespace OptenScraper
{
    class OptenLogic
    {
        private IWebDriver Driver { get; set; }
        private WebDriverWait DriverWait { get; set; }
        private IJavaScriptExecutor JS { get; set; }
        private List<string> CompaniesLink { get; set; }
        private List<Company> Companies { get; set; }
        private readonly string UserName = ConfigurationManager.AppSettings.Get("optenUserName");
        private readonly string Password = ConfigurationManager.AppSettings.Get("optenPassword");

        static void Main(string[] args)
        {
            OptenLogic optenLogic = new OptenLogic();            
            optenLogic.ScrapeOpten();
        }

        private void ScrapeOpten()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--auto-open-devtools-for-tabs");
            options.AddArguments("--start-maximized");
            options.AddArgument("--user-agent=Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
            options.AddArgument("no-sandbox");
            Driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(120));
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            JS = (IJavaScriptExecutor) Driver;
            CompaniesLink = new List<string>();
            Companies = new List<Company>();

            try
            {
                Login();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                GoToActivities();
                ReadOnlyCollection<IWebElement> activities = GetActivities();
                int numberOfActivities = activities.Count;
                for (int i = 0; i < numberOfActivities; i++)
                {
                    List<string> companiesLink = new List<string>();
                    SearchActivity(i);
                    GetCompanyLinksFromActivity(companiesLink);

                    foreach (string companyLink in companiesLink)
                    {
                        OpenCompanyPageOnNewTab(companyLink);
                    }

                    Utility.Util.WriteToFile(Companies, "opten");

                    RestartChromeDriver();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }



        }

        private void Login()
        {
            Driver.Url = "https://www.opten.hu/";
            Thread.Sleep(1000);

            IWebElement loginForm = Driver.FindElement(By.ClassName("head02-login__form"));

            IWebElement loginButton1 = Driver.FindElement(By.ClassName("head02-login__button"));

            JS.ExecuteScript("arguments[0].click();", loginButton1);

            IWebElement userName = Driver.FindElements(By.ClassName("head02-login__input"))[0];
            userName.SendKeys(UserName);

            IWebElement password = Driver.FindElements(By.ClassName("head02-login__input"))[1];
            password.SendKeys(Password);

            IWebElement loginButton2 = loginForm.FindElement(By.TagName("button"));

            JS.ExecuteScript("arguments[0].click();", loginButton2);

            Thread.Sleep(1000);
        }


        private void GoToActivities()
        {
            Driver.Url = "https://www.opten.hu/cegtar/kereso";

            Thread.Sleep(200);

            IWebElement resetSearchButton = Driver.FindElement(By.Id("reset_button"));
            JS.ExecuteScript("arguments[0].click();", resetSearchButton);

            Thread.Sleep(1000);

            IWebElement plusSign = Driver.FindElements(By.ClassName("srch02-section__icon"))[0];
            JS.ExecuteScript("arguments[0].click();", plusSign);

            IWebElement activityDiv = Driver.FindElement(By.Id("tevekenyseg_search_panel"));
            JS.ExecuteScript("arguments[0].click();", activityDiv);

            Thread.Sleep(1000);
        }


        private ReadOnlyCollection<IWebElement> GetActivities()
        {
            IWebElement industryDiv = DriverWait.Until<IWebElement>((d) =>
            {
                return Driver.FindElement(By.Id("cteaor08_scroll_body"));
            });

            return industryDiv.FindElements(By.TagName("div"));
        }


        private void SearchActivity(int numberOfActivity)
        {
            GoToActivities();

            IWebElement searchButton = Driver.FindElement(By.Id("send_button"));

            ReadOnlyCollection<IWebElement> activities = GetActivities();

            IWebElement activity = activities[numberOfActivity];

            IWebElement checkBox = activity.FindElement(By.TagName("input"));

            JS.ExecuteScript("arguments[0].click();", checkBox);
            JS.ExecuteScript("arguments[0].click();", searchButton);
        }


        private void GetCompanyLinksFromActivity(List<string> companiesLink)
        {
            IWebElement companyPerPageSelector = Driver.FindElements(By.ClassName("selection"))[1];
            companyPerPageSelector.Click();

            Thread.Sleep(500);

            ReadOnlyCollection<IWebElement> companyPerPages = Driver.FindElements(By.ClassName("select2-results__option"));
            companyPerPages[3].Click();


            while (true)
            {
                List<string> newComapnyLinks = Driver.FindElements(By.ClassName("cegnev")).ToList().Select(x => x.GetAttribute("href")).ToList();
                foreach (string companyLink in newComapnyLinks)
                {
                    if (!CompaniesLink.Contains(companyLink))
                    {
                        companiesLink.Add(companyLink);
                        CompaniesLink.Add(companyLink);
                    }
                }

                IWebElement nextButton = Driver.FindElement(By.ClassName("pagination__button--next"));

                if (nextButton.GetAttribute("href") != null)
                {
                    JS.ExecuteScript("arguments[0].click();", nextButton);
                }
                else
                {
                    break;
                }
            }
        }


        public void OpenCompanyPageOnNewTab(string url)
        {
            try
            {
                var tabs = Driver.WindowHandles;
                JS.ExecuteScript("window.open();");
                Driver.SwitchTo().Window(Driver.WindowHandles.Last());
                Driver.Navigate().GoToUrl(url);
                Thread.Sleep(100);
                GetDataFromCompanyPage();

                Driver.Close();
                Driver.SwitchTo().Window(tabs[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occoured for opening new tab" + Environment.NewLine + ex.ToString());
            }
        }


        private void GetDataFromCompanyPage()
        {
            Company company = new Company();

            ReadOnlyCollection<IWebElement> registrationAndTaxNumber = Driver.FindElements(By.ClassName("pgc01-content__list-item"));
            company.CompanyRegistrationNumber = registrationAndTaxNumber[0].Text;
            company.CompanyTaxNumber = registrationAndTaxNumber[1].Text;

            ReadOnlyCollection<IWebElement> panels = Driver.FindElements(By.ClassName("panel__body"));

            IWebElement baseData = panels[0];
            ReadOnlyCollection<IWebElement> baseDataDivs = baseData.FindElements(By.ClassName("ms-xs-15"));
            company.CompanyName = baseDataDivs[0].FindElements(By.TagName("p"))[1].Text;

            ReadOnlyCollection<IWebElement> companyInfos = baseDataDivs[2].FindElements(By.TagName("p"));
            company.CompanyStatus = companyInfos[0].Text;
            company.TaxNumberStatus = companyInfos[1].Text;
            company.BannedContactStatus = companyInfos[2].Text;
            company.NumberOfEmployees = companyInfos[^1].Text;
            company.DateOfFoundation = companyInfos[^3].Text;
            company.MainActivity = baseDataDivs[3].FindElements(By.TagName("p"))[1].Text;

            IWebElement financialData = panels[6];
            company.LastNetIncome = financialData.FindElements(By.TagName("p"))[1].Text.Split(new string[] { "eFt" }, StringSplitOptions.None)[0].Replace(" ", "") + "000"; ;
            company.LastProfitBeforeTax = financialData.FindElements(By.TagName("p"))[3].Text.Split(new string[] { "eFt" }, StringSplitOptions.None)[0].Replace(" ", "") + "000";

            string companyCertLink = Driver.FindElement(By.Id("acegbiz")).GetAttribute("href");

            Driver.Url = companyCertLink;

            try
            {
                IWebElement rovat90 = Driver.FindElement(By.ClassName("rovat-90"));
                IWebElement companyElectronicContact = rovat90.FindElement(By.ClassName("panel__body")).FindElement(By.TagName("ul"));
                ReadOnlyCollection<IWebElement> companyElectronicContactLi = companyElectronicContact.FindElements(By.TagName("li"));
                IWebElement validCompanyElectronicContact = companyElectronicContactLi[^1].FindElement(By.TagName("p"));
                company.CompanyEmail = validCompanyElectronicContact.GetAttribute("innerHTML").Trim() + Environment.NewLine;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't find rovat-90" + ex.ToString());
            }

            try
            {
                IWebElement rovat165 = Driver.FindElement(By.ClassName("rovat-165"));
                IWebElement companyOfficialElectronicContact = Driver.FindElement(By.ClassName("rovat-165")).FindElement(By.ClassName("panel__body")).FindElement(By.TagName("ul"));
                ReadOnlyCollection<IWebElement> companyOfficialElectronicContactLi = companyOfficialElectronicContact.FindElements(By.TagName("li"));
                IWebElement validOfficialCompanyElectronicContact = companyOfficialElectronicContactLi[^1].FindElement(By.TagName("p"));
                company.CompanyEmail += validOfficialCompanyElectronicContact.GetAttribute("innerHTML").Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't find rovat-165" + ex.ToString());
            }

      

            Console.WriteLine(company.CompanyName);
            Console.WriteLine(company.CompanyRegistrationNumber);
            Console.WriteLine(company.CompanyTaxNumber);
            Console.WriteLine(company.CompanyStatus);
            Console.WriteLine(company.TaxNumberStatus);
            Console.WriteLine(company.BannedContactStatus);
            Console.WriteLine(company.NumberOfEmployees);
            Console.WriteLine(company.DateOfFoundation);
            Console.WriteLine(company.MainActivity);
            Console.WriteLine(company.LastNetIncome);
            Console.WriteLine(company.LastProfitBeforeTax);
            Console.WriteLine(company.CompanyEmail);


            Companies.Add(company);
        }


        private void RestartChromeDriver()
        {
            Driver.Close();
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--auto-open-devtools-for-tabs");
            options.AddArguments("--start-maximized");
            options.AddArgument("--user-agent=Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
            options.AddArgument("no-sandbox");
            Driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(120));
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            JS = (IJavaScriptExecutor)Driver;
            Login();
        }

    }
}
