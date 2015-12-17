using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace lab4unittest
{
    [TestClass]
    public class UnitTest1
    {
        IWebDriver driver;
        const string baseURL = "http://dimaoq.ispringlearn.com";

        [TestInitialize]
        public void Initialize()
        {
            driver = new ChromeDriver();
            driver.Navigate().GoToUrl(baseURL);
            driver.FindElement(By.Id("email")).SendKeys("dimaoq@gmail.com");
            driver.FindElement(By.Id("password")).SendKeys("cCj2Wm");
            driver.FindElement(By.Id("loginButtonButtonLabel")).Click();
            driver.FindElement(By.ClassName("close_button")).Click();
            driver.FindElement(By.Id("mainMenusettings")).Click();
        }

        [TestMethod]
        public void CheckCorrectColorChanging() //#5C8BB5 ok
        {
            driver.Navigate().GoToUrl(baseURL + "/settings");
            driver.FindElement(By.Id("accountBrandingLink")).Click();
            driver.FindElement(By.CssSelector("div.color_theme_select.blue1 > div.inner_border > div.color_container")).Click();
            driver.FindElement(By.Id("themeSaveButtonButtonLabel")).Click();
            string script = "return window.document.defaultView.getComputedStyle(window.document.getElementsByClassName('header_container customizable_header_background')[0]).getPropertyValue('background-color')";
            string color = (string)((IJavaScriptExecutor)driver).ExecuteScript(script);
            Assert.IsTrue(color == "rgb(92, 139, 181)");
            driver.Navigate().Refresh();
            driver.FindElement(By.CssSelector("div.color_theme_select.default > div.inner_border > div.color_container")).Click();
            driver.FindElement(By.Id("themeSaveButtonButtonLabel")).Click();
        }

        [TestMethod]
        public void AddOrganizationsWithoutDescription() //ok
        {
            driver.Navigate().GoToUrl(baseURL + "/settings");
            driver.FindElement(By.Id("organizationsSettingsLink")).Click();
            driver.FindElement(By.CssSelector("#addOrganizationLinkButtonLabel > span.icon")).Click();
            driver.FindElement(By.Id("organizationName")).Clear();
            string newOrg = Helper.GetRandomString(10);
            driver.FindElement(By.Id("organizationName")).SendKeys(newOrg);
            driver.FindElement(By.Id("createOrganizationSaveButtonButtonLabel")).Click();
            Helper.ShortSleep();
            string XPath = "//table[@id='organizationsList']/tbody/tr[";
            bool ok = false;
            int tryTimes = 150;
            for (int i = 2; !ok && tryTimes > 0; i++)
            {
                try
                {
                    string orgString = newOrg + System.Environment.NewLine + "Edit" + System.Environment.NewLine + "Groups"
                        + System.Environment.NewLine + "Users";
                    IWebElement element = driver.FindElement(By.XPath(XPath + i.ToString() + "]/td"));
                    tryTimes = 150;
                    if (orgString == element.Text)
                    {
                        ok = true;
                    }
                }
                catch (Exception es)
                {
                    tryTimes--;
                    i--;
                }
            }
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void AddExistedOrganizations() //ok
        {
            driver.Navigate().GoToUrl(baseURL + "/settings");
            driver.FindElement(By.Id("organizationsSettingsLink")).Click();
            driver.FindElement(By.Id("addOrganizationLinkButtonLabel")).Click();
            driver.FindElement(By.Id("organizationName")).Clear();
            driver.FindElement(By.Id("organizationName")).SendKeys("dimaoq");
            driver.FindElement(By.XPath("//div[@id='createOrganizationSaveButton']/div/div")).Click();
            string errMessage = "An organization with this name already exists." + System.Environment.NewLine + "Hide";
            for (int second = 0; ; second++)
            {
                if (second >= 5) Assert.Fail("timeout");
                try
                {
                    if (errMessage == driver.FindElement(By.CssSelector("div.status.error")).Text)
                        break;
                }
                catch (Exception)
                {
                }
                Helper.ShortSleep();
            }
            try
            {
                Assert.AreEqual(errMessage, driver.FindElement(By.CssSelector("div.status.error")).Text);
            }
            catch (Exception)
            {
                Assert.Fail("exception");
            }
        }

        [TestMethod]
        public void CheckForUpButtonInUserProfileFields() //ok
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/user_fields");
            driver.FindElement(By.Id("moveFieldUp6")).Click();
            Helper.ShortSleep();
            driver.FindElement(By.Id("moveFieldUp6")).Click();
            Helper.ShortSleep();
            Assert.IsFalse(driver.FindElement(By.Id("moveFieldUp6")).Displayed);
            driver.FindElement(By.Id("moveFieldDown6")).Click();
            Helper.ShortSleep();
            driver.FindElement(By.Id("moveFieldDown6")).Click();
            Helper.ShortSleep();

            try
            {
                Assert.IsTrue(driver.FindElement(By.Id("moveFieldUp6")).Displayed);
            }
            catch (Exception)
            {
                Assert.Fail("exception in check up/down buttons");
            }
        }

        [TestMethod]
        public void CheckForDownButtonInUserProfileFields() //ok
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/user_fields");
            driver.FindElement(By.Id("moveFieldDown13")).Click();
            Helper.ShortSleep();
            Assert.IsFalse(driver.FindElement(By.Id("moveFieldDown13")).Displayed);
            driver.FindElement(By.Id("moveFieldUp13")).Click();
            Helper.ShortSleep();
            try
            {
                Assert.IsTrue(driver.FindElement(By.Id("moveFieldDown13")).Displayed);
            }
            catch (Exception)
            {
                Assert.Fail("exception in check up/down buttons");
            }
        }

        [TestMethod]
        public void CheckForSameUserFields() //error - не должно проходить этот тест
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/user_fields");
            driver.FindElement(By.Id("editField5")).Click(); //change "Phone" field
            driver.FindElement(By.CssSelector("#editFieldForm5 > div.controls > div.control > input.text_input.field_label")).Clear();
            driver.FindElement(By.CssSelector("#editFieldForm5 > div.controls > div.control > input.text_input.field_label")).SendKeys("Last Name"); //Last Name already exists
            driver.FindElement(By.Id("editFieldForm5FieldActive")).Click();
            driver.FindElement(By.Id("editFieldForm5SaveCancelButtonsSaveButtonButtonLabel")).Click();
            Helper.MediumSleep();

            try
            {
                Assert.AreEqual("Last Name" + System.Environment.NewLine + "Edit", driver.FindElement(By.CssSelector("#fieldRow5 > td.first")).Text);
            }
            catch (Exception)
            {
                Assert.Fail("Exception in check for same user fields (last name)");
            }

            driver.FindElement(By.Id("editField5")).Click();
            driver.FindElement(By.CssSelector("#editFieldForm5 > div.controls > div.control > input.text_input.field_label")).Clear();
            driver.FindElement(By.CssSelector("#editFieldForm5 > div.controls > div.control > input.text_input.field_label")).SendKeys("Phone"); //Last Name already exists
            driver.FindElement(By.Id("editFieldForm5FieldActive")).Click();
            driver.FindElement(By.Id("editFieldForm5SaveCancelButtonsSaveButton")).Click();
            Helper.MediumSleep();

            try
            {
                Assert.AreEqual("Phone" + System.Environment.NewLine + "Edit", driver.FindElement(By.CssSelector("#fieldRow5 > td.first")).Text);
            }
            catch (Exception)
            {
                Assert.Fail("Exception in check for same user fields");
            }
        }

        [TestMethod]
        public void CheckForHTTPS()
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/administrator_settings");
            driver.FindElement(By.Id("forceHttps")).Click();
            driver.FindElement(By.Id("administratorSettingsButtonSaveButtonButtonLabel")).Click(); //set check
            Helper.MediumSleep();
            Assert.IsTrue(driver.Url.StartsWith("https"));

            driver.FindElement(By.Id("forceHttps")).Click();
            driver.FindElement(By.Id("administratorSettingsButtonSaveButtonButtonLabel")).Click(); //unset check
            Helper.MediumSleep();
            Assert.IsFalse(driver.Url.StartsWith("https"));
        }

        [TestMethod]
        public void CheckForRedirectToPreview()
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/account_branding");
            driver.FindElement(By.Id("previewCustomStyleLink")).Click();
            Helper.LongSleep();

            driver.SwitchTo().Window(driver.WindowHandles[1]);
            Assert.IsTrue(driver.Url == "http://dimaoq.ispringlearn.com/content?preview=1");
        }

        [TestMethod]
        public void CheckEnableUserPortal()
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/user_portal_settings");
            
            driver.FindElement(By.XPath("//form[@id='changeUserPortalSettingsForm']/div/label")).Click(); //unset check
            Helper.ShortSleep();
            try
            {
                Assert.IsFalse(driver.FindElement(By.Id("welcomeTitleField")).Displayed);
            }
            catch (Exception)
            {
                Assert.Fail("Exception from unset check");
            }

            driver.FindElement(By.Id("userPortalSettingsSaveButtonButtonLabel")).Click();
            Helper.ShortSleep();

            driver.FindElement(By.XPath("//form[@id='changeUserPortalSettingsForm']/div/label")).Click(); //set check
            Helper.ShortSleep();
            try
            {
                Assert.IsTrue(driver.FindElement(By.Id("welcomeTitleField")).Displayed);
            }
            catch (Exception)
            {
                Assert.Fail("Exception from set check");
            }

            driver.FindElement(By.CssSelector("#userPortalSettingsSaveButton > div.main.inner_button_border > div.button_body")).Click();
            Helper.ShortSleep();
        }

        [TestMethod]
        public void CheckUserPortalChangingFromMainSettings()
        {
            driver.Navigate().GoToUrl(baseURL + "/settings/user_portal_settings");
            string title = Helper.GetRandomString();
            string message = Helper.GetRandomString();
            driver.FindElement(By.Id("welcomeTitleField")).Clear();
            driver.FindElement(By.Id("welcomeTitleField")).SendKeys(title);
            driver.FindElement(By.Id("instructionMessageArea")).Clear();
            driver.FindElement(By.Id("instructionMessageArea")).SendKeys(message);
            driver.FindElement(By.Id("userPortalSettingsSaveButtonButtonLabel")).Click();
            Helper.ShortSleep();
            driver.FindElement(By.Id("previewUserPortalLink")).Click();
            Helper.LongSleep();
            driver.SwitchTo().Window(driver.WindowHandles[1]);

            try
            {
                Assert.AreEqual(title, driver.FindElement(By.CssSelector("h1.welcome_title")).Text);
            }
            catch (Exception)
            {
                Assert.Fail("Exception from welcomeTitle");
            }
            try
            {
                Assert.AreEqual(message, driver.FindElement(By.CssSelector("p.welcome_message")).Text);
            }
            catch (Exception)
            {
                Assert.Fail("Exception from welcome message");
            }

            //switch to normal state
            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.FindElement(By.Id("welcomeTitleField")).Clear();
            driver.FindElement(By.Id("welcomeTitleField")).SendKeys("Welcome title !");
            driver.FindElement(By.Id("instructionMessageArea")).Clear();
            driver.FindElement(By.Id("instructionMessageArea")).SendKeys("Welcome message !");
            driver.FindElement(By.Id("userPortalSettingsSaveButtonButtonLabel")).Click();
        }

        [TestCleanup]
        public void Cleanup()
        {
            driver.Quit();
        }

    }

    static public class Helper
    {
        static private Random random = new Random();
        static private Random lengthRandom = new Random();

        static public string GetRandomString(int stringSize = 0)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            if (stringSize == 0)
            {
                stringSize = lengthRandom.Next(30) + 10;
            }

            var stringChars = new char[stringSize];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        static public void ShortSleep()
        {
            Thread.Sleep(2000);
        }

        static public void MediumSleep()
        {
            Thread.Sleep(4000);
        }

        static public void LongSleep()
        {
            Thread.Sleep(8000);
        }
    }
}
