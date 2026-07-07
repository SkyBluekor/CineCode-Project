using System.Windows;
using CineCode.Views;

namespace CineCode
{
    /// 애플리케이션의 메인 화면과 메뉴별 콘텐츠 전환을 관리합니다.
    public partial class MainWindow : Window
    {
        /// 메인 창을 초기화하고 대시보드를 기본 화면으로 표시합니다.
        public MainWindow()
        {
            InitializeComponent();
            MainContentArea.Content = new DashboardView();
        }

        // 사이드바 메뉴 클릭 시 선택한 화면으로 콘텐츠를 교체합니다.
        private void DashboardMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new DashboardView();
        }

        private void ContentManagementMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ContentManagementView();
        }

        private void ContentMappingMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ContentMappingView();
        }

        private void SubScriptionsMenuButton_Click(Object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new SubscriptionsView();
        }

        private void WatchHistoryMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new WatchHistoryView();
        }

        private void UserRatingMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new UserRatingsView();
        }

        private void StatisticsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new StatisticsView();
        }

        private void UserManagementMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new UserManagementView();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            // 설정 창의 소유자를 메인 창으로 지정한 후 모달로 표시합니다.
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        /// 전달받은 페이지 이름에 맞는 화면으로 이동합니다.
        /// <param name="pageName">이동할 페이지의 이름입니다.</param>
        public void NavigateTo(string pageName)
        {
            switch (pageName)
            {
                case "Dashboard":
                    MainContentArea.Content = new DashboardView();
                    break;

                case "ContentManagement":
                    MainContentArea.Content = new ContentManagementView();
                    break;

                case "ContentMapping":
                    MainContentArea.Content = new ContentMappingView();
                    break;

                case "Subscriptions":
                    MainContentArea.Content = new SubscriptionsView();
                    break;

                case "WatchHistory":
                    MainContentArea.Content = new WatchHistoryView();
                    break;

                case "UserRatings":
                    MainContentArea.Content = new UserRatingsView();
                    break;

                case "Statistics":
                    MainContentArea.Content = new StatisticsView();
                    break;

                case "UserManagement":
                    MainContentArea.Content = new UserManagementView();
                    break;
            }
        }
    }
}
