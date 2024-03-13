using System.Windows;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg;
using WpTema1.Business.Encryption;
using WpTema1.Database;
using WpTema1.Persistance.Data;
using WpTema1.Persistance.Service;
namespace WpTema1.Presentation;

public partial class LoginWindow : Window
{
    private UserDao userDao;
    private DBConnection _connection;
    public LoginWindow()
    {
        _connection = DBConnection.Instance;
        _connection.OpenConnection();
        _connection.UseConnection();
        userDao = new UserDao();
        InitializeComponent();
    }

    private void LoginBtn_OnClick(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text;
        string password = PasswordEncryption.Encrypt(txtPassword.Password);

        if (userDao.ValidateUser(username, password) == false)
        {
           MessageBoxResult result= MessageBox.Show("Incorrect credentials!","Incorrect credentials!", MessageBoxButton.OK, MessageBoxImage.Error);
           if (result == MessageBoxResult.OK)
           {
               txtUsername.Clear();
               txtPassword.Clear();
           }
        }
        else
        {
            Hide();
            Shop shop = new Shop();
            shop.Show();
        }
    }
    
}