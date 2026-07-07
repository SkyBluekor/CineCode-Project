using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CineCode.Models.WatchHistory;
using CineCode.Services;

namespace CineCode.Views
{
    public partial class WatchHistoryView : UserControl
    {
        private int? _selectedStartUserId = null;
        private int? _selectedStartContentId = null;
        private int? _selectedStartOttId = null;

        private bool _isStartUserSearchUpdating = false;
        private bool _isStartContentSearchUpdating = false;

        private int? _selectedWatchId = null;
        private int _selectedRunningTime = 0;
        private int _selectedCurrentWatchMinutes = 0;
        private string _selectedWatchStatus = "";

        private DateTime? _selectedWatchStartedAt = null;
        private DateTime? _selectedWatchEndedAt = null;
        private List<WatchHistoryListItemModel> _watchHistories = new List<WatchHistoryListItemModel>();
        private List<WatchHistoryListItemModel> _filteredWatchHistories = new List<WatchHistoryListItemModel>();

        private readonly WatchHistoryService _watchHistoryService = new WatchHistoryService();

        private List<WatchHistoryUserSelectItemModel> _searchedStartUsers = new List<WatchHistoryUserSelectItemModel>();
        private List<WatchHistoryContentSelectItemModel> _searchedStartContents = new List<WatchHistoryContentSelectItemModel>();

        // 시청 기록 화면을 초기화하고 기본 목록 화면을 표시합니다.
        public WatchHistoryView()
        {
            InitializeComponent();

            ShowWatchHistoryListPanel();

            ResetStartWatchForm();
            ResetEndWatchForm();
            ResetEditWatchMinutesForm();

            SetActionButtonsEnabled(false, false, false);

            LoadWatchHistoryList();
        }

        // 시청 기록 목록을 조회하고 OTT 필터, 목록 필터, 요약 카드를 갱신합니다.
        private void LoadWatchHistoryList()
        {
            try
            {
                _watchHistories = _watchHistoryService.GetWatchHistoryList();

                LoadWatchHistoryOttFilterComboBox();

                ApplyWatchHistoryFilter();

                LoadWatchHistorySummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"시청 기록 목룍을 불러오는 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "시청 기록 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }
        }

        // 현재 시청 기록에 포함된 OTT 제공자 목록을 필터 ComboBox에 구성합니다.
        private void LoadWatchHistoryOttFilterComboBox()
        {
            if (WatchHistoryOttFilterComboBox == null)
            {
                return;
            }

            string currentSelectedText = GetSelectedComboBoxText(WatchHistoryOttFilterComboBox);

            List<string> ottNames = _watchHistories
                .Select(watchHistory => watchHistory.OTT_Name)
                .Where(ottName => !string.IsNullOrWhiteSpace(ottName))
                .Distinct()
                .OrderBy(ottName => ottName)
                .ToList();

            WatchHistoryOttFilterComboBox.Items.Clear();

            ComboBoxItem allItem = new ComboBoxItem
            {
                Content = "OTT 전체"
            };

            WatchHistoryOttFilterComboBox.Items.Add(allItem);

            foreach (string ottName in ottNames)
            {
                WatchHistoryOttFilterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = ottName
                });
            }

            WatchHistoryOttFilterComboBox.SelectedIndex = 0;

            if (!string.IsNullOrWhiteSpace(currentSelectedText)
                && !currentSelectedText.Contains("전체"))
            {
                foreach (object item in WatchHistoryOttFilterComboBox.Items)
                {
                    if (item is ComboBoxItem comboBoxItem
                        && comboBoxItem.Content?.ToString() == currentSelectedText)
                    {
                        WatchHistoryOttFilterComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
        }

        // 검색어, 상태, 콘텐츠 유형, OTT 조건을 조합해 시청 기록 목록을 필터링합니다.
        private void ApplyWatchHistoryFilter()
        {
            if (WatchHistoryDataGrid == null
                || WatchHistorySearchTextBox == null
                || WatchHistoryStatusFilterComboBox == null
                || WatchHistoryTypeFilterComboBox == null
                || WatchHistoryOttFilterComboBox == null)
            {
                return;
            }

            string keyword = WatchHistorySearchTextBox.Text.Trim().ToLower();

            string statusFilter = GetSelectedComboBoxText(WatchHistoryStatusFilterComboBox);
            string typeFilter = GetSelectedComboBoxText(WatchHistoryTypeFilterComboBox);
            string ottFilter = GetSelectedComboBoxText(WatchHistoryOttFilterComboBox);

            _filteredWatchHistories = _watchHistories
                .Where(watchHistory =>
                {
                    bool keywordMatched =
                        string.IsNullOrWhiteSpace(keyword)
                        || watchHistory.Watch_ID.ToString().Contains(keyword)
                        || watchHistory.User_ID.ToString().Contains(keyword)
                        || watchHistory.User_Name.ToLower().Contains(keyword)
                        || watchHistory.Content_Name.ToLower().Contains(keyword)
                        || watchHistory.Content_Type.ToLower().Contains(keyword)
                        || watchHistory.OTT_Name.ToLower().Contains(keyword)
                        || watchHistory.StatusText.ToLower().Contains(keyword);

                    bool statusMatched =
                        string.IsNullOrWhiteSpace(statusFilter)
                        || statusFilter.Contains("전체")
                        || watchHistory.StatusText == statusFilter;

                    bool typeMatched =
                        string.IsNullOrWhiteSpace(typeFilter)
                        || typeFilter.Contains("전체")
                        || watchHistory.Content_Type == typeFilter;

                    bool ottMatched =
                        string.IsNullOrWhiteSpace(ottFilter)
                        || ottFilter.Contains("전체")
                        || watchHistory.OTT_Name == ottFilter;

                    return keywordMatched
                           && statusMatched
                           && typeMatched
                           && ottMatched;
                })
                .ToList();

            WatchHistoryDataGrid.ItemsSource = null;
            WatchHistoryDataGrid.ItemsSource = _filteredWatchHistories;
        }

        // 시청 기록 검색어가 바뀌면 목록 필터를 다시 적용합니다.
        private void WatchHistorySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyWatchHistoryFilter();
        }

        // 시청 기록 필터가 바뀌면 목록 필터를 다시 적용합니다.
        private void WatchHistoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyWatchHistoryFilter();
        }

