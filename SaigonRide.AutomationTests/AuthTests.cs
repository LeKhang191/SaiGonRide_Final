using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SaigonRide.AutomationTests
{
    public class SignUpTests
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
        public void Test_UserSignUp_And_AutoLogin_Success()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Account/Register");
            Thread.Sleep(2000);

            string randomEmail = "tester_" + System.DateTime.Now.Ticks + "@ex.com";

            driver.FindElement(By.Name("FullName")).SendKeys("Auto Test User");
            driver.FindElement(By.Name("Email")).SendKeys(randomEmail);
            driver.FindElement(By.Name("Password")).SendKeys("123456");
            driver.FindElement(By.Name("IdNumber")).SendKeys("079204012345");


            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2000);

            Assert.That(driver.Url == baseUrl + "/" || driver.Url == baseUrl, Is.True, "Registration failed, cannot access the homepage.");
        }

        [Test]
        public void Test_UserLogin_Success()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Account/Login");
            Thread.Sleep(2000);
            driver.FindElement(By.Name("email")).SendKeys("admin@ex.com");
            driver.FindElement(By.Name("password")).SendKeys("admin123");
            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(2000);

            Assert.That(driver.Url == baseUrl + "/" || driver.Url == baseUrl, Is.True, "Login failed, URL is incorrect.");
        }

        [Test]
        public void Test_UserLogin_WithWrongPassword_ShowsError()
        {
            driver.Navigate().GoToUrl(baseUrl + "/Account/Login");
            Thread.Sleep(1000);

            driver.FindElement(By.Name("email")).SendKeys("admin@ex.com");
            driver.FindElement(By.Name("password")).SendKeys("mat_khau_sai_tum_lum");

            driver.FindElement(By.XPath("//button[@type='submit']")).Click();
            Thread.Sleep(1000);

            Assert.That(driver.Url.Contains("/Account/Login"), Is.True, "Security vulnerability: Incorrect password entered but the system was still bypassed!");

            string pageSource = driver.PageSource;
            Assert.That(pageSource.Contains("Invalid"), Is.True, "The system does not display error messages to the user.");
        }

        [TearDown]
        public void Teardown()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}