namespace CineCode.Models.Statistics
{
    // 통계 화면 상단 요약 카드에 표시할 모델
    public class StatisticsSummaryModel
    {
        // 현재 기간의 핵심 지표
        public int TotalUserCount { get; set; }
        public decimal TotalWatchHours { get; set; }
        public decimal AverageRatingScore { get; set; }
        public decimal ActiveSubscriptionRate { get; set; }

        // 이전 기간 대비 변화량
        public decimal TotalUserChangeRate { get; set; }
        public decimal WatchHourChangeRate { get; set; }
        public decimal AverageRatingChange { get; set; }
        public decimal ActiveSubscriptionRateChange { get; set; }
    }
}
