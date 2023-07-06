using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Thu6.model
{
    public class Dictionary
    {
        public string Dictionary_id { get; set; } = string.Empty;

        public string User_id { get; set; } = string.Empty;

        public string Dictionary_name { get; set; } = string.Empty;

        public DateTime Last_view_at { get; set; }

    }
}
