using System.ComponentModel.DataAnnotations;

namespace Thu6.model
{
    public class SaveLogModel
    {
        [Required]
        public string screen_info { get; set; }

        /// <summary>
        /// Loại hành động
        /// </summary> 
       [Required]
        public int? action_type { get; set; }

        /// <summary>
        /// Thông tin tham chiếu, vd: id dictionary đang thao tác
        /// </summary>
        public string reference { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary> 
         [Required]
        public string description { get; set; }
    }
}
