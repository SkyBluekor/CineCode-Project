using System.Windows;
using System.Windows.Controls;
using CineCode.Models.Mapping;
using CineCode.Services;

namespace CineCode.Views.ContentMapping
{
    public partial class ContentRelationManageView : UserControl
    {
        // 콘텐츠-장르/OTT 연결 정보를 처리하는 서비스입니다.
        private readonly ContentMappingService _contentMappingService = new ContentMappingService();

        // 화면 왼쪽 콘텐츠 목록과 현재 선택된 콘텐츠를 보관합니다.
        private List<ContentMappingContentItemModel> _contents = new List<ContentMappingContentItemModel>();
        private ContentMappingContentItemModel? _selectedContent = null;

        // 선택된 콘텐츠 기준으로 표시되는 장르 목록과 선택 상태입니다.
        private List<ContentMappingGenreItemModel> _genres = new List<ContentMappingGenreItemModel>();
        private ContentMappingGenreItemModel? _selectedGenre = null;

        // 선택된 콘텐츠 기준으로 표시되는 OTT 목록과 선택 상태입니다.
        private List<ContentMappingOttItemModel> _otts = new List<ContentMappingOttItemModel>();
        private ContentMappingOttItemModel? _selectedOtt = null;

        public ContentRelationManageView()
        {
            InitializeComponent();

            LoadContentList();
        }

        // 화면 진입 시 콘텐츠 목록을 불러오고 검색 조건을 적용합니다.
        private void LoadContentList()
        {
            try
            {
                _contents = _contentMappingService.GetContentMappingContentList();

                ApplyContentSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "콘텐츠 목록을 불러오는 중 오류가 발생했습니다. \n\n" + ex.Message,
                    "콘텐츠 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 검색어가 있으면 콘텐츠 ID, 타입, 제목, 매핑 상태를 기준으로 목록을 거릅니다.
        private void ApplyContentSearch()
        {
            if (_contents == null)
            {
                ContentDataGrid.ItemsSource = null;
                return;
            }

            string keyword = ContentSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ContentDataGrid.ItemsSource = null;
                ContentDataGrid.ItemsSource = _contents;
                return;
            }

            keyword = keyword.ToLower();

            List<ContentMappingContentItemModel> filteredContents = _contents.Where(content =>
                content.Content_ID.ToString().Contains(keyword)
                || content.Content_Type.ToLower().Contains(keyword)
                || content.Title.ToLower().Contains(keyword)
                || content.MappingStatus.ToLower().Contains(keyword)
            ).ToList();

            ContentDataGrid.ItemsSource = null;
            ContentDataGrid.ItemsSource = filteredContents;
        }

        private void ContentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyContentSearch();
        }

        // 콘텐츠를 선택하면 오른쪽 장르/OTT 연결 목록을 새로 불러옵니다.
        private void ContentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentDataGrid.SelectedItem is not ContentMappingContentItemModel selectedContent)
            {
                _selectedContent = null;

                SelectedContentInfoText.Text = "선택된 콘텐츠 없음";

                ContentGenreDataGrid.ItemsSource = null;
                ContentOttDataGrid.ItemsSource = null;

                SetRelationButtons(false);
                return;
            }

            _selectedContent = selectedContent;

            SelectedContentInfoText.Text =
                $"선택된 콘텐츠: [{selectedContent.Content_ID}] {selectedContent.Title} / {selectedContent.Content_Type}";

            LoadGenreRelationList();
            LoadOttRelationList();

