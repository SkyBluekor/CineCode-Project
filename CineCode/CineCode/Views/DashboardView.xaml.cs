using System.Windows;
using System.Windows.Controls;
using CineCode.Model.DashBoard;
using CineCode.Services;

namespace CineCode.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly DashboardService _dashboardService = new DashboardService();

        // 대시보드 화면을 초기화하고 로드 완료 시점에 데이터를 조회합니다.
        public DashboardView()
        {
            InitializeComponent();

            Loaded += DashboardView_Loaded;
        }

        // 화면이 로드되면 최신 대시보드 데이터를 표시합니다.
        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        // 빠른 실행 버튼의 Tag 값에 따라 해당 관리 화면으로 이동하거나 CSV 추출 창을 엽니다.
        private void QuickActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            string target = button.Tag?.ToString() ?? "";
            Window window = Window.GetWindow(this);

            if (window is not MainWindow mainWindow)
            {
                return;
            }

            // XAML 버튼 Tag와 MainWindow의 화면 키를 연결합니다.
            switch (target)
            {
                case "ContentManagement":
                    mainWindow.NavigateTo("ContentManagement");
                    break;

                case "UserManagement":
                    mainWindow.NavigateTo("UserManagement");
                    break;

                case "ContentMapping":
                    mainWindow.NavigateTo("ContentMapping");
                    break;

                case "CsvExport":
                    CsvExportWindow csvExportWindow = new CsvExportWindow();
                    csvExportWindow.Owner = Window.GetWindow(this);
                    csvExportWindow.ShowDialog();
                    break;
            }
        }

        // 서비스에서 조회한 요약 데이터를 화면의 텍스트 영역에 반영합니다.
        private void LoadDashboard()
        {
            try
            {
                DashboardModel dashboard = _dashboardService.GetDashboardData();

                TotalContentsText.Text = dashboard.TotalContentCount.ToString("N0");
                TotalUsersText.Text = dashboard.TotalUserCount.ToString("N0");
                ActiveSubscriptionText.Text = dashboard.ActiveSubscriptionCount.ToString("N0");
                TotalOTTProviderText.Text = dashboard.OttProviderCount.ToString("N0");

                MissingGenreLinksText.Text = dashboard.UnmappedGenreContentCount.ToString("N0");
                MissingOTTLinksText.Text = dashboard.UnmappedOttContentCount.ToString("N0");
                ExpiringSoonText.Text = dashboard.ExpiringSubscriptionCount.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "대시보드 정보를 불러오는 중 오ꇜ가 발생했습니다. \n\n" + ex.Message,
                    "대시보드 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
