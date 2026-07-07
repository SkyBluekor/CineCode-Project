namespace CineCode.Models.Tmdb
{
    // TMDB 상세 조회 결과에서 필요한 콘텐츠 상세 정보 모델
    public class TmdbContentDetailModel
    {
        // TMDB 콘텐츠 식별 정보
        public int TmdbId { get; set; }

        public string ContentType { get; set; } = "";

        // 상세 화면에서 가져오는 부가 정보
        public int? RunningTime { get; set; }

        public string AgeRating { get; set; } = "";
    }
}
