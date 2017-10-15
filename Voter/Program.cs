using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Voter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("How many votes?");

            uint votes = 0;
            while (votes <= 0)
            {
                string line = Console.ReadLine();
                if(!uint.TryParse(line, out votes) || votes <= 0)
                {
                    Console.WriteLine("Invalid input. Input must also be bigger than 0");
                }
            }

            uint totalDone = 0;

            try
            {
                for (; totalDone < votes; totalDone++)
                {
                    using (ChromeDriver driver = new ChromeDriver())
                    {
                        driver.Navigate().GoToUrl(@"https://temp-mail.org/");

                        IWebElement mailElement = driver.FindElement(By.XPath("//input[@data-original-title='Your temporary Email address']"));
                        string mailAddress = mailElement.GetAttribute("value");

                        string mailWindowHandle = driver.WindowHandles.Last();

                        GoToUrlInNewTab(driver, "NameTab", @"http://www.fakenamegenerator.com/gen-random-nl-nl.php");

                        IWebElement addressElement = driver.FindElement(By.ClassName("address"));
                        IWebElement nameElement = addressElement.FindElement(By.TagName("h3"));

                        string name = nameElement.Text;

                        IWebElement fullAddressElement = addressElement.FindElement(By.ClassName("adr"));
                        string state = fullAddressElement.Text.Split(new string[] { "  " }, StringSplitOptions.None)[1];

                        GoToUrlInNewTab(driver, "VoteTab", @"http://www.stoffenbeurs.nl/naaimachine-winnen-janome/?ref_id=t8447719");

                        driver.SwitchTo().Frame("uvembed31169");

                        IWebElement nameInputElement = driver.FindElement(By.Name("txt_name"));
                        nameInputElement.Clear();
                        nameInputElement.SendKeys(name);

                        IWebElement emailInputElement = driver.FindElement(By.Name("txt_email"));
                        emailInputElement.Clear();
                        emailInputElement.SendKeys(mailAddress);

                        IWebElement stateInputElement = driver.FindElement(By.Name("woonplaats"));
                        stateInputElement.Clear();
                        stateInputElement.SendKeys(state);

                        IWebElement submitElement = driver.FindElement(By.Id("lead_button"));

                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript("arguments[0].click()", submitElement);

                        driver.SwitchTo().Window(mailWindowHandle);

                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(600));
                        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(ElementNotVisibleException));

                        IWebElement viewMailElement = wait.Until(x => x.FindElement(By.XPath("//span[@class='glyphicon glyphicon-chevron-right']"))).FindElement(By.XPath(".."));

                        string hrefView = viewMailElement.GetAttribute("href");

                        driver.Navigate().GoToUrl(hrefView);

                        IWebElement confirmationElement = driver.FindElement(By.XPath("//a[text() = 'Bevestig mijn deelname']"));

                        string hrefConfirmation = confirmationElement.GetAttribute("href");

                        driver.Navigate().GoToUrl(hrefConfirmation);

                        CloseWindows(driver);
                    }
                }

                Console.WriteLine(String.Format("All {0} votes done!", votes));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(String.Format("Something went wrong, was able to do {0} votes", totalDone));
            }

            Console.ReadKey();
        }

        private static void CloseWindows(IWebDriver driver)
        {
            foreach(string windowHandle in driver.WindowHandles)
            {
                driver.SwitchTo().Window(windowHandle);
                driver.Close();
            }
        }

        private static void GoToUrlInNewTab(ChromeDriver driver, string tabName, string url)
        {
            driver.ExecuteScript(string.Format("window.open('_blank', '{0}');", tabName));
            driver.SwitchTo().Window(driver.WindowHandles.Last());

            driver.Navigate().GoToUrl(url);
        }
    }
}
