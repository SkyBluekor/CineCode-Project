namespace CineCode.Model.DashBoard
{
    // 대시보드 상단 카드와 알림 영역에 표시할 집계 데이터 모델
    public class DashboardModel
    {
        // 전체 현황 카운트
        public int TotalContentCount { get; set; }
        public int TotalUserCount { get; set; }
        public int ActiveSubscriptionCount { get; set; }
        public int OttProviderCount { get; set; }

        // 관리자가 확인해야 하는 미연결/만료 임박 항목 카운트
        public int UnmappedGenreContentCount { get; set; }
        public int UnmappedOttContentCount { get; set; }
        public int ExpiringSubscriptionCount { get; set; }
    }
}
