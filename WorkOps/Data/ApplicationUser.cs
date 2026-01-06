using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using WorkOps.Models;

namespace WorkOps.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// –¼
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// ‹Î–±ŠÔID
        /// </summary>
        [ForeignKey(nameof(MWorkTime))]
        public int? WorkTimeId { get; set; }

        /// <summary>
        /// ‹Î–±ŠÔ
        /// </summary>
        public MWorkTime? MWorkTime { get; set; }

        /// <summary>
        /// íœÏ‚İ
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }

}
