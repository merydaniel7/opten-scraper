using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Configuration;
using System.Collections.Specialized;

namespace OptenScraper
{
    class OptenLogic
    {
        private IWebDriver Driver { get; set; }
        private WebDriverWait DriverWait { get; set; }
        private IJavaScriptExecutor JS { get; set; }
        private string UserName = ConfigurationManager.AppSettings.Get("optenUserName");
        private string Password = ConfigurationManager.AppSettings.Get("optenPassword");

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
                ReadOnlyCollection<IWebElement> activities = GetActivities();
                int numberOfActivities = activities.Count;
                for (int i = 0; i < numberOfActivities; i++)
                {
                    SearchActivity(i);
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

            Driver.Url = "https://www.opten.hu/cegtar/kereso";
        }


        private ReadOnlyCollection<IWebElement> GetActivities()
        {
            IWebElement plusSign = Driver.FindElements(By.ClassName("srch02-section__icon"))[0];
            JS.ExecuteScript("arguments[0].click();", plusSign);

            IWebElement industryDiv = Driver.FindElement(By.Id("cteaor08_scroll_body"));

            return industryDiv.FindElements(By.TagName("div"));
        }

        private void SearchActivity(int numberOfActivity)
        {
            IWebElement searchButton = Driver.FindElement(By.Id("send_button"));

            ReadOnlyCollection<IWebElement> activities = GetActivities();

            IWebElement activity = activities[numberOfActivity];

            IWebElement checkBox = activity.FindElement(By.TagName("input"));

            JS.ExecuteScript("arguments[0].click();", checkBox);
            JS.ExecuteScript("arguments[0].click();", searchButton);

        }

    }
}
