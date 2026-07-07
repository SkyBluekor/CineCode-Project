using System.Windows.Media;

namespace CineCode.Models.Statistics
{
    // OTT 플랫폼별 시청 점유율 통계 모델
    public class OttPlatformShareModel
    {
        // OTT 이름과 시청 데이터
        public string OttName { get; set; } = "";

        public int WatchCount { get; set; }

        public decimal TotalWatchMinutes { get; set; }

        // 점유율과 차트 색상
        public decimal ShareRate { get; set; }

        public string ColorHex { get; set; } = "#999999";

        // HEX 색상 문자열을 WPF Brush로 변환
        public Brush ColorBrush
        {
            get
            {
                return new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(ColorHex)
                );
            }
        }

        // 점유율을 퍼센트 문자열로 표시
        public string ShareRateText
        {
            get
            {
                return ShareRate.ToString("0.0") + "%";
            }
        }
    }
}
