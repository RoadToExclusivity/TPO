using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace lab4unittest
{
    [TestClass]
    public class UnitTest1
    {
        IWebDriver chrome;

        [TestInitialize]
        public void Initialize()
        {
            chrome = new ChromeDriver();
            chrome.Navigate().GoToUrl("http://dimaoq.ispringlearn.com/");
            chrome.FindElement(By.Id("email")).SendKeys("dimaoq@gmail.com");
            chrome.FindElement(By.Id("password")).SendKeys("cCj2Wm");
            chrome.FindElement(By.Id("loginButtonButtonLabel")).Click();
            chrome.FindElement(By.ClassName("close_button")).Click();
            chrome.FindElement(By.Id("mainMenusettings")).Click();
        }

        [TestMethod]
        public void AddOrganizationsWithoutDescription()
        {
            chrome.FindElement(By.Id("organizationsSettingsLink")).Click();
            chrome.FindElement(By.Id("addOrganizationLinkButtonLabel")).Click();
            string orgName = Helper.GetRandomString();
            chrome.FindElement(By.Id("organizationName")).SendKeys(orgName);
            chrome.FindElement(By.Id("createOrganizationSaveButton")).Click();

            bool ok = false;
            var table = chrome.FindElement(By.ClassName("list"));
            var elements = table.FindElements(By.TagName("tr"));
            foreach (var elem in elements)
            {
                var className = elem.GetAttribute("class");
                if (className == "item_row mouse_states" || className == "item_row mouse_states first")
                {
                    var tag = elem.FindElement(By.XPath("td/a"));
                    if (tag.Text == orgName)
                    {
                        ok = true;
                    }
                }
            }

            Assert.IsTrue(ok);
        }

        [TestCleanup]
        public void Cleanup()
        {
            chrome.Quit();
        }

    }

    static public class Helper
    {
        static public string GetRandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}
