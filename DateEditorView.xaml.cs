using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using MainProperties = AddDateStamp.Properties;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
    //class fontList : ObservableCollection<string>
    //{
    //    public fontList()
    //    {
    //        Add("Times New Roman");
    //        Add("Katherine Plus");
    //        //Type type = typeof(Fonts);
    //        //foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static |
    //        //                                                         BindingFlags.Public))
    //        //{
    //        //    if (propertyInfo.Name.Equals("SystemFontFamilies"))
    //        //    {
    //        //        Add(propertyInfo.Name);

    //        //    }
    //        //}
    //    }
    //}
    public partial class DateEditoeView
    {
        public string selectedDateStr { get; set; }
        public string fontSize { get; set; }
        public bool cancel { get; set; }
        //public string fontFamilyName { get; set; }
        public System.Drawing.Color textColor { get; set; }

        //public bool transparentBackground { get; set; }

        public int intFontSize { get; set; }

        public DateTime selectedDate { get; set; }

        //public FontFamily katherinePlus { get; set; }





        //private void GetListOfFonts()
        //{

        //    //Type type = typeof(Fonts);
        //    //foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static |
        //    //                                                         BindingFlags.Public))
        //    //{
        //    //    if (propertyInfo.Name.Equals("SystemFontFamilies"))
        //    //    {
        //    //        var name = propertyInfo.Name;
        //    //        this.fontList = (ReadOnlyCollection<FontFamily>)propertyInfo.GetValue(null);
        //    //    }
        //    //}
        //    //using (System.Drawing.Text.InstalledFontCollection installedFonts = new System.Drawing.Text.InstalledFontCollection())
        //    //{
        //    //    foreach (System.Drawing.FontFamily fontFamily in installedFonts.Families)
        //    //    {
        //    //        fontList.Add(fontFamily.Name);
        //    //    }
        //    //}
        //}

        public DateEditoeView()
        {
            //string[] fontList = new string[] { "Times New Roman", "Katherine Plus" };

            InitializeComponent();
            textColor = System.Drawing.Color.Black;
            selectedDate = DateTime.Today.Date;
            selectedDateStr = selectedDate.ToString("dd.MM.yy");
            shownDate.Text = selectedDateStr;
            shownDate.Foreground = fontSizeName.Foreground;
            calendar.SelectedDate = selectedDate;
            calendar.DisplayDate = selectedDate;
            fontSize = "14";
            shownDate.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.5);
            inputFontSize.Text = fontSize;
            fontSizeSlider.Value = Convert.ToDouble(fontSize);
            cancel = true;
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            //if (transparentBackgroundCheckbox.IsChecked == true)
            //    transparentBackground = true;
            //else
            //    transparentBackground = false;
            //selectedDateStr = shownDate.Text;
            fontSize = inputFontSize.Text;
            cancel = false;
            Close();
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            cancel = true;
            Close();
        }

        private void OnDateChanged(object sender, RoutedEventArgs e)
        {
            selectedDateStr = calendar.SelectedDate.Value.ToString("dd.MM.yy");
            System.Windows.Input.Mouse.Capture(null);
            if (shownDate != null) shownDate.Text = selectedDateStr;
        }

        //private void PrintedRadioBTNChecked(object sender, RoutedEventArgs e)
        //{
        //    fontFamilyName = "Times New Roman";
        //    inputText.FontFamily = new FontFamily("Times New Roman");
        //}

        //private void HandWriteRadioBTNChecked(object sender, RoutedEventArgs e)
        //{
        //    fontFamilyName = "Katherine Plus";
        //    inputText.FontFamily = new FontFamily(new Uri("pack://application:,,,/AddDateStamp.ext2;Component/resources/"), "./#Katherine Plus");
        //}

        //private void BlackRadioBTNChecked(object sender, RoutedEventArgs e)
        //{
        //    textColor = System.Drawing.Color.Black;
        //}

        //private void NavyRadioBTNChecked(object sender, RoutedEventArgs e)
        //{
        //    textColor = System.Drawing.Color.Navy;
        //}

        private void FontSizeTextChanged(object sender, RoutedEventArgs e)
        {
            fontSize = inputFontSize.Text;
            try
            {
                shownDate.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.5);
            }
            catch
            {
                fontSize = "14";
                shownDate.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.5);
            }
        }

        private void FontSizeSliderMove(object sender, RoutedEventArgs e)
        {
            fontSize = fontSizeSlider.Value.ToString();
            inputFontSize.Text = fontSize;
            shownDate.FontSize = Math.Round(Convert.ToDouble(fontSize) * 1.5);
        }


        //private void MouseWheelHandler(object sender, System.Windows.Input.MouseWheelEventArgs e)
        //{
        //    // If the mouse wheel delta is positive, move the box up.
        //    if (e.Delta > 0)
        //    {
        //        inputText.FontSize += 1;
        //        inputFontSize.Text = inputText.FontSize.ToString();
        //    }

        //    // If the mouse wheel delta is negative, move the box down.
        //    if (e.Delta < 0)
        //    {
        //        inputText.FontSize -= 1;
        //        inputFontSize.Text = inputText.FontSize.ToString();
        //    }
        //}

    }
}
