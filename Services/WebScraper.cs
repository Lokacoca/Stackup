using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Stackup.Services
{
     class WebScraper
    {
        public static IWebDriver driver;

        public string[] Scrape(string data)
        {
            var results = new string[3];
            driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://tracker.gg/valorant");

            var Input = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[3]/div/main/div[3]/div[1]/div/div/div[1]/div[2]/div[1]/form/input"));



            Input.Click();
            Input.SendKeys(data);
            Input.SendKeys(Keys.Enter);
                Thread.Sleep(6000);

            var accept = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div[2]/div/button[2]"));
            accept.Click();
            var KD = FindElements(By.XPath("/html/body/div[1]/div/div[2]/div[3]/div/main/div[3]/div/div[2]/div[3]/div/div[2]/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/span[2]/span"));
           if(KD != null)
            {
                Console.WriteLine(KD);

            }
            else
            {

                Console.WriteLine("NO KD FOUND");
            }
            Console.WriteLine("DONE");
            results[0] = Convert.ToString(KD);

            driver.Quit();
            return results;
        }

        static IReadOnlyCollection<IWebElement> FindElements(By by)
        {

            Stopwatch w = Stopwatch.StartNew();

            while(w.ElapsedMilliseconds < 20000)
            {

                var element = driver.FindElements(by);

                if(element.Count > 0)
                {
                    return element;
                }
                Thread.Sleep(25);

            }
            return null;
        }
    }
}
