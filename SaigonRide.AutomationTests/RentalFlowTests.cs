using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SaigonRide.AutomationTests
{
    public class RentalFlowTests
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
        public void Test_FullFlow_FromSignUp_To_EcoLeaderboard()
        {
            string randomId = DateTime.Now.Ticks.ToString().Substring(8);
            string randomEmail = $"ecorider_{randomId}@ex.com";
            string userName = $"Eco Rider {randomId}";

            driver.Navigate().GoToUrl(baseUrl + "/Account/Register");
            Thread.Sleep(1000);

            driver.FindElement(By.Name("FullName")).SendKeys(userName);
            driver.FindElement(By.Name("Email")).SendKeys(randomEmail);
            driver.FindElement(By.Name("Password")).SendKeys("123456");
            driver.FindElement(By.Name("IdNumber")).SendKeys("079204012345");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(1000);

            driver.Navigate().GoToUrl(baseUrl + "/Rental/StartRental");
            Thread.Sleep(2500);

            IWebElement userDropdown = driver.FindElement(By.Name("userId"));
            SelectElement selectUser = new SelectElement(userDropdown);
            selectUser.SelectByText(userName);

            IWebElement stationDropdown = driver.FindElement(By.Name("startStationId"));
            SelectElement selectStation = new SelectElement(stationDropdown);
            selectStation.SelectByIndex(1);

            IWebElement vehicleDropdown = driver.FindElement(By.Name("vehicleId"));
            SelectElement selectVehicle = new SelectElement(vehicleDropdown);
            selectVehicle.SelectByIndex(1);


            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2500);

            IWebElement endBtn = driver.FindElement(By.PartialLinkText("END"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", endBtn);

            Thread.Sleep(1500);

            IWebElement endStationDropdown = driver.FindElement(By.Name("endStationId"));
            SelectElement selectEndStation = new SelectElement(endStationDropdown);
            selectEndStation.SelectByIndex(1);

            Thread.Sleep(2000);

            IWebElement cashRadio = driver.FindElement(By.Id("cash"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cashRadio);
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            driver.Navigate().GoToUrl(baseUrl + "/Rental/Leaderboard");
            Thread.Sleep(1000);

            string pageContent = driver.FindElement(By.TagName("body")).Text.ToUpper();
            string searchName = userName.ToUpper();
            Assert.That(pageContent.Contains(searchName), Is.True, "Logical Error: The customer's CO2 score could not be calculated after the trip was completed!");
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}