        // 현재 시청 기록 목록 기준으로 요약 카드 값을 계산합니다.
        private void LoadWatchHistorySummary()
        {
            if(_watchHistories == null || _watchHistories.Count == 0)
            {
                TotalWatchHistoryCountText.Text = "0";
                CompletedWatchCountText.Text = "0";
                WatchingCountText.Text = "0";
                AverageProgressText.Text = "0%";
                return;
            }

            int totalCount = _watchHistories.Count;

            int completedCount = _watchHistories.Count(watchhistory => watchhistory.IsCompleted == true);

            int watchingCount = _watchHistories.Count(watchhistory => watchhistory.WatchEndedAt == null);

            decimal averageProgress = _watchHistories.Average(watchHistory => watchHistory.ProgressPercentage ?? 0);

            TotalWatchHistoryCountText.Text = totalCount.ToString();
            CompletedWatchCountText.Text = completedCount.ToString();
            WatchingCountText.Text = watchingCount.ToString();
            AverageProgressText.Text = averageProgress.ToString("0.#")+"%";
        }

        // 시청 기록 목록에서 선택한 행 상태에 따라 종료, 수정, 삭제 버튼을 제어합니다.
        private void WatchHistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WatchHistoryDataGrid.SelectedItem is not WatchHistoryListItemModel selectedWatchHistory)
            {
                ClearSelectedWatchInfo();
                SetActionButtonsEnabled(false, false, false);
                return;
            }

            LoadSelectedWatchInfo(selectedWatchHistory);

            bool isWatching = selectedWatchHistory.WatchEndedAt == null;
            bool isEnded = selectedWatchHistory.WatchEndedAt != null;

            SetActionButtonsEnabled(
                canEnd: isWatching,
                canEdit: isEnded,
                canDelete: true
            );
        }

        // 선택한 시청 기록 정보를 종료/수정 화면 표시값에 반영합니다.
        private void LoadSelectedWatchInfo(WatchHistoryListItemModel selectedWatchHistory)
        {
            _selectedWatchId = selectedWatchHistory.Watch_ID;
            _selectedRunningTime = selectedWatchHistory.Running_Time ?? 0;
            _selectedCurrentWatchMinutes = selectedWatchHistory.WatchMinutes ?? 0;
            _selectedWatchStatus = GetWatchStatusText(selectedWatchHistory);

            _selectedWatchStartedAt = selectedWatchHistory.WatchStartedAt;
            _selectedWatchEndedAt = selectedWatchHistory.WatchEndedAt;


            string title = selectedWatchHistory.Content_Name;
            string startedAt = selectedWatchHistory.WatchStartedText;

            EndWatchIdText.Text = _selectedWatchId?.ToString() ?? "-";
            EndContentTitleText.Text = string.IsNullOrWhiteSpace(title) ? "-" : title;
            EndRunningTimeText.Text = _selectedRunningTime > 0 ? $"{_selectedRunningTime}분" : "-";
            EndStartedAtText.Text = string.IsNullOrWhiteSpace(startedAt) ? "-" : startedAt;

            EditWatchIdText.Text = _selectedWatchId?.ToString() ?? "-";
            EditContentTitleText.Text = string.IsNullOrWhiteSpace(title) ? "-" : title;
            EditRunningTimeText.Text = _selectedRunningTime > 0 ? $"{_selectedRunningTime}분" : "-";

            CurrentWatchMinutesText.Text =
                _selectedCurrentWatchMinutes > 0
                    ? $"{_selectedCurrentWatchMinutes}분"
                    : "-";

            CurrentWatchStatusText.Text =
                string.IsNullOrWhiteSpace(_selectedWatchStatus)
                    ? "-"
                    : _selectedWatchStatus;
        }

        // 선택된 시청 기록 상태와 종료/수정 화면 표시값을 초기화합니다.
        private void ClearSelectedWatchInfo()
        {
            _selectedWatchId = null;
            _selectedRunningTime = 0;
            _selectedCurrentWatchMinutes = 0;
            _selectedWatchStatus = "";

            _selectedWatchStartedAt =null;
            _selectedWatchEndedAt = null;

            EndWatchIdText.Text = "-";
            EndContentTitleText.Text = "-";
            EndRunningTimeText.Text = "-";
            EndStartedAtText.Text = "-";
            EndExpectedProgressText.Text = "-";
            EndExpectedStatusText.Text = "-";

            EditWatchIdText.Text = "-";
            EditContentTitleText.Text = "-";
            EditRunningTimeText.Text = "-";
            CurrentWatchMinutesText.Text = "-";
            CurrentWatchStatusText.Text = "-";
            EditExpectedProgressText.Text = "-";
            EditExpectedStatusText.Text = "-";
        }

        // 선택한 시청 기록 상태에 따라 액션 버튼 활성 여부를 바꿉니다.
        private void SetActionButtonsEnabled(
            bool canEnd,
            bool canEdit,
            bool canDelete)
        {
            EndWatchButton.IsEnabled = canEnd;
            EditWatchMinutesButton.IsEnabled = canEdit;
            DeleteWatchHistoryButton.IsEnabled = canDelete;
        }

        // 시청 기록 목록 화면만 표시합니다.
        private void ShowWatchHistoryListPanel()
        {
            WatchHistoryListPanel.Visibility = Visibility.Visible;
            WatchStartPanel.Visibility = Visibility.Collapsed;
            WatchEndPanel.Visibility = Visibility.Collapsed;
            WatchMinutesEditPanel.Visibility = Visibility.Collapsed;
        }

        // 시청 시작 등록 화면만 표시합니다.
        private void ShowWatchStartPanel()
        {
            WatchHistoryListPanel.Visibility = Visibility.Collapsed;
            WatchStartPanel.Visibility = Visibility.Visible;
            WatchEndPanel.Visibility = Visibility.Collapsed;
            WatchMinutesEditPanel.Visibility = Visibility.Collapsed;
        }

        // 시청 종료 처리 화면만 표시합니다.
        private void ShowWatchEndPanel()
        {
            WatchHistoryListPanel.Visibility = Visibility.Collapsed;
            WatchStartPanel.Visibility = Visibility.Collapsed;
            WatchEndPanel.Visibility = Visibility.Visible;
            WatchMinutesEditPanel.Visibility = Visibility.Collapsed;
        }

        // 시청 시간 수정 화면만 표시합니다.
        private void ShowWatchMinutesEditPanel()
        {
            WatchHistoryListPanel.Visibility = Visibility.Collapsed;
            WatchStartPanel.Visibility = Visibility.Collapsed;
            WatchEndPanel.Visibility = Visibility.Collapsed;
            WatchMinutesEditPanel.Visibility = Visibility.Visible;
        }

        // 시청 시작 등록 화면으로 이동합니다.
        private void StartWatchButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStartWatchForm();
            ShowWatchStartPanel();
        }

        // 시청 시작 등록 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromStartPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWatchHistoryListPanel();
        }

        // 시청 시작 등록 입력값을 초기화합니다.
        private void ResetStartWatchFormButton_Click(object sender, RoutedEventArgs e)
        {
            ResetStartWatchForm();
        }

        // 시청 시작 입력값을 검증한 뒤 새 시청 기록을 등록합니다.
        private void SaveStartWatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartWatchForm())
            {
                return;
            }

            int userId = _selectedStartUserId.Value;
            int contentId = _selectedStartContentId.Value;
            int ottId = Convert.ToInt32(StartOttProviderComboBox.SelectedValue);

            TimeSpan startTime = TimeSpan.ParseExact(
                StartWatchTimeTextBox.Text.Trim(),
                @"hh\:mm",
                CultureInfo.InvariantCulture
            );

            DateTime watchStartedAt = StartWatchDatePicker.SelectedDate.Value.Date + startTime;

            try
            {
                int newWatchID = _watchHistoryService.StartWatchHistory(
                    userId,
                    contentId,
                    ottId,
                    watchStartedAt);

                MessageBox.Show(
                    $"시청 기록이 시작되었습니다. \n\nWatch_ID: {newWatchID}",
                    "시청 시작 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ResetStartWatchForm();

                LoadWatchHistoryList();

                ShowWatchHistoryListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"시청 시작 등록 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "시청 시작 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }
        }

        // 시청 시작 등록 화면의 사용자, 콘텐츠, OTT, 시작 일시 입력값을 초기화합니다.
        private void ResetStartWatchForm()
        {
            _selectedStartUserId = null;
            _selectedStartContentId = null;
            _selectedStartOttId = null;

            _isStartContentSearchUpdating = true;
            StartUserSearchTextBox.Clear();
            _isStartUserSearchUpdating = false;

            _isStartContentSearchUpdating = true;
            StartContentSearchTextBox.Clear();
            _isStartContentSearchUpdating = false;

            StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
            StartUserSearchResultListBox.ItemsSource = null;

            StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
            StartContentSearchResultListBox.ItemsSource = null;

            SelectedStartUserInfoText.Text = "선택된 사용자 없음";
            SelectedStartContentInfoText.Text = "선택된 콘텐츠 없음";

            StartContentTypeTextBox.Text = "-";

            StartOttProviderComboBox.SelectedIndex = -1;
            StartOttProviderComboBox.ItemsSource = null;
            StartOttProviderComboBox.IsEnabled = false;

            StartWatchDatePicker.SelectedDate = DateTime.Today;
            StartWatchTimeTextBox.Text = DateTime.Now.ToString("HH:mm");
        }

        // 시청 시작 등록에 필요한 선택값과 시작 일시 형식을 검증합니다.
        private bool ValidateStartWatchForm()
        {
            if (_selectedStartUserId == null)
            {
                MessageBox.Show("사용자를 선택하세요.");
                StartUserSearchTextBox.Focus();
                return false;
            }

            if (_selectedStartContentId == null)
            {
                MessageBox.Show("콘텐츠를 선택하세요.");
                StartContentSearchTextBox.Focus();
                return false;
            }

            if (StartOttProviderComboBox.SelectedValue == null)
            {
                MessageBox.Show("OTT 제공자를 선택하세요.");
                StartOttProviderComboBox.Focus();
                return false;
            }

            if (!StartWatchDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("시청 시작일을 선택하세요.");
                StartWatchDatePicker.Focus();
                return false;
            }

            if (!TimeSpan.TryParseExact(
                    StartWatchTimeTextBox.Text.Trim(),
                    @"hh\:mm",
                    CultureInfo.InvariantCulture,
                    out TimeSpan startTime))
            {
                MessageBox.Show("시청 시작 시간은 HH:mm 형식으로 입력하세요. 예: 13:30");
                StartWatchTimeTextBox.Focus();
                return false;
            }

            DateTime watchStartedAt = StartWatchDatePicker.SelectedDate.Value.Date + startTime;

            if (watchStartedAt > DateTime.Now.AddMinutes(1))
            {
                MessageBox.Show("시청 시작 시간은 현재 시간보다 미래일 수 없습니다.");
                StartWatchTimeTextBox.Focus();
                return false;
            }

            return true;
        }

        // 사용자 검색어가 바뀌면 시청 기록 등록 대상 사용자를 조회합니다.
        private void StartUserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isStartUserSearchUpdating)
            {
                return;
            }

            _selectedStartUserId = null;
            SelectedStartUserInfoText.Text = "선택된 사용자 없음";

            string keyword = StartUserSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
                StartUserSearchResultListBox.ItemsSource = null;
                return;
            }

            try
            {
                _searchedStartUsers = _watchHistoryService.SearchUsersForWatchHistory(keyword);

                StartUserSearchResultListBox.ItemsSource = null;
                StartUserSearchResultListBox.ItemsSource = _searchedStartUsers;

                if (_searchedStartUsers.Count > 0)
                {
                    StartUserSearchResultListBox.Visibility = Visibility.Visible;
                    StartUserSearchResultListBox.SelectedIndex = 0;
                }
                else
                {
                    StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"사용자 검색 중 오류 발생. \n\n{ex.Message}",
                    "사용자 검색 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 사용자 검색 TextBox에서 방향키, Enter, Escape 입력을 처리합니다.
        private void StartUserSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (StartUserSearchResultListBox.Items.Count > 0)
                {
                    StartUserSearchResultListBox.Visibility = Visibility.Visible;

                    if (StartUserSearchResultListBox.SelectedIndex < 0)
                    {
                        StartUserSearchResultListBox.SelectedIndex = 0;
                    }

                    StartUserSearchResultListBox.Focus();
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Enter)
            {
                if (StartUserSearchResultListBox.SelectedItem is WatchHistoryUserSelectItemModel selectedUser)
                {
                    CommitSelectedStartUser(selectedUser);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        // 사용자 검색 결과 목록에서 Enter, Escape, 위 방향키 입력을 처리합니다.
        private void StartUserSearchResultListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (StartUserSearchResultListBox.SelectedItem is WatchHistoryUserSelectItemModel selectedUser)
                {
                    CommitSelectedStartUser(selectedUser);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
                StartUserSearchTextBox.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up && StartUserSearchResultListBox.SelectedIndex == 0)
            {
                StartUserSearchTextBox.Focus();
                e.Handled = true;
            }
        }

        // 사용자 검색 결과를 마우스로 클릭하면 선택 사용자로 확정합니다.
        private void StartUserSearchResultListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (StartUserSearchResultListBox.SelectedItem is WatchHistoryUserSelectItemModel selectedUser)
            {
                CommitSelectedStartUser(selectedUser);
            }
        }

        // 검색 결과에서 선택한 사용자를 입력칸과 선택 상태에 반영합니다.
        private void CommitSelectedStartUser(WatchHistoryUserSelectItemModel selectedUser)
        {
            _isStartUserSearchUpdating = true;

            _selectedStartUserId = selectedUser.User_ID;

            StartUserSearchTextBox.Text = selectedUser.DisplayText;
            StartUserSearchTextBox.CaretIndex = StartUserSearchTextBox.Text.Length;

            SelectedStartUserInfoText.Text =
                $"선택된 사용자: {selectedUser.DisplayText}";

            StartUserSearchResultListBox.Visibility = Visibility.Collapsed;
            StartUserSearchResultListBox.ItemsSource = null;

            _isStartUserSearchUpdating = false;

            StartUserSearchTextBox.Focus();
        }

        // 콘텐츠 검색어가 바뀌면 시청 시작 가능한 콘텐츠를 조회합니다.
        private void StartContentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isStartContentSearchUpdating)
            {
                return;
            }

            _selectedStartContentId = null;
            _selectedStartOttId = null;

            SelectedStartContentInfoText.Text = "선택된 콘텐츠 없음";
            StartContentTypeTextBox.Text = "-";

            StartOttProviderComboBox.SelectedIndex = -1;
            StartOttProviderComboBox.ItemsSource = null;
            StartOttProviderComboBox.IsEnabled = false;

            string keyword = StartContentSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
                StartContentSearchResultListBox.ItemsSource = null;
                return;
            }

            try
            {
                _searchedStartContents = _watchHistoryService.SearchContentsForWatchHistory(keyword);

                StartContentSearchResultListBox.ItemsSource = null;
                StartContentSearchResultListBox.ItemsSource = _searchedStartContents;

                if (_searchedStartContents.Count > 0)
                {
                    StartContentSearchResultListBox.Visibility = Visibility.Visible;
                    StartContentSearchResultListBox.SelectedIndex = 0;
                }
                else
                {
                    StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"콘텐츠 검색 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "콘텐츠 검색 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }

        // 콘텐츠 검색 TextBox에서 방향키, Enter, Escape 입력을 처리합니다.
        private void StartContentSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (StartContentSearchResultListBox.Items.Count > 0)
                {
                    StartContentSearchResultListBox.Visibility = Visibility.Visible;

                    if (StartContentSearchResultListBox.SelectedIndex < 0)
                    {
                        StartContentSearchResultListBox.SelectedIndex = 0;
                    }

                    StartContentSearchResultListBox.Focus();
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.Enter)
            {
                if (StartContentSearchResultListBox.SelectedItem is WatchHistoryContentSelectItemModel selectedContent)
                {
                    CommitSelectedStartContent(selectedContent);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        // 콘텐츠 검색 결과 목록에서 Enter, Escape, 위 방향키 입력을 처리합니다.
        private void StartContentSearchResultListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (StartContentSearchResultListBox.SelectedItem is WatchHistoryContentSelectItemModel selectedContent)
                {
                    CommitSelectedStartContent(selectedContent);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
                StartContentSearchTextBox.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up && StartContentSearchResultListBox.SelectedIndex == 0)
            {
                StartContentSearchTextBox.Focus();
                e.Handled = true;
            }
        }

        // 콘텐츠 검색 결과를 마우스로 클릭하면 선택 콘텐츠로 확정합니다.
        private void StartContentSearchResultListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (StartContentSearchResultListBox.SelectedItem is WatchHistoryContentSelectItemModel selectedContent)
            {
                CommitSelectedStartContent(selectedContent);
            }
        }

        // 검색 결과에서 선택한 콘텐츠를 입력칸과 OTT 후보 목록에 반영합니다.
        private void CommitSelectedStartContent(WatchHistoryContentSelectItemModel selectedContent)
        {
            _isStartContentSearchUpdating = true;

            _selectedStartContentId = selectedContent.Content_ID;
            _selectedStartOttId = null;

            StartContentSearchTextBox.Text = selectedContent.DisplayText;
            StartContentSearchTextBox.CaretIndex = StartContentSearchTextBox.Text.Length;

            SelectedStartContentInfoText.Text = $"선택된 콘텐츠: {selectedContent.DisplayText}";

            StartContentTypeTextBox.Text = selectedContent.Content_Type;

            StartContentSearchResultListBox.Visibility = Visibility.Collapsed;
            StartContentSearchResultListBox.ItemsSource = null;

            _isStartContentSearchUpdating = false;

            LoadStartOttProvidersByContent(selectedContent.Content_ID);

            StartContentSearchTextBox.Focus();
        }

        // 선택 콘텐츠를 제공하는 OTT 목록을 조회해 시작 등록 ComboBox에 연결합니다.
        private void LoadStartOttProvidersByContent(int Content_ID)
        {
            try
            {
                List<WatchHistoryOttProviderSelectItemModel> ottproviders = _watchHistoryService.GetOttProvidersByContentId(Content_ID);

                StartOttProviderComboBox.ItemsSource = null;
                StartOttProviderComboBox.ItemsSource = ottproviders;
                StartOttProviderComboBox.SelectedIndex = -1;

                StartOttProviderComboBox.IsEnabled = ottproviders.Count > 0;

                if (ottproviders.Count == 0)
                {
                    MessageBox.Show(
                        "해당 콘텐츠에 연결된 OTT 제공자가 없습니다. \nCOntent Mapping화면에서 먼저 OTT 연결을 해야 합니다.",
                        "OTT 제공자 없음",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"OTT 제공자 목록을 불러오는 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "OTT 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);


            }
        }

        // 시청 중인 기록의 종료 처리 화면으로 이동합니다.
        private void EndWatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWatchId == null)
            {
                MessageBox.Show("시청 종료할 기록을 선택하세요.");
                return;
            }

            if (_selectedWatchStatus != "시청 중")
            {
                MessageBox.Show("시청 중인 기록만 종료할 수 있습니다.");
                return;
            }

            ResetEndWatchForm();
            ShowWatchEndPanel();
        }

        // 시청 종료 처리 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromEndPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWatchHistoryListPanel();
        }

        // 시청 종료 처리 입력값을 초기화합니다.
        private void ResetEndWatchFormButton_Click(object sender, RoutedEventArgs e)
        {
            ResetEndWatchForm();
        }

        // 시청 종료 입력값을 검증한 뒤 시청 기록을 종료 처리합니다.
        private void SaveEndWatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWatchId == null)
            {
                MessageBox.Show("시청 종료할 기록을 선택하세요.");
                return;
            }
            DateTime standardEndTime = DateTime.Now;

            if (!TryGetNormalizedWatchMinutes(
                    EndWatchMinutesTextBox,
                    _selectedRunningTime,
                    _selectedWatchStartedAt,
                    standardEndTime,
                    out int watchMinutes))
            {
                return;
            }

            try
            {
                _watchHistoryService.EndWatchHistory(
                    _selectedWatchId.Value,
                    watchMinutes);

                    MessageBox.Show
                    ("시청종료가 완료되었습니다.",
                    "시청 종료 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );

                    ResetEndWatchForm();
                    ClearSelectedWatchInfo();
                    LoadWatchHistoryList();
                    ShowWatchHistoryListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"시청 종료 처리 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "시청 종료 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }



        }

        // 시청 종료 처리 화면의 시청 시간과 예상 결과를 초기화합니다.
        private void ResetEndWatchForm()
        {
            EndWatchMinutesTextBox.Clear();
            EndExpectedProgressText.Text = "-";
            EndExpectedStatusText.Text = "-";
        }

        // 종료 처리 시 입력한 시청 시간 기준으로 예상 진행률과 상태를 갱신합니다.
        private void EndWatchMinutesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExpectedResult(
                EndWatchMinutesTextBox,
                EndExpectedProgressText,
                EndExpectedStatusText
            );
        }

        // 종료된 시청 기록의 시청 시간 수정 화면으로 이동합니다.
        private void EditWatchMinutesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWatchId == null)
            {
                MessageBox.Show("수정할 시청 기록을 선택하세요.");
                return;
            }

            if (_selectedWatchStatus == "시청 중")
            {
                MessageBox.Show("시청 중인 기록은 수정할 수 없습니다. 먼저 시청 종료를 처리하세요.");
                return;
            }

            ResetEditWatchMinutesForm();

            if (_selectedCurrentWatchMinutes > 0)
            {
                EditWatchMinutesTextBox.Text = _selectedCurrentWatchMinutes.ToString();
            }

            ShowWatchMinutesEditPanel();
        }

        // 시청 시간 수정 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromEditPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWatchHistoryListPanel();
        }

        // 시청 시간 수정 입력값을 선택 기록의 현재 값으로 되돌립니다.
        private void ResetEditWatchMinutesButton_Click(object sender, RoutedEventArgs e)
        {
            ResetEditWatchMinutesForm();

            if (_selectedCurrentWatchMinutes > 0)
            {
                EditWatchMinutesTextBox.Text = _selectedCurrentWatchMinutes.ToString();
            }
        }

        // 수정 입력값을 검증한 뒤 시청 시간을 저장합니다.
        private void SaveEditWatchMinutesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWatchId == null)
            {
                MessageBox.Show("수정할 시청 기록을 선택하세요.");
                return;
            }

            DateTime standardEndTime = _selectedWatchEndedAt ?? DateTime.Now;

            if (!TryGetNormalizedWatchMinutes(
                    EditWatchMinutesTextBox,
                    _selectedRunningTime,
                    _selectedWatchStartedAt,
                    standardEndTime,
                    out int watchMinutes))
            {
                return;
            }

            try
            {
                _watchHistoryService.UpdateWatchMinutes(
                    _selectedWatchId.Value,
                    watchMinutes);

                MessageBox.Show(
                    "시청 시간이 수정되었습니다.",
                    "시청 시간 수정 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ResetEditWatchMinutesForm();
                ClearSelectedWatchInfo();
                LoadWatchHistoryList();

                ShowWatchHistoryListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"시청 시간 수정 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "시청 시간 수정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 시청 시간 수정 화면의 입력값과 예상 결과를 초기화합니다.
        private void ResetEditWatchMinutesForm()
        {
            EditWatchMinutesTextBox.Clear();
            EditExpectedProgressText.Text = "-";
            EditExpectedStatusText.Text = "-";
        }

        // 수정 시 입력한 시청 시간 기준으로 예상 진행률과 상태를 갱신합니다.
        private void EditWatchMinutesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExpectedResult(
                EditWatchMinutesTextBox,
                EditExpectedProgressText,
                EditExpectedStatusText
            );
        }

        // 선택한 시청 기록 삭제 확인 흐름을 실행합니다.
        private void DeleteWatchHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWatchId == null)
            {
                MessageBox.Show("삭제할 시청 기록을 선택하세요.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Watch_ID {_selectedWatchId} 시청 기록을 삭제하시겠습니까?",
                "삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _watchHistoryService.DeleteWatchHistory(_selectedWatchId.Value);

                MessageBox.Show(
                    "시청 기록이 삭제되었습니다.",
                    "시청 기록 삭제 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ClearSelectedWatchInfo();
                LoadWatchHistoryList();

                ShowWatchHistoryListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"시청 기록 삭제 중 오류가 발생했습니다. \n\n{ex.Message}",
                    "시청 기록 삭제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            }

        // 현재 DataGrid에 표시된 시청 기록 목록을 CSV 파일로 저장합니다.
        private void ExportWatchHistoryCsvButton_Click(object sender, RoutedEventArgs e)
        {
            List<WatchHistoryListItemModel> exportList =
                WatchHistoryDataGrid.ItemsSource?.Cast<WatchHistoryListItemModel>().ToList()
                ?? new List<WatchHistoryListItemModel>();

            if (exportList.Count == 0)
            {
                MessageBox.Show(
                    "CSV로 추출할 구독 데이터가 없습니다.",
                    "CSV 추출",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            try
            {
                string exportDirectory = _watchHistoryService.GetCsvExportPath();

                if (!Directory.Exists(exportDirectory))
                {
                    Directory.CreateDirectory(exportDirectory);
                }

                string fileName = $"watchhistorys_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(exportDirectory, fileName);

                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "Watch_ID,User_ID,User_Name,Content_Name,Content_Type,OTT_Name,Watch_Started_At,Watch_Ended_At,Watch_Minutes,progress_Percentage,is_completed"
                );

                foreach (WatchHistoryListItemModel watchHistory in exportList)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(watchHistory.Watch_ID.ToString()),
                        EscapeCsv(watchHistory.User_ID.ToString()),
                        EscapeCsv(watchHistory.User_Name),
                        EscapeCsv(watchHistory.Content_Name.ToString()),
                        EscapeCsv(watchHistory.Content_Type),
                        EscapeCsv(watchHistory.OTT_Name),
                        EscapeCsv(watchHistory.WatchStartedText),
                        EscapeCsv(watchHistory.WatchEndedText),
                        EscapeCsv(watchHistory.WatchMinutesText),
                        EscapeCsv(watchHistory.ProgressPercentText),
                        EscapeCsv(watchHistory.IsCompleted.ToString())
                    ));
                }

                File.WriteAllText(
                    filePath,
                    csv.ToString(),
                    new UTF8Encoding(true)
                );

                MessageBox.Show(
                    $"CSV 파일 추출이 완료되었습니다.\n\n저장 위치:\n{filePath}",
                    "CSV 추출 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"CSV 추출 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "CSV 추출 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 입력한 시청 시간으로 예상 진행률과 완료/중단 상태를 계산해 표시합니다.
        private void UpdateExpectedResult(
            TextBox minutesTextBox,
            TextBlock progressTextBlock,
            TextBlock statusTextBlock)
        {
            if (_selectedRunningTime <= 0)
            {
                progressTextBlock.Text = "-";
                statusTextBlock.Text = "-";
                return;
            }

            if (!int.TryParse(minutesTextBox.Text.Trim(), out int watchMinutes))
            {
                progressTextBlock.Text = "-";
                statusTextBlock.Text = "-";
                return;
            }

            if (watchMinutes < 0)
            {
                progressTextBlock.Text = "-";
                statusTextBlock.Text = "-";
                return;
            }

            if (watchMinutes > _selectedRunningTime)
            {
                watchMinutes = _selectedRunningTime;
            }

            decimal progress = CalculateProgress(watchMinutes, _selectedRunningTime);
            string status = watchMinutes >= _selectedRunningTime ? "완료" : "중단";

            progressTextBlock.Text = $"{progress:N2}%";
            statusTextBlock.Text = status;
        }

        // 시청 시간 입력값을 검증하고 러닝타임과 실제 경과 시간 범위 안으로 보정합니다.
        private bool TryGetNormalizedWatchMinutes(
            TextBox minutesTextBox,
            int runningTime,
            DateTime? watchStartedAt,
            DateTime standardEndTime,
            out int watchMinutes)
        {
            watchMinutes = 0;

            if (!int.TryParse(minutesTextBox.Text.Trim(), out int inputMinutes))
            {
                MessageBox.Show("시청 시간은 숫자로 입력하세요.");
                minutesTextBox.Focus();
                return false;
            }

            if (inputMinutes < 0)
            {
                MessageBox.Show("시청 시간은 음수일 수 없습니다.");
                minutesTextBox.Focus();
                return false;
            }

            if (runningTime <= 0)
            {
                MessageBox.Show("콘텐츠 러닝타임 정보가 없습니다. Running_Time 조회가 필요합니다.");
                return false;
            }

            if (watchStartedAt == null)
            {
                MessageBox.Show("시청 시작 시간 정보가 없습니다.");
                return false;
            }

            if (standardEndTime <= watchStartedAt.Value)
            {
                MessageBox.Show("시청 종료 기준 시간은 시작 시간보다 이후여야 합니다.");
                return false;
            }

            int elapsedMinutes =
                (int)Math.Floor((standardEndTime - watchStartedAt.Value).TotalMinutes);

            if (elapsedMinutes < 0)
            {
                elapsedMinutes = 0;
            }

            int maxAllowedMinutes = Math.Min(runningTime, elapsedMinutes);

            if (inputMinutes > maxAllowedMinutes)
            {
                MessageBox.Show(
                    $"입력한 시청 시간이 허용 범위를 초과했습니다.\n\n" +
                    $"러닝타임: {runningTime}분\n" +
                    $"실제 경과 시간: {elapsedMinutes}분\n" +
                    $"입력 가능한 최대 시청 시간: {maxAllowedMinutes}분\n\n" +
                    $"입력값을 {maxAllowedMinutes}분으로 자동 변경합니다.",
                    "시청 시간 자동 보정",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                inputMinutes = maxAllowedMinutes;
                minutesTextBox.Text = maxAllowedMinutes.ToString();
            }

            watchMinutes = inputMinutes;
            return true;
        }

        // 시청 시간과 러닝타임으로 진행률을 계산합니다.
        private decimal CalculateProgress(int watchMinutes, int runningTime)
        {
            if (runningTime <= 0)
            {
                return 0;
            }

            if (watchMinutes >= runningTime)
            {
                return 100;
            }

            return Math.Round(watchMinutes * 100.0m / runningTime, 2);
        }

        // 시청 종료 여부와 완료 여부를 화면 표시용 상태 문자열로 변환합니다.
        private string GetWatchStatusText(WatchHistoryListItemModel watchHistory)
        {
            if (watchHistory.WatchEndedAt == null)
            {
                return "시청 중";
            }

            if (watchHistory.IsCompleted == true)
            {
                return "완료";
            }

            return "중단";
        }

        // ComboBoxItem 기반 필터와 일반 Text 기반 입력 모두에서 선택 문자열을 가져옵니다.
        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString()?.Trim() ?? "";
            }

            return comboBox.Text?.Trim() ?? "";
        }

        // DataRowView와 일반 모델 객체 양쪽에서 같은 방식으로 값을 꺼냅니다.
        private object GetValue(object item, string columnName)
        {
            if (item is DataRowView rowView)
            {
                if (rowView.Row.Table.Columns.Contains(columnName))
                {
                    return rowView[columnName];
                }

                return null;
            }

            PropertyInfo property = item.GetType().GetProperty(columnName);

            if (property == null)
            {
                return null;
            }

            return property.GetValue(item);
        }

        // null, DB null, 문자열 숫자를 int로 변환합니다.
        private int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            return 0;
        }

        // null, DB null, 문자열 숫자를 nullable int로 변환합니다.
        private int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            return null;
        }

        // null 또는 DB null을 빈 문자열로 변환합니다.
        private string ToStringValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            return value.ToString() ?? "";
        }

        // CSV 필드 안의 따옴표, 쉼표, 줄바꿈을 RFC 형식에 맞게 이스케이프합니다.
        private string EscapeCsv(string? value)
        {
            if (value == null)
            {
                return "";
            }

            string result = value.Replace("\"", "\"\"");

            if (result.Contains(",")
                || result.Contains("\"")
                || result.Contains("\n")
                || result.Contains("\r"))
            {
                return $"\"{result}\"";
            }

            return result;
        }

    }
}
