using CineCode.Models.Mapping;
using CineCode.Models.Tmdb;
using CineCode.Services;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CineCode.Views.ContentMapping
{
    public partial class GenreManageView : UserControl
    {
        // 현재 선택된 장르입니다. null이면 추가 모드로 동작합니다.
        private GenreListItemModel? _selectedGenre = null;

        // TMDB API는 실제로 필요할 때만 생성합니다.
        private TmdbService? _tmdbService = null;

        // 장르 데이터 조회/추가/수정/삭제를 담당하는 서비스입니다.
        private readonly ContentMappingService _contentMappingService = new ContentMappingService();

        // 화면에 표시할 전체 장르 목록입니다. 검색 시 이 목록에서 필터링합니다.
        private List<GenreListItemModel> _genres = new List<GenreListItemModel>();

        public GenreManageView()
        {
            InitializeComponent();

            Loaded += GenreManageView_Loaded;

            SetEditMode(false);
        }

        private void GenreManageView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenreList();
        }

        // 장르 목록을 불러온 뒤 현재 검색어를 다시 적용합니다.
        private void LoadGenreList()
        {
            try
            {
                _genres = _contentMappingService.GetGenreList();

                ApplyGenreSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 목록을 불러오는 중 오류가 발생했습니다. \n\n" + ex.Message,
                    "장르 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
            }
        }

        // 검색어가 있으면 장르명, 콘텐츠 타입, TMDB 장르 ID 기준으로 목록을 거릅니다.
        private void ApplyGenreSearch()
        {
            if (_genres == null)
            {
                GenreDataGrid.ItemsSource = null;
                return;
            }

            string keyword = GenreSearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                GenreDataGrid.ItemsSource = null;
                GenreDataGrid.ItemsSource = _genres;
                return;
            }

            keyword = keyword.ToLower();

            List<GenreListItemModel> filteredGenres = _genres
                .Where(genre =>
                    genre.Genre_Name.ToLower().Contains(keyword)
                    || genre.Content_Type.ToLower().Contains(keyword)
                    || (genre.TMDB_Genre_ID.HasValue
                        && genre.TMDB_Genre_ID.Value.ToString().Contains(keyword))
                )
                .ToList();

            GenreDataGrid.ItemsSource = null;
            GenreDataGrid.ItemsSource = filteredGenres;
        }

        private void GenreSearchTexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyGenreSearch();
        }

        // 목록에서 장르를 선택하면 왼쪽 입력 폼에 값을 채우고 수정 모드로 전환합니다.
        private void GenreDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenreDataGrid.SelectedItem is not GenreListItemModel selectedGenre)
            {
                _selectedGenre = null;
                SetEditMode(false);
                return;
            }

            _selectedGenre = selectedGenre;

            GenreNameTextBox.Text = selectedGenre.Genre_Name;
            TmdbGenreIdTextBox.Text = selectedGenre.TMDB_Genre_ID?.ToString() ?? "";

            SetContentTypeComboBox(selectedGenre.Content_Type);

            SetEditMode(true);
        }

        // 입력 폼의 값으로 새 장르를 추가합니다.
        private void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string genreName = GenreNameTextBox.Text.Trim();
                string contentType = GetSelectedContentType();
                int? tmdbGenreId = GetNullableInt(TmdbGenreIdTextBox.Text);

                if (string.IsNullOrWhiteSpace(genreName))
                {
                    MessageBox.Show("장르 이름을 입력하세요.");
                    GenreNameTextBox.Focus();
                    return;
                }

                _contentMappingService.AddGenre(
                    tmdbGenreId,
                    contentType,
                    genreName
                );

                MessageBox.Show(
                    "장르가 추가되었습니다.",
                    "장르 추가 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadGenreList();
                ClearGenreForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 추가 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 추가 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 선택된 장르를 입력 폼의 값으로 수정합니다.
        private void UpdateGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGenre == null)
            {
                MessageBox.Show("수정할 장르를 선택하세요.");
                return;
            }

            try
            {
                string genreName = GenreNameTextBox.Text.Trim();
                string contentType = GetSelectedContentType();
                int? tmdbGenreId = GetNullableInt(TmdbGenreIdTextBox.Text);

                if(string.IsNullOrWhiteSpace(genreName))
                {
                    MessageBox.Show("장르 이름을 입력하세요.");
                    GenreNameTextBox.Focus();
                    return;
                }

                _contentMappingService.UpdateGenre(
                    _selectedGenre.Genre_ID,
                    tmdbGenreId,
                    contentType,
                    genreName);

                MessageBox.Show(
                    "장르가 수정되었습니다.",
                    "장르 수정 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadGenreList();
                ClearGenreForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("장르 수정 중 오류가 발생했습니다. \n\n"+ex.Message,
                    "장르 수정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 선택된 장르를 삭제합니다. 연결된 콘텐츠가 있으면 한 번 더 확인합니다.
        private void DeleteGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGenre == null)
            {
                MessageBox.Show("삭제할 장르를 선택하세요.");
                return;
            }

            MessageBoxResult firstConfirm = MessageBox.Show(
                $"선택한 장르를 삭제하시겠습니까?\n\n장르명: {_selectedGenre.Genre_Name}\n콘텐츠 타입: {_selectedGenre.Content_Type}",
                "장르 삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (firstConfirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                GenreDeleteResultModel result =
                    _contentMappingService.DeleteGenreWithRelations(
                        _selectedGenre.Genre_ID,
                        false
                    );

                MessageBox.Show(
                    "장르가 삭제되었습니다.",
                    "장르 삭제 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadGenreList();
                ClearGenreForm();
            }
            catch (SqlException ex) when (ex.Number == 50002)
            {
                MessageBoxResult relationConfirm = MessageBox.Show(
                    ex.Message + "\n\n연결 정보까지 함께 삭제하시겠습니까?",
                    "연결된 콘텐츠 존재",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (relationConfirm != MessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    GenreDeleteResultModel forceResult =
                        _contentMappingService.DeleteGenreWithRelations(
                            _selectedGenre.Genre_ID,
                            true
                        );

                    MessageBox.Show(
                        $"장르가 삭제되었습니다.\n\n삭제된 연결 정보: {forceResult.Delete_ContentGenre_Count:N0}개",
                        "장르 삭제 완료",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    LoadGenreList();
                    ClearGenreForm();
                }
                catch (Exception forceEx)
                {
                    MessageBox.Show(
                        "장르 강제 삭제 중 오류가 발생했습니다.\n\n" + forceEx.Message,
                        "장르 삭제 오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 삭제 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 삭제 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ClearGenreButton_Click(object sender, RoutedEventArgs e)
        {
            ClearGenreForm();
        }

        // TMDB에서 영화/TV 장르를 가져와 DB에 일괄 반영합니다.
        private async void ImportTmdbGenreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportTmdbGenreButton.IsEnabled = false;
                ImportTmdbGenreButton.Content = "가져오는 중..";

                TmdbService tmdbService = GetTmdbService();

                List<TmdbGenreModel> movieGenres =
                    await tmdbService.GetGenreLIstAsync("MOVIE");

                List<TmdbGenreModel> tvGenres =
                    await tmdbService.GetGenreLIstAsync("TV");

                List<TmdbGenreModel> allgenres =
                    movieGenres.Concat(tvGenres).ToList();

                int insertedCount =
                    _contentMappingService.ImportTmdbGenres(allgenres);

                MessageBox.Show(
                    $"TMDB 장르: {insertedCount:N0}개가 추가되었습니다.",
                    "TMDB 장르 가져오기 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadGenreList();
                ClearGenreForm();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "TMDB 설정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "TMDB 장르를 가져오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "TMDB 장르 가져오기 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                ImportTmdbGenreButton.IsEnabled = true;
                ImportTmdbGenreButton.Content = "TMDB 가져오기";
            }
        }

        // 입력 폼과 선택 상태를 초기화하고 추가 모드로 되돌립니다.
        private void ClearGenreForm()
        {
            GenreDataGrid.SelectedItem = null;

            _selectedGenre = null;

            GenreNameTextBox.Clear();
            TmdbGenreIdTextBox.Clear();
            ContentTypeComboBox.SelectedIndex = 0;

            SetEditMode(false);

            GenreNameTextBox.Focus();
        }

        // 수정 모드 여부에 따라 버튼 활성화와 색상을 함께 변경합니다.
        private void SetEditMode(bool isEditMode)
        {
            AddGenreButton.IsEnabled = !isEditMode;
            UpdateGenreButton.IsEnabled = isEditMode;
            DeleteGenreButton.IsEnabled = isEditMode;

            // 선택된 행이 있으면 TMDB 장르 ID 수정 가능
            TmdbGenreIdTextBox.IsReadOnly = !isEditMode;

            if (isEditMode)
            {
                // 추가 버튼 비활성 느낌
                AddGenreButton.Background = BrushFromHex("#ECEAF1");
                AddGenreButton.BorderBrush = BrushFromHex("#ECEAF1");
                AddGenreButton.Foreground = BrushFromHex("#AAA4B5");

                // 수정 버튼 활성 느낌
                UpdateGenreButton.Background = BrushFromHex("#6D5DF6");
                UpdateGenreButton.BorderBrush = BrushFromHex("#6D5DF6");
                UpdateGenreButton.Foreground = Brushes.White;

                // 삭제 버튼 활성 느낌
                DeleteGenreButton.Background = BrushFromHex("#D9365B");
                DeleteGenreButton.BorderBrush = BrushFromHex("#D9365B");
                DeleteGenreButton.Foreground = Brushes.White;
            }
            else
            {
                // 추가 버튼 활성 느낌
                AddGenreButton.Background = BrushFromHex("#6D5DF6");
                AddGenreButton.BorderBrush = BrushFromHex("#6D5DF6");
                AddGenreButton.Foreground = Brushes.White;

                // 수정 버튼 비활성 느낌
                UpdateGenreButton.Background = BrushFromHex("#ECEAF1");
                UpdateGenreButton.BorderBrush = BrushFromHex("#ECEAF1");
                UpdateGenreButton.Foreground = BrushFromHex("#AAA4B5");

                // 삭제 버튼 비활성 느낌
                DeleteGenreButton.Background = BrushFromHex("#ECEAF1");
                DeleteGenreButton.BorderBrush = BrushFromHex("#ECEAF1");
                DeleteGenreButton.Foreground = BrushFromHex("#AAA4B5");
            }
        }

        // 저장된 콘텐츠 타입 값에 맞춰 ComboBox 선택 항목을 맞춥니다.
        private void SetContentTypeComboBox(string contentType)
        {
            string targetText = contentType.ToUpper() == "TV" ? "TV" : "Movie";

            foreach (object item in ContentTypeComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    string itemText = comboBoxItem.Content?.ToString() ?? "";

                    if (itemText.Equals(targetText, StringComparison.OrdinalIgnoreCase))
                    {
                        ContentTypeComboBox.SelectedItem = comboBoxItem;
                        return;
                    }
                }
            }

            ContentTypeComboBox.SelectedIndex = 0;
        }

        private string GetSelectedContentType()
        {
            if (ContentTypeComboBox.SelectedItem is ComboBoxItem item)
            {
                string text = item.Content?.ToString() ?? "";

                if (text.Equals("TV", StringComparison.OrdinalIgnoreCase))
                {
                    return "TV";
                }
            }

            return "MOVIE";
        }

        private int? GetNullableInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (int.TryParse(text.Trim(), out int value))
            {
                return value;
            }

            throw new Exception("TMDB 장르 ID는 숫자만 입력할 수 있습니다.");
        }

        private TmdbService GetTmdbService()
        {
            if (_tmdbService == null)
            {
                _tmdbService = new TmdbService();
            }

            return _tmdbService;
        }

        private SolidColorBrush BrushFromHex(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex)
            );
        }
    }
}
