using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CineCode.Models.UserRating;
using CineCode.Services;

namespace CineCode.Views
{
    public partial class UserRatingsView : UserControl
    {
        private int? _selectedUserId = null;
        private int? _selectedContentId = null;
        private decimal _selectedRatingScore = 0;
        private DateTime? _selectedRatedAt = null;
        private DateTime? _selectedAddRatingWatchEndedAt = null;

        private int? _selectedAddRatingUserId = null;

        private bool _isAddRatingUserSearchUpdating = false;
        private bool _isUserRatingMonthFilterLoading = false;

        private readonly UserRatingService _userRatingService =
            new UserRatingService();

        private List<UserRatingListItemModel> _userRatings =
            new List<UserRatingListItemModel>();

        private List<UserRatingListItemModel> _filteredUserRatings =
            new List<UserRatingListItemModel>();

        private List<UserRatingUserSelectItemModel> _searchedAddRatingUsers =
            new List<UserRatingUserSelectItemModel>();

        private List<UserRatingContentSelectItemModel> _rateableContents =
            new List<UserRatingContentSelectItemModel>();

        // 사용자 평점 화면을 초기화하고 목록 화면을 표시합니다.
        public UserRatingsView()
        {
            InitializeComponent();

            ResetAddRatingForm();
            ResetEditRatingForm();

            SetRatingActionButtonsEnabled(false);

            LoadUserRatingList();

            ShowRatingListPanel();
        }

        // 사용자 평점 목록을 조회하고 월 필터, 목록 필터, 요약 카드를 갱신합니다.
        private void LoadUserRatingList()
        {
            try
            {
                _userRatings = _userRatingService.GetUserRatingList();

                LoadUserRatingMonthFilterComboBox();

                ApplyUserRatingFilter();

                LoadUserRatingSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"평점 목록을 불러오는 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "평점 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 검색어와 평점, 콘텐츠 유형, 월 조건을 조합해 평점 목록을 필터링합니다.
        private void ApplyUserRatingFilter()
        {
            if (UserRatingDataGrid == null
                || UserRatingSearchTextBox == null
                || UserRatingScoreFilterComboBox == null
                || UserRatingTypeFilterComboBox == null
                || UserRatingMonthFilterComboBox == null)
            {
                return;
            }

            string keyword = UserRatingSearchTextBox.Text.Trim().ToLower();

            string scoreFilter = GetSelectedComboBoxText(UserRatingScoreFilterComboBox);
            string typeFilter = GetSelectedComboBoxText(UserRatingTypeFilterComboBox);
            string monthFilter = GetSelectedComboBoxText(UserRatingMonthFilterComboBox);

            // 검색어와 각 필터 조건을 모두 만족하는 평점만 DataGrid에 표시합니다.
            _filteredUserRatings = _userRatings
                .Where(rating =>
                {
                    bool keywordMatched =
                        string.IsNullOrWhiteSpace(keyword)
                        || rating.User_ID.ToString().Contains(keyword)
                        || rating.Content_ID.ToString().Contains(keyword)
                        || rating.User_Name.ToLower().Contains(keyword)
                        || rating.Content_Name.ToLower().Contains(keyword)
                        || rating.Content_Type.ToLower().Contains(keyword)
                        || rating.RatingScoreText.ToLower().Contains(keyword)
                        || rating.ProgressPercentText.ToLower().Contains(keyword)
                        || rating.RatedAtText.ToLower().Contains(keyword);

                    bool scoreMatched =
                        string.IsNullOrWhiteSpace(scoreFilter)
                        || scoreFilter.Contains("전체")
                        || scoreFilter == "5점 이상" && rating.Rating_Score >= 5
                        || scoreFilter == "4점 이상" && rating.Rating_Score >= 4
                        || scoreFilter == "3점 이상" && rating.Rating_Score >= 3
                        || scoreFilter == "3점 미만" && rating.Rating_Score < 3;

                    bool typeMatched =
                        string.IsNullOrWhiteSpace(typeFilter)
                        || typeFilter.Contains("전체")
                        || rating.Content_Type == typeFilter;

                    bool monthMatched =
                        string.IsNullOrWhiteSpace(monthFilter)
                        || monthFilter.Contains("전체")
                        || rating.Rated_At.ToString("yyyy-MM") == monthFilter;

                    return keywordMatched
                           && scoreMatched
                           && typeMatched
                           && monthMatched;
                })
                .ToList();

            UserRatingDataGrid.ItemsSource = null;
            UserRatingDataGrid.ItemsSource = _filteredUserRatings;

            LoadUserRatingSummary();
        }

        // 사용자 평점 검색어가 바뀌면 목록 필터를 다시 적용합니다.
        private void UserRatingSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyUserRatingFilter();
        }

        // 평점 필터가 바뀌면 목록 필터를 다시 적용합니다.
        private void UserRatingFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUserRatingMonthFilterLoading)
            {
                return;
            }

