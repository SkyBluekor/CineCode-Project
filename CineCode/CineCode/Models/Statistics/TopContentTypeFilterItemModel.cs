namespace CineCode.Models.Statistics
{
    // 인기 콘텐츠 통계에서 사용할 콘텐츠 타입 필터 모델
    public class TopContentTypeFilterItemModel
    {
        // ALL, MOVIE, TV 같은 필터 값
        public string ContentType { get; set; } = "ALL";

        // 화면에 표시할 필터 이름
        public string DisplayText { get; set; } = "";
    }
}
