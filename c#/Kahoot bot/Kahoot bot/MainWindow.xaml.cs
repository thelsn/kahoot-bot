using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace KahootBot
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var waitTime = int.Parse(WaitTimeTextBox.Text);
            var nickname = NicknameTextBox.Text;

            await RequestWithRandomNumber(waitTime, nickname, cancellationTokenSource.Token);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            LogListBox.Items.Add("Search stopped.");
        }

        private async Task RequestWithRandomNumber(int waitTime, string nickname, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var random = new Random();
                var randomLength = random.Next(5, 9);
                var randomCode = GenerateRandomNumber(randomLength);
                var url = $"https://kahoot.it/reserve/session/{randomCode}";

                try
                {
                    var response = await httpClient.GetAsync(url, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        LogListBox1.Items.Add($"Code Found: {randomCode}");
                        LogListBox1.ScrollIntoView(LogListBox1.Items[LogListBox1.Items.Count - 1]);
                        await Task.Delay(5000 + waitTime * 1000, cancellationToken); // Search Time
                        OpenKahootInBrowser(randomCode, nickname);
                    }
                    else
                    {
                        LogListBox1.Items.Add($"Bad code: {randomCode}");
                        LogListBox1.ScrollIntoView(LogListBox1.Items[LogListBox1.Items.Count - 1]);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Task was canceled, break the loop
                    break;
                }
                catch (Exception ex)
                {
                    LogListBox.Items.Add($"An error occurred: {ex.Message}");
                }
            }
        }



        private void OpenKahootInBrowser(string code, string nickname)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--incognito");

            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl("https://kahoot.it/");

                // Wait for page to load
                System.Threading.Thread.Sleep(2000);

                try
                {
                    var inputField = driver.FindElement(By.Id("game-input"));
                    inputField.SendKeys(code);

                    var submitButton = driver.FindElement(By.CssSelector(".button__Button-sc-vzgdbz-0"));
                    submitButton.Click();

                    System.Threading.Thread.Sleep(1500);

                    // Check for any error elements
                    var errorClasses = new List<string> { "namerator__PageWrapper-sc-yvb5ka-0", "network-dialog__Body-sc-s0ocva-0", "join__CollaborationOrTeamModeBadge-sc-1ezg926-2", "two-factor-cards__square-button" };
                    var foundError = errorClasses.Any(className => driver.FindElements(By.ClassName(className)).Any());

                    if (foundError)
                    {
                        LogListBox.Items.Add($"Error joining with code: {code}");
                        return;
                    }

                    var nicknameField = driver.FindElement(By.Id("nickname"));
                    nicknameField.SendKeys(nickname);

                    submitButton = driver.FindElement(By.CssSelector(".button__Button-sc-vzgdbz-0"));
                    submitButton.Click();

                    System.Threading.Thread.Sleep(1000);

                    // Check for additional error conditions
                    if (driver.FindElements(By.ClassName("gATPEc")).Any() || driver.FindElements(By.ClassName("new-podium__Ribbon-sc-1f3tqpx-15")).Any())
                    {
                        LogListBox.Items.Add($"Error joining with code: {code}");
                    }
                    else
                    {
                        LogListBox.Items.Add($"Success with code: {code}");
                        // Keep the browser open for a while (e.g., 1000 seconds)
                        System.Threading.Thread.Sleep(1000000);
                    }
                    LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                }
                catch (Exception ex)
                {
                    LogListBox.Items.Add($"An error occurred: {ex.Message}");
                }
            }
        }


        private string GenerateRandomNumber(int length)
        {
            var random = new Random();
            var chars = "0123456789";
            var result = new char[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }
    }
}
