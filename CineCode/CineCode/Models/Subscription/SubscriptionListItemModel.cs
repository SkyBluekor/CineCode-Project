namespace CineCode.Models.Subscription
{
    // 구독 관리 목록에 표시할 구독 정보 모델
    public class SubscriptionListItemModel
    {
        // 구독과 연결된 사용자/OTT 기본 정보
        public int Subscription_ID { get; set; }
        public int User_ID { get; set; }
        public string User_Name { get; set; } = "";
        public int OTT_ID { get; set; }
        public string OTT_Name { get; set; } = "";

        // 결제 및 구독 기간 정보
        public int Monthly_Price { get; set; }
        public DateTime Started_At { get; set; }
        public DateTime Expired_At { get; set; }
        public int Paid_Amount { get; set; }

        // 시작일 표시용 문자열
        public string StartedAtText
        {
            get
            {
                return Started_At.ToString("yyyy-MM-dd");
            }
        }

        // 만료일 표시용 문자열
        public string ExpiredAtText
        {
            get
            {
                return Expired_At.ToString("yyyy-MM-dd");
            }
        }

        // 결제 금액 표시용 문자열
        public string PaidAmountText
        {
            get
            {
                return Paid_Amount.ToString("N0") + "원";
            }
        }

        // 오늘 기준 남은 구독 일수
        public int RemainingDays
        {
            get
            {
                return (Expired_At.Date - DateTime.Today).Days;
            }
        }

        // 남은 일수를 사용자가 읽기 쉬운 문구로 표시
        public string RemainingDaysText
        {
            get
            {
                if (RemainingDays < 0)
                {
                    return "만료됨";
                }

                if (RemainingDays == 0)
                {
                    return "오늘 만료";
                }

                return RemainingDays + "일 남음";
            }
        }

        // 구독이 아직 유효한지 여부
        public bool IsActive
        {
            get
            {
                return Expired_At.Date >= DateTime.Today;
            }
        }

        // 구독이 만료되었는지 여부
        public bool IsExpired
        {
            get
            {
                return Expired_At.Date < DateTime.Today;
            }
        }

        // 7일 이내 만료 예정인지 여부
        public bool IsExpiringSoon
        {
            get
            {
                return RemainingDays >= 0 && RemainingDays <= 7;
            }
        }

        // 구독 상태를 목록 표시용 문구로 변환
        public string SubscriptionStatus
        {
            get
            {
                if (Expired_At.Date < DateTime.Today)
                {
                    return "만료";
                }

                if (RemainingDays <= 7)
                {
                    return "만료 임박";
                }

                return "활성";
            }
        }

        // OTT 이름을 기준으로 아이콘 이미지 경로 생성
        public string IconPath
        {
            get
            {
                string iconname = OTT_Name.Trim().ToLowerInvariant();
                return $"/Images/OTTicon/{iconname}.png";
            }
        }
    }
}
