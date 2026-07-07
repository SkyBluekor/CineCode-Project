namespace CineCode.Models.Statistics
{
    // 인기 콘텐츠 통계에서 사용할 OTT 필터 모델
    public class TopContentOttFilterItemModel
    {
        // null이면 전체 OTT를 의미
        public int? OttId { get; set; }

        public string OttName { get; set; } = "";

        // 전체 선택이면 고정 문구를 표시
        public string DisplayText
        {
            get
            {
                if (OttId == null)
                {
                    return "OTT 전체";
                }

                return OttName;
            }
        }
    }
}
