namespace CineCode.Models.Tmdb
{
    // TMDB 검색 결과 목록에 표시할 콘텐츠 모델
    public class TmdbSearchResultModel
    {
        // TMDB 콘텐츠 식별 정보
        public int TmdbId { get; set; }

        // MOVIE 또는 TV
        public string ContentType { get; set; } = "";

        // 검색 결과 기본 정보
        public string Title { get; set; } = "";

        public string ReleaseDate { get; set; } = "";

        public decimal TmdbAverageRating { get; set; }

        public string Overview { get; set; } = "";

        public string PosterPath { get; set; } = "";

        public string PosterUrl { get; set; } = "";

        // TMDB 장르 ID 목록
        public List<int> GenreIds { get; set; } = new List<int>();

        // 장르 ID 목록을 DB 저장/표시용 문자열로 변환
        public string GenreIdsText
        {
            get
            {
                if (GenreIds == null || GenreIds.Count == 0)
                {
                    return "";
                }

                return string.Join(",", GenreIds);
            }
        }

        // 제목과 공개일을 조합해 목록 표시 문구 생성
        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ReleaseDate))
                {
                    return Title;
                }

                return $"{Title} ({ReleaseDate})";
            }
        }

        // TMDB 평점을 화면 표시용 문자열로 변환
        public string DisplayScore
        {
            get
            {
                return $"TMDB {TmdbAverageRating:N1}";
            }
        }
    }
}
