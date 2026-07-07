namespace CineCode.Models.Statistics
{
    // 장르별 인기 통계 목록에 표시할 모델
    public class GenrePopularityModel
    {
        // 순위와 장르 기본 정보
        public int Rank { get; set; }

        public string GenreName { get; set; } = "";

        // 시청 수와 점유율
        public int WatchCount { get; set; }

        public decimal ShareRate { get; set; }

        // 막대 그래프 표시 비율
        public double BarPercent { get; set; }

        // 점유율을 퍼센트 문자열로 표시
        public string ShareRateText
        {
            get
            {
                return ShareRate.ToString("0.0") + "%";
            }
        }
    }
}
