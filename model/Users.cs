using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Thu6.model
{
    public class Users
    {
        
        public string User_id { get; set; } = string.Empty;

        public string User_name { get; set; }= string.Empty;
        [Required, MinLength(8), PasswordPropertyText]
        public string Password { get; set; } = string.Empty;

        public string Full_name { get;set; } = string.Empty;

        public string Display_name { get; set; }=string.Empty;
        public DateTime Birthday { get; set; }

        public string Position { get; set; } = string.Empty;

        public string Avatar  { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int Status { get; set; } 

        public string DictionaryId { get;set; } = string.Empty;
        public string DictionaryName { get; set; } = string.Empty;

        public DateTime Created_at { get; set; }

        public DateTime Modified_at { get; set;}
    }
}