            ApplyUserRatingFilter();
        }

        // 현재 평점 목록에서 사용 가능한 평가 월 목록을 필터 ComboBox에 구성합니다.
        private void LoadUserRatingMonthFilterComboBox()
        {
            if (UserRatingMonthFilterComboBox == null)
            {
                return;
            }

            // Items.Clear/Items.Add 중 SelectionChanged가 발생하므로 필터 적용을 잠시 막습니다.
            _isUserRatingMonthFilterLoading = true;

            string currentSelectedText =
                GetSelectedComboBoxText(UserRatingMonthFilterComboBox);

            List<string> months = _userRatings
                .Select(rating => rating.Rated_At.ToString("yyyy-MM"))
                .Distinct()
                .OrderByDescending(month => month)
                .ToList();

            UserRatingMonthFilterComboBox.Items.Clear();

            UserRatingMonthFilterComboBox.Items.Add(new ComboBoxItem
            {
                Content = "전체 기간"
            });

            foreach (string month in months)
            {
                UserRatingMonthFilterComboBox.Items.Add(new ComboBoxItem
                {
                    Content = month
                });
            }

            UserRatingMonthFilterComboBox.SelectedIndex = 0;

            if (!string.IsNullOrWhiteSpace(currentSelectedText)
                && !currentSelectedText.Contains("전체"))
            {
                foreach (object item in UserRatingMonthFilterComboBox.Items)
                {
                    if (item is ComboBoxItem comboBoxItem
                        && comboBoxItem.Content?.ToString() == currentSelectedText)
                    {
                        UserRatingMonthFilterComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }

            _isUserRatingMonthFilterLoading = false;
        }

        // 필터링된 평점 목록 기준으로 요약 카드 값을 계산합니다.
        private void LoadUserRatingSummary()
        {
            List<UserRatingListItemModel> summarySource = _filteredUserRatings;

            int pendingRatingCount = _userRatingService.GetPendingUserRatingCount();

            if (summarySource == null || summarySource.Count == 0)
            {
                TotalRatingCountText.Text = "0";
                AverageRatingScoreText.Text = "0.0 / 5";
                HighRatingContentText.Text = "0";
                PendingRatingCountText.Text = pendingRatingCount.ToString();
                return;
            }

            int totalCount = summarySource.Count;

            decimal averageRating =
                summarySource.Average(rating => rating.Rating_Score);

            int highRatingCount =
                summarySource.Count(rating => rating.Rating_Score >= 4);

            TotalRatingCountText.Text = totalCount.ToString();
            AverageRatingScoreText.Text = averageRating.ToString("0.0") + " / 5";
            HighRatingContentText.Text = highRatingCount.ToString();
            PendingRatingCountText.Text = pendingRatingCount.ToString();
        }

        // 평점 목록에서 선택한 행 정보를 수정 화면 상태로 보관합니다.
        private void UserRatingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserRatingDataGrid.SelectedItem == null)
            {
                ClearSelectedRatingInfo();
                SetRatingActionButtonsEnabled(false);
                return;
            }

            LoadSelectedRatingInfo();
            SetRatingActionButtonsEnabled(true);
        }

        // 선택한 평점 정보를 필드와 수정 화면 입력값에 반영합니다.
        private void LoadSelectedRatingInfo()
        {
            if (UserRatingDataGrid.SelectedItem is not UserRatingListItemModel selectedRating)
            {
                ClearSelectedRatingInfo();
                return;
            }

            _selectedUserId = selectedRating.User_ID;
            _selectedContentId = selectedRating.Content_ID;
            _selectedRatingScore = selectedRating.Rating_Score;
            _selectedRatedAt = selectedRating.Rated_At;

            EditRatingUserNameText.Text =
                string.IsNullOrWhiteSpace(selectedRating.User_Name)
                    ? "-"
                    : selectedRating.User_Name;

            EditRatingContentTitleText.Text =
                string.IsNullOrWhiteSpace(selectedRating.Content_Name)
                    ? "-"
                    : selectedRating.Content_Name;

            EditRatingContentTypeText.Text =
                string.IsNullOrWhiteSpace(selectedRating.Content_Type)
                    ? "-"
                    : selectedRating.Content_Type;

            EditRatingWatchProgressText.Text =
                string.IsNullOrWhiteSpace(selectedRating.ProgressPercentText)
                    ? "-"
                    : selectedRating.ProgressPercentText;

            SetRatingComboBoxValue(EditRatingScoreComboBox, _selectedRatingScore);

            EditRatingDatePicker.SelectedDate =
                _selectedRatedAt ?? DateTime.Today;
        }

