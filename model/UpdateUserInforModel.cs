using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace Thu6.model
{
    public class UpdateUserInforModel
    {




        [Required]
        [FromHeader]
        public string Token { get; set; } = string.Empty;
        public string Full_name { get; set; } = string.Empty;

        public string Display_name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }

        public string Position { get; set; } = string.Empty;

        public string Avatar { get; set; }=string.Empty;
      
    }
}
