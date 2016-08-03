using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WeatherForecastWidget
{
    /// <summary>
    /// Interaction logic for WeatherForecastWidget.xaml
    /// </summary>
    public partial class WeatherForecastWidget : UserControl
    {
        
        public int conditionID;




        public WeatherForecastWidget()
        {
            InitializeComponent();

            ////  Load initial forecast data
            InitialForecastDataUpload();



            // Create a timer with a 15-minutes interval.
            System.Timers.Timer weatherTimer = new System.Timers.Timer(900000);

            // Hook up the Elapsed event for the timer. 
            weatherTimer.Elapsed += ForecastData;
            weatherTimer.Enabled = true;
        }







        public void ForecastData(Object source, ElapsedEventArgs e)
        {
            int i = 0;
            while (i < 5)
            {
                try
                {
                    ////  Use web client to grab weaher data from api as json
                    string jsonNow = new WebClient().DownloadString("http://api.openweathermap.org/data/2.5/weather?zip=65616&APPID=e0633625fba64cb2e3586111f669a705").Replace("[", "").Replace("]", "");

                    var jsonForecast = new WebClient().DownloadString("http://api.openweathermap.org/data/2.5/forecast/daily?q=Branson&ampmode=json&ampunits=imperial&ampcnt=5&APPID=e0633625fba64cb2e3586111f669a705");
                    
                    ////  Parse json data to exclude all unneeded data
                    int first = jsonForecast.IndexOf('[');
                    int last = jsonForecast.LastIndexOf(']');
                    string prefix = jsonForecast.Substring(0, first + 1);
                    string query = jsonForecast.Substring(first + 1, last - first - 1);
                    string suffix = jsonForecast.Substring(last);
                    jsonForecast = prefix + query.Replace("[", "").Replace("]", "") + suffix;

                    
                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonNow);


                    double currentKelvin = Convert.ToDouble(((Dictionary<string, object>)dict["main"])["temp"]);
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        currentTemp_TB.Text = Math.Round(((currentKelvin - 273.15) * 1.8000) + 32).ToString();

                        currentDate_TB.Text = "Last Updated: " + e.SignalTime.ToString("M/d/yyyy HH:mm tt");
                        currentCondition_TB.Text = WeatherCodes.WeatherCondition(Convert.ToInt32(((Dictionary<string, object>)dict["weather"])["id"]));
                    }));

                    conditionID = Convert.ToInt32(((Dictionary<string, object>)dict["weather"])["id"]);
                    
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        ChangeBackground();
                    }));


                    var parsed = JObject.Parse(jsonForecast);
                    var forecast = new WeatherForecast();


                    ////  Pass data to display
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        double kelvinMax2 = Convert.ToDouble(parsed.SelectToken("list[1].temp.max").Value<decimal>());
                        day1HighTemp.Text = Math.Round(((kelvinMax2 - 273.15) * 1.8000) + 32).ToString();           ////  Converts Kelvin to Fahrenheit

                        double kelvinMax3 = Convert.ToDouble(parsed.SelectToken("list[2].temp.max").Value<decimal>());
                        day2HighTemp.Text = Math.Round(((kelvinMax3 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMax4 = Convert.ToDouble(parsed.SelectToken("list[3].temp.max").Value<decimal>());
                        day3HighTemp.Text = Math.Round(((kelvinMax4 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMax5 = Convert.ToDouble(parsed.SelectToken("list[4].temp.max").Value<decimal>());
                        day4HighTemp.Text = Math.Round(((kelvinMax5 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMax6 = Convert.ToDouble(parsed.SelectToken("list[5].temp.max").Value<decimal>());
                        day5HighTemp.Text = Math.Round(((kelvinMax6 - 273.15) * 1.8000) + 32).ToString();





                        double kelvinMin2 = Convert.ToDouble(parsed.SelectToken("list[1].temp.min").Value<decimal>());
                        day1LowTemp.Text = Math.Round(((kelvinMin2 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMin3 = Convert.ToDouble(parsed.SelectToken("list[2].temp.min").Value<decimal>());
                        day2LowTemp.Text = Math.Round(((kelvinMin3 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMin4 = Convert.ToDouble(parsed.SelectToken("list[3].temp.min").Value<decimal>());
                        day3LowTemp.Text = Math.Round(((kelvinMin4 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMin5 = Convert.ToDouble(parsed.SelectToken("list[4].temp.min").Value<decimal>());
                        day4LowTemp.Text = Math.Round(((kelvinMin5 - 273.15) * 1.8000) + 32).ToString();

                        double kelvinMin6 = Convert.ToDouble(parsed.SelectToken("list[5].temp.min").Value<decimal>());
                        day5LowTemp.Text = Math.Round(((kelvinMin6 - 273.15) * 1.8000) + 32).ToString();



                        ////  Populates the date
                        dateDisplay1.Text = Date(parsed.SelectToken("list[1].dt").Value<int>());
                        dateDisplay2.Text = Date(parsed.SelectToken("list[2].dt").Value<int>());
                        dateDisplay3.Text = Date(parsed.SelectToken("list[3].dt").Value<int>());
                        dateDisplay4.Text = Date(parsed.SelectToken("list[4].dt").Value<int>());
                        dateDisplay5.Text = Date(parsed.SelectToken("list[5].dt").Value<int>());
                        


                        ////  Populates the day
                        dayOfWeek1.Text = DateTime.Parse(Date(parsed.SelectToken("list[1].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                        dayOfWeek2.Text = DateTime.Parse(Date(parsed.SelectToken("list[2].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                        dayOfWeek3.Text = DateTime.Parse(Date(parsed.SelectToken("list[3].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                        dayOfWeek4.Text = DateTime.Parse(Date(parsed.SelectToken("list[4].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                        dayOfWeek5.Text = DateTime.Parse(Date(parsed.SelectToken("list[5].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                        



                        ////  Populates the icon that depicts the weather for that day.
                        day1ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[1].weather.icon").Value<string>() + ".png", UriKind.Relative));
                        day2ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[2].weather.icon").Value<string>() + ".png", UriKind.Relative));
                        day3ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[3].weather.icon").Value<string>() + ".png", UriKind.Relative));
                        day4ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[4].weather.icon").Value<string>() + ".png", UriKind.Relative));
                        day5ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[5].weather.icon").Value<string>() + ".png", UriKind.Relative));
                        
                    }));
                    
                    break;
                }
                catch
                {
                    i++;
                }
            }


            i = 0;
        }





        



        public void InitialForecastDataUpload()
        {
            int i = 0;
            while (i < 5)
            {
                try
                {
                    ////  Use web client to grab weaher data from api as json
                    string jsonNow = new WebClient().DownloadString("http://api.openweathermap.org/data/2.5/weather?zip=65616&APPID=e0633625fba64cb2e3586111f669a705").Replace("[", "").Replace("]", "");
                    
                    var jsonForecast = new WebClient().DownloadString("http://api.openweathermap.org/data/2.5/forecast/daily?q=Branson&ampmode=json&ampunits=imperial&ampcnt=5&APPID=e0633625fba64cb2e3586111f669a705");

                    ////  Parse json data to exclude all unneeded data
                    int first = jsonForecast.IndexOf('[');
                    int last = jsonForecast.LastIndexOf(']');
                    string prefix = jsonForecast.Substring(0, first + 1);
                    string query = jsonForecast.Substring(first + 1, last - first - 1);
                    string suffix = jsonForecast.Substring(last);
                    jsonForecast = prefix + query.Replace("[", "").Replace("]", "") + suffix;


                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonNow);

                    double currentKelvin = Convert.ToDouble(((Dictionary<string, object>)dict["main"])["temp"]);
                    currentTemp_TB.Text = Math.Round(((currentKelvin - 273.15) * 1.8000) + 32).ToString();
                     
                    currentDate_TB.Text = "Last Updated: " + DateTime.Now.ToString("M/d/yyyy HH:mm tt");
                    currentCondition_TB.Text = WeatherCodes.WeatherCondition(Convert.ToInt32(((Dictionary<string, object>)dict["weather"])["id"]));



                    conditionID = Convert.ToInt32(((Dictionary<string, object>)dict["weather"])["id"]);
                    ChangeBackground();


                    var parsed = JObject.Parse(jsonForecast);
                    var forecast = new WeatherForecast();


                    ////  Pass data to display
                    double kelvinMax2 = Convert.ToDouble(parsed.SelectToken("list[1].temp.max").Value<decimal>());
                    day1HighTemp.Text = Math.Round(((kelvinMax2 - 273.15) * 1.8000) + 32).ToString();           ////  Converts Kelvin to Fahrenheit

                    double kelvinMax3 = Convert.ToDouble(parsed.SelectToken("list[2].temp.max").Value<decimal>());
                    day2HighTemp.Text = Math.Round(((kelvinMax3 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMax4 = Convert.ToDouble(parsed.SelectToken("list[3].temp.max").Value<decimal>());
                    day3HighTemp.Text = Math.Round(((kelvinMax4 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMax5 = Convert.ToDouble(parsed.SelectToken("list[4].temp.max").Value<decimal>());
                    day4HighTemp.Text = Math.Round(((kelvinMax5 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMax6 = Convert.ToDouble(parsed.SelectToken("list[5].temp.max").Value<decimal>());
                    day5HighTemp.Text = Math.Round(((kelvinMax6 - 273.15) * 1.8000) + 32).ToString();





                    double kelvinMin2 = Convert.ToDouble(parsed.SelectToken("list[1].temp.min").Value<decimal>());
                    day1LowTemp.Text = Math.Round(((kelvinMin2 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMin3 = Convert.ToDouble(parsed.SelectToken("list[2].temp.min").Value<decimal>());
                    day2LowTemp.Text = Math.Round(((kelvinMin3 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMin4 = Convert.ToDouble(parsed.SelectToken("list[3].temp.min").Value<decimal>());
                    day3LowTemp.Text = Math.Round(((kelvinMin4 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMin5 = Convert.ToDouble(parsed.SelectToken("list[4].temp.min").Value<decimal>());
                    day4LowTemp.Text = Math.Round(((kelvinMin5 - 273.15) * 1.8000) + 32).ToString();

                    double kelvinMin6 = Convert.ToDouble(parsed.SelectToken("list[5].temp.min").Value<decimal>());
                    day5LowTemp.Text = Math.Round(((kelvinMin6 - 273.15) * 1.8000) + 32).ToString();



                    ////  Populates the date
                    dateDisplay1.Text = Date(parsed.SelectToken("list[1].dt").Value<int>());
                    dateDisplay2.Text = Date(parsed.SelectToken("list[2].dt").Value<int>());
                    dateDisplay3.Text = Date(parsed.SelectToken("list[3].dt").Value<int>());
                    dateDisplay4.Text = Date(parsed.SelectToken("list[4].dt").Value<int>());
                    dateDisplay5.Text = Date(parsed.SelectToken("list[5].dt").Value<int>());



                    ////  Populates the day
                    dayOfWeek1.Text = DateTime.Parse(Date(parsed.SelectToken("list[1].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                    dayOfWeek2.Text = DateTime.Parse(Date(parsed.SelectToken("list[2].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                    dayOfWeek3.Text = DateTime.Parse(Date(parsed.SelectToken("list[3].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                    dayOfWeek4.Text = DateTime.Parse(Date(parsed.SelectToken("list[4].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();
                    dayOfWeek5.Text = DateTime.Parse(Date(parsed.SelectToken("list[5].dt").Value<int>()), CultureInfo.InvariantCulture).DayOfWeek.ToString();




                    ////  Populates the icon that depicts the weather for that day.
                    day1ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[1].weather.icon").Value<string>() + ".png", UriKind.Relative));
                    day2ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[2].weather.icon").Value<string>() + ".png", UriKind.Relative));
                    day3ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[3].weather.icon").Value<string>() + ".png", UriKind.Relative));
                    day4ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[4].weather.icon").Value<string>() + ".png", UriKind.Relative));
                    day5ConditionLogo.Source = new BitmapImage(new Uri("WeatherIcons/" + parsed.SelectToken("list[5].weather.icon").Value<string>() + ".png", UriKind.Relative));
                    

                    break;
                }
                catch
                {
                    i++;
                }
            }

            i = 0;
        }



        private string Date(int time)
        {

            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time).ToShortDateString();

        }






        private void ChangeBackground()
        {
            try
            {
                cloudyWeather.Visibility = Visibility.Collapsed;
                foggyWeather.Visibility = Visibility.Collapsed;
                icyWeather.Visibility = Visibility.Collapsed;
                partlyCloudyWeather.Visibility = Visibility.Collapsed;
                rainyWeather.Visibility = Visibility.Collapsed;
                snowyWeather.Visibility = Visibility.Collapsed;
                sunnyWeather.Visibility = Visibility.Collapsed;
                thunderstormWeather.Visibility = Visibility.Collapsed;
                sandyWeather.Visibility = Visibility.Collapsed;
                volcanicWeather.Visibility = Visibility.Collapsed;
                tornadicWeather.Visibility = Visibility.Collapsed;
                windyWeather.Visibility = Visibility.Collapsed;
                heatAdvisoryWeather.Visibility = Visibility.Collapsed;
                coldAdvisoryWeather.Visibility = Visibility.Collapsed;
                hailWeather.Visibility = Visibility.Collapsed;





                if (conditionID == 200) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 201) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 202) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 210) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 211) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 212) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 221)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 230)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 231)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 232)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 300)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 301)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 302)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 310)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 311)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 312)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 313)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 314)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 321)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 500)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 501)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 502)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 503)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 504)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 511)
                {
                    icyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 520)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 521)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 522)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 531)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 600)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 601)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 602)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 611)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 612)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 615)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 616)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 620)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 621)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 622)
                {
                    snowyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 701)
                {
                    rainyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 711)
                {
                    foggyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 721)
                {
                    foggyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 731)
                {
                    sandyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 741)
                {
                    foggyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 751)
                {
                    sandyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 761)
                {
                    sandyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 762)
                {
                    volcanicWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 781)
                {
                    tornadicWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 800)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 801)
                {
                    partlyCloudyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 802)
                {
                    partlyCloudyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 803)
                {
                    partlyCloudyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 804)
                {
                    cloudyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 900)
                {
                    tornadicWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 901)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 902)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 903)
                {
                    coldAdvisoryWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 904)
                {
                    heatAdvisoryWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 905)
                {
                    windyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 906)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 951)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 952)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 953)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 954)
                {
                    partlyCloudyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 955)
                {
                    sunnyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 956)
                {
                    windyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 957)
                {
                    windyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 958)
                {
                    windyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 959)
                {
                    windyWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 960)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 961)
                {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }

                else if (conditionID == 962) {
                    thunderstormWeather.Visibility = Visibility.Visible;
                }
            }
            catch (Exception r)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"[Set your file location here]", true))
                {
                    file.WriteLine(DateTime.Now + "  -  " + r);
                }
            }
        }
    }
}