        // 선택 평점 상태와 수정 화면 값을 초기화합니다.
        private void ClearSelectedRatingInfo()
        {
            _selectedUserId = null;
            _selectedContentId = null;
            _selectedRatingScore = 0;
            _selectedRatedAt = null;

            EditRatingUserNameText.Text = "-";
            EditRatingContentTitleText.Text = "-";
            EditRatingContentTypeText.Text = "-";
            EditRatingWatchProgressText.Text = "-";

            EditRatingScoreComboBox.SelectedIndex = -1;
            EditRatingDatePicker.SelectedDate = DateTime.Today;
        }

        // 선택 여부에 따라 수정/삭제 버튼을 활성화합니다.
        private void SetRatingActionButtonsEnabled(bool isEnabled)
        {
            EditRatingButton.IsEnabled = isEnabled;
            DeleteRatingButton.IsEnabled = isEnabled;
        }

        // 평점 목록 화면만 표시합니다.
        private void ShowRatingListPanel()
        {
            RatingListPanel.Visibility = Visibility.Visible;
            RatingAddPanel.Visibility = Visibility.Collapsed;
            RatingEditPanel.Visibility = Visibility.Collapsed;
        }

        // 평점 등록 화면만 표시합니다.
        private void ShowRatingAddPanel()
        {
            RatingListPanel.Visibility = Visibility.Collapsed;
            RatingAddPanel.Visibility = Visibility.Visible;
            RatingEditPanel.Visibility = Visibility.Collapsed;
        }

        // 평점 수정 화면만 표시합니다.
        private void ShowRatingEditPanel()
        {
            RatingListPanel.Visibility = Visibility.Collapsed;
            RatingAddPanel.Visibility = Visibility.Collapsed;
            RatingEditPanel.Visibility = Visibility.Visible;
        }

