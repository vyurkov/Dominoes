using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using DominoClient.DominoesService;

namespace DominoClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private LoginDelegate d;

        private bool frgtwin = false;

        public LoginWindow(LoginDelegate sender)
        {
            InitializeComponent();
            d = sender;
        }

        private void ForgotBtn_Click(object sender, RoutedEventArgs e)
        {
            if(frgtwin) {ForgotPasswordGrid.Visibility = Visibility.Hidden;
                frgtwin = false;
            }
            else {ForgotPasswordGrid.Visibility = Visibility.Visible;
                frgtwin = true;
            }
        }

        private void ShowForgotPassBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = this.Owner as MainWindow;
            string user = LoginForgotTextBox.Text.ToLower();
            HintText.Content = (user == "")?"Please enter valid login!": main.client.GetReminderText(user).ToString();
            LoginForgotTextBox.Text = "";
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = this.Owner as MainWindow;

            string user = LoginTextBox.Text.ToLower();
            string pass = PasswordTextBox.Password.ToLower();
            Player p = new Player();

            int result = main.client.Login(user, pass, out p);

            switch (result)
            {
                case 0:
                    d(p);
                    this.Close();
                    break;
                case 1: LoginErrorInfo.Content = "The username or password you enter is incorrect!"; break;
                case 2: LoginErrorInfo.Content = "Already online!"; break;
                case 3: LoginErrorInfo.Content = "You try to use forbidden characters!"; break;
                case 4: LoginErrorInfo.Content = "ERROR: Something going wrong!"; break;
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = this.Owner as MainWindow;

            string user = SignUpLoginTextBox.Text.ToLower();
            string pass = SignUpPasswordTextBox.Password.ToLower();
            string reminder = ReminderSignUpTextBox.Text;

            if(user == "" || pass == "") SignUpErrorInfo.Content = "The username or password you enter is empty!";

            int result = main.client.Registration(user, pass, reminder);

            switch (result)
            {
                case 0:
                    MessageBox.Show("You have successfully registered!"); break;
                case 1: SignUpErrorInfo.Content = "The username or password you enter is incorrect!"; break;
                case 2: SignUpErrorInfo.Content = "You try to use forbidden characters!"; break;
            }

            SignUpLoginTextBox.Text = "";
            SignUpPasswordTextBox.Password = "";
            ReminderSignUpTextBox.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
