using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SaigonRide.AutomationTests
{
    public class EcoLeaderboardTests
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
        public void Test_EcoLeaderboard_LoadsSuccessfully()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Rental/Leaderboard");
            Thread.Sleep(2000);

            var headerText = driver.FindElement(By.TagName("h1")).Text;
            Assert.That(headerText.Contains("ECO IMPACT"), Is.True, "The Leaderboard page cannot load or has the wrong title.");
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}