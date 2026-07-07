namespace CineCode.Models.Statistics
{
    // 월별 시청 활동 통계에 표시할 모델
    public class MonthlyWatchActivityModel
    {
        // 월 표시 정보
        public string MonthText { get; set; } = "";
        public string ShortMonthText { get; set; } = "";

        // 월별 시청 횟수와 시간
        public int WatchCount { get; set; }
        public decimal TotalWatchHours { get; set; }

        // 시청 횟수 표시용 문자열
        public string WatchCountText
        {
            get
            {
                return WatchCount.ToString("N0") + "회";
            }
        }

        // 총 시청 시간을 시간 단위 문자열로 표시
        public string TotalWatchHoursText
        {
            get
            {
                return TotalWatchHours.ToString("N1") + "시간";
            }
        }
    }
}
