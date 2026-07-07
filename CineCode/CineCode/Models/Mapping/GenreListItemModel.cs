namespace CineCode.Models.Mapping
{
    // 장르 관리 목록에 표시할 장르 정보 모델
    public class GenreListItemModel
    {
        // 장르 기본 정보
        public int Genre_ID { get; set; }
        public int? TMDB_Genre_ID { get; set; }
        public string Content_Type { get; set; } = "";
        public string Genre_Name { get; set; } = "";
        // 이 장르와 연결된 콘텐츠 수
        public int LinkedContentCount { get; set; }
    }
}
