using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;
using System.Data;
using ClosedXML.Excel;

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
            //options.AddArgument("--headless=new");
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

                //try
                //{
                //    IWebElement cookieConsent = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("uc-main-dialog")));
                //    IWebElement acceptButton = cookieConsent.FindElement(By.XPath("//button[contains(text(), 'Accept All')]"));
                //    acceptButton.Click();
                //    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("uc-main-dialog")));
                //}
                //catch (WebDriverTimeoutException)
                //{
                //    // If the overlay is not found, continue with the script
                //}
                //IWebElement cookie = driver.FindElement(By.XPath("//*[@id=\"deny\"]"));
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
                Task.Delay(10000).Wait();

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
                        //Task.Delay(3000).Wait();
                        IWebElement selectCheckbox = driver.FindElement(By.Id("selectUnselectAll"));
                        selectCheckbox.Click();
                        //Task.Delay(3000).Wait();

                        IWebElement dropdown = driver.FindElement(By.XPath("//*[@id=\"exportArticles\"]/div/div[1]/div[2]/div/div[2]/div[2]/div/div"));
                        dropdown.Click(); // Open the dropdown
                        //Task.Delay(3000).Wait();

                        // Wait for the options to be visible and locate the option
                        IWebElement option = wait.Until(d => d.FindElement(By.XPath("//*[@id=\"exportArticles\"]/div/div[1]/div[2]/div/div[2]/div[2]/div/div/div/ul/li[6]")));
                        option.Click();
                        //Task.Delay(3000).Wait();

                        IWebElement exportButton = driver.FindElement(By.Id("articleBrowserExport_top"));
                        exportButton.Click();
                        //Task.Delay(3000).Wait();

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
                        Task.Delay(3000).Wait();
                        WriteToExcel();
                    }
                    catch (Exception ex)
                    {
                        throw;
                        //i = i - 1;
                    }
                }



                //IWebElement searchButton = driver.FindElement(By.ClassName("quick-search__button"));
                //searchButton.Click();



                // Pause the program for a few seconds to see the result
                System.Threading.Thread.Sleep(5000); // 5 seconds
            }
            catch (Exception ex)
            {
                throw;
                //Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Close the browser after the automation
                driver.Quit();
            }
        }

        private void WriteToExcel()
        {
            string downloadDirectory = @"C:\CustomDownloads\mdpi";
            string excelFilePath = Path.Combine(downloadDirectory, "output.xlsx");

            // Get all .txt files from the download directory
            string[] txtFiles = Directory.GetFiles(downloadDirectory, "*.txt");

            DataTable datatable = new DataTable();
            char[] delimiter = new char[] { '\t' };

            foreach (string file in txtFiles)
            {
                using (StreamReader streamreader = new StreamReader(file))
                {
                    // Read the column headers from the first file
                    if (datatable.Columns.Count == 0)
                    {
                        string[] columnheaders = streamreader.ReadLine().Split(delimiter);
                        foreach (string columnheader in columnheaders)
                        {
                            datatable.Columns.Add(columnheader);
                        }
                    }

                    // Read the data rows
                    while (streamreader.Peek() > 0)
                    {
                        DataRow datarow = datatable.NewRow();
                        datarow.ItemArray = streamreader.ReadLine().Split(delimiter);
                        datatable.Rows.Add(datarow);
                    }
                }
            }

            // Append data to Excel file
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;
                if (workbook.Worksheets.Count == 0)
                {
                    worksheet = workbook.Worksheets.Add("Sheet1");
                }
                else
                {
                    worksheet = workbook.Worksheet(1);
                }

                int startRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 1;

                // Write the headers if the worksheet is empty
                if (startRow == 1)
                {
                    for (int i = 0; i < datatable.Columns.Count; i++)
                    {
                        worksheet.Cell(startRow, i + 1).Value = datatable.Columns[i].ColumnName;
                    }
                    startRow++;
                }

                // Write the data
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    for (int j = 0; j < datatable.Columns.Count; j++)
                    {
                        worksheet.Cell(startRow + i, j + 1).Value = datatable.Rows[i][j].ToString();
                    }
                }

                workbook.SaveAs(excelFilePath);
            }
        }

    }
}
