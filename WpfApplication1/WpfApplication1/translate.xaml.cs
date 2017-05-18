using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for translate.xaml
    /// </summary>
    public partial class translate : Window
    {
        public translate()
        {
            InitializeComponent();
        }

        private void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            string strTranslatedText = null;
           try
            {
                TranslatorService.LanguageServiceClient client = new TranslatorService.LanguageServiceClient("BasicHttpBinding_LanguageService");
                client = new TranslatorService.LanguageServiceClient("BasicHttpBinding_LanguageService");
                strTranslatedText = client.Translate("6CE9C85A41571C050C379F60DA173D286384E0F2", txtTraslateFrom.Text, "", "en");
                txtTranslatedText.Text = strTranslatedText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
