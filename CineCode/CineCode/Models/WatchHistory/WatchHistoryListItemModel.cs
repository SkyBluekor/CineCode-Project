namespace CineCode.Models.WatchHistory
{
    // 시청 기록 목록에 표시할 시청 정보 모델
    public class WatchHistoryListItemModel
    {
        // 시청 기록과 연결된 기본 ID
        public int Watch_ID { get; set; }
        public int User_ID { get; set; }

        public int Content_ID { get; set; }
        public int OTT_ID { get; set; }

        // 목록에 표시할 이름 정보
        public string User_Name { get; set; } = "";
        public string Content_Name { get; set; } = "";
        public string Content_Type { get; set; } = "";
        public string OTT_Name { get; set; } = "";

        // 시청 시작/종료 시간
        public DateTime WatchStartedAt { get; set; }
        public DateTime? WatchEndedAt { get; set; }

        // 시청 시간과 진행률 정보
        public int? WatchMinutes { get; set; }
        public int? Running_Time { get; set; }
        public decimal? ProgressPercentage { get; set; }

        // 완주 여부
        public bool? IsCompleted { get; set; }

        // 시청 시작 시간을 목록 표시용 문자열로 변환
        public string WatchStartedText
        {
            get
            {
                return WatchStartedAt.ToString("yyyy-MM-dd HH:mm");
            }
        }

        // 종료 시간이 없으면 '-'로 표시
        public string WatchEndedText
        {
            get
            {
                if (WatchEndedAt == null)
                {
                    return "-";
                }

                return WatchEndedAt.Value.ToString("yyyy-MM-dd HH:mm");
            }
        }

        // 실제 시청 시간을 분 단위 문자열로 표시
        public string WatchMinutesText
        {
            get
            {
                if (WatchMinutes == null)
                {
                    return "-";
                }

                return WatchMinutes.Value + "분";
            }
        }

        // 진행률을 퍼센트 문자열로 표시
        public string ProgressPercentText
        {
            get
            {
                if (ProgressPercentage == null)
                {
                    return "0%";
                }
                return ProgressPercentage.Value.ToString("0.#") + "%";
            }
        }

        // 종료/완주 여부에 따라 시청 상태 표시
        public string StatusText
        {
            get
            {
                if (WatchEndedAt == null)
                {
                    return "시청 중";
                }

                if (IsCompleted == true)
                {
                    return "완료";
                }

                return "중단";
            }
        }

        // 콘텐츠 전체 러닝타임을 분 단위 문자열로 표시
        public string RunningTimeText
        {
            get
            {
                if (Running_Time == null)
                {
                    return "-";
                }
                return Running_Time.Value + "분";
            }
        }
    }
}
