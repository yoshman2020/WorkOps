using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Models;

namespace WorkOps.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 氏名
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 勤務時間ID
        /// </summary>
        [ForeignKey(nameof(MWorkTime))]
        public int? WorkTimeId { get; set; }

        /// <summary>
        /// 勤務時間
        /// </summary>
        public MWorkTime? MWorkTime { get; set; }

        /// <summary>
        /// 削除済み
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 前日・先週の未入力を通知する
        /// </summary>
        /// <remarks>true: 通知する / false: 通知しない</remarks>
        public bool IsPrevReminderEnabled { get; set; } = false;
    }

}
