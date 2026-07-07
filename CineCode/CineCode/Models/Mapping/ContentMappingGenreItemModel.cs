namespace CineCode.Models.Mapping
{
    // 콘텐츠와 장르 연결 여부를 선택 목록에 표시하기 위한 모델
    public class ContentMappingGenreItemModel
    {
        // 장르 기본 정보
        public int Genre_ID { get; set; }

        public int? TMDB_Genre_ID { get; set; }

        public string Genre_Name { get; set; } = "";

        public string Content_Type { get; set; } = "";

        // 현재 콘텐츠와 연결되어 있는지 여부
        public bool IsLinked { get; set; }

        // 연결 여부를 화면 표시용 문구로 변환
        public string LinkStatus
        {
            get
            {
                return IsLinked ? "연결됨" : "미연결";
            }
        }
    }
}
