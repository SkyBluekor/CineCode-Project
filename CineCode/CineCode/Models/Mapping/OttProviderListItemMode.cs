namespace CineCode.Models.Mapping
{
    // OTT 플랫폼 관리 목록에 표시할 플랫폼 정보 모델
    public class OttProviderListItemModel
    {
        // OTT 기본 정보
        public int OTT_ID { get; set; }

        public string OTT_NAME { get; set; } = "";

        public int monthly_price { get; set; }

        // 삭제/관리 시 참고할 연결 데이터 수
        public int LinkedContentCount { get; set; }

        public int SubscriptionCount { get; set; }

        public int WatchHistoryCount { get; set; }
    }
}
