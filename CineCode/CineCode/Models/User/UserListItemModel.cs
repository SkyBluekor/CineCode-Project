namespace CineCode.Models.User
{
    // 사용자 관리 목록에 표시할 사용자 정보 모델
    public class UserListItemModel
    {
        // 사용자 기본 정보
        public int User_ID { get; set; }
        public string User_Name { get; set; } = "";
        public DateTime User_BirthDate { get; set; }
        public string User_PhoneNumber { get; set; } = "";

        public string User_Sex { get; set; } = "";

        // 구독 요약 정보
        public int SubscriptionCount { get; set; } = 0;

        public string ExpireDateText { get; set; } = "-";

        public string StatusText { get; set; } = "-";

        // 생년월일을 목록 표시용 날짜 문자열로 변환
        public string BirthDateText
        {
            get
            {
                return User_BirthDate.ToString("yyyy-MM-dd");
            }
        }

        // 생년월일을 기준으로 현재 나이를 계산
        public int age
        {
            get
            {
                DateTime today = DateTime.Today;
                int age = today.Year - User_BirthDate.Year;

                if (User_BirthDate.Date > today.AddYears(-age))
                {
                    age--;
                }
                return age;
            }
        }

        // 나이에 따라 사용자 구분 문구를 표시
        public string UserType
        {
            get
            {
                return age >= 19 ? "성인" : "아동";
            }
        }

        // 전화번호를 하이픈이 포함된 형태로 표시
        public string PhoneText
        {
            get
            {
                string phone = User_PhoneNumber ?? "";

                phone = phone.Replace("-", "").Trim();

                if (phone.Length == 11)
                {
                    return $"{phone.Substring(0, 3)}-{phone.Substring(3, 4)}-{phone.Substring(7, 4)}";
                }

                if (phone.Length == 10)
                {
                    return $"{phone.Substring(0, 3)}-{phone.Substring(3, 3)}-{phone.Substring(6, 4)}";
                }

                return phone;
            }
        }

        // 가장 최근 구독 만료일
        public DateTime? LatestExpiredAt { get; set; }

        // 만료일이 없으면 '-'로 표시
        public string LatestExpiredAtText
        {
            get
            {
                return LatestExpiredAt.HasValue
                    ? LatestExpiredAt.Value.ToString("yyyy-MM-dd")
                    : "-";
            }
        }

        // 구독 만료일 기준 사용자 상태 표시
        public string UserStatus
        {
            get
            {
                if (!LatestExpiredAt.HasValue)
                {
                    return "미구독";
                }

                if (LatestExpiredAt.Value.Date >= DateTime.Today)
                {
                    return "활성";
                }

                return "만료";
            }
        }
    }
}
