namespace CineCode.Models.Mapping
{
    // 장르 삭제 처리 후 삭제된 관련 데이터 수를 담는 결과 모델
    public class GenreDeleteResultModel
    {
        public int Genre_ID { get; set; }

        // 콘텐츠-장르 연결 테이블에서 삭제된 수
        public int Delete_ContentGenre_Count { get; set; }

        // 장르 테이블에서 삭제된 수
        public int Delete_Genre_Count { get; set; }

        // 삭제 결과 안내 메시지
        public string Message { get; set; } = "";
    }
}
