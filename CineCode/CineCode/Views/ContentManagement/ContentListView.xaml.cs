using System.Collections;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using CineCode.Models.Content;
using CineCode.Services;

namespace CineCode.Views.ContentManagement
{
    public partial class ContentListView : UserControl
    {
        // 콘텐츠 목록 조회와 CSV 저장 경로 조회에 사용하는 서비스
        private readonly ContentService _contentService = new ContentService();
        private readonly SystemSettingService _systemSettingService = new SystemSettingService();

        // 전체 콘텐츠 목록과 검색/필터가 적용된 목록
        private List<ContentListItemModel> _contents = new List<ContentListItemModel>();
        private List<ContentListItemModel> _filteredContents = new List<ContentListItemModel>();

        // DataGrid 인라인 수정 중 원래 값을 되돌리기 위한 임시 저장 값
        private ContentListItemModel? _editingContent = null;

        private string _originalTitle = "";
        private string _originalReleaseDate = "";
        private int? _originalRunningTime = null;
        private string _originalPosterUrl = "";
        private string _originalOverview = "";

        private bool _hasPendingEdit = false;
        private bool _isEditApplyButtonPressed = false;

        // 화면 초기화 후 콘텐츠 목록을 불러오고 버튼 상태를 맞춤
        public ContentListView()
        {
            InitializeComponent();

            LoadContentList();
            UpdateSelectionButtonState();
        }

        // DB에서 콘텐츠 목록을 다시 읽고 기본 필터 상태로 화면에 표시
        private void LoadContentList()
        {
            try
            {
                _contents = _contentService.GetContentList();

                ApplyFilter("", "All");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "콘텐츠 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "콘텐츠 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 검색어와 콘텐츠 타입 조건을 적용해 DataGrid 목록을 갱신
        public void ApplyFilter(string keyword, string type)
        {
            if (_contents == null)
            {
                ContentDataGrid.ItemsSource = null;
                TotalContentCountText.Text = "총 0개 항목";
                return;
            }

            keyword = keyword.Trim().ToLower();
            type = type.Trim().ToUpper();

            _filteredContents = _contents
                .Where(content =>
                {
                    // 검색어는 ID, 제목, 타입, 출시일, 관람등급, TMDB 점수 중 하나라도 포함되면 통과
                    bool keywordMatched =
                        string.IsNullOrWhiteSpace(keyword)
                        || content.ContentID.ToString().Contains(keyword)
                        || (content.Title ?? "").ToLower().Contains(keyword)
                        || (content.Type ?? "").ToLower().Contains(keyword)
                        || (content.ReleaseDate ?? "").ToLower().Contains(keyword)
                        || (content.AgeRating ?? "").ToLower().Contains(keyword)
                        || (content.TmdbScore ?? "").ToLower().Contains(keyword);

                    bool typeMatched =
                        type == "ALL"
                        || string.IsNullOrWhiteSpace(type)
                        || content.Type.Equals(type, StringComparison.OrdinalIgnoreCase);

                    return keywordMatched && typeMatched;
                })
                .ToList();

            ContentDataGrid.ItemsSource = null;
            ContentDataGrid.ItemsSource = _filteredContents;

            TotalContentCountText.Text = $"총 {_filteredContents.Count}개 항목";

            UpdateSelectionButtonState();
        }

        // 선택된 콘텐츠 수가 바뀔 때 수정/삭제 버튼 활성화 상태를 갱신
        private void ContentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionButtonState();
        }

        // 선택 개수에 따라 수정은 1개 선택일 때만, 삭제는 1개 이상 선택일 때만 허용
        private void UpdateSelectionButtonState()
        {
            int selectedCount = ContentDataGrid.SelectedItems.Count;

            if (selectedCount == 0)
            {
                EditContentButton.IsEnabled = false;
                DeleteContentButton.IsEnabled = false;
                return;
            }

            if (selectedCount == 1)
            {
                EditContentButton.IsEnabled = true;
                DeleteContentButton.IsEnabled = true;
                return;
            }

            EditContentButton.IsEnabled = false;
            DeleteContentButton.IsEnabled = true;
        }

        // 선택한 콘텐츠 1개의 수정 내용을 저장
        private void EditContentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContentDataGrid.SelectedItems.Count != 1)
                {
                    MessageBox.Show("수정은 콘텐츠를 1개만 선택했을 때 가능합니다.");
                    _isEditApplyButtonPressed = false;
                    return;
                }

