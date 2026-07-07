namespace CineCode.Models.Mapping
{
    // OTT 플랫폼 삭제 처리 후 삭제된 관련 데이터 수를 담는 결과 모델
    public class OttProviderDeleteResultModel
    {
        public int OTT_ID { get; set; }

        // 시청 기록에서 삭제된 수
        public int Delete_WatchHistory_Count { get; set; }

        // 사용자 구독에서 삭제된 수
        public int Delete_UserSubscription_Count { get; set; }

        // 콘텐츠-OTT 연결에서 삭제된 수
        public int Delete_ContentOTTProvider_Count { get; set; }

        // OTT 플랫폼 테이블에서 삭제된 수
        public int Delete_OTTProvider_Count { get; set; }

        // 삭제 결과 안내 메시지
        public string Message { get; set; } = "";
    }
}
