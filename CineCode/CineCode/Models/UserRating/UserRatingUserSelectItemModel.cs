namespace CineCode.Models.UserRating
{
    // 사용자 평점 등록/수정 화면에서 선택할 사용자 모델
    public class UserRatingUserSelectItemModel
    {
        // 사용자 기본 정보
        public int User_ID { get; set; }

        public string User_Name { get; set; } = "";

        public string User_PhoneNumber { get; set; } = "";

        // 전화번호가 있으면 함께 표시하는 선택 문구
        public string DisplayText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(User_PhoneNumber))
                {
                    return $"{User_ID} - {User_Name}";
                }

                return $"{User_ID} - {User_Name} ({User_PhoneNumber})";
            }
        }
    }
}
