using CineCode.Models.Subscription;
using CineCode.Services;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CineCode.Views
{
    public partial class SubscriptionsView : UserControl
    {
        private readonly SubscriptionManagementService _subscriptionManagementService = new SubscriptionManagementService();
        private int? _selectedUserId = null;
        private List<SubscriptionUserSelectItemModel> _searchedUsers = new List<SubscriptionUserSelectItemModel>();
        private bool _isUserSearchUpdating = false;

        private List<SubscriptionOttPorviderSelectedItemModel> _OttProviders = new List<SubscriptionOttPorviderSelectedItemModel>();
        private List<SubscriptionListItemModel> _subscriptions = new List<SubscriptionListItemModel>();

        private List<SubscriptionListItemModel> _filteredSubscriptions = new List<SubscriptionListItemModel>();

        private SubscriptionListItemModel? _selectedSubscription = null;

        // 구독 화면을 초기화하고 목록과 OTT 제공자 선택값을 불러옵니다.
        public SubscriptionsView()
        {
            InitializeComponent();

            LoadSubscriptionList();
            LoadOttProviderComboBox();
        }

        // 구독 목록을 조회한 뒤 필터와 요약 카드를 함께 갱신합니다.
        private void LoadSubscriptionList()
        {
            try
            {
                _subscriptions = _subscriptionManagementService.GetSubscriptionList();

                ApplySubscriptionFilter();

                LoadSubscriptionSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"구독 목록을 불러오는 중 오류 발생 \n\n{ex.Message}",
                    "구독 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 현재 구독 목록 기준으로 활성, 만료 임박, 만료, 월 예상 매출을 계산합니다.
        private void LoadSubscriptionSummary()
        {
            if (_subscriptions == null)
            {
                ActiveSubscriptionCountText.Text = "0";
                ExpiringSoonCountText.Text = "0";
                ExpiredSubscriptionCountText.Text = "0";
                MonthlyRevenueText.Text = "₩ 0";
                return;
            }

            int activeCount = _subscriptions.Count(subscription => subscription.IsActive);
            int expiringSoonCount = _subscriptions.Count(subscription => subscription.IsExpiringSoon);
            int expiredCount = _subscriptions.Count(subscription => subscription.IsExpired);
            long monthlyRevenue = _subscriptions.Where(subscription => subscription.IsActive).Sum(subscription => subscription.Monthly_Price);

            ActiveSubscriptionCountText.Text = activeCount.ToString();
            ExpiringSoonCountText.Text = expiringSoonCount.ToString();
            ExpiredSubscriptionCountText.Text = expiredCount.ToString();
            MonthlyRevenueText.Text = FormatWonShort(monthlyRevenue);
            MonthlyRevenueText.ToolTip = "₩ " + monthlyRevenue.ToString("N0");
        }

        // 구독 추가용 OTT 목록과 목록 필터용 OTT 목록을 각각 세팅합니다.
        private void LoadOttProviderComboBox()
        {
            try
            {
                _OttProviders = _subscriptionManagementService.GetOTTProviderListForSubscription();

                AddOttProviderComboBox.ItemsSource = null;
                AddOttProviderComboBox.ItemsSource = _OttProviders;

                List<SubscriptionOttPorviderSelectedItemModel> filterOttProviders =
                    new List<SubscriptionOttPorviderSelectedItemModel>();

                filterOttProviders.Add(new SubscriptionOttPorviderSelectedItemModel
                {
                    OTT_ID = 0,
                    OTT_Name = "OTT 제공자 전체",
                    Monthly_Price = 0
                });

                filterOttProviders.AddRange(_OttProviders);

                OttProviderFilterComboBox.ItemsSource = null;
                OttProviderFilterComboBox.ItemsSource = filterOttProviders;
                OttProviderFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"OTT 제공자 목록을 불러오는데 실패했습니다. \n\n{ex.Message}",
                    "OTT 제공자 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 검색어, 상태, OTT 제공자 조건을 조합해 화면에 표시할 구독 목록을 필터링합니다.
        private void ApplySubscriptionFilter()
        {
            if (SubscriptionDataGrid == null
                || SubscriptionSearchTextBox == null
                || SubscriptionStatusComboBox == null
                || OttProviderFilterComboBox == null)
            {
                return;
            }

            string keyword = SubscriptionSearchTextBox.Text.Trim().ToLower();
            string statusFilter = GetSelectedComboBoxText(SubscriptionStatusComboBox);

            int selectedOttID = 0;

            if (OttProviderFilterComboBox.SelectedItem is SubscriptionOttPorviderSelectedItemModel selectedOtt)
            {
                selectedOttID = selectedOtt.OTT_ID;
            }

            // 검색어, 상태, OTT 조건이 모두 맞는 행만 DataGrid에 다시 연결합니다.
            _filteredSubscriptions = _subscriptions.Where(subscription =>
            {
                bool keywordMatched =
                    string.IsNullOrWhiteSpace(keyword)
                    || subscription.Subscription_ID.ToString().Contains(keyword)
                    || subscription.User_ID.ToString().Contains(keyword)
                    || subscription.User_Name.ToString().Contains(keyword)
                    || subscription.OTT_Name.ToLower().Contains(keyword)
                    || subscription.SubscriptionStatus.ToLower().Contains(keyword)
                    || subscription.RemainingDaysText.ToLower().Contains(keyword)
                    || subscription.PaidAmountText.ToLower().Contains(keyword);

                bool statusMatched =
                    string.IsNullOrWhiteSpace(statusFilter)
                    || statusFilter.Contains("전체")
                    || subscription.SubscriptionStatus == statusFilter;

                bool ottMatched =
                    selectedOttID == 0
                    || subscription.OTT_ID == selectedOttID;

                return keywordMatched && ottMatched && statusMatched;
            }).ToList();

            SubscriptionDataGrid.ItemsSource = null;
            SubscriptionDataGrid.ItemsSource = _filteredSubscriptions;
        }

        // 검색어가 바뀌면 현재 목록 필터를 다시 적용합니다.
        private void SubscriptionSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySubscriptionFilter();
        }

        // 상태 또는 OTT 필터가 바뀌면 현재 목록 필터를 다시 적용합니다.
        private void SubScriptionFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySubscriptionFilter();
        }

        // 선택된 구독 행을 저장하고 갱신/해지 버튼 활성 상태를 바꿉니다.
        private void SubscriptionDataGrid_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (SubscriptionDataGrid.SelectedItem is not SubscriptionListItemModel selecetedSubscription)
            {
                _selectedSubscription = null;
                SetSubscriptionActionButtonsEnabled(false);
                return;
            }

            _selectedSubscription = selecetedSubscription;
            SetSubscriptionActionButtonsEnabled(true);
        }

        // 선택 여부에 따라 구독 액션 버튼을 활성화하거나 비활성화합니다.
        private void SetSubscriptionActionButtonsEnabled(bool isEnabled)
        {
            RenewSubscriptionButton.IsEnabled = isEnabled;
            CancelSubscriptionButton.IsEnabled = isEnabled;
        }

        // 현재 DataGrid에 표시된 구독 목록을 CSV 파일로 저장합니다.
        private void ExportSubscriptionCsvButton_Click(object sender, RoutedEventArgs e)
        {
            List<SubscriptionListItemModel> exportList =
                SubscriptionDataGrid.ItemsSource?.Cast<SubscriptionListItemModel>().ToList()
                ?? new List<SubscriptionListItemModel>();

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
                string exportDirectory = _subscriptionManagementService.GetCsvExportPath();

                if (!Directory.Exists(exportDirectory))
                {
                    Directory.CreateDirectory(exportDirectory);
                }

                string fileName = $"subscriptions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(exportDirectory, fileName);

                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "Subscription_ID,User_ID,User_Name,OTT_ID,OTT_Name,Started_At,Expired_At,Paid_Amount,Status,Remaining_Days"
                );

                foreach (SubscriptionListItemModel subscription in exportList)
                {
                    csv.AppendLine(string.Join(",",
                        EscapeCsv(subscription.Subscription_ID.ToString()),
                        EscapeCsv(subscription.User_ID.ToString()),
                        EscapeCsv(subscription.User_Name),
                        EscapeCsv(subscription.OTT_ID.ToString()),
                        EscapeCsv(subscription.OTT_Name),
                        EscapeCsv(subscription.StartedAtText),
                        EscapeCsv(subscription.ExpiredAtText),
                        EscapeCsv(subscription.PaidAmountText),
                        EscapeCsv(subscription.SubscriptionStatus),
                        EscapeCsv(subscription.RemainingDaysText)
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

        // 선택된 구독을 해지 처리합니다.
        private void CancelSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSubscription == null)
            {
                MessageBox.Show("해지할 구독을 선택하세요.");
                return;
            }

            if (!_selectedSubscription.IsActive)
            {
                MessageBox.Show("이미 만료된 구독은 해지할 수 없습니다.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"선택한 구독을 해지하시겠습니까?\n\n사용자: {_selectedSubscription.User_Name}\nOTT: {_selectedSubscription.OTT_Name}",
                "구독 해지 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _subscriptionManagementService.CancelSubscription(
                    _selectedSubscription.Subscription_ID
                );

                MessageBox.Show("구독이 해지되었습니다.");

                LoadSubscriptionList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"구독 해지 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "구독 해지 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 선택된 구독을 1개월 갱신합니다. 만료 구독이면 새 구독으로 다시 등록합니다.
        private void RenewSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSubscription == null)
            {
                MessageBox.Show("갱신할 구독을 선택하세요.");
                return;
            }

            try
            {
                if (_selectedSubscription.IsActive)
                {
                    _subscriptionManagementService.RenewActiveSubscriptionOneMonth(
                        _selectedSubscription.Subscription_ID
                    );

                    MessageBox.Show("구독이 1개월 갱신되었습니다.");
                }
                else
                {
                    int newSubscriptionId =
                        _subscriptionManagementService.AddUserSubscription(
                            _selectedSubscription.User_ID,
                            _selectedSubscription.OTT_ID,
                            DateTime.Today,
                            DateTime.Today.AddMonths(1),
                            _selectedSubscription.Monthly_Price
                        );

                    MessageBox.Show(
                        $"만료된 구독이므로 새 구독으로 등록했습니다.\n\n새 구독 ID: {newSubscriptionId}"
                    );
                }

                LoadSubscriptionList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"구독 갱신 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "구독 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 구독 추가 화면으로 전환합니다.
        private void AddSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddSubscriptionPanel();
        }

        // 구독 추가 화면을 표시하고 입력 폼을 초기화합니다.
        private void ShowAddSubscriptionPanel()
        {
            SubscriptionListPanel.Visibility = Visibility.Collapsed;
            SubscriptionAddPanel.Visibility = Visibility.Visible;

            ResetAddSubscriptionForm();
        }

        // 구독 추가 화면에서 목록 화면으로 돌아갑니다.
        private void BackToSubscriptionListButton_Click(object sender, RoutedEventArgs e)
        {
            SubscriptionAddPanel.Visibility = Visibility.Collapsed;
            SubscriptionListPanel.Visibility = Visibility.Visible;
        }

        // 구독 추가 입력값을 초기 상태로 되돌립니다.
        private void ResetAddSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAddSubscriptionForm();
        }

        // 사용자 선택, OTT 선택, 결제금액, 기간 입력을 기본값으로 초기화합니다.
        private void ResetAddSubscriptionForm()
        {
            _selectedUserId = null;
            _searchedUsers.Clear();

            AddUserSearchTextBox.Clear();
            AddUserSearchResultListBox.ItemsSource = null;
            AddUserSearchResultListBox.Visibility = Visibility.Collapsed;
            SelectedUserInfoText.Text = "선택된 사용자 없음";

            AddOttProviderComboBox.SelectedIndex = -1;
            AddPaidAmountTextBox.Clear();

            AddStartedDatePicker.SelectedDate = DateTime.Today;
            AddExpiredDatePicker.SelectedDate = DateTime.Today.AddMonths(1);
        }

        // 시작일이 바뀌면 만료일을 자동으로 1개월 뒤로 맞춥니다.
        private void AddStartedDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddStartedDatePicker.SelectedDate.HasValue)
            {
                AddExpiredDatePicker.SelectedDate =
                    AddStartedDatePicker.SelectedDate.Value.AddMonths(1);
            }
        }

        // OTT 제공자 선택값에 맞춰 결제금액을 표시합니다.
        private void AddOttProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddOttProviderComboBox.SelectedItem is not SubscriptionOttPorviderSelectedItemModel selectedOtt)
            {
                AddPaidAmountTextBox.Clear();
                return;
            }

            AddPaidAmountTextBox.Text = selectedOtt.Monthly_Price.ToString("N0");
        }

        // 사용자 검색어가 바뀌면 구독 등록 가능한 사용자를 조회합니다.
        private void AddUserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUserSearchUpdating)
            {
                return;
            }

            string keyword = AddUserSearchTextBox.Text.Trim();

            _selectedUserId = null;
            SelectedUserInfoText.Text = "선택된 사용자 없음";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _searchedUsers.Clear();
                AddUserSearchResultListBox.ItemsSource = null;
                AddUserSearchResultListBox.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                _searchedUsers = _subscriptionManagementService.SearchUsersForSubscription(keyword);

                AddUserSearchResultListBox.ItemsSource = null;
                AddUserSearchResultListBox.ItemsSource = _searchedUsers;

                AddUserSearchResultListBox.Visibility =
                    _searchedUsers.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                if (_searchedUsers.Count > 0)
                {
                    AddUserSearchResultListBox.SelectedIndex = 0;
                }
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
        private void AddUserSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (AddUserSearchResultListBox.Visibility == Visibility.Visible
                    && AddUserSearchResultListBox.Items.Count > 0)
                {
                    AddUserSearchResultListBox.Focus();

                    if (AddUserSearchResultListBox.SelectedIndex < 0)
                    {
                        AddUserSearchResultListBox.SelectedIndex = 0;
                    }

                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Enter)
            {
                if (AddUserSearchResultListBox.SelectedItem is SubscriptionUserSelectItemModel selectedUser)
                {
                    CommitSelectedUser(selectedUser);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                AddUserSearchResultListBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        // 사용자 검색 결과 목록에서 Enter 또는 Escape 입력을 처리합니다.
        private void AddUserSearchResultListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (AddUserSearchResultListBox.SelectedItem is SubscriptionUserSelectItemModel selectedUser)
                {
                    CommitSelectedUser(selectedUser);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Escape)
            {
                AddUserSearchResultListBox.Visibility = Visibility.Collapsed;
                AddUserSearchTextBox.Focus();
                e.Handled = true;
            }
        }

        // 사용자 검색 결과를 마우스로 클릭하면 선택 사용자로 확정합니다.
        private void AddUserSearchResultListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AddUserSearchResultListBox.SelectedItem is SubscriptionUserSelectItemModel selectedUser)
            {
                CommitSelectedUser(selectedUser);
            }
        }

        // 검색 결과에서 선택한 사용자를 입력칸과 선택 상태에 반영합니다.
        private void CommitSelectedUser(SubscriptionUserSelectItemModel selectedUser)
        {
            // TextBox.Text를 코드로 바꿀 때 TextChanged 검색이 다시 실행되지 않도록 막습니다.
            _isUserSearchUpdating = true;

            _selectedUserId = selectedUser.User_ID;

            AddUserSearchTextBox.Text = selectedUser.DisplayText;
            AddUserSearchTextBox.CaretIndex = AddUserSearchTextBox.Text.Length;

            SelectedUserInfoText.Text =
                $"선택된 사용자: {selectedUser.DisplayText}";

            AddUserSearchResultListBox.Visibility = Visibility.Collapsed;
            AddUserSearchResultListBox.ItemsSource = null;

            _isUserSearchUpdating = false;

            AddUserSearchTextBox.Focus();
        }

        // 구독 추가 입력값을 검증하고 새 구독을 등록합니다.
        private void SaveAddSubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show(
                    "사용자를 검색해서 선택하세요.",
                    "입력 확인",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (AddOttProviderComboBox.SelectedItem is not SubscriptionOttPorviderSelectedItemModel selecetedOtt)
            {
                MessageBox.Show(
                    "OTT 제공자를 선택하세요",
                    "입력 확인",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                AddOttProviderComboBox.Focus();
                return;
            }

            int UserID = _selectedUserId.Value;
            int OTTID = selecetedOtt.OTT_ID;
            DateTime StartedAt = AddStartedDatePicker.SelectedDate.Value.Date;
            DateTime ExpiredAt = AddExpiredDatePicker.SelectedDate.Value.Date;
            int PaidAmount = selecetedOtt.Monthly_Price;

            try
            {
                int NewSubscriptionID =
                    _subscriptionManagementService.AddUserSubscription(
                        UserID,
                        OTTID,
                        StartedAt,
                        ExpiredAt,
                        PaidAmount);

                MessageBox.Show(
                    $"구동 등록이 완료되었습니다. \n\n구독 ID:{NewSubscriptionID}",
                    "구독 등록 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ResetAddSubscriptionForm();

                SubscriptionAddPanel.Visibility = Visibility.Collapsed;
                SubscriptionListPanel.Visibility = Visibility.Visible;

                LoadSubscriptionList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"구독 등록 중 오류 발생. \n\n{ex.Message}",
                    "구독 등록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ComboBoxItem 기반 필터와 일반 Text 기반 필터 모두에서 선택 문자열을 가져옵니다.
        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString()?.Trim() ?? "";
            }

            return comboBox.Text?.Trim() ?? "";
        }

        // 큰 원화 금액을 화면 카드에 맞게 만원/억원 단위로 축약합니다.
        private string FormatWonShort(long amount)
        {
            if (amount >= 100_000_000)
            {
                decimal uk = amount / 100_000_000m;
                return "₩ " + uk.ToString("0.#") + "억원";
            }

            if (amount >= 10_000)
            {
                long man = amount / 10_000;
                return "₩ " + man.ToString("N0") + "만원";
            }

            return "₩ " + amount.ToString("N0");
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
