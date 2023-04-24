using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading;

namespace AskMeWhatever
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _Proxy { get; set; } = ConfigurationManager.AppSettings.Get("ProxyServer");
        public string? Proxy
        {
            get { return _Proxy; }
            set { _Proxy = value; }
        }

        private string? _API_KEY { get; set; } = ConfigurationManager.AppSettings.Get("API_KEY");
        public string? API_KEY
        {
            get { return _API_KEY; }
            set { _API_KEY = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            TextProxy.Text = Proxy;
        }

        // Задать вопрос 
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await SendQuestions(TextQuestions.Text);

        }

        // Метод задать вопрос
        async Task SendQuestions(string question)
        {
            // ДЛЯ ВАРИАНТА С ПРОКСИ
            //WebProxy proxyObject = new WebProxy(proxy, true);

            /*  HttpClient client = new HttpClient(new HttpClientHandler
              {
                  Proxy = proxyObject,
                  Credentials = new NetworkCredential("user102812", "mdfu9e")
              });*/

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("authorization", $"Bearer {API_KEY}");

            var content = new StringContent("{\"model\": \"text-davinci-003\", \"prompt\": \"" + question + "\",\"temperature\": 1,\"max_tokens\": 1000}",
            Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", content);

            /*    int count = 0;

                while (!response.IsSuccessStatusCode)
                {
                    TextResponse.Text = count.ToString();
                    count++;
                    Thread.Sleep(500);
                }*/


            string responseString = await response.Content.ReadAsStringAsync();

            try
            {
                var dyData = JsonConvert.DeserializeObject<dynamic>(responseString);

                TextResponse.Foreground = Brushes.Green;
                TextResponse.Text = dyData!.choices[0].text;

            }
            catch (Exception ex)
            {
                TextResponse.Foreground = Brushes.Red;
                TextResponse.Text = $"ОШИБКА! НЕ получилось десериализовать JSON: {ex.Message}";
            }
        }


        // Устанавливаю значение в app.config
        public void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        // Ввести прокси сервер
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Proxy = TextProxy.Text;
            AddUpdateAppSettings("ProxyServer", Proxy);
        }

        // Ввести ключ API
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            API_KEY = TextApiKey.Text;
            AddUpdateAppSettings("API_KEY", API_KEY);
        }

        //Отслеживаю ресайз главного окна и изменяю по нему размер текстовых блоков
        private void ResizeWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double sizeWin = this.ActualHeight;
            TextQuestions.Height = sizeWin - 170;
            TextResponse.Height = sizeWin - 120;

        }
    }
}
