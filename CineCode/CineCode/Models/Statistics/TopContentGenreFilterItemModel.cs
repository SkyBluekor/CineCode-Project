namespace CineCode.Models.Statistics
{
    // 인기 콘텐츠 통계에서 사용할 장르 필터 모델
    public class TopContentGenreFilterItemModel
    {
        // null이면 전체 장르를 의미
        public int? GenreId { get; set; }

        public string GenreName { get; set; } = "";

        public string ContentType { get; set; } = "";

        // 콘텐츠 타입과 장르명을 조합해 필터 문구 생성
        public string DisplayText
        {
            get
            {
                if (GenreId == null)
                {
                    return "장르 전체";
                }

                if (ContentType == "MOVIE")
                {
                    return "영화 - " + GenreName;
                }

                if (ContentType == "TV")
                {
                    return "TV - " + GenreName;
                }

                return GenreName;
            }
        }
    }
}
