using System;
using Newtonsoft.Json; //for json strings for http
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Diagnostics; // needed for command line arguments
using System.IO;
using System.Net; //needed for http requests
using System.Text;
using System.Text.RegularExpressions; //for checking input is a number
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // for HTTPCLIENT

namespace FeroUserUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public class PublicWebsite
    {
        public string amount { get; set; }
        public int closingtime { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public bool notification { get; set; }
        public int openingtime { get; set; }
        public string trashtype { get; set; }
        public int zipcode { get; set; }
        public string roadname { get; set; }
    }

    

    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToKeyValue(this object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = child.ToKeyValue();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                jValue?.ToString("o", CultureInfo.InvariantCulture) :
                jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            

        }


   

        public static string[] readRecord(string searchTerm, string filepath, int positionOfSearchTerm)
        {
            positionOfSearchTerm--;
            string[] recordNotFound = { "Record not found" };
          

            try
            {
                
                string[] lines = System.IO.File.ReadAllLines(@"C:\\Users\\tj\\source\\repos\\FeroUserUI\\FeroUserUI\\bin\\Debug\\PostalCode.csv");
                for (int i = 0; i <lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    if (recordMatches(searchTerm, record: fields, positionOfSearchTerm))
                    {
                        Console.WriteLine("Record found");
                        return fields;
                    }
                    
                }
                return recordNotFound;
            }
            catch (Exception ex)    
            {
                Console.WriteLine("This program did an oopsie",ex);
                return recordNotFound;
                throw new ApplicationException("This Program did an oopsie :", ex);

            }
        }
        public static bool recordMatches(string searchTerm, string[] record, int positionOfSearchTerm)
        {
            if(record[positionOfSearchTerm].Equals(searchTerm))
            {
                return true;
            }
            return false;
        }
        private void Search_Postal(object sender, EventArgs e)
        {
            Console.WriteLine(string.Join(" ", readRecord(User_zipcode.Text, @"C:\\Users\\tj\\source\\repos\\FeroUserUI\\FeroUserUI\\bin\\Debug\\PostalCode.csv", 1)));
            Console.ReadLine();
            RoadName.Text = string.Join(" ", readRecord(User_zipcode.Text, @"C:\\Users\\tj\\source\\repos\\FeroUserUI\\FeroUserUI\\bin\\Debug\\PostalCode.csv", 1)).Remove(0, 6);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

      
        
        private void SearchKG_Click(object sender, RoutedEventArgs e)
        {
            Process aeaseller = new Process();
            aeaseller.StartInfo.FileName = "cmd";
            aeaseller.StartInfo.WorkingDirectory = "C:\\Users\\kevan\\FETCH\\my_aea_projects\\http_test";
            aeaseller.StartInfo.Arguments = "/c aea run";
            aeaseller.Start();

            PublicWebsite public_user = new PublicWebsite();
            public_user.name = User_name.Text;
            public_user.trashtype = User_trashtype.Text;
            public_user.id = 1;
            public_user.notification = false;
            public_user.amount = User_amount.Text;
            public_user.openingtime = int.Parse(User_openingtime.Text);
            public_user.closingtime = int.Parse(User_closingtime.Text);
            public_user.zipcode = int.Parse(User_zipcode.Text);

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:4444/");
            listener.Start();
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                string msg = JsonConvert.SerializeObject(public_user);
                context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                using (Stream stream = context.Response.OutputStream)
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(msg);
                    }
                }
            }
        }

        private void SearchKG_Click_Website(object sender, RoutedEventArgs e)
        {

            PublicWebsite public_user = new PublicWebsite();
            public_user.name = User_name.Text;
            public_user.trashtype = User_trashtype.Text;
            public_user.id = 1;
            //public_user.notification = false;
            public_user.amount = User_amount.Text;
            public_user.openingtime = int.Parse(User_openingtime.Text);
            public_user.closingtime = int.Parse(User_closingtime.Text);
            public_user.zipcode = int.Parse(User_zipcode.Text);

            HttpClient post_client = new HttpClient();
            //string content = JsonConvert.SerializeObject(public_user);
            //var response = post_client.PostAsync("https://feropublic.herokuapp.com", new StringContent(content, Encoding.UTF8, "application/json"));
            var keyValues = public_user.ToKeyValue();
            var content = new FormUrlEncodedContent(keyValues);
            var response = post_client.PostAsync("https://feropublic.herokuapp.com", content);
            response.Wait();
            var responseString = response.Result.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseString);

            
        }





        private void User_openingtime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
    }
}
