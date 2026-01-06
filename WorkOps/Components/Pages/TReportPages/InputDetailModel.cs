using System.ComponentModel.DataAnnotations;
using WorkOps.Models;

namespace WorkOps.Components.Pages.TReportPages
{
    public class InputDetailModel : BaseInputModel
    {
        /// <summary>
        /// インデックス番号（１～）
        /// </summary>
        public string IndexNo { get; set; } = string.Empty;

        /// <summary>
        /// プロジェクトID
        /// </summary>
        public int? MProjectId { get; set; }

        /// <summary>
        /// プロジェクト名
        /// </summary>
        [Display(Name = "プロジェクト")]
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// 作業内容
        /// </summary>
        [Display(Name = "作業内容")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 課題・問題
        /// </summary>
        [Display(Name = "課題・問題")]
        public string Problem { get; set; } = string.Empty;

        /// <summary>
        /// 今後の予定
        /// </summary>
        [Display(Name = "今後の予定")]
        public string Schedule { get; set; } = string.Empty;
    }
}
