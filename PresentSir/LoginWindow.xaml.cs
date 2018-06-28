using System;
using System.Linq;
using System.Windows;
using Emgu.CV;

namespace AttendanceSystem
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                databaseEntities db = new databaseEntities();
                if (TBx_User.Text != string.Empty || PBx_Password.Password != string.Empty)
                {
                    int username = Convert.ToInt32(TBx_User.Text);
                    var user = db.logins.FirstOrDefault(a => a.Id.Equals(username));
                    if (user != null)
                    {
                        if (user.pass.Equals(PBx_Password.Password))
                        {
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Hide();
                        }
                        else
                        {
                            LBl_Login_Message.Content = "Login id or password is incorrect";
                        }
                    }
                    else
                    {
                        LBl_Login_Message.Content = "Login id or password is incorrect";
                    }
                }
                else
                {
                    LBl_Login_Message.Content = "Enter login id and password";
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show( ex.Message);
            }
        }
    }
}
