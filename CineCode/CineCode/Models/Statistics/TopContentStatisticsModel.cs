namespace CineCode.Models.Statistics
{
    // 인기 콘텐츠 통계 표에 표시할 요약 모델
    public class TopContentStatisticsModel
    {
        // 순위와 콘텐츠 기본 정보
        public int Rank { get; set; }
        public int ContentId { get; set; }
        public string Title { get; set; } = "";
        public string OttName { get; set; } = "";

        // 장르와 통계 값
        public string GenreNames { get; set; } = "";
        public int WatchCount { get; set; }
        public decimal TotalWatchHours { get; set; }
        public decimal AverageRatingScore { get; set; }

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

        // 평균 평점이 없으면 '-'로 표시
        public string AverageRatingScoreText
        {
            get
            {
                if (AverageRatingScore <= 0)
                {
                    return "-";
                }
                return AverageRatingScore.ToString("0.0");
            }
        }
    }
}
