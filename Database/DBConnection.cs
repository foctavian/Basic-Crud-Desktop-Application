using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MySql.Data.MySqlClient;
using WpTema1.Persistance.Service;

namespace WpTema1.Database;

public sealed class DBConnection
{
    private static readonly Lazy<DBConnection> _instance = new Lazy<DBConnection>(() => new DBConnection());
    public static DBConnection Instance
    {
        get { return _instance.Value; }
    }

    private MySqlConnection _connection;
    private string _connectionString = "Server=localhost;Port=3306;Uid=root;Pwd=admin";
    private string _initializationString = "CREATE DATABASE IF NOT EXISTS SHOP_DB;USE SHOP_DB";
    private string _tableCreationString = "CREATE TABLE IF NOT EXISTS ";
    private string _tableInsertionString = "INSERT INTO ";
    
    private DBConnection()
    {
        this._connection = new MySqlConnection(_connectionString);
    }

    public MySqlConnection GetConnection()
    {
        return _connection;
    }

    public void UseConnection()
    {
        OpenConnection();
        MySqlCommand cmd = new MySqlCommand("USE SHOP_DB;", _connection);
        try
        {
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public void OpenConnection() 
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
        
    }

    public void CloseConnection()
    {
        if(_connection.State != ConnectionState.Closed){_connection.Close();}
    }

    public void CreateDatabase()
    {
        OpenConnection();
        MySqlCommand cmd = new MySqlCommand(_initializationString, _connection);
        try
        {
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public void CreateTable<T>(string name)
    {
        OpenConnection();
        string s = TableCreationStringBuilder<T>();
        MySqlCommand cmd = new MySqlCommand(s, _connection);
        try
        {
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    private string TableCreationStringBuilder<T>()
    {
        string name = typeof(T).Name;
        var data = DataDao<T>.GetPropertyInfo();
        StringBuilder s = new StringBuilder(_tableCreationString);
        s.Append(name).Append("(");
        for (uint i = 0; i < data.types.Length; i++)
        {
            s.Append(data.columnNames[i]).Append(" ");
            Console.WriteLine(data.types[i]);
            switch (data.types[i])
            {
                case "String":
                    s.Append("varchar(255)");
                    break;
                case "Int32":
                    s.Append("int");
                    break;
                case "Boolean":
                    s.Append("boolean");
                    break;
            }

            if (i == 0) s.Append(" AUTO_INCREMENT PRIMARY KEY ");
            if (i != data.types.Length-1) s.Append(", ");
        }

        s.Append(");");
        return s.ToString();
    }
    
    public void InsertIntoTable<T>(T obj)
    {
        OpenConnection();

        string nameOfTable = GetTableName<T>();

        // Build parameterized query using reflection
        StringBuilder s = new StringBuilder(_tableInsertionString);
        s.Append(nameOfTable);
        s.Append(" (");

        List<string> parameterNames = new List<string>();
        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            s.Append(property.Name).Append(", ");
            parameterNames.Add("@" + property.Name);
        }
        s.Remove(s.Length - 2, 2); // Remove trailing comma and space
        s.Append(") VALUES (");

        for (int i = 0; i < parameterNames.Count; i++)
        {
            s.Append(parameterNames[i]);
            if (i != parameterNames.Count - 1)
            {
                s.Append(", ");
            }
        }
        s.Append(");");

        MySqlCommand command = new MySqlCommand(s.ToString(), _connection);
        
        for (int i = 0; i < parameterNames.Count; i++)
        {
            command.Parameters.AddWithValue(parameterNames[i], properties[i].GetValue(obj));
        }

        try
        {
            command.ExecuteNonQuery();
            CloseConnection();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public MySqlDataReader CallReader(string cmd)
    {
        MySqlCommand command = new MySqlCommand();
        command.CommandText = cmd.ToString();
        command.Connection = GetConnection();
        MySqlDataReader dataReader = command.ExecuteReader();
        return dataReader;
    }
    
    public MySqlDataReader CallReader(MySqlCommand cmd)
    {
        MySqlDataReader dataReader = cmd.ExecuteReader();
        return dataReader;
    }
    
    public MySqlCommand InsertIntoStatement(StringBuilder s, Dictionary<string, object> fields)
    {
        foreach (var key in fields.Keys)
        {
            s.Append(key).Append("=").Append("@").Append(key).Append(" AND ");
        }

        s.Remove(s.Length - 4, 4);
        s.Append(";");

        MySqlCommand cmd = new MySqlCommand(s.ToString(), _connection);
        foreach (var (key, value) in fields)
        {
            cmd.Parameters.AddWithValue(key, value);
        }
        
        Console.WriteLine(cmd.CommandText);
        return cmd;
    }
    
    private static string GetTableName<T>()
    {
        return typeof(T).Name.ToLower();
    }
    
    
}