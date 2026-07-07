namespace CineCode.Models.Tmdb
{
    // TMDB 장르 API 응답을 담는 모델
    public class TmdbGenreModel
    {
        // TMDB API 원본 필드명에 맞춘 장르 정보
        public int id { get; set; }
        public string name { get; set; } = "";

        // 영화/TV 구분
        public string ContentType { get; set; } = "";
    }
}
