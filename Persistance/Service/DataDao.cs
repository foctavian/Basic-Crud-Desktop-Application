using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;
using WpTema1.Database;
namespace WpTema1.Persistance.Service;

public class DataDao<T>
{
    private readonly Type type = typeof(T);
    private readonly string table = typeof(T).Name.ToLower();
    DBConnection _connection = DBConnection.Instance;

    //TODO : PRONE TO BUGS. TBF
    public static (string[] columnNames, string[] types) GetPropertyInfo()
    {
        PropertyInfo[] props = typeof(T).GetProperties();
        string[] columnNames = new string[props.Length];
        string[] types = new string[props.Length];
        for (uint i = 0; i < props.Length; i++)
        {
            columnNames[i] = props[i].Name.ToLower();
            types[i] = props[i].PropertyType.Name;
        }

        return (columnNames, types);
    }

    public string[] GetInstanceData(T obj)
    {
        var data = GetPropertyInfo();
        PropertyInfo[] props = type.GetProperties();
        string[] instanceData = new string[props.Length];
        for (uint i = 1; i < props.Length; i++)
        {
            instanceData[i] = props[i].GetMethod.Invoke(obj, null).ToString();
        }

        return instanceData;
    }

    public List<T>? FindAll(params string[] fields)
    {
        try
        {
            _connection.OpenConnection();
            StringBuilder cmdText = new StringBuilder("SELECT ");
            if (CheckRequiredFields( fields)) //check if all the fields are requested
            {
                cmdText.Append("* FROM ").Append(table).Append(";");
            }
            else
            {
                foreach (string field in fields) //insert only the requested fields of the table
                {
                    cmdText.Append(field.ToLower()).Append(", ");
                }

                cmdText.Remove(cmdText.Length - 2, 2);
                cmdText.Append(" FROM ").Append(table.ToLower()).Append(";");
            }

            MySqlDataReader reader = _connection.CallReader(cmdText.ToString());
            return CreateObjects(reader);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }

    public T? FindById(int id)
    {
        try
        {
            _connection.OpenConnection();
            StringBuilder s = new StringBuilder($"SELECT * FROM {table} WHERE {table}id={id}");
            MySqlDataReader reader = _connection.CallReader(s.ToString());
            List<T>? objects = CreateObjects(reader);
            _connection.CloseConnection();
            if (objects != null) return objects.FirstOrDefault();
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return default(T?);
    }

    public List<T>? FindUsingFields(Dictionary<string, object> fields)
    {
        try
        {
            _connection.OpenConnection();
            StringBuilder s = new StringBuilder($"SELECT * FROM {table} WHERE ");
            MySqlDataReader reader = _connection.CallReader(_connection.InsertIntoStatement(s, fields));
            return CreateObjects(reader);
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }

    public bool DeleteById(int id)
    {
        try
        {
            _connection.OpenConnection();
            StringBuilder s = new StringBuilder($"DELETE FROM {table}");

        }
    }

    // print the result of a read for debug purposes
    private void PrintResult(MySqlDataReader reader)
    {
        if (reader.HasRows)
        {
            int count = reader.FieldCount;
            while (reader.Read())
            {
                for (var i = 0; i < count; i++)
                {
                    Console.WriteLine(reader.GetValue(i));
                }
            }
        }
    }

    private List<T>? CreateObjects(MySqlDataReader reader) //TODO SIMPLIFY
    {
        if (!reader.HasRows)
        {
            return null; 
        }
        List<T> list = new List<T>();

        var properties = typeof(T).GetProperties();

        while (reader.Read())
        {
            T instance = Activator.CreateInstance<T>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                if (property != null && property.CanWrite)
                {
                    var value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
            list.Add(instance);
        }

        return list;
    }
    
    private bool CheckRequiredFields( string[] fields)
    {
        if (type.GetProperties().Length == fields.Length) return true;
        return false;
    }

 
}