            UpdateGenreButtonState();
            UpdateOttButtonState();
        }

        // 선택된 콘텐츠와 같은 타입의 장르 목록을 불러오고 연결 여부를 표시합니다.
        private void LoadGenreRelationList()
        {
            if (_selectedContent == null)
            {
                ContentGenreDataGrid.ItemsSource = null;
                return;
            }

            try
            {
                _genres = _contentMappingService.GetContentMappingGenreList(
                    _selectedContent.Content_ID,
                    _selectedContent.Content_Type
                );

                ContentGenreDataGrid.ItemsSource = null;
                ContentGenreDataGrid.ItemsSource = _genres;

                _selectedGenre = null;
                ContentGenreDataGrid.SelectedItem = null;

                UpdateGenreButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 연결 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 연결 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 선택된 콘텐츠에 연결 가능한 OTT 목록을 불러오고 연결 여부를 표시합니다.
        private void LoadOttRelationList()
        {
            if (_selectedContent == null)
            {
                ContentOttDataGrid.ItemsSource = null;
                return;
            }

            try
            {
                _otts = _contentMappingService.GetContentMappingOttList(
                    _selectedContent.Content_ID
                );

                ContentOttDataGrid.ItemsSource = null;
                ContentOttDataGrid.ItemsSource = _otts;

                _selectedOtt = null;
                ContentOttDataGrid.SelectedItem = null;

                UpdateOttButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 연결 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 연결 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ContentGenreDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentGenreDataGrid.SelectedItem is ContentMappingGenreItemModel selectedGenre)
            {
                _selectedGenre = selectedGenre;
            }
            else
            {
                _selectedGenre = null;
            }

            UpdateGenreButtonState();
        }

        private void ContentOttDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentOttDataGrid.SelectedItem is ContentMappingOttItemModel selectedOtt)
            {
                _selectedOtt = selectedOtt;
            }
            else
            {
                _selectedOtt = null;
            }

            UpdateOttButtonState();
        }

        // 장르 연결 버튼: 선택된 콘텐츠와 장르를 연결합니다.
        private void LinkGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContent == null)
            {
                MessageBox.Show("콘텐츠를 먼저 선택하세요.");
                return;
            }

            if (_selectedGenre == null)
            {
                MessageBox.Show("연결할 장르를 선택하세요.");
                return;
            }

            if (_selectedGenre.IsLinked)
            {
                MessageBox.Show("이미 연결된 장르입니다.");
                return;
            }

            try
            {
                int contentId = _selectedContent.Content_ID;

                _contentMappingService.LinkGenreToContent(
                    _selectedContent.Content_ID,
                    _selectedGenre.Genre_ID
                );

                RefreshContentAndKeepSelection(contentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 연결 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 연결 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 장르 연결 해제 버튼: 사용자 확인 후 선택된 장르 연결을 해제합니다.
        private void UnlinkGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContent == null)
            {
                MessageBox.Show("콘텐츠를 먼저 선택하세요.");
                return;
            }

            if (_selectedGenre == null)
            {
                MessageBox.Show("연결 해제할 장르를 선택하세요.");
                return;
            }

            if (!_selectedGenre.IsLinked)
            {
                MessageBox.Show("연결되지 않은 장르입니다.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"선택한 장르 연결을 해제하시겠습니까?\n\n장르: {_selectedGenre.Genre_Name}",
                "장르 연결 해제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int contentId = _selectedContent.Content_ID;

                _contentMappingService.UnlinkGenreFromContent(
                    _selectedContent.Content_ID,
                    _selectedGenre.Genre_ID
                );

                RefreshContentAndKeepSelection(contentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 연결 해제 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 연결 해제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // OTT 연결 버튼: 선택된 콘텐츠와 OTT 제공자를 연결합니다.
        private void LinkOttButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContent == null)
            {
                MessageBox.Show("콘텐츠를 먼저 선택하세요.");
                return;
            }

            if (_selectedOtt == null)
            {
                MessageBox.Show("연결할 OTT를 선택하세요.");
                return;
            }

            if (_selectedOtt.IsLinked)
            {
                MessageBox.Show("이미 연결된 OTT입니다.");
                return;
            }

            try
            {
                int contentId = _selectedContent.Content_ID;

                _contentMappingService.LinkOttToContent(
                    _selectedContent.Content_ID,
                    _selectedOtt.OTT_ID
                );

                RefreshContentAndKeepSelection(contentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 연결 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 연결 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // OTT 연결 해제 버튼: 사용자 확인 후 선택된 OTT 연결을 해제합니다.
        private void UnlinkOttButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContent == null)
            {
                MessageBox.Show("콘텐츠를 먼저 선택하세요.");
                return;
            }

            if (_selectedOtt == null)
            {
                MessageBox.Show("연결 해제할 OTT를 선택하세요.");
                return;
            }

            if (!_selectedOtt.IsLinked)
            {
                MessageBox.Show("연결되지 않은 OTT입니다.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"선택한 OTT 연결을 해제하시겠습니까?\n\nOTT: {_selectedOtt.OTT_NAME}",
                "OTT 연결 해제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int contentId = _selectedContent.Content_ID;

                _contentMappingService.UnlinkOttFromContent(
                    _selectedContent.Content_ID,
                    _selectedOtt.OTT_ID
                );

                RefreshContentAndKeepSelection(contentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT 연결 해제 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 연결 해제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 콘텐츠가 선택되지 않았을 때 모든 연결 버튼을 한 번에 비활성화합니다.
        private void SetRelationButtons(bool isEnabled)
        {
            LinkGenreButton.IsEnabled = isEnabled;
            UnlinkGenreButton.IsEnabled = isEnabled;
            LinkOttButton.IsEnabled = isEnabled;
            UnlinkOttButton.IsEnabled = isEnabled;
        }

        // 이미 연결된 장르는 해제 버튼만, 연결되지 않은 장르는 연결 버튼만 활성화합니다.
        private void UpdateGenreButtonState()
        {
            if (_selectedContent == null || _selectedGenre == null)
            {
                LinkGenreButton.IsEnabled = false;
                UnlinkGenreButton.IsEnabled = false;
                return;
            }

            if (_selectedGenre.IsLinked)
            {
                LinkGenreButton.IsEnabled = false;
                UnlinkGenreButton.IsEnabled = true;
            }
            else
            {
                LinkGenreButton.IsEnabled = true;
                UnlinkGenreButton.IsEnabled = false;
            }
        }

        // 이미 연결된 OTT는 해제 버튼만, 연결되지 않은 OTT는 연결 버튼만 활성화합니다.
        private void UpdateOttButtonState()
        {
            if (_selectedContent == null || _selectedOtt == null)
            {
                LinkOttButton.IsEnabled = false;
                UnlinkOttButton.IsEnabled = false;
                return;
            }

            if (_selectedOtt.IsLinked)
            {
                LinkOttButton.IsEnabled = false;
                UnlinkOttButton.IsEnabled = true;
            }
            else
            {
                LinkOttButton.IsEnabled = true;
                UnlinkOttButton.IsEnabled = false;
            }
        }

        // 연결 변경 후 콘텐츠 목록을 다시 읽고, 방금 수정한 콘텐츠 선택을 유지합니다.
        private void RefreshContentAndKeepSelection(int contentId)
        {
            LoadContentList();

            if (ContentDataGrid.ItemsSource is not IEnumerable<ContentMappingContentItemModel> currentItems)
            {
                return;
            }

            ContentMappingContentItemModel? restoredContent = currentItems
                .FirstOrDefault(content => content.Content_ID == contentId);

            if (restoredContent == null)
            {
                _selectedContent = null;

                SelectedContentInfoText.Text = "선택된 콘텐츠 없음";
                ContentGenreDataGrid.ItemsSource = null;

                SetRelationButtons(false);
                return;
            }

            ContentDataGrid.SelectedItem = restoredContent;
            ContentDataGrid.ScrollIntoView(restoredContent);
        }
    }
}
