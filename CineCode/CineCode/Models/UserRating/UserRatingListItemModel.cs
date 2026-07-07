namespace CineCode.Models.UserRating
{
    // 사용자 평점 목록에 표시할 평점 정보 모델
    public class UserRatingListItemModel
    {
        // 평점과 연결된 사용자/콘텐츠 ID
        public int User_ID { get; set; }
        public int Content_ID { get; set; }

        // 목록에 표시할 이름 정보
        public string User_Name { get; set; } = "";
        public string Content_Name { get; set; } = "";
        public string Content_Type { get; set; } = "";

        // 평점 점수
        public decimal Rating_Score { get; set; }

        // 시청 진행률
        public decimal? Progress_Percentage { get; set; }

        // 평점 등록일
        public DateTime Rated_At { get; set; }

        // 평점 점수를 화면 표시용 문자열로 변환
        public string RatingScoreText
        {
            get
            {
                return Rating_Score.ToString("0.#");
            }
        }

        // 진행률이 없으면 '-'로 표시
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

        // 평점 등록일을 날짜 문자열로 표시
        public string RatedAtText
        {
            get
            {
                return Rated_At.ToString("yyyy-MM-dd");
            }
        }
    }
}
