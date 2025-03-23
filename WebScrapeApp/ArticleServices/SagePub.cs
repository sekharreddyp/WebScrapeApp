using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using OpenQA.Selenium.Edge;

namespace WebScrapeApp.ArticleServices
{
    public class SagePub
    {
        public void RunSelenium(string SearchText)
        {
            // Set up ChromeDriver (you can use other browsers if you prefer)
            IWebDriver driver = new EdgeDriver();

            try
            {
                // Navigate to the SAGE Journals website
                driver.Navigate().GoToUrl("https://journals.sagepub.com/");

                // Maximize the browser window
                driver.Manage().Window.Maximize();

                // Wait for the page to load (optional)
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.ClassName("website-sage")));

                IWebElement cookies = driver.FindElement(By.Id("onetrust-accept-btn-handler"));
                cookies.Click();

                IWebElement search = driver.FindElement(By.ClassName("quick-search__input"));
                search.SendKeys(SearchText);
                search.SendKeys(Keys.Enter);
                Task.Delay(20000).Wait();

                //IWebElement searchButton = driver.FindElement(By.ClassName("quick-search__button"));
                //searchButton.Click();


                // Wait for the search results page
                wait.Until(d => d.FindElement(By.ClassName("search-page")));

                // Example: Extracting titles of journals from the search result page
                IReadOnlyCollection<IWebElement> journalTitles = driver.FindElements(By.CssSelector("div.result-list-item h2 a"));
                foreach (var title in journalTitles)
                {
                    Console.WriteLine(title.Text);
                }

                // Example: Click on the first result
                IWebElement firstJournal = driver.FindElement(By.CssSelector("div.result-list-item h2 a"));
                firstJournal.Click();

                // Wait for the article page to load
                wait.Until(d => d.FindElement(By.CssSelector("h1")));

                // Take a screenshot of the article page (optional)
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile("screenshot.png");

                // Pause the program for a few seconds to see the result
                System.Threading.Thread.Sleep(5000); // 5 seconds
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Close the browser after the automation
                driver.Quit();
            }
        }
    }
}
