using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

public class Helper
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
    public static bool IsPasswordValid(string password)
    {
        // Định nghĩa quy tắc định dạng mật khẩu
        string pattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$";

        // Kiểm tra khớp mật khẩu với quy tắc
        return Regex.IsMatch(password, pattern);
    }
    
}
