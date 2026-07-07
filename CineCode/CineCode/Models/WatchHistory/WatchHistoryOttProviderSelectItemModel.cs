namespace CineCode.Models.WatchHistory
{
    // 시청 기록 등록/수정 화면에서 선택할 OTT 플랫폼 모델
    public class WatchHistoryOttProviderSelectItemModel
    {
        // OTT 기본 정보
        public int OTT_ID { get; set; }

        public string OTT_Name { get; set; } = "";
    }
}
