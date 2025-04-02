using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;

namespace WebScrapeApp.ArticleServices
{
    public class Mdpi
    {
        public void RunSelenium(string SearchText)
        {
            //string downloadDirectory = @"C:\CustomDownloads";
            string downloadDirectory = @"C:\CustomDownloads\mdpi";
            // Ensure the directory exists
            //Directory.CreateDirectory(downloadDirectory);
            if (!Directory.Exists(downloadDirectory))
                Directory.CreateDirectory(downloadDirectory);
            else
            {
                //remove all text the files in the directory
                //DirectoryInfo di = new DirectoryInfo(downloadDirectory);
                //foreach (FileInfo file in di.GetFiles())
                //{
                //    if (file.Extension == ".txt")
                //        file.Delete();
                //}
            }

            // Set Edge options for downloading files
            EdgeOptions options = new EdgeOptions();
            options.AddUserProfilePreference("download.default_directory", downloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            // Set up ChromeDriver (you can use other browsers if you prefer)
            IWebDriver driver = new EdgeDriver(options);

            try
            {
                // Navigate to the SAGE Journals website
                driver.Navigate().GoToUrl("https://www.mdpi.com/");

                // Maximize the browser window
                driver.Manage().Window.Maximize();

                // Wait for the page to load (optional)
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.Id("q")));

                //IWebElement cookie = driver.FindElement(By.Id("accept"));
                //cookie.Click();
                //IWebElement acceptCookiesButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("accept")));
                //acceptCookiesButton.Click();

                //IWebElement cookieConsent = driver.FindElement(By.Id("uc-main-dialog"));
                //if (cookieConsent.Displayed)
                //{
                //    IWebElement acceptButton = cookieConsent.FindElement(By.Id("accept"));
                //    acceptButton.Click();
                //    //wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("accept")));
                //}

                IWebElement search = driver.FindElement(By.Id("q"));
                search.SendKeys(SearchText);
                search.SendKeys(Keys.Enter);
                //Task.Delay(10000).Wait();

                IWebElement pageInfoDiv = driver.FindElement(By.XPath("//div[contains(text(), 'Displaying article') and contains(text(), 'on page')]"));

                // Extract and print the text content of the div
                string displayedText = pageInfoDiv.Text;
                Console.WriteLine("Extracted text: " + displayedText);

                // Regular expression to extract the total number of pages
                Regex regex = new Regex(@"page (\d+) of (\d+)");
                Match match = regex.Match(displayedText);
                var totalPages = 0;
                if (match.Success)
                {
                    // Extract the total number of pages (the second captured group)
                    string totalPages1 = match.Groups[2].Value;
                    totalPages = int.Parse(totalPages1);

                    // Output the total number of pages
                    Console.WriteLine("Total pages: " + totalPages1);
                }

                for (int i = 1; i < totalPages; i++)
                //foreach (var i in Enumerable.Range(1, totalPages))
                {
                    try
                    {
                        IWebElement export = driver.FindElement(By.ClassName("export-options-show"));
                        export.Click();
                        Task.Delay(3000).Wait();
                        IWebElement selectCheckbox = driver.FindElement(By.Id("selectUnselectAll"));
                        selectCheckbox.Click();
                        Task.Delay(3000).Wait();

                        IWebElement dropdown = driver.FindElement(By.XPath("//*[@id=\"exportArticles\"]/div/div[1]/div[2]/div/div[2]/div[2]/div/div"));
                        dropdown.Click(); // Open the dropdown
                        Task.Delay(3000).Wait();

                        // Wait for the options to be visible and locate the option
                        IWebElement option = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"exportArticles\"]/div/div[1]/div[2]/div/div[2]/div[2]/div/div/div/ul/li[6]")));
                        option.Click();
                        Task.Delay(3000).Wait();

                        IWebElement exportButton = driver.FindElement(By.Id("articleBrowserExport_top"));
                        exportButton.Click();
                        Task.Delay(3000).Wait();

                        //// Extract the text from the element
                        //displayedText = articleTextDiv.Text;
                        //// Output the text
                        //Console.WriteLine(displayedText);
                        // Click the next button
                        IWebElement pagenumber = driver.FindElement(By.Id("pager-page-number"));
                        pagenumber.Clear();
                        pagenumber.SendKeys((i + 1).ToString());
                        pagenumber.SendKeys(Keys.Enter);

                        // Wait for the next page to load
                        Task.Delay(5000).Wait();
                    }
                    catch (Exception ex)
                    {
                        break;
                        //i = i - 1;
                    }
                }



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
