namespace CineCode.Models.Mapping
{
    // 콘텐츠별 장르/OTT 매핑 상태를 목록에 표시하기 위한 모델
    public class ContentMappingContentItemModel
    {
        // 콘텐츠 기본 정보
        public int Content_ID { get; set; }
        public string Content_Type { get; set; } = "";
        public string Title { get; set; } = "";

        // 연결된 매핑 수와 화면 표시 상태
        public int GenreCount { get; set; }
        public int OttCount { get; set; }
        public string MappingStatus { get; set; } = "";

        // 정렬 시 우선순위를 맞추기 위한 값
        public int MappingSortOrder { get; set; }
    }
}
