namespace CineCode.Models.Subscription
{
    // 구독 등록/수정 화면에서 선택할 사용자 정보 모델
    public class SubscriptionUserSelectItemModel
    {
        // 사용자 기본 정보
        public int User_ID { get; set; }
        public string User_Name { get; set; } = "";
        public DateTime User_BirthDate { get; set; }
        public string User_PhoneNumber { get; set; } = "";
        public string User_Sex { get; set; } = "";

        // 전화번호를 하이픈이 포함된 형태로 표시
        public string Phone_Text
        {
            get
            {
                string phone = User_PhoneNumber.Replace("-", "").Trim();

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

        // 생년월일 기준 현재 나이
        public int Age
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

        // 나이에 따른 사용자 구분 문구
        public string UserType
        {
            get
            {
                return Age >= 19 ? "성인" : "아동";
            }
        }

        // 콤보박스에 표시할 사용자 요약 문구
        public string DisplayText
        {
            get
            {
                return $"[{User_ID}] {User_Name} / {Phone_Text} / {UserType} ";
            }
        }
    }
}
