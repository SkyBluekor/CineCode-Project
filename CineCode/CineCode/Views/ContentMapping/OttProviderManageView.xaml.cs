using CineCode.Models.Mapping;
using CineCode.Services;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CineCode.Views.ContentMapping
{
    public partial class OttProviderManageView : UserControl
    {
        // OTT 제공자 데이터 조회/추가/수정/삭제를 담당하는 서비스입니다.
        private readonly ContentMappingService _contentMappingService = new ContentMappingService();

        // 화면에 표시할 전체 OTT 제공자 목록입니다. 검색 시 이 목록에서 필터링합니다.
        private List<OttProviderListItemModel> _ottProviders = new List<OttProviderListItemModel>();

        // 현재 선택된 OTT 제공자입니다. null이면 추가 모드로 동작합니다.
        private OttProviderListItemModel? _selectedOttProvider = null;

        public OttProviderManageView()
        {
            InitializeComponent();

            LoadOttProviderList();
            SetEditMode(false);
        }

        // OTT 제공자 목록을 불러온 뒤 현재 검색어를 다시 적용합니다.
        private void LoadOttProviderList()
        {
            try
            {
                _ottProviders = _contentMappingService.GetOttProviderList();

                ApplyOttProviderSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 제공자 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 제공자 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 검색어가 있으면 OTT 이름, ID, 월 구독료 기준으로 목록을 거릅니다.
        private void ApplyOttProviderSearch()
        {
            if (_ottProviders == null)
            {
                OttProviderDataGrid.ItemsSource = null;
                return;
            }

            string keyword = OttProviderSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                OttProviderDataGrid.ItemsSource = null;
                OttProviderDataGrid.ItemsSource = _ottProviders;
                return;
            }

            keyword = keyword.ToLower();

            List<OttProviderListItemModel> filteredProviders = _ottProviders
                .Where(provider =>
                    provider.OTT_NAME.ToLower().Contains(keyword)
                    || provider.OTT_ID.ToString().Contains(keyword)
                    || provider.monthly_price.ToString().Contains(keyword)
                )
                .ToList();

            OttProviderDataGrid.ItemsSource = null;
            OttProviderDataGrid.ItemsSource = filteredProviders;
        }

        private void OttProviderSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyOttProviderSearch();
        }

        // 목록에서 OTT 제공자를 선택하면 왼쪽 입력 폼에 값을 채우고 수정 모드로 전환합니다.
        private void OttProviderDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OttProviderDataGrid.SelectedItem is not OttProviderListItemModel selectedOttProvider)
            {
                _selectedOttProvider = null;
                SetEditMode(false);
                return;
            }

            _selectedOttProvider = selectedOttProvider;

            OttNameTextBox.Text = selectedOttProvider.OTT_NAME;
            MonthlyPriceTextBox.Text = selectedOttProvider.monthly_price.ToString();

            SetEditMode(true);
        }

        // 입력 폼의 값으로 새 OTT 제공자를 추가합니다.
        private void AddOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ottName = OttNameTextBox.Text.Trim();
                int monthlyPrice = GetMonthlyPrice(MonthlyPriceTextBox.Text);

                if (string.IsNullOrWhiteSpace(ottName))
                {
                    MessageBox.Show("OTT 이름을 입력하세요.");
                    OttNameTextBox.Focus();
                    return;
                }

                _contentMappingService.AddOttProvider(
                    ottName,
                    monthlyPrice
                );

                MessageBox.Show(
                    "OTT 제공자가 추가되었습니다.",
                    "OTT 추가 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadOttProviderList();
                ClearOttProviderForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 제공자 추가 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 추가 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 선택된 OTT 제공자를 입력 폼의 값으로 수정합니다.
        private void UpdateOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOttProvider == null)
            {
                MessageBox.Show("수정할 OTT 제공자를 선택하세요.");
                return;
            }

            try
            {
                string ottName = OttNameTextBox.Text.Trim();
                int monthlyPrice = GetMonthlyPrice(MonthlyPriceTextBox.Text);

                if (string.IsNullOrWhiteSpace(ottName))
                {
                    MessageBox.Show("OTT 이름을 입력하세요.");
                    OttNameTextBox.Focus();
                    return;
                }

                _contentMappingService.UpdateOttProvider(
                    _selectedOttProvider.OTT_ID,
                    ottName,
                    monthlyPrice
                );

                MessageBox.Show(
                    "OTT 제공자가 수정되었습니다.",
                    "OTT 수정 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadOttProviderList();
                ClearOttProviderForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 제공자 수정 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 수정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 선택된 OTT 제공자를 삭제합니다. 연결 데이터가 있으면 강제 삭제 여부를 한 번 더 확인합니다.
        private void DeleteOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOttProvider == null)
            {
                MessageBox.Show("삭제할 OTT 제공자를 선택하세요.");
                return;
            }

            MessageBoxResult firstConfirm = MessageBox.Show(
                $"선택한 OTT 제공자를 삭제하시겠습니까?\n\nOTT 이름: {_selectedOttProvider.OTT_NAME}\n월 구독료: {_selectedOttProvider.monthly_price:N0}원",
                "OTT 삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (firstConfirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OttProviderDeleteResultModel result =
                    _contentMappingService.DeleteOttProviderWithRelations(
                        _selectedOttProvider.OTT_ID,
                        false
                    );

                MessageBox.Show(
                    "OTT 제공자가 삭제되었습니다.",
                    "OTT 삭제 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadOttProviderList();
                ClearOttProviderForm();
            }
            catch (SqlException ex) when (ex.Number == 50002)
            {
                MessageBoxResult relationConfirm = MessageBox.Show(
                    ex.Message
                    + "\n\n주의: 계속하면 콘텐츠 연결, 사용자 구독 정보, 시청 기록까지 함께 삭제됩니다."
                    + "\n\n정말 관련 정보까지 모두 삭제하시겠습니까?",
                    "연결 데이터 존재",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (relationConfirm != MessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    OttProviderDeleteResultModel forceResult =
                        _contentMappingService.DeleteOttProviderWithRelations(
                            _selectedOttProvider.OTT_ID,
                            true
                        );

                    MessageBox.Show(
                        "OTT 제공자가 삭제되었습니다.\n\n"
                        + $"삭제된 시청 기록: {forceResult.Delete_WatchHistory_Count:N0}개\n"
                        + $"삭제된 구독 정보: {forceResult.Delete_UserSubscription_Count:N0}개\n"
                        + $"삭제된 콘텐츠 연결: {forceResult.Delete_ContentOTTProvider_Count:N0}개",
                        "OTT 삭제 완료",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LoadOttProviderList();
                    ClearOttProviderForm();
                }
                catch (Exception forceEx)
                {
                    MessageBox.Show(
                        "OTT 제공자 강제 삭제 중 오류가 발생했습니다.\n\n" + forceEx.Message,
                        "OTT 삭제 오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 제공자 삭제 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 삭제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ClearOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            ClearOttProviderForm();
        }

        // 입력 폼과 선택 상태를 초기화하고 추가 모드로 되돌립니다.
        private void ClearOttProviderForm()
        {
            OttProviderDataGrid.SelectedItem = null;

            _selectedOttProvider = null;

            OttNameTextBox.Clear();
            MonthlyPriceTextBox.Clear();

            SetEditMode(false);

            OttNameTextBox.Focus();
        }

        // 수정 모드 여부에 따라 버튼 활성화와 색상을 함께 변경합니다.
        private void SetEditMode(bool isEditMode)
        {
            AddOttProviderButton.IsEnabled = !isEditMode;
            UpdateOttProviderButton.IsEnabled = isEditMode;
            DeleteOttProviderButton.IsEnabled = isEditMode;

            if (isEditMode)
            {
                AddOttProviderButton.Background = BrushFromHex("#ECEAF1");
                AddOttProviderButton.BorderBrush = BrushFromHex("#ECEAF1");
                AddOttProviderButton.Foreground = BrushFromHex("#AAA4B5");

                UpdateOttProviderButton.Background = BrushFromHex("#3B82E6");
                UpdateOttProviderButton.BorderBrush = BrushFromHex("#3B82E6");
                UpdateOttProviderButton.Foreground = Brushes.White;

                DeleteOttProviderButton.Background = BrushFromHex("#D9365B");
                DeleteOttProviderButton.BorderBrush = BrushFromHex("#D9365B");
                DeleteOttProviderButton.Foreground = Brushes.White;
            }
            else
            {
                AddOttProviderButton.Background = BrushFromHex("#3B82E6");
                AddOttProviderButton.BorderBrush = BrushFromHex("#3B82E6");
                AddOttProviderButton.Foreground = Brushes.White;

                UpdateOttProviderButton.Background = BrushFromHex("#ECEAF1");
                UpdateOttProviderButton.BorderBrush = BrushFromHex("#ECEAF1");
                UpdateOttProviderButton.Foreground = BrushFromHex("#AAA4B5");

                DeleteOttProviderButton.Background = BrushFromHex("#ECEAF1");
                DeleteOttProviderButton.BorderBrush = BrushFromHex("#ECEAF1");
                DeleteOttProviderButton.Foreground = BrushFromHex("#AAA4B5");
            }
        }

        // 월 구독료 입력값을 숫자로 변환하고 음수 입력을 막습니다.
        private int GetMonthlyPrice(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("월 구독료를 입력하세요.");
            }

            if (!int.TryParse(text.Trim(), out int price))
            {
                throw new Exception("월 구독료는 숫자만 입력할 수 있습니다.");
            }

            if (price < 0)
            {
                throw new Exception("월 구독료는 음수일 수 없습니다.");
            }

            return price;
        }

        private SolidColorBrush BrushFromHex(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex)
            );
        }
    }
}
