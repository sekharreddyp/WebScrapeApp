using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace WebScrapeApp.Services;


public class SeleniumService
{
    //dynamic content scraping
    public Task OpenBrowser(string url)
    {
        var edgeOptions = new EdgeOptions();

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
            driver.Quit();

            var htmldoc = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                //OptionOutputAsXml = true,
                //OptionWriteEmptyNodes = true
            };
            htmldoc.LoadHtml(content);

            var cont = htmldoc.DocumentNode.InnerHtml.Trim().Replace(" +", "").Replace("\n", "");

            return Task.CompletedTask;
        }
    }

}
