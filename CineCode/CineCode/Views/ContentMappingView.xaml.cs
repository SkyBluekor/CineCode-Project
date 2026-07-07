using CineCode.Models.Mapping;
using CineCode.Services;
using CineCode.Views.ContentMapping;
using System.Windows;
using System.Windows.Controls;

namespace CineCode.Views
{
    public partial class ContentMappingView : UserControl
    {
        // 콘텐츠 매핑 화면의 요약 정보와 관리 데이터를 가져오는 서비스입니다.
        private readonly ContentMappingService _contentMappingService = new ContentMappingService();

        public ContentMappingView()
        {
            InitializeComponent();

            // 화면 진입 시 상단 요약 카드와 기본 하위 화면을 먼저 구성합니다.
            LoadContentMappingSummary();

            MappingBodyArea.Content = new GenreManageView();
        }

        // 장르, OTT, 매핑 누락 콘텐츠 수를 조회해서 상단 요약 카드에 표시합니다.
        private void LoadContentMappingSummary()
        {
            try
            {
                ContentMappingSummaryModel summary =
                    _contentMappingService.GetContentMappingSummary();

                TotalGenreCountText.Text = summary.RegisteredGenreCount.ToString("N0");
                TotalOttProviderCountText.Text = summary.OttProviderCount.ToString("N0");
                MissingMappingCountText.Text = summary.UnmappedContentCount.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Content Mapping 요약 정보를 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "요약 정보 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // Genre 관리 탭을 선택하고 장르 관리 화면을 본문 영역에 표시합니다.
        private void GenreTabButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTab(GenreTabButton);
            MappingBodyArea.Content = new GenreManageView();
        }

        // OTT Provider 관리 탭을 선택하고 OTT 관리 화면을 본문 영역에 표시합니다.
        private void OttProviderTabButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTab(OttProviderTabButton);
            MappingBodyArea.Content = new OttProviderManageView();
        }

        // 콘텐츠 연결 관리 탭을 선택하고 콘텐츠-장르/OTT 연결 화면을 표시합니다.
        private void ContentRelationTabButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTab(ContentRelationTabButton);
            MappingBodyArea.Content = new ContentRelationManageView();
        }

        // XAML 스타일 트리거가 Tag="Selected" 값을 기준으로 선택된 탭 색상을 바꿉니다.
        private void SelectTab(Button selectedButton)
        {
            GenreTabButton.Tag = null;
            OttProviderTabButton.Tag = null;
            ContentRelationTabButton.Tag = null;

            selectedButton.Tag = "Selected";
        }
    }
}
