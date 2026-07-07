namespace CineCode.Models.WatchHistory
{
    // 시청 기록 등록/수정 화면에서 선택할 콘텐츠 모델
    public class WatchHistoryContentSelectItemModel
    {
        // 콘텐츠 기본 정보
        public int Content_ID { get; set; }

        public string Title { get; set; } = "";

        public string Content_Type { get; set; } = "";

        public int? Running_Time { get; set; }

        // 콤보박스에 표시할 콘텐츠 요약 문구
        public string DisplayText
        {
            get
            {
                return $"{Content_ID} - {Title}";
            }
        }
    }
}