        // 평점 등록 화면으로 이동합니다.
        private void AddRatingButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAddRatingForm();
            ShowRatingAddPanel();
        }

        // 등록 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromAddRatingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRatingListPanel();
        }

        // 평점 등록 입력값을 초기화합니다.
        private void ResetAddRatingFormButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAddRatingForm();
        }

        // 등록 입력값을 검증한 뒤 평점을 저장합니다.
        private void SaveAddRatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddRatingForm())
            {
                return;
            }

            int userId = _selectedAddRatingUserId.Value;
            int contentId = Convert.ToInt32(AddRatingContentComboBox.SelectedValue);
            decimal ratingScore = GetSelectedRatingScore(AddRatingScoreComboBox);
            DateTime ratedAt = AddRatingDatePicker.SelectedDate.Value;

            try
            {
                _userRatingService.AddOrUpdateUserRating(
                    userId,
                    contentId,
                    ratingScore,
                    ratedAt
                );

                MessageBox.Show(
                    "평점이 등록되었습니다.",
                    "평점 등록 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ResetAddRatingForm();

                LoadUserRatingList();

                ShowRatingListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"평점 등록 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "평점 등록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 등록 화면 입력값과 선택 상태를 기본값으로 되돌립니다.
        private void ResetAddRatingForm()
        {
            _selectedAddRatingUserId = null;

            _isAddRatingUserSearchUpdating = true;
            AddRatingUserSearchTextBox.Clear();
            _isAddRatingUserSearchUpdating = false;

            AddRatingUserSearchResultListBox.ItemsSource = null;
            AddRatingUserSearchResultListBox.Visibility = Visibility.Collapsed;

            AddRatingContentComboBox.ItemsSource = null;
            AddRatingContentComboBox.SelectedIndex = -1;
            _selectedAddRatingWatchEndedAt = null;
            AddRatingScoreComboBox.SelectedIndex = -1;

            AddRatingContentTypeText.Text = "-";
            AddRatingWatchProgressText.Text = "-";

            AddRatingDatePicker.SelectedDate = DateTime.Today;
        }

        // 평점 등록에 필요한 선택값과 평가일 범위를 검증합니다.
        private bool ValidateAddRatingForm()
        {
            if (_selectedAddRatingUserId == null)
            {
                MessageBox.Show("사용자를 검색해서 선택하세요.");
                AddRatingUserSearchTextBox.Focus();
                return false;
            }

            if (AddRatingContentComboBox.SelectedValue == null)
            {
                MessageBox.Show("콘텐츠를 선택하세요.");
                AddRatingContentComboBox.Focus();
                return false;
            }

            if (AddRatingScoreComboBox.SelectedItem == null)
            {
                MessageBox.Show("평점을 선택하세요.");
                AddRatingScoreComboBox.Focus();
                return false;
            }

            if (!AddRatingDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("평가일을 선택하세요.");
                AddRatingDatePicker.Focus();
                return false;
            }

            if (_selectedAddRatingWatchEndedAt == null)
            {
                MessageBox.Show("시청 종료 시간 정보가 없어 평가일을 검증할 수 없습니다.");
                return false;
            }

            DateTime ratedDate = AddRatingDatePicker.SelectedDate.Value.Date;
            DateTime watchEndDate = _selectedAddRatingWatchEndedAt.Value.Date;
            DateTime today = DateTime.Today;

            if (ratedDate < watchEndDate)
            {
                MessageBox.Show(
                    $"평가일은 시청 종료일 이후로만 선택할 수 있습니다.\n\n" +
                    $"시청 종료일: {watchEndDate:yyyy-MM-dd}\n" +
                    $"선택한 평가일: {ratedDate:yyyy-MM-dd}",
                    "평가일 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                AddRatingDatePicker.Focus();
                return false;
            }

            if (ratedDate > today)
            {
                MessageBox.Show(
                    $"평가일은 오늘 이후 날짜로 선택할 수 없습니다.\n\n" +
                    $"오늘 날짜: {today:yyyy-MM-dd}\n" +
                    $"선택한 평가일: {ratedDate:yyyy-MM-dd}",
                    "평가일 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                AddRatingDatePicker.Focus();
                return false;
            }

            return true;
        }

        // 사용자 검색어가 바뀌면 평가 등록 가능한 사용자 목록을 조회합니다.
        private void AddRatingUserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isAddRatingUserSearchUpdating)
            {
                return;
            }

            _selectedAddRatingUserId = null;

            AddRatingContentComboBox.ItemsSource = null;
            AddRatingContentComboBox.SelectedIndex = -1;

            AddRatingContentTypeText.Text = "-";
            AddRatingWatchProgressText.Text = "-";

            string keyword = AddRatingUserSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                AddRatingUserSearchResultListBox.ItemsSource = null;
                AddRatingUserSearchResultListBox.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                _searchedAddRatingUsers =
                    _userRatingService.SearchUsersForUserRating(keyword);

                AddRatingUserSearchResultListBox.ItemsSource = null;
                AddRatingUserSearchResultListBox.ItemsSource = _searchedAddRatingUsers;

                AddRatingUserSearchResultListBox.Visibility =
                    _searchedAddRatingUsers.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"사용자 검색 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "사용자 검색 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 사용자 검색 TextBox에서 방향키, Enter, Escape 입력을 처리합니다.
        private void AddRatingUserSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (AddRatingUserSearchResultListBox.Visibility == Visibility.Visible
                    && AddRatingUserSearchResultListBox.Items.Count > 0)
                {
                    AddRatingUserSearchResultListBox.SelectedIndex = 0;
                    AddRatingUserSearchResultListBox.Focus();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (AddRatingUserSearchResultListBox.Visibility == Visibility.Visible
                    && AddRatingUserSearchResultListBox.Items.Count > 0)
                {
                    if (AddRatingUserSearchResultListBox.SelectedIndex < 0)
                    {
                        AddRatingUserSearchResultListBox.SelectedIndex = 0;
                    }

                    CommitSelectedAddRatingUser();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                AddRatingUserSearchResultListBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        // 사용자 검색 결과 목록에서 Enter 또는 Escape 입력을 처리합니다.
        private void AddRatingUserSearchResultListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitSelectedAddRatingUser();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                AddRatingUserSearchResultListBox.Visibility = Visibility.Collapsed;
                AddRatingUserSearchTextBox.Focus();
                e.Handled = true;
            }
        }

        // 사용자 검색 결과를 마우스로 클릭하면 선택 사용자로 확정합니다.
        private void AddRatingUserSearchResultListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommitSelectedAddRatingUser();
        }

        // 검색 결과에서 선택한 사용자를 입력칸과 콘텐츠 후보 목록에 반영합니다.
        private void CommitSelectedAddRatingUser()
        {
            if (AddRatingUserSearchResultListBox.SelectedItem is not UserRatingUserSelectItemModel selectedUser)
            {
                return;
            }

            _selectedAddRatingUserId = selectedUser.User_ID;

            // TextBox.Text를 코드로 바꿀 때 TextChanged 검색이 다시 실행되지 않도록 막습니다.
            _isAddRatingUserSearchUpdating = true;
            AddRatingUserSearchTextBox.Text = selectedUser.DisplayText;
            _isAddRatingUserSearchUpdating = false;

            AddRatingUserSearchResultListBox.Visibility = Visibility.Collapsed;

            AddRatingContentComboBox.ItemsSource = null;
            AddRatingContentComboBox.SelectedIndex = -1;

            AddRatingContentTypeText.Text = "-";
            AddRatingWatchProgressText.Text = "-";

            LoadRateableContentsByUserId(selectedUser.User_ID);
        }

        // 선택 사용자 기준으로 아직 평가할 수 있는 콘텐츠 목록을 불러옵니다.
        private void LoadRateableContentsByUserId(int userId)
        {
            try
            {
                _rateableContents =
                    _userRatingService.GetRateableContentsByUserId(userId);

                AddRatingContentComboBox.ItemsSource = null;
                AddRatingContentComboBox.ItemsSource = _rateableContents;
                AddRatingContentComboBox.DisplayMemberPath = "DisplayText";
                AddRatingContentComboBox.SelectedValuePath = "Content_ID";

                if (_rateableContents.Count == 0)
                {
                    MessageBox.Show(
                        "평가 가능한 콘텐츠가 없습니다.\n\n" +
                        "이미 평가했거나, 최소 평가 가능 시청률을 만족한 시청 기록이 없습니다.",
                        "평가 가능 콘텐츠 없음",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"평가 가능 콘텐츠 목록을 불러오는 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "콘텐츠 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 평가할 콘텐츠가 바뀌면 콘텐츠 유형, 시청률, 시청 종료일을 갱신합니다.
        private void AddRatingContentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddRatingContentTypeText.Text = "-";
            AddRatingWatchProgressText.Text = "-";
            _selectedAddRatingWatchEndedAt = null;

            if (AddRatingContentComboBox.SelectedItem is not UserRatingContentSelectItemModel selectedContent)
            {
                return;
            }

            AddRatingContentTypeText.Text =
                string.IsNullOrWhiteSpace(selectedContent.Content_Type)
                    ? "-"
                    : selectedContent.Content_Type;

            AddRatingWatchProgressText.Text =
                string.IsNullOrWhiteSpace(selectedContent.ProgressPercentText)
                    ? "-"
                    : selectedContent.ProgressPercentText;

            _selectedAddRatingWatchEndedAt = selectedContent.WatchEndedAt;
        }

        // 선택한 평가 정보를 수정 화면에 불러옵니다.
        private void EditRatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserRatingDataGrid.SelectedItem == null)
            {
                MessageBox.Show("수정할 평가를 선택하세요.");
                return;
            }

            ResetEditRatingForm();
            LoadSelectedRatingInfo();
            ShowRatingEditPanel();
        }

        // 수정 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromEditRatingButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRatingListPanel();
        }

        // 수정 입력값을 선택 평가 원본 값으로 되돌립니다.
        private void ResetEditRatingFormButton_Click(object sender, RoutedEventArgs e)
        {
            ResetEditRatingForm();
            LoadSelectedRatingInfo();
        }

        // 수정 입력값을 검증하고 평점을 저장합니다.
        private void SaveEditRatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null || _selectedContentId == null)
            {
                MessageBox.Show("수정할 평가를 선택하세요.");
                return;
            }

            if (!ValidateEditRatingForm())
            {
                return;
            }

            int userId = _selectedUserId.Value;
            int contentId = _selectedContentId.Value;
            decimal ratingScore = GetSelectedRatingScore(EditRatingScoreComboBox);
            DateTime ratedAt = EditRatingDatePicker.SelectedDate.Value;

            try
            {
                _userRatingService.AddOrUpdateUserRating(
                    userId,
                    contentId,
                    ratingScore,
                    ratedAt
                );

                MessageBox.Show(
                    "평점이 수정되었습니다.",
                    "평점 수정 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadUserRatingList();

                ShowRatingListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"평점 수정 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "평점 수정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 수정 화면 입력값을 기본 상태로 되돌립니다.
        private void ResetEditRatingForm()
        {
            EditRatingUserNameText.Text = "-";
            EditRatingContentTitleText.Text = "-";
            EditRatingContentTypeText.Text = "-";
            EditRatingWatchProgressText.Text = "-";

            EditRatingScoreComboBox.SelectedIndex = -1;
            EditRatingDatePicker.SelectedDate = DateTime.Today;
        }

        // 평점 수정에 필요한 입력값을 검증합니다.
        private bool ValidateEditRatingForm()
        {
            if (EditRatingScoreComboBox.SelectedItem == null)
            {
                MessageBox.Show("평점을 선택하세요.");
                EditRatingScoreComboBox.Focus();
                return false;
            }

            if (!EditRatingDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("평가일을 선택하세요.");
                EditRatingDatePicker.Focus();
                return false;
            }

            return true;
        }

        // 선택한 평점을 삭제합니다.
        private void DeleteRatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null || _selectedContentId == null)
            {
                MessageBox.Show(
                    "삭제할 평점을 선택하세요.",
                    "평점 삭제",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "선택한 평점을 삭제하시겠습니까?",
                "평점 삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _userRatingService.DeleteUserRating(
                    _selectedUserId.Value,
                    _selectedContentId.Value
                );

                MessageBox.Show(
                    "평점이 삭제되었습니다.",
                    "평점 삭제 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                _selectedUserId = null;
                _selectedContentId = null;
                _selectedRatingScore = 0;
                _selectedRatedAt = null;

                LoadUserRatingList();

                ShowRatingListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"평점 삭제 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "평점 삭제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 현재 DataGrid에 표시된 평점 목록을 CSV 파일로 저장합니다.
        private void ExportRatingCsvButton_Click(object sender, RoutedEventArgs e)
        {
            List<UserRatingListItemModel> exportList =
                UserRatingDataGrid.ItemsSource
                    ?.Cast<UserRatingListItemModel>()
                    .ToList()
                ?? new List<UserRatingListItemModel>();

            if (exportList.Count == 0)
            {
                MessageBox.Show(
                    "CSV로 추출할 평점 데이터가 없습니다.",
                    "CSV 추출",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return;
            }

            try
            {
                string exportDirectory = _userRatingService.GetCsvExportPath();

                if (!Directory.Exists(exportDirectory))
                {
                    Directory.CreateDirectory(exportDirectory);
                }

                string fileName = $"user_ratings_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(exportDirectory, fileName);

                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "User_ID,User_Name,Content_ID,Content_Name,Content_Type,Rating_Score,Progress_Percentage,Rated_At"
                );

                foreach (UserRatingListItemModel rating in exportList)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(rating.User_ID.ToString()),
                        EscapeCsv(rating.User_Name),
                        EscapeCsv(rating.Content_ID.ToString()),
                        EscapeCsv(rating.Content_Name),
                        EscapeCsv(rating.Content_Type),
                        EscapeCsv(rating.RatingScoreText),
                        EscapeCsv(rating.ProgressPercentText),
                        EscapeCsv(rating.RatedAtText)
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

        // ComboBoxItem 기반 필터와 일반 Text 기반 입력 모두에서 선택 문자열을 가져옵니다.
        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString()?.Trim() ?? "";
            }

            return comboBox.Text?.Trim() ?? "";
        }

        // 선택된 평점 ComboBox 항목을 decimal 점수로 변환합니다.
        private decimal GetSelectedRatingScore(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                if (decimal.TryParse(item.Content?.ToString(), out decimal score))
                {
                    return score;
                }
            }

            return 0;
        }

        // 저장된 평점 값과 일치하는 ComboBoxItem을 찾아 선택합니다.
        private void SetRatingComboBoxValue(ComboBox comboBox, decimal score)
        {
            comboBox.SelectedIndex = -1;

            foreach (object item in comboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    if (decimal.TryParse(comboBoxItem.Content?.ToString(), out decimal itemScore))
                    {
                        if (itemScore == score)
                        {
                            comboBox.SelectedItem = comboBoxItem;
                            return;
                        }
                    }
                }
            }
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
