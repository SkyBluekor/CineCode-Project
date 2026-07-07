using System.Windows;
using System.Windows.Controls;
using CineCode.Views.ContentManagement;

namespace CineCode.Views
{
    public partial class ContentManagementView : UserControl
    {
        // 현재 본문 영역에 표시 중인 콘텐츠 목록 화면입니다.
        private ContentListView? _contentListView;

        public ContentManagementView()
        {
            InitializeComponent();

            ShowContentList();
        }

        // 콘텐츠 목록 화면을 본문 영역에 표시하고 현재 검색/타입 필터를 적용합니다.
        private void ShowContentList()
        {
            _contentListView = new ContentListView();

            ContentBodyArea.Content = _contentListView;

            ExportCsvButton.IsEnabled = true;

            ApplyContentFilter();
        }

        // 콘텐츠 추가 화면으로 전환하고, 목록 전용 기능인 CSV 추출 버튼은 비활성화합니다.
        private void AddContentButton_Click(object sender, RoutedEventArgs e)
        {
            ContentAddView addView = new ContentAddView();

            // 추가 화면에서 목록으로 돌아가야 할 때 다시 목록 화면을 표시합니다.
            addView.BackToListRequested += ShowContentList;

            ContentBodyArea.Content = addView;

            ExportCsvButton.IsEnabled = false;
        }

        // 현재 콘텐츠 목록 화면에 표시된 데이터를 CSV로 추출합니다.
        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (_contentListView == null)
            {
                MessageBox.Show("CSV로 추출할 콘텐츠 목록이 없습니다.");
                return;
            }

            _contentListView.ExportToCsv();
        }

        private void ContentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyContentFilter();
        }

        private void ContentTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyContentFilter();
        }

        // 검색어와 콘텐츠 타입 필터를 콘텐츠 목록 화면에 전달합니다.
        private void ApplyContentFilter()
        {
            if (_contentListView == null)
            {
                return;
            }

            string keyword = ContentSearchTextBox.Text.Trim();
            string type = GetSelectedTypeFilter();

            _contentListView.ApplyFilter(keyword, type);
        }

        // ComboBox에서 선택된 타입 필터 텍스트를 가져옵니다.
        private string GetSelectedTypeFilter()
        {
            if (ContentTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "All";
            }

            return "All";
        }
    }
}
