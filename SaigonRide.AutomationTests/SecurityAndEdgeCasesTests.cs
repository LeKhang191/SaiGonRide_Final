using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace SaigonRide.AutomationTests
{
    public class SecurityAndEdgeCasesTests
    {
        private IWebDriver driver;
        private readonly string baseUrl = "http://localhost:5236";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.BinaryLocation = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        [Test]
        public void Test_UnauthorizedAccess_RegularUserCannotAccessAdmin()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Account/Login");
            driver.FindElement(By.Name("email")).SendKeys("khangle@ex.com");
            driver.FindElement(By.Name("password")).SendKeys("1234");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2000);

            driver.Navigate().GoToUrl(baseUrl + "/Admin/Dashboard");
            Thread.Sleep(2000);

            Assert.That(driver.Url.Contains("Admin/Dashboard"), Is.False,
                "Serious security vulnerability: Ordinary users can access the Admin page!");
        }

        [Test]
        public void Test_Validation_DuplicateEmail_ShowsError()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Account/Register");
            Thread.Sleep(1000);

            driver.FindElement(By.Name("FullName")).SendKeys("Hacker");
            driver.FindElement(By.Name("Email")).SendKeys("khangle@ex.com");
            driver.FindElement(By.Name("Password")).SendKeys("123456");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(1500);

            Assert.That(driver.Url.Contains("/Account/Register"), Is.True,
                "Data Error: The system allowed registration of two accounts with the same email address!");

            string pageText = driver.FindElement(By.TagName("body")).Text.ToLower();
            Assert.That(pageText.Contains("email") || pageText.Contains("exist"), Is.True,
                "The system blocked registrations but did not display any error messages to the user.");
        }

        [Test]
        public void Test_StartRental_NoVehicleOrMissingData_CannotSubmit()
        {
            // Bước 1: Login
            driver.Navigate().GoToUrl(baseUrl + "/Account/Login");
            driver.FindElement(By.Name("email")).SendKeys("khangle@ex.com");
            driver.FindElement(By.Name("password")).SendKeys("1234");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2000);

            driver.Navigate().GoToUrl(baseUrl + "/Rental/StartRental");
            Thread.Sleep(1500);

            IWebElement userDropdown = driver.FindElement(By.Name("userId"));
            new SelectElement(userDropdown).SelectByIndex(1);

            IWebElement stationDropdown = driver.FindElement(By.Name("startStationId"));
            new SelectElement(stationDropdown).SelectByIndex(1);

            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(1500);

            Assert.That(driver.Url.Contains("/Rental/StartRental"), Is.True,
                "Logical Error: Allows renting a trip with no vehicle available!");
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}