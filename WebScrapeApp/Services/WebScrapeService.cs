using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace WebScrapeApp.Services;


public class ScrapeResult
{
    public string Url { get; set; }
    public string Content { get; set; }

    public List<string> Emails { get; set; }

}
public class WebScrapeService
{
    //static content scraping
    public async Task<ScrapeResult> ScrapUrl(string url)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var htmldoc= new HtmlDocument{
            OptionAutoCloseOnEnd = true,
            OptionFixNestedTags = true,
            //OptionOutputAsXml = true,
            //OptionWriteEmptyNodes = true
        };
        htmldoc.LoadHtml(content);


        return new ScrapeResult 
        { 
            Url = url, 
            Content = htmldoc.DocumentNode.InnerHtml.Trim().Replace(" +","").Replace("\n","")
        };
    }

    //dynamic content scraping
    public Task<ScrapeResult> ScrapUrlDynamic(string url)
    {
        //var chromeOptions = new ChromeOptions();
        var chromeOptions = new EdgeOptions();
        chromeOptions.AddArgument("--headless=new");
        chromeOptions.AddArgument("--no-sandbox");
        chromeOptions.AddArgument("--disable-dev-shm-usage");
        chromeOptions.AddArgument("--disable-gpu");
        chromeOptions.AddArgument("--disable-extensions");
        chromeOptions.AddArgument("--disable-popup-blocking");
        chromeOptions.AddArgument("--disable-extensions");
        chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
        chromeOptions.AddArgument("--disable-plugins");
        chromeOptions.AddArgument("--disable-extensions");
        chromeOptions.AddArgument("--disable-plugins-discovery");
        chromeOptions.AddArgument("--disable-infobars");
        chromeOptions.AddArgument("--disable-notifications");
        chromeOptions.AddArgument("--disable-popup-blocking");
        chromeOptions.AddArgument("--disable-extensions");
        chromeOptions.AddArgument("--disable-plugins-discovery");
        chromeOptions.AddArgument("--disable-gpu");
        chromeOptions.AddArgument("--disable-software-rasterizer");
        chromeOptions.AddArgument("--disable-dev-shm-usage");
        chromeOptions.AddArgument("--disable-features=VizDisplayCompositor");
        chromeOptions.AddArgument("--disable-features=IsolateOrigins,site-per-process");
        chromeOptions.AddArgument("--disable-features=NetworkService,NetworkServiceInProcess");
        chromeOptions.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
        chromeOptions.AddArgument("--disable-features=ImprovedCookieControls,SameSiteByDefaultCookies");
        chromeOptions.AddArgument("--disable-features=SharedArrayBuffer");

        using (var driver = new EdgeDriver(chromeOptions))
        {
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(webdriver => webdriver.FindElement(By.TagName("body")));
            var content = driver.PageSource;
            driver.Quit();

            var htmldoc= new HtmlDocument{
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                //OptionOutputAsXml = true,
                //OptionWriteEmptyNodes = true
            };
            htmldoc.LoadHtml(content);

            var cont = htmldoc.DocumentNode.InnerHtml.Trim().Replace(" +", "").Replace("\n", "");
            var emailList = ExtractEmailsFromHtml(cont);
            var ScrapeResult= new ScrapeResult 
            { 
                Url = url, 
                Content = cont,
                Emails = emailList
            };

            return Task.FromResult(ScrapeResult);
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

}
