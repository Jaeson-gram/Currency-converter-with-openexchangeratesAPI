using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace CurrencyConverter_static
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region This region makes the maximize button hidden: https://stackoverflow.com/questions/18707782/disable-maximize-button-of-wpf-window-keeping-resizing-feature-intact#:~:text=WPF%20does%20not%20have%20the%20native%20capability%20to,will%20need%20to%20resort%20to%20a%20WinAPI%20call.

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button



        private IntPtr _windowHandle;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            //DisableMinimizeButton();
            DisableMaximizeButton();
        }

        protected void DisableMinimizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }

        protected void DisableMaximizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("The Window has not yet been completely initialized");
            }

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        }
        #endregion



        #region This region makes the window immovable : https://www.codeproject.com/questions/490600/immovablepluswindowplusinpluswpf

        private void Window1_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion



        Root val = new Root();

        public class Root
        {
            public Rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        //Make sure API return value that name and where you want to store that name are same. Like in API get response INR then set it with INR name.
        public class Rate
        {
            public double AED { get; set; }
            public double NGN { get; set; }
            public double ZMW { get; set; }
            public double GHS { get; set; }
            public double CZK { get; set; }
            public double BTC { get; set; }
            public double CAD { get; set; }
            public double ARS { get; set; }
            public double AFN { get; set; }
            public double JPY { get; set; }
            public double USD { get; set; }
            public double EUR { get; set; }
            public double BYR { get; set; }
            public double ZAR { get; set; }
            public double GBP { get; set; }
            public double JMD { get; set; }
            public double KPW { get; set; }
            public double MAD { get; set; }
            public double RUB { get; set; }
            public double SLL { get; set; }

        }

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
            SourceInitialized += Window1_SourceInitialized;
            ClearControls();
            GetValue();

        }

        private void MainWindow_SourceInitialized1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }




        private async void GetValue()
        {
            //we use the GetData<>() task we just created (see async task below) and we provide the string (url) required
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=myID");
            BindCurrency();
        }


        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();

            try
            {

                using (var client = new HttpClient())
                {
                    //how long does our program wait for the process
                    client.Timeout = TimeSpan.FromMinutes(1);

                    //get the url provided, then save it to the response variable
                    //a http response message including the statuscode(200 for OK, 404 for 'not found') and data (we'll make ours JSON)
                    //we use the await keyword there because it's a process that we'll be waiting for, ansd we don't want our 
                    HttpResponseMessage response = await client.GetAsync(url);

                    //if the fetching works
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //new variables to get the (fetched) response from the web, and store it as string,
                        //then convert it to JSON, actually, it's a JSON file, but we just want to let the program kno and treat it accordingly
                        var ResponseString = await response.Content.ReadAsStringAsync();
                        var ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString);

                        //if that works, display this: the timestamp of the data recieved, with the timestamp given by the API, we can also display the licence or the whole object
                        //actually, we can diplay as many as we've defined in our root class
                        // Add a few buttons and image
                        MessageBox.Show($"License: " + ResponseObject.license, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        //return API response
                        return ResponseObject;
                    }

                    return myRoot;
                }

            }
            catch
            {
                return myRoot;
            }
        }


        private void BindCurrency()
        {
            DataTable dtTable = new DataTable();

            dtTable.Columns.Add("Text");
            dtTable.Columns.Add("Rate");

            dtTable.Rows.Add("--SELECT--", 0);
            dtTable.Rows.Add("AED", val.rates.AED);
            dtTable.Rows.Add("NGN", val.rates.NGN);
            dtTable.Rows.Add("GBP", val.rates.GBP);
            dtTable.Rows.Add("BTC", val.rates.BTC);
            dtTable.Rows.Add("JPY", val.rates.JPY);
            dtTable.Rows.Add("EUR", val.rates.EUR);
            dtTable.Rows.Add("USD", val.rates.USD);
            dtTable.Rows.Add("CZK", val.rates.CZK);
            dtTable.Rows.Add("CAD", val.rates.CAD);
            dtTable.Rows.Add("ZMW", val.rates.ZMW);
            dtTable.Rows.Add("GHS", val.rates.GHS);
            dtTable.Rows.Add("MAD", val.rates.MAD);
            dtTable.Rows.Add("ARS", val.rates.ARS);
            dtTable.Rows.Add("ZAR", val.rates.ZAR);
            dtTable.Rows.Add("BYR", val.rates.BYR);
            dtTable.Rows.Add("AFN", val.rates.AFN);
            dtTable.Rows.Add("RUB", val.rates.RUB);
            dtTable.Rows.Add("JMD", val.rates.JMD);
            dtTable.Rows.Add("KPW", val.rates.KPW);
            dtTable.Rows.Add("SLL", val.rates.SLL);




            //From where does the Combobox fetch it's data
            FromCurrencyCombobox.ItemsSource = dtTable.DefaultView;

            //what should be displayed on in the Combobox
            FromCurrencyCombobox.DisplayMemberPath = "Text";

            //what value it's calculating
            FromCurrencyCombobox.SelectedValuePath = "Rate";

            //what will be selected first in the combobox when we open the app
            FromCurrencyCombobox.SelectedIndex = 0;


            ToCurrencyCombobox.ItemsSource = dtTable.DefaultView;

            ToCurrencyCombobox.DisplayMemberPath = "Text";
            ToCurrencyCombobox.SelectedValuePath = "Rate";
            ToCurrencyCombobox.SelectedIndex = 0;
            txtCurrency.Focus();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            //Currencylabel.Content = "Tomorrow, we'll get it Converted";
            //window.Opacity = 0.3;

            double ConvertedCurrency;

            //if the amount textbox is null or blank, that is, if the iser has not entered a value yet
            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //attempts to set focus on the textbox so the user sees it.
                txtCurrency.Focus();
            }

            //if the user has not selected anything (currency value) to convert from, and the 0th item (--SELECT--) remains selected
            else if (FromCurrencyCombobox.SelectedValue == null || FromCurrencyCombobox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a currency to convert from", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //attempts to set focus on the Combobox that 
                //FromCurrencyCombobox.Focus();

            }

            else if (ToCurrencyCombobox.SelectedIndex == 0 || ToCurrencyCombobox.SelectedValue == null)
            {
                MessageBox.Show("Please select a currency to convert to", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                //ToCurrencyCombobox.Focus();
            }

            //if the user just wants to play around and converts to and form the same currency..
            else if (ToCurrencyCombobox.Text == FromCurrencyCombobox.Text)
            {
                ConvertedCurrency = double.Parse(txtCurrency.Text);
                Currencylabel.Content = ToCurrencyCombobox.Text + " " + ConvertedCurrency.ToString("N3");
            }

            else if (ToCurrencyCombobox.Text != FromCurrencyCombobox.Text)
            {

                //logic for the conversion... Multiply the input with the currencyFrom value, then divide it by value of the currency to convert to

                //string strfromcurr = Convert.ToString(FromCurrencyCombobox.SelectedValue);
                //string strtocurr = Convert.ToString(ToCurrencyCombobox.SelectedValue);
                //string strCurr = txtCurrency.Text;

                //double neww = Double.Parse(strfromcurr);
                //double newh = Double.Parse(strtocurr);
                //double newg = Double.Parse(strCurr);

                //ConvertedCurrency = neww * newg / newh;


                //we have to convert the FromCurrencyCombobox.SelectedValue and ToCurrencyCombobox.SelectedValue from type Object to type string first, before converting to double
                //this way it's faster to process, compared to the above lines
                ConvertedCurrency = double.Parse(txtCurrency.Text) * double.Parse(ToCurrencyCombobox.SelectedValue.ToString()) / double.Parse(FromCurrencyCombobox.SelectedValue.ToString());




                //show the converted currency name and it's value, and add a '.000'
                Currencylabel.Content = ToCurrencyCombobox.Text + " " + ConvertedCurrency.ToString("N3");


            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //Currencylabel.Content = "";
            ClearControls();
        }

        //this method clreas everything and will be used in the clearbutton_click method... we didn't put the codes directly in the clearButton_click method in case of reusability
        private void ClearControls()
        {
            txtCurrency.Text = "";

            if (FromCurrencyCombobox.SelectedIndex > 0)
            {
                FromCurrencyCombobox.SelectedIndex = 0;
                //FromCurrencyCombobox.Items.Clear();
            }
            if (ToCurrencyCombobox.SelectedIndex > 0)
            {
                ToCurrencyCombobox.SelectedIndex = 0;
                //ToCurrencyCombobox.Items.Clear();
            }

            Currencylabel.Content = "";
            txtCurrency.Focus();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //see REGEX (Regular Expressions)
            //Regex regex = new Regex("[0-9]+");
            //e.Handled = regex.IsMatch(e.Text);

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


    }
}
