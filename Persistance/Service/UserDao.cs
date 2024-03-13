using WpTema1.Persistance.Data;

namespace WpTema1.Persistance.Service;

public class UserDao : DataDao<User>
{
    public bool ValidateUser(string username, string password)
    {
        var fields = new Dictionary<string, object>()
        {
            { "username", username },
            { "password", password }
        };
        User user = FindUsingFields(fields).First();

        if (user != null) return true;
        return false;
    }
}