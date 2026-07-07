namespace CineCode.Models.Statistics
{
    // 인기 콘텐츠 순위 목록에 표시할 모델
    public class TopContentRankingItemModel
    {
        // 순위와 콘텐츠 기본 정보
        public int Rank { get; set; }

        public int ContentId { get; set; }

        public string Title { get; set; } = "";

        public string ContentType { get; set; } = "";

        public string OttNames { get; set; } = "";

        public string GenreNames { get; set; } = "";

        // 시청/평점 통계 값
        public int WatchCount { get; set; }

        public int ViewerCount { get; set; }

        public decimal TotalWatchMinutes { get; set; }

        public decimal AverageRatingScore { get; set; }

        public decimal CompletionRate { get; set; }

        public string PosterUrl { get; set; } = "";

        // 콘텐츠 타입 코드를 화면 표시용 문구로 변환
        public string ContentTypeText
        {
            get
            {
                if (ContentType == "MOVIE")
                {
                    return "영화";
                }

                if (ContentType == "TV")
                {
                    return "TV";
                }

                return ContentType;
            }
        }

        // 시청 횟수 표시용 문자열
        public string WatchCountText
        {
            get
            {
                return WatchCount.ToString("N0") + "회";
            }
        }

        // 시청자 수 표시용 문자열
        public string ViewerCountText
        {
            get
            {
                return ViewerCount.ToString("N0") + "명";
            }
        }

        // 총 시청 시간을 분/시간 단위로 표시
        public string TotalWatchTimeText
        {
            get
            {
                if (TotalWatchMinutes <= 0)
                {
                    return "-";
                }

                if (TotalWatchMinutes < 60)
                {
                    return TotalWatchMinutes.ToString("0") + "분";
                }

                decimal hours = TotalWatchMinutes / 60;

                return hours.ToString("0.0") + "시간";
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

                return AverageRatingScore.ToString("0.0") + " / 5";
            }
        }

        // 완주율을 퍼센트 문자열로 표시
        public string CompletionRateText
        {
            get
            {
                return CompletionRate.ToString("0.0") + "%";
            }
        }
    }
}
