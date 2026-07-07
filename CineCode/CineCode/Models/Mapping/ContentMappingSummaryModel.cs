namespace CineCode.Models.Mapping
{
    // 매핑 관리 화면 상단에 보여줄 요약 카운트 모델
    public class ContentMappingSummaryModel
    {
        // 등록된 장르 수
        public int RegisteredGenreCount { get; set; }

        // 등록된 OTT 플랫폼 수
        public int OttProviderCount { get; set; }

        // 아직 장르/OTT가 연결되지 않은 콘텐츠 수
        public int UnmappedContentCount { get; set; }
    }
}
