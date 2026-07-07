using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CineCode.Models.Content
{
    // 콘텐츠 목록과 편집 화면에서 사용하는 콘텐츠 표시 모델
    public class ContentListItemModel : INotifyPropertyChanged
    {
        // 화면 갱신이 필요한 값은 backing field로 관리
        private string _title = "";
        private string _releaseDate = "";
        private string _posterUrl = "";
        private string _overview = "";
        private int? _runningTime = null;

        // 바인딩된 UI에 값 변경을 알려주는 이벤트
        public event PropertyChangedEventHandler? PropertyChanged;

        // 콘텐츠 기본 정보
        public int ContentID { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value ?? "";
                OnPropertyChanged();
            }
        }

        public string Type { get; set; } = "";

        public string ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                _releaseDate = value ?? "";
                OnPropertyChanged();
            }
        }

        // 화면에 바로 표시할 부가 정보
        public string TmdbScore { get; set; } = "";

        public string AgeRating { get; set; } = "";

        public int OttCount { get; set; }

        public string PosterUrl
        {
            get { return _posterUrl; }
            set
            {
                _posterUrl = value ?? "";
                OnPropertyChanged();
            }
        }

        public string Overview
        {
            get { return _overview; }
            set
            {
                _overview = value ?? "";
                OnPropertyChanged();
                OnPropertyChanged(nameof(OverviewPreview));
            }
        }

        public int? RunningTime
        {
            get { return _runningTime; }
            set
            {
                _runningTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RunningTimeText));
                OnPropertyChanged(nameof(RunningTimeEditText));
            }
        }

        // 상영 시간을 목록 표시용 문자열로 변환
        public string RunningTimeText
        {
            get
            {
                return RunningTime.HasValue ? $"{RunningTime.Value}분" : "";
            }
        }

        // DataGrid 수정용. "120분" 대신 숫자 120만 편집하게 함.
        public string RunningTimeEditText
        {
            get
            {
                return RunningTime.HasValue ? RunningTime.Value.ToString() : "";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    RunningTime = null;
                    return;
                }

                if (int.TryParse(value.Trim(), out int result))
                {
                    RunningTime = result;
                    return;
                }

                RunningTime = null;
            }
        }

        // 긴 줄거리를 목록에서 짧게 보여주기 위한 미리보기
        public string OverviewPreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Overview))
                {
                    return "";
                }

                if (Overview.Length <= 45)
                {
                    return Overview;
                }

                return Overview.Substring(0, 45) + "...";
            }
        }

        // 속성 변경을 UI 바인딩에 전달
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}
