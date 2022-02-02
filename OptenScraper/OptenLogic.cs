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
        private Dictionary<string, string> MainActivities { get; set; }
        private string CompanyActivity { get; set; }
        private readonly string UserName = ConfigurationManager.AppSettings.Get("optenUserName");
        private readonly string Password = ConfigurationManager.AppSettings.Get("optenPassword");
        private readonly string RunType = ConfigurationManager.AppSettings.Get("runType");

        static void Main(string[] args)
        {
            OptenLogic optenLogic = new OptenLogic();            
            optenLogic.ScrapeOpten();
        }

        private void ScrapeOpten()
        {
            ChromeOptions options = new ChromeOptions();      
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("start-maximized");
            options.AddArgument("enable-automation");
            options.AddArgument("--user-agent=Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
            Driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromSeconds(120));
            DriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            JS = (IJavaScriptExecutor) Driver;
            CompaniesLink = new List<string>();
            Companies = new List<Company>();
            MainActivities = Utility.Util.GetMainActivities();

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

                    if (RunType == "simple")
                    {
                        GetDataFromListPage();
                    }

                    if (RunType == "detailed")
                    {
                        GetCompanyLinksFromActivity(companiesLink);

                        Logout();
                        RestartChromeDriver();

                        int counter = 0;
                        foreach (string companyLink in companiesLink)
                        {
                            OpenCompanyPageOnNewTab(companyLink);
                            counter++;

                            if (counter == 201)
                            {
                                counter = 0;
                                Logout();
                                RestartChromeDriver();
                            }
                        }
                    }

                    try
                    {
                        Utility.Util.WriteToFile(Companies, "opten");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    

                    Logout();
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


        private void Logout()
        {
            Driver.Url = "https://www.opten.hu/ousers/logout";
        }


        private void GoToActivities()
        {
            Driver.Url = "https://www.opten.hu/cegtar/kereso";

            Thread.Sleep(500);

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

            CompanyActivity = checkBox.GetAttribute("title").Trim();

            JS.ExecuteScript("arguments[0].click();", checkBox);
            JS.ExecuteScript("arguments[0].click();", searchButton);
        }


        private void GetDataFromListPage()
        {
            IWebElement companyPerPageSelector = Driver.FindElements(By.ClassName("selection"))[1];
            companyPerPageSelector.Click();

            Thread.Sleep(500);

            ReadOnlyCollection<IWebElement> companyPerPages = Driver.FindElements(By.ClassName("select2-results__option"));
            companyPerPages[3].Click();

            Thread.Sleep(500);

            while (true)
            {
                ReadOnlyCollection<IWebElement> companiesDivs = Driver.FindElements(By.ClassName("srcr01-panel_21"));

                foreach (IWebElement companyDiv in companiesDivs)
                {
                    Company company = new Company();

                    string companyName = companyDiv.FindElement(By.ClassName("srcr01-panel__title")).Text.Trim();
                    company.CompanyName = companyName;

                    string companyLink = companyDiv.FindElement(By.ClassName("cegnev")).GetAttribute("href");
                    company.CompanyLink = companyLink;

                    string taxNumber = companyDiv.FindElement(By.ClassName("hint-taxno")).GetAttribute("innerHTML").Trim().Replace("Adószám: ", "");
                    company.CompanyTaxNumber = taxNumber;
                    company.Id = "OPT" + taxNumber;

                    string contact = companyDiv.FindElement(By.ClassName("hint-contact")).GetAttribute("innerHTML").Trim().Replace("Elérhetőségek: ", "");
                    string[] contactList = contact.Split(new string[] { ", " }, StringSplitOptions.None);

                    foreach (string element in contactList)
                    {
                        if (element.Contains("tel:"))
                        {
                            company.CompanyPhone = element.Trim().Replace("tel: ", "");
                            continue;
                        }

                        if (element.Contains("e-mail:"))
                        {
                            company.CompanyEmail = element.Trim().Replace("e-mail: ", "");
                            continue;
                        }

                        if (!element.Contains("tel:") && !element.Contains("e-mail:"))
                        {
                            company.CompanyPage = element.Trim();
                        }
                    }

                    ReadOnlyCollection<IWebElement> liElements = companyDiv.FindElements(By.ClassName("srcr01-panel__item"));
                    string companyStatus = liElements[0].FindElement(By.ClassName("company-status")).GetAttribute("innerHTML").Trim();
                    company.CompanyStatus = companyStatus.Split(new string[] { "<b>" }, StringSplitOptions.None)[1].Replace("</b>", "");

                    company.Activity = CompanyActivity;

                    string mainActivityString = CompanyActivity.Split(new string[] { " " }, StringSplitOptions.None)[0].Substring(0, 2);
                    company.MainActivity = MainActivities[mainActivityString];

                    List<string> companyIds = Companies.Select(x => x.Id).ToList();
                    bool notScraped = !companyIds.Contains(company.Id);

                    if (notScraped)
                    {
                        Companies.Add(company);
                    }

                    Console.WriteLine(company.CompanyName);
                    Console.WriteLine(company.CompanyLink);
                    Console.WriteLine(company.CompanyTaxNumber);
                    Console.WriteLine(company.CompanyStatus);
                    Console.WriteLine(company.CompanyEmail);
                    Console.WriteLine(Environment.NewLine);                    
                }

                IWebElement nextButton = Driver.FindElement(By.ClassName("pagination__button--next"));

                if (nextButton.GetAttribute("href") != null)
                {
                    //JS.ExecuteScript("arguments[0].click();", nextButton);

                    string nextPage = nextButton.GetAttribute("href");
                    var tabs = Driver.WindowHandles;
                    
                    if (tabs.Count > 1)
                    {
                        Driver.Close();
                        Driver.SwitchTo().Window(tabs[0]);
                        Thread.Sleep(500);
                    }

                    JS.ExecuteScript("window.open();");
                    Driver.SwitchTo().Window(Driver.WindowHandles.Last());
                    Driver.Navigate().GoToUrl(nextPage);
                }
                else
                {
                    break;
                }
            }
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
                IWebElement companyElectronicContact = Driver.FindElement(By.ClassName("rovat-90")).FindElement(By.ClassName("panel__body")).FindElement(By.TagName("ul"));
                ReadOnlyCollection<IWebElement> companyElectronicContactLi = companyElectronicContact.FindElements(By.TagName("li"));
                IWebElement validCompanyElectronicContact = companyElectronicContactLi[^1].FindElement(By.TagName("p"));
                company.CompanyEmail = validCompanyElectronicContact.GetAttribute("innerHTML").Trim() + Environment.NewLine;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Company has no electronic contact!" + ex.ToString());
            }

            try
            {
                IWebElement companyOfficialElectronicContact = Driver.FindElement(By.ClassName("rovat-165")).FindElement(By.ClassName("panel__body")).FindElement(By.TagName("ul"));
                ReadOnlyCollection<IWebElement> companyOfficialElectronicContactLi = companyOfficialElectronicContact.FindElements(By.TagName("li"));
                IWebElement validOfficialCompanyElectronicContact = companyOfficialElectronicContactLi[^1].FindElement(By.TagName("p"));
                company.CompanyEmail += validOfficialCompanyElectronicContact.GetAttribute("innerHTML").Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Company has no valid electronic contact!" + ex.ToString());
            }

            List<string> companyIds = Companies.Select(x => x.Id).ToList();
            bool notScraped = !companyIds.Contains(company.Id);

            if (notScraped)
            {
                Companies.Add(company);
            }

            Console.WriteLine(company.CompanyName);
            Console.WriteLine(Environment.NewLine);
        }


        private void RestartChromeDriver()
        {
            Driver.Quit();
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
