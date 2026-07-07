namespace CineCode.Models.Subscription
{
    // 구독 등록/수정 화면에서 선택할 OTT 플랫폼 모델
    public class SubscriptionOttPorviderSelectedItemModel
    {
        // OTT 기본 정보
        public int OTT_ID { get; set; }
        public string OTT_Name { get; set; } = "";
        public int Monthly_Price { get; set; }

        // 월 요금을 원화 문자열로 표시
        public string PriceText
        {
            get
            {
                return Monthly_Price.ToString("N0") + "원";
            }
        }
    }
}
