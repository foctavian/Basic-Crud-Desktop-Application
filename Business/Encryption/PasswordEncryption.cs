using System.Text;

namespace WpTema1.Business.Encryption;

public static class PasswordEncryption
{
    public static string Encrypt(string password)
    {
        try
        {
            byte[] encodedDataByte = new byte[password.Length];
            encodedDataByte = System.Text.Encoding.UTF8.GetBytes(password);
            string encodedData = Convert.ToBase64String(encodedDataByte);
            return encodedData;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in encryption : " + ex.Message);
        }
    }

    public static string Decrypt(string encodedData)
    {
        UTF8Encoding encoder = new UTF8Encoding();
        Decoder utf8Decoder = encoder.GetDecoder();
        byte[] decodedDataByte = Convert.FromBase64String(encodedData);
        int charCount = utf8Decoder.GetCharCount(decodedDataByte, 0, decodedDataByte.Length);
        char[] decodedDataChar = new char[charCount];
        utf8Decoder.GetChars(decodedDataByte, 0, decodedDataByte.Length, decodedDataChar, 0);
        string result = new String(decodedDataChar);
        return result;
    }
}