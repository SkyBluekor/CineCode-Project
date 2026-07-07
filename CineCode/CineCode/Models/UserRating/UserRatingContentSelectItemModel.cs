namespace CineCode.Models.UserRating
{
    // 사용자 평점 등록/수정 화면에서 선택할 콘텐츠 모델
    public class UserRatingContentSelectItemModel
    {
        // 콘텐츠 기본 정보
        public int Content_ID { get; set; }

        public string Title { get; set; } = "";

        public string Content_Type { get; set; } = "";

        public decimal? Progress_Percentage { get; set; }

        public DateTime? WatchStartedAt { get; set; }

        public DateTime? WatchEndedAt { get; set; }

        // 콤보박스에 표시할 콘텐츠 요약 문구
        public string DisplayText
        {
            get
            {
                return $"{Content_ID} - {Title}";
            }
        }

        // 시청 진행률을 퍼센트 문자열로 표시
        public string ProgressPercentText
        {
            get
            {
                if (Progress_Percentage == null)
                {
                    return "-";
                }

                return Progress_Percentage.Value.ToString("0.#") + "%";
            }
        }

        // 시청 시작/종료 기간을 날짜 범위로 표시
        public string WatchPeriodText
        {
            get
            {
                if (WatchStartedAt == null || WatchEndedAt == null)
                {
                    return "-";
                }

                return $"{WatchStartedAt.Value:yyyy-MM-dd} ~ {WatchEndedAt.Value:yyyy-MM-dd}";
            }
        }
    }
}
