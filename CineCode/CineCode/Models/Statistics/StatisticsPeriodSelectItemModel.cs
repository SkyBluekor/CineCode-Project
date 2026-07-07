namespace CineCode.Models.Statistics
{
    // 통계 조회 기간 선택용 모델
    public class StatisticsPeriodSelectItemModel
    {
        // 화면에 표시할 기간 이름
        public string DisplayText { get; set; } = "";

        // 조회 시작일과 종료일
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // 전체 기간 선택 여부
        public bool IsAllPeriod { get; set; }
    }
}
