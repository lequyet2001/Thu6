
namespace HUST.Core.Models.Entity
{
    /// <summary>
    /// Bảng audit_log: Bảng chứa thông tin lịch sử truy cập
    /// </summary>

    public class audit_log 
    {
        /// <summary>
        /// Id khóa chính
        /// </summary>
       public int RowNumber { get; set; }
        
        public string audit_log_id { get; set; }

        /// <summary>
        /// Id người dùng
        /// </summary>
        public string user_id { get; set; }

        /// <summary>
        /// Thông tin màn hình/Tên màn hình
        /// </summary>
        public string screen_info { get; set; }

        /// <summary>
        /// Loại hành động
        /// </summary>
        public int? action_type { get; set; }

        /// <summary>
        /// Thông tin tham chiếu, vd: id dictionary đang thao tác
        /// </summary>
        public string reference { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Thông tin user agent
        /// </summary>
        public string user_agent { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_date { get;set; }
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }



    }
}
