namespace WpTema1.Persistance.Data;

public record User(int UserId, string Username, string Password, string Email, bool Admin)
{
  public User() : this(0, "", "", "", false)
  {
  }
  public User(string username, string pass, string mail, bool admin) : this(0, username, pass, mail, admin)
  {
    
  }
  
}

