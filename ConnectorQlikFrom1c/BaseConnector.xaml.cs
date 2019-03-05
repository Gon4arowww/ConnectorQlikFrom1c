using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace СonnectorQlikFrom1c {
    /// <summary>
    /// Логика взаимодействия для BaseConnector.xaml
    /// </summary>
    public partial class BaseConnector : Window {
        public BaseConnector() {
            InitializeComponent();
        }

        public BaseConnector(Dictionary<string, string> io_params = null) {
            InitializeComponent();

            if (io_params != null) {
                //string Srvr, File, Ref, Usr, Pwd, Query, QV_TableName;
                io_params.TryGetValue("File", out string lo_file);
                io_params.TryGetValue("Srvr", out string lo_srvr);
                io_params.TryGetValue("Ref", out string lo_ref);
                io_params.TryGetValue("UserDB", out string lo_usr);
                io_params.TryGetValue("PasswordDB", out string lo_pwd);
                io_params.TryGetValue("Query", out string lo_query);
                io_params.TryGetValue("QV_Table", out string lo_tableNameQV);

                if (lo_srvr == null) {
                    ServerBase.IsChecked = false;
                    base_location.Text = lo_file;
                }
                else {
                    ServerBase.IsChecked = true;
                    base_location.Text = lo_srvr;
                    base_name.Text = lo_ref;
                }
                username.Text = lo_usr;
                password.Password = lo_pwd;
                query_text.Text = lo_query;
                qv_table.Text = lo_tableNameQV;
            }

            /*debug value*/
            ServerBase.IsChecked = true;
            base_location.Text = "dev01";
            base_name.Text = "bf_3_1_markova";
            username.Text = string.Empty;
            password.Password = string.Empty;
            query_text.Text = "ВЫБРАТЬ Код, Наименование, КоррСчет, Город ИЗ Справочник.Банки";
            qv_table.Text = "tableNameQV" + Convert.ToString(DateTime.Today.Hour) + Convert.ToString(DateTime.Today.Minute);
            /*end debug value*/

            SetVisibility();
        }

        private void SetVisibility() {
            Visibility elemVisibility = (ServerBase.IsChecked.Value) ? Visibility.Visible : Visibility.Hidden;
            label_name.Visibility = elemVisibility;
            base_name.Visibility = elemVisibility;
            label_location.Content = (ServerBase.IsChecked.Value) ? "Сервер" : "Каталог";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void ServerBase_Click_1(object sender, RoutedEventArgs e) {
            SetVisibility();
        }

        public bool GetServerBase() {
            return ServerBase.IsChecked.Value;
        }

        public string GetBaseLocation() {
            return base_location.Text;
        }

        public string GetBaseName() {
            return base_name.Text;
        }

        public string GetQvTable() {
            return qv_table.Text;
        }

        public string GetQueryText() {
            return query_text.Text;
        }

        public string GetUser() {
            return username.Text;
        }

        public string GetPassword() {
            return password.Password;
        }

        private void Hyperlink_Start1c(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
