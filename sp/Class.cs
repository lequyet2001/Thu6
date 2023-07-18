using System;

public class Class
{
    public static void Main()
    {

        CryptoHelper a=new CryptoHelper();
        // Tạo một khóa ngẫu nhiên
        string key = a.GenerateRandomId();

        // Chuỗi gốc
        string plaintext = "Hello, World!";

        // Mã hóa chuỗi
        string encryptedText = a.Encrypt(plaintext, key);

        // Giải mã chuỗi
        string decryptedText = a.Decrypt(encryptedText, key);

        // In kết quả
        Console.WriteLine("Chuỗi gốc: " + plaintext);
        Console.WriteLine("Chuỗi đã mã hóa: " + encryptedText);
        Console.WriteLine("Chuỗi đã giải mã: " + decryptedText);
    }
}
