using CineCode.Models.Statistics;
using CineCode.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CineCode.Views
{
    public partial class StatisticsView : UserControl
    {
        private readonly StatisticsService _statisticsService = new StatisticsService();
        private bool _isLoadingStatistics = false;
        private bool _isLoadingTopContentFilters = false;
        private DateTime _topContentRankingStartDate;
        private DateTime _topContentRankingEndDate;
        private string _topContentRankingPeriodText = "전체 기간";

        // 통계 화면을 초기화하고 기본 화면을 표시합니다.
        public StatisticsView()
        {
            InitializeComponent();

            Loaded += StatisticsView_Loaded;
            ShowStatisticsMainPanel();
        }

        // 화면이 로드되면 기본 필터와 통계 데이터를 조회합니다.
        private void StatisticsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatisticsDefaultView();
        }

        // 기본 필터 선택값을 세팅한 뒤 메인 통계 영역을 한 번에 갱신합니다.
        private void LoadStatisticsDefaultView()
        {
            // ItemsSource와 SelectedIndex 설정 중 발생하는 SelectionChanged를 무시하기 위한 플래그입니다.
            _isLoadingStatistics = true;

            LoadAvailablePeriods();

            StatisticsPeriodComboBox.SelectedIndex = 0;
            StatisticsContentTypeComboBox.SelectedIndex = 0;

            _isLoadingStatistics = false;

            LoadStatisticsSummary();
            LoadMonthlyWatchActivity();
            LoadGenrePopularity();
            LoadOttPlatformShare();
            LoadTopContents();
        }

        // 조회 가능한 기간 목록을 기간 필터에 연결합니다.
        private void LoadAvailablePeriods()
        {
            List<StatisticsPeriodSelectItemModel> periods =
                _statisticsService.GetAvailablePeriods();

            StatisticsPeriodComboBox.DisplayMemberPath = "DisplayText";
            StatisticsPeriodComboBox.ItemsSource = periods;
        }

        // 기간 또는 콘텐츠 유형 필터가 바뀌면 메인 통계 데이터를 다시 조회합니다.
        private void StatisticsFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingStatistics)
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            LoadStatisticsSummary();
            LoadMonthlyWatchActivity();
            LoadGenrePopularity();
            LoadTopContents();
        }

        // 상단 요약 카드의 통계값과 비교 문구를 갱신합니다.
        private void LoadStatisticsSummary()
        {
            StatisticsPeriodSelectItemModel? selectedPeriod =
                GetSelectedPeriod();

            if (selectedPeriod == null)
            {
                return;
            }

            string selectedContentType =
                GetSelectedContentType();

            string selectedCompareType = GetSelectedCompareType();

            StatisticsSummaryModel summary =
                _statisticsService.GetStatisticsSummary(
                    selectedPeriod.StartDate,
                    selectedPeriod.EndDate,
                    selectedContentType,
                    selectedPeriod.IsAllPeriod,
                    selectedCompareType
                );

            TotalUserCountText.Text =
                summary.TotalUserCount.ToString("N0");

            TotalWatchTimeText.Text =
                summary.TotalWatchHours.ToString("N1") + " 시간";

            AverageRatingText.Text =
                summary.AverageRatingScore.ToString("0.0") + " / 5";

            ActiveSubscriptionRateText.Text =
                summary.ActiveSubscriptionRate.ToString("0.0") + "%";

            if (selectedPeriod.IsAllPeriod)
            {
                TotalUserCompareText.Text = "전체 기간";
                TotalWatchTimeCompareText.Text = "전체 기간";
                AverageRatingCompareText.Text = "전체 기간";
                ActiveSubscriptionCompareText.Text = "전체 기간";
            }
            else
            {
                TotalUserCompareText.Text =
                    selectedCompareType + " " + FormatPercentChange(summary.TotalUserChangeRate);

                TotalWatchTimeCompareText.Text =
                    selectedCompareType + " " + FormatPercentChange(summary.WatchHourChangeRate);

                AverageRatingCompareText.Text =
                    selectedCompareType + " " + FormatScoreChange(summary.AverageRatingChange);

                ActiveSubscriptionCompareText.Text =
                    selectedCompareType + " " + FormatPercentChange(summary.ActiveSubscriptionRateChange);
            }
        }

        // 월별 시청 활동 데이터를 조회하고 라인 차트와 월 라벨에 반영합니다.
        private void LoadMonthlyWatchActivity()
        {
            string selectedContentType = GetSelectedContentType();

            List<MonthlyWatchActivityModel> monthlyActivities =
                _statisticsService.GetMonthlyWatchActivity(
                    selectedContentType,
                    6
                );

            DrawMonthlyWatchActivityChart(monthlyActivities);
            MonthlyWatchActivityMonthLabelsItemsControl.ItemsSource = monthlyActivities;
        }

        // 선택 기간과 콘텐츠 유형 기준으로 인기 장르 TOP 5를 조회합니다.
        private void LoadGenrePopularity()
        {
            StatisticsPeriodSelectItemModel? selectedPeriod =
                GetSelectedPeriod();

            if (selectedPeriod == null)
            {
                return;
            }

            string selectedContentType =
                GetSelectedContentType();

            List<GenrePopularityModel> genres =
                _statisticsService.GetGenrePopularity(
                    selectedPeriod.StartDate,
                    selectedPeriod.EndDate,
                    selectedContentType,
                    5
                );

            GenrePopularityItemsControl.ItemsSource = genres;
        }

        // OTT별 시청 비중을 조회하고 범례, 총 시청 시간, 도넛 차트를 갱신합니다.
        private void LoadOttPlatformShare()
        {
            List<OttPlatformShareModel> ottShares =
                _statisticsService.GetOttPlatformShare(7);

            OttPlatformShareLegendItemsControl.ItemsSource =
                ottShares;

            decimal totalWatchMinutes =
                ottShares.Sum(item => item.TotalWatchMinutes);

            decimal totalWatchHours =
                Math.Round(totalWatchMinutes / 60, 1);

            OttTotalWatchHoursText.Text =
                totalWatchHours.ToString("N1");

            DrawOttPlatformShareDonut(ottShares);
        }

        // 선택 기간 기준 TOP 5 콘텐츠 목록을 조회합니다.
        private void LoadTopContents()
        {
            StatisticsPeriodSelectItemModel? selectedPeriod =
                GetSelectedPeriod();

            if (selectedPeriod == null)
            {
                return;
            }

            List<TopContentStatisticsModel> topContents =
                _statisticsService.GetTopContents(
                    selectedPeriod.StartDate,
                    selectedPeriod.EndDate,
                    5
                );

            TopContentItemsControl.ItemsSource =
                topContents;
        }

        // 월별 시청 시간을 차트 높이에 맞춰 정규화한 뒤 Polyline 점으로 변환합니다.
        private void DrawMonthlyWatchActivityChart(List<MonthlyWatchActivityModel> monthlyActivities)
        {
            MonthlyWatchActivityPolyline.Points.Clear();

            if (monthlyActivities == null || monthlyActivities.Count == 0)
            {
                return;
            }

            double chartWidth = 520;
            double chartHeight = 120;

            double startX = 40;
            double startY = 20;

            int pointCount = monthlyActivities.Count;

            decimal maxWatchHours =
                monthlyActivities.Max(item => item.TotalWatchHours);

            if (maxWatchHours <= 0)
            {
                maxWatchHours = 1;
            }

            for (int i = 0; i < pointCount; i++)
            {
                MonthlyWatchActivityModel item = monthlyActivities[i];

                double x;

                if (pointCount == 1)
                {
                    x = startX;
                }
                else
                {
                    x = startX + (chartWidth / (pointCount - 1)) * i;
                }

                double rate =
                    Convert.ToDouble(item.TotalWatchHours / maxWatchHours);

                double y =
                    startY + chartHeight - (chartHeight * rate);

                MonthlyWatchActivityPolyline.Points.Add(
                    new Point(x, y)
                );
            }
        }

        // OTT 점유율을 각도 값으로 변환해 도넛 차트 조각을 그립니다.
        private void DrawOttPlatformShareDonut(List<OttPlatformShareModel> ottShares)
        {
            OttPlatformShareDonutCanvas.Children.Clear();

            double centerX = 95;
            double centerY = 95;
            double outerRadius = 62;
            double innerRadius = 42;

            if (ottShares == null || ottShares.Count == 0)
            {
                Ellipse emptyEllipse =
                    new Ellipse
                    {
                        Width = outerRadius * 2,
                        Height = outerRadius * 2,
                        Stroke = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#E5E7EB")
                        ),
                        StrokeThickness = outerRadius - innerRadius
                    };

                Canvas.SetLeft(emptyEllipse, centerX - outerRadius);
                Canvas.SetTop(emptyEllipse, centerY - outerRadius);

                OttPlatformShareDonutCanvas.Children.Add(emptyEllipse);

                return;
            }

            double startAngle = -90;

            foreach (OttPlatformShareModel item in ottShares)
            {
                double sweepAngle =
                    Convert.ToDouble(item.ShareRate) / 100.0 * 360.0;

                if (sweepAngle <= 0)
                {
                    continue;
                }

                if (sweepAngle >= 360)
                {
                    sweepAngle = 359.99;
                }

                Path segment =
                    CreateDonutSegment(
                        centerX,
                        centerY,
                        outerRadius,
                        innerRadius,
                        startAngle,
                        sweepAngle,
                        item.ColorHex
                    );

                OttPlatformShareDonutCanvas.Children.Add(segment);

                startAngle += sweepAngle;
            }
        }

        // 바깥쪽 호와 안쪽 호를 연결해 하나의 도넛 조각 Path를 만듭니다.
        private Path CreateDonutSegment(
            double centerX,
            double centerY,
            double outerRadius,
            double innerRadius,
            double startAngle,
            double sweepAngle,
            string colorHex)
        {
            Point outerStart =
                GetCirclePoint(centerX, centerY, outerRadius, startAngle);

            Point outerEnd =
                GetCirclePoint(centerX, centerY, outerRadius, startAngle + sweepAngle);

            Point innerStart =
                GetCirclePoint(centerX, centerY, innerRadius, startAngle + sweepAngle);

            Point innerEnd =
                GetCirclePoint(centerX, centerY, innerRadius, startAngle);

            bool isLargeArc =
                sweepAngle > 180;

            PathFigure figure =
                new PathFigure
                {
                    StartPoint = outerStart,
                    IsClosed = true
                };

            figure.Segments.Add(
                new ArcSegment
                {
                    Point = outerEnd,
                    Size = new Size(outerRadius, outerRadius),
                    IsLargeArc = isLargeArc,
                    SweepDirection = SweepDirection.Clockwise
                }
            );

            figure.Segments.Add(
                new LineSegment
                {
                    Point = innerStart
                }
            );

            figure.Segments.Add(
                new ArcSegment
                {
                    Point = innerEnd,
                    Size = new Size(innerRadius, innerRadius),
                    IsLargeArc = isLargeArc,
                    SweepDirection = SweepDirection.Counterclockwise
                }
            );

            PathGeometry geometry =
                new PathGeometry();

            geometry.Figures.Add(figure);

            Path path =
                new Path
                {
                    Data = geometry,
                    Fill = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(colorHex)
                    )
                };

            return path;
        }

        // 원 중심, 반지름, 각도를 기준으로 원 둘레의 좌표를 계산합니다.
        private Point GetCirclePoint(
            double centerX,
            double centerY,
            double radius,
            double angle)
        {
            double radians =
                angle * Math.PI / 180.0;

            double x =
                centerX + radius * Math.Cos(radians);

            double y =
                centerY + radius * Math.Sin(radians);

            return new Point(x, y);
        }

        // 통계 메인 패널만 보이도록 화면 상태를 초기화합니다.
        private void ShowStatisticsMainPanel()
        {
            StatisticsMainPanel.Visibility = Visibility.Visible;
            TopContentFullPanel.Visibility = Visibility.Collapsed;
        }

        // TOP 콘텐츠 전체 순위 화면을 표시하고 현재 선택 기간을 전체 순위 조회 조건으로 고정합니다.
        private void ShowTopContentFullPanel()
        {
            StatisticsPeriodSelectItemModel? selectedPeriod =
                GetSelectedPeriod();

            if (selectedPeriod == null)
            {
                MessageBox.Show("조회 기간을 선택해주세요.");
                return;
            }

            _topContentRankingStartDate = selectedPeriod.StartDate;
            _topContentRankingEndDate = selectedPeriod.EndDate;
            _topContentRankingPeriodText = selectedPeriod.DisplayText;

            TopContentFullPanel.Background =
                new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#F8F7FC")
                );

            Panel.SetZIndex(TopContentFullPanel, 100);

            TopContentFullPanel.Visibility =
                Visibility.Visible;

            LoadTopContentFullFilters();
            LoadTopContentFullRanking();
        }

        // TOP 5 카드의 전체 순위 보기 버튼 이벤트입니다.
        private void ShowTopContentFullButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTopContentFullPanel();
        }

        // 전체 순위 화면을 닫고 메인 통계 화면으로 돌아갑니다.
        private void BackToStatisticsMainButton_Click(object sender, RoutedEventArgs e)
        {
            TopContentFullPanel.Visibility =
                Visibility.Collapsed;
        }

        // 전체 순위 화면의 필터 목록을 초기화합니다.
        private void LoadTopContentFullFilters()
        {
            // 필터 ItemsSource와 SelectedIndex 설정 중 발생하는 SelectionChanged를 무시합니다.
            _isLoadingTopContentFilters = true;

            LoadTopContentTypeFilter();
            LoadTopContentOttFilter();
            LoadTopContentGenreFilter("ALL");

            _isLoadingTopContentFilters = false;
        }

        // 전체 순위 화면의 콘텐츠 유형 필터를 조회합니다.
        private void LoadTopContentTypeFilter()
        {
            List<TopContentTypeFilterItemModel> typeItems =
                _statisticsService.GetTopContentTypeFilterItems();

            TopContentTypeFilterComboBox.ItemsSource = typeItems;

            if (typeItems.Count > 0)
            {
                TopContentTypeFilterComboBox.SelectedIndex = 0;
            }
        }

        // 전체 순위 화면의 OTT 필터를 조회합니다.
        private void LoadTopContentOttFilter()
        {
            List<TopContentOttFilterItemModel> ottItems =
                _statisticsService.GetTopContentOttFilterItems();

            TopContentOttFilterComboBox.ItemsSource = ottItems;

            if (ottItems.Count > 0)
            {
                TopContentOttFilterComboBox.SelectedIndex = 0;
            }
        }

        // 선택한 콘텐츠 유형에 맞는 장르 필터를 조회합니다.
        private void LoadTopContentGenreFilter(string contentType)
        {
            List<TopContentGenreFilterItemModel> genreItems =
                _statisticsService.GetTopContentGenreFilterItems(contentType);

            TopContentGenreFilterComboBox.ItemsSource = genreItems;

            if (genreItems.Count > 0)
            {
                TopContentGenreFilterComboBox.SelectedIndex = 0;
            }
        }

        // 전체 순위 화면의 필터 선택값 기준으로 콘텐츠 순위를 조회합니다.
        private void LoadTopContentFullRanking()
        {
            string keyword =
                TopContentSearchTextBox.Text?.Trim() ?? "";

            string selectedContentType =
                GetSelectedTopContentType();

            int? selectedOttId =
                GetSelectedTopContentOttId();

            int? selectedGenreId =
                GetSelectedTopContentGenreId();

            List<TopContentRankingItemModel> rankingItems =
                _statisticsService.GetTopContentRanking(
                    _topContentRankingStartDate,
                    _topContentRankingEndDate,
                    keyword,
                    selectedContentType,
                    selectedOttId,
                    selectedGenreId
                );

            TopContentFullDataGrid.ItemsSource =
                rankingItems;
        }

        // OTT 또는 장르 필터가 바뀌면 전체 순위 목록을 다시 조회합니다.
        private void TopContentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingTopContentFilters)
            {
                return;
            }

            LoadTopContentFullRanking();
        }

        // 콘텐츠 유형이 바뀌면 해당 유형에 맞는 장르 필터를 다시 불러온 뒤 전체 순위를 조회합니다.
        private void TopContentTypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingTopContentFilters)
            {
                return;
            }

            string selectedContentType =
                GetSelectedTopContentType();

            _isLoadingTopContentFilters = true;

            LoadTopContentGenreFilter(selectedContentType);

            _isLoadingTopContentFilters = false;

            LoadTopContentFullRanking();
        }

        // 검색어 입력 후 Enter를 누르면 전체 순위 목록을 다시 조회합니다.
        private void TopContentSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadTopContentFullRanking();
            }
        }

        // 메인 통계 화면의 콘텐츠 유형 필터 값을 반환합니다.
        private string GetSelectedContentType()
        {
            if (StatisticsContentTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString() ?? "전체";
            }

            return "전체";
        }

        // 메인 통계 화면의 기간 필터 값을 반환합니다.
        private StatisticsPeriodSelectItemModel? GetSelectedPeriod()
        {
            return StatisticsPeriodComboBox.SelectedItem as StatisticsPeriodSelectItemModel;
        }

        // 비교 기준 필터 값을 반환합니다.
        private string GetSelectedCompareType()
        {
            if (ComparePeriodComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString() ?? "전월 대비";
            }

            return "전월 대비";
        }

        // 전체 순위 화면의 콘텐츠 유형 필터 값을 반환합니다.
        private string GetSelectedTopContentType()
        {
            if (TopContentTypeFilterComboBox.SelectedItem is TopContentTypeFilterItemModel selectedItem)
            {
                return selectedItem.ContentType;
            }

            return "ALL";
        }

        // 전체 순위 화면의 OTT 필터 값을 반환합니다.
        private int? GetSelectedTopContentOttId()
        {
            if (TopContentOttFilterComboBox.SelectedItem is TopContentOttFilterItemModel selectedItem)
            {
                return selectedItem.OttId;
            }

            return null;
        }

        // 전체 순위 화면의 장르 필터 값을 반환합니다.
        private int? GetSelectedTopContentGenreId()
        {
            if (TopContentGenreFilterComboBox.SelectedItem is TopContentGenreFilterItemModel selectedItem)
            {
                return selectedItem.GenreId;
            }

            return null;
        }

        // 증감률에 양수 기호와 퍼센트 단위를 붙입니다.
        private string FormatPercentChange(decimal value)
        {
            if (value > 0)
            {
                return "+" + value.ToString("0.00") + "%";
            }

            return value.ToString("0.00") + "%";
        }

        // 평점 증감값에 양수 기호를 붙입니다.
        private string FormatScoreChange(decimal value)
        {
            if (value > 0)
            {
                return "+" + value.ToString("0.00");
            }

            return value.ToString("0.00");
        }
    }
}
