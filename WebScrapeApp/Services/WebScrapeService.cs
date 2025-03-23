using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace WebScrapeApp.Services;


public class ScrapeResult
{
    public string Url { get; set; }
    public string Content { get; set; }

    public List<string> Emails { get; set; }
    public List<string> Links { get; set; }

}
public class WebScrapeService
{
    //static content scraping
    public async Task<ScrapeResult> ScrapUrl(string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var htmldoc = new HtmlDocument
        {
            OptionAutoCloseOnEnd = true,
            OptionFixNestedTags = true,
            //OptionOutputAsXml = true,
            //OptionWriteEmptyNodes = true
        };
        htmldoc.LoadHtml(content);


        return new ScrapeResult
        {
            Url = url,
            Content = htmldoc.DocumentNode.InnerHtml.Trim().Replace(" +", "").Replace("\n", "")
        };
    }

    //dynamic content scraping
    public Task<ScrapeResult> ScrapUrlDynamic(string url, bool openBrowser = false)
    {
        //ScrapeWiley();
        var edgeOptions = new EdgeOptions();
        if (!openBrowser)
        {
            edgeOptions.AddArgument("--headless=new");
        }

        edgeOptions.AddArgument("--no-sandbox");
        edgeOptions.AddArgument("--disable-dev-shm-usage");
        edgeOptions.AddArgument("--disable-gpu");
        edgeOptions.AddArgument("--disable-extensions");
        edgeOptions.AddArgument("--disable-popup-blocking");
        edgeOptions.AddArgument("--disable-blink-features=AutomationControlled");
        edgeOptions.AddArgument("--disable-plugins");
        edgeOptions.AddArgument("--disable-infobars");
        edgeOptions.AddArgument("--disable-notifications");
        edgeOptions.AddArgument("--disable-software-rasterizer");
        edgeOptions.AddArgument("--disable-features=VizDisplayCompositor");
        edgeOptions.AddArgument("--disable-features=IsolateOrigins,site-per-process");
        edgeOptions.AddArgument("--disable-features=NetworkService,NetworkServiceInProcess");
        edgeOptions.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
        edgeOptions.AddArgument("--disable-features=ImprovedCookieControls,SameSiteByDefaultCookies");
        edgeOptions.AddArgument("--disable-features=SharedArrayBuffer");

        using (var driver = new EdgeDriver(edgeOptions))
        {
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(webdriver => webdriver.FindElement(By.TagName("body")));
            var content = driver.PageSource;

            //driver.Quit();
            var htmldoc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
            };
            htmldoc.LoadHtml(content);

            var cont = htmldoc.DocumentNode.InnerHtml.Trim().Replace(" +", "").Replace("\n", "");
            var emailList = ExtractEmailsFromHtml(cont);
            var linkList = ExtractLinksFromHtml(htmldoc);

            var scrapeResult = new ScrapeResult
            {
                Url = url,
                Content = cont,
                Emails = emailList,
                Links = linkList
            };

            return Task.FromResult(scrapeResult);
        }
    }


    private List<string> ExtractEmailsFromHtml(string htmlContent)
    {
        List<string> emailList = new List<string>();

        // Regular expression for matching emails
        string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";

        // Create a Regex object
        Regex emailRegex = new Regex(emailPattern);

        // Find matches within the HTML content
        MatchCollection matches = emailRegex.Matches(htmlContent);

        foreach (Match match in matches)
        {
            // Add matched emails to the list if they aren't duplicates
            if (!emailList.Contains(match.Value))
            {
                emailList.Add(match.Value);
            }
        }

        return emailList;
    }

    private List<string> ExtractLinksFromHtml(HtmlDocument htmldoc)
    {
        List<string> linkList = new List<string>();

        // Select all anchor tags
        var anchorNodes = htmldoc.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes != null)
        {
            foreach (var node in anchorNodes)
            {
                var hrefValue = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(hrefValue) && IsValidUrl(hrefValue) && !linkList.Contains(hrefValue))
                {
                    linkList.Add(hrefValue);
                }
            }
        }

        return linkList;
    }

    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    public async Task<ScrapeResult> ScrapeWiley(string searchTerm = "COVID VIRUS")
    {
        var edgeOptions = new EdgeOptions();
        //edgeOptions.AddArgument("--headless=new");
        edgeOptions.AddArgument("--no-sandbox");
        edgeOptions.AddArgument("--disable-dev-shm-usage");
        edgeOptions.AddArgument("--disable-gpu");
        edgeOptions.AddArgument("--disable-extensions");
        edgeOptions.AddArgument("--disable-popup-blocking");
        edgeOptions.AddArgument("--disable-blink-features=AutomationControlled");
        edgeOptions.AddArgument("--disable-plugins");
        edgeOptions.AddArgument("--disable-infobars");
        edgeOptions.AddArgument("--disable-notifications");
        edgeOptions.AddArgument("--disable-software-rasterizer");
        edgeOptions.AddArgument("--disable-features=VizDisplayCompositor");
        edgeOptions.AddArgument("--disable-features=IsolateOrigins,site-per-process");
        edgeOptions.AddArgument("--disable-features=NetworkService,NetworkServiceInProcess");
        edgeOptions.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
        edgeOptions.AddArgument("--disable-features=ImprovedCookieControls,SameSiteByDefaultCookies");
        edgeOptions.AddArgument("--disable-features=SharedArrayBuffer");

        using (var driver = new EdgeDriver(edgeOptions))
        {
            driver.Navigate().GoToUrl("https://onlinelibrary.wiley.com/");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(webdriver => webdriver.FindElement(By.Id("searchField1")));

            var searchBox = driver.FindElement(By.Id("searchField1"));
            searchBox.SendKeys(searchTerm);
            searchBox.SendKeys(Keys.Enter);

            wait.Until(webdriver => webdriver.FindElement(By.ClassName("search-result__title")));

            var searchResults = driver.FindElements(By.ClassName("search-result__title"));
            var linkList = new List<string>();
            var titleList = new List<string>();

            foreach (var result in searchResults)
            {
                var link = result.FindElement(By.TagName("a")).GetAttribute("href");
                var title = result.FindElement(By.TagName("a")).Text;
                if (IsValidUrl(link))
                {
                    linkList.Add(link);
                    titleList.Add(title);
                }
            }

            driver.Quit();

            var scrapeResult = new ScrapeResult
            {
                Url = "https://onlinelibrary.wiley.com/",
                Content = string.Join("\n", titleList),
                Links = linkList
            };

            return await Task.FromResult(scrapeResult);
        }
    }

}