                if (ContentDataGrid.SelectedItem is not ContentListItemModel selectedContent)
                {
                    MessageBox.Show("수정할 콘텐츠를 선택하세요.");
                    _isEditApplyButtonPressed = false;
                    return;
                }

                // 현재 편집 중인 셀 값을 모델에 반영
                ContentDataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                ContentDataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                if (string.IsNullOrWhiteSpace(selectedContent.Title))
                {
                    MessageBox.Show("Title은 필수입니다.");
                    RevertPendingEdit();
                    return;
                }

                DateTime? releaseDate = GetNullableDate(selectedContent.ReleaseDate);

                _contentService.UpdateContent(
                    selectedContent.ContentID,
                    selectedContent.Title.Trim(),
                    releaseDate,
                    selectedContent.Overview,
                    selectedContent.PosterUrl,
                    selectedContent.RunningTime
                );

                MessageBox.Show(
                    "콘텐츠 정보가 수정되었습니다.",
                    "콘텐츠 수정 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ClearEditState();
                LoadContentList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "콘텐츠 수정 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "콘텐츠 수정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                RevertPendingEdit();
            }
            finally
            {
                _isEditApplyButtonPressed = false;
            }
        }

        // 선택한 콘텐츠를 삭제하고 목록을 다시 조회
        private void DeleteContentButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show("삭제할 콘텐츠를 선택하세요.");
                return;
            }

            List<ContentListItemModel> selectedContents = ContentDataGrid.SelectedItems
                .Cast<ContentListItemModel>()
                .ToList();

            string message;

            if (selectedContents.Count == 1)
            {
                message =
                    $"선택한 콘텐츠를 삭제하시겠습니까?\n\n" +
                    $"제목: {selectedContents[0].Title}";
            }
            else
            {
                message =
                    $"선택한 콘텐츠 {selectedContents.Count}개를 삭제하시겠습니까?\n\n" +
                    "삭제하면 콘텐츠의 장르 연결, OTT 연결, 시청 기록, 평점 정보도 함께 삭제될 수 있습니다.";
            }

            MessageBoxResult result = MessageBox.Show(
                message,
                "콘텐츠 삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                List<int> contentIds = selectedContents
                    .Select(content => content.ContentID)
                    .ToList();

                _contentService.DeleteContents(contentIds);

                MessageBox.Show(
                    "콘텐츠가 삭제되었습니다.",
                    "콘텐츠 삭제 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadContentList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "콘텐츠 삭제 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "콘텐츠 삭제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // DataGrid 편집을 시작할 때 원래 값을 저장해 두었다가 취소 시 복구
        private void ContentDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is not ContentListItemModel content)
            {
                return;
            }

            // 다른 콘텐츠를 수정하려고 하면 기존 수정 내용은 취소
            if (_hasPendingEdit && _editingContent != null && !ReferenceEquals(_editingContent, content))
            {
                RevertPendingEdit();
            }

            CaptureOriginalValues(content);
        }

        // 수정 적용 버튼이 아닌 방식으로 셀 편집이 끝나면 원래 값으로 되돌림
        private void ContentDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            if (!_isEditApplyButtonPressed)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!_isEditApplyButtonPressed)
                    {
                        RevertPendingEdit();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // 수정 버튼 클릭으로 편집이 끝나는 경우에는 복구하지 않고 저장 흐름으로 보냄
        private void EditContentButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isEditApplyButtonPressed = true;
        }

        // 현재 편집 대상의 원래 값을 저장
        private void CaptureOriginalValues(ContentListItemModel content)
        {
            _editingContent = content;

            _originalTitle = content.Title;
            _originalReleaseDate = content.ReleaseDate;
            _originalRunningTime = content.RunningTime;
            _originalPosterUrl = content.PosterUrl;
            _originalOverview = content.Overview;

            _hasPendingEdit = true;
        }

        // 저장하지 않은 편집 내용을 원래 값으로 되돌림
        private void RevertPendingEdit()
        {
            if (_editingContent == null)
            {
                ClearEditState();
                return;
            }

            _editingContent.Title = _originalTitle;
            _editingContent.ReleaseDate = _originalReleaseDate;
            _editingContent.RunningTime = _originalRunningTime;
            _editingContent.PosterUrl = _originalPosterUrl;
            _editingContent.Overview = _originalOverview;

            ClearEditState();
        }

        // 편집 추적 상태와 원본값 캐시를 초기화
        private void ClearEditState()
        {
            _editingContent = null;

            _originalTitle = "";
            _originalReleaseDate = "";
            _originalRunningTime = null;
            _originalPosterUrl = "";
            _originalOverview = "";

            _hasPendingEdit = false;
            _isEditApplyButtonPressed = false;
        }

        // 현재 DataGrid에 표시 중인 콘텐츠 목록을 CSV 파일로 저장
        public void ExportToCsv()
        {
            try
            {
                if (ContentDataGrid.ItemsSource == null)
                {
                    MessageBox.Show("CSV로 추출할 콘텐츠 목록이 없습니다.");
                    return;
                }

                IEnumerable items = ContentDataGrid.ItemsSource;

                List<ContentListItemModel> exportItems =
                    items.Cast<ContentListItemModel>().ToList();

                if (exportItems.Count == 0)
                {
                    MessageBox.Show("CSV로 추출할 콘텐츠 목록이 없습니다.");
                    return;
                }

                string exportPath = _systemSettingService.GetCsvExportPath();

                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                string fileName = $"ContentList_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string fullPath = Path.Combine(exportPath, fileName);

                using StreamWriter writer = new StreamWriter(
                    fullPath,
                    false,
                    new UTF8Encoding(true)
                );

                writer.WriteLine(
                    "Content ID,Title,Type,Release Date,Running Time,TMDB Score,Age Rating,OTT Count,Poster URL,Overview"
                );

                foreach (ContentListItemModel content in exportItems)
                {
                    string line = string.Join(",",
                        EscapeCsv(content.ContentID),
                        EscapeCsv(content.Title),
                        EscapeCsv(content.Type),
                        EscapeCsv(content.ReleaseDate),
                        EscapeCsv(content.RunningTime.HasValue ? content.RunningTime.Value.ToString() : ""),
                        EscapeCsv(content.TmdbScore),
                        EscapeCsv(content.AgeRating),
                        EscapeCsv(content.OttCount),
                        EscapeCsv(content.PosterUrl),
                        EscapeCsv(content.Overview)
                    );

                    writer.WriteLine(line);
                }

                MessageBox.Show(
                    "CSV 파일이 저장되었습니다.\n\n" + fullPath,
                    "CSV 추출 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "CSV 추출 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "CSV 추출 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // CSV에서 쉼표, 줄바꿈, 큰따옴표가 컬럼 구조를 깨지 않도록 변환
        private string EscapeCsv(object? value)
        {
            string text = value?.ToString() ?? "";

            text = text.Replace("\r\n", " ");
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");

            text = text.Replace("\"", "\"\"");

            return $"\"{text}\"";
        }

        // 빈 날짜는 null로 두고, 값이 있으면 DateTime으로 변환
        private DateTime? GetNullableDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string trimmedText = text.Trim();

            if (DateTime.TryParse(trimmedText, out DateTime date))
            {
                return date;
            }

            throw new Exception("날짜 형식이 올바르지 않습니다. 예: 2024-01-01");
        }
    }
}

