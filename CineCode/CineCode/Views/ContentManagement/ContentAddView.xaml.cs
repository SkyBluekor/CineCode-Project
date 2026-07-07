using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CineCode.Models.Mapping;
using CineCode.Models.Tmdb;
using CineCode.Services;

namespace CineCode.Views.ContentManagement
{
    public partial class ContentAddView : UserControl
    {
        // 콘텐츠 등록에 필요한 서비스 객체
        private readonly ContentService _contentService = new ContentService();
        private readonly ContentMappingService _contentMappingService = new ContentMappingService();
        private TmdbService? _tmdbService = null;

        // 화면에서 사용자가 선택한 장르와 OTT 목록
        private List<SelectItemModel> _selectedGenres = new List<SelectItemModel>();
        private List<SelectItemModel> _selectedOttProviders = new List<SelectItemModel>();

        // TMDB 검색 결과와 현재 선택된 검색 결과
        private List<TmdbSearchResultModel> _tmdbSearchResults = new List<TmdbSearchResultModel>();
        private TmdbSearchResultModel? _selectedTmdbResult = null;

        // 목록 화면으로 돌아가야 할 때 부모 화면에 알리는 이벤트
        public event Action? BackToListRequested;

        public ContentAddView()
        {
            InitializeComponent();

            LoadGenreComboBox();
            LoadOttProviderComboBox();
        }

        // ComboBox와 ListBox에서 공통으로 사용하는 선택 항목 모델
        public class SelectItemModel
        {
            public int Id { get; set; }

            public string Name { get; set; } = "";
        }

        // 장르 ComboBox에 표시할 장르 목록을 DB에서 조회
        private void LoadGenreComboBox()
        {
            try
            {
                List<GenreListItemModel> genres = _contentMappingService.GetGenreList();

                List<SelectItemModel> genreItems = genres
                    .Select(genre => new SelectItemModel
                    {
                        Id = genre.Genre_ID,
                        Name = $"[{genre.Content_Type}] {genre.Genre_Name}"
                    })
                    .ToList();

                GenreComboBox.ItemsSource = genreItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "장르 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "장르 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // OTT Provider ComboBox에 표시할 OTT 목록을 DB에서 조회
        private void LoadOttProviderComboBox()
        {
            try
            {
                List<OttProviderListItemModel> ottProviders = _contentMappingService.GetOttProviderList();

                List<SelectItemModel> ottItems = ottProviders
                    .Select(ott => new SelectItemModel
                    {
                        Id = ott.OTT_ID,
                        Name = $"{ott.OTT_NAME} / {ott.monthly_price:N0}원"
                    })
                    .ToList();

                OttProviderComboBox.ItemsSource = ottItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "OTT Provider 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "OTT 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // TMDB 서비스는 API 키 오류가 있을 수 있어서 실제 검색할 때 처음 생성
        private TmdbService GetTmdbService()
        {
            if (_tmdbService == null)
            {
                _tmdbService = new TmdbService();
            }

            return _tmdbService;
        }

        // TMDB 검색 버튼 클릭 시 검색 API를 호출하고 결과 목록을 갱신
        private async void TmdbSearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string keyword = TmdbSearchTitleTextBox.Text.Trim();
                string searchType = GetSelectedComboBoxText(TmdbSearchTypeComboBox);

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    MessageBox.Show("검색 제목을 입력하세요.");
                    TmdbSearchTitleTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(searchType))
                {
                    searchType = "Movie";
                }

                TmdbSearchButton.IsEnabled = false;
                TmdbSearchButton.Content = "검색 중...";

                // 새 검색을 시작하므로 이전 선택 결과와 목록을 먼저 초기화
                _selectedTmdbResult = null;
                _tmdbSearchResults.Clear();

                TmdbResultListBox.ItemsSource = null;

                TmdbService tmdbService = GetTmdbService();

                _tmdbSearchResults =
                    await tmdbService.SearchContentAsync(keyword, searchType);

                TmdbResultListBox.ItemsSource = _tmdbSearchResults;

                if (_tmdbSearchResults.Count == 0)
                {
                    MessageBox.Show("검색 결과가 없습니다.");
                }
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
                    "TMDB 검색 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "TMDB 검색 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                TmdbSearchButton.IsEnabled = true;
                TmdbSearchButton.Content = "TMDB 검색";
            }
        }

        // TMDB 검색 결과를 선택하면 오른쪽 입력 폼에 자동 반영
        private async void TmdbSearchResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TmdbResultListBox.SelectedItem is not TmdbSearchResultModel selectedResult)
            {
                return;
            }

            try
            {
                _selectedTmdbResult = selectedResult;

                await FillContentInputFromTmdbAsync(selectedResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "TMDB 검색 결과를 적용하는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "TMDB 결과 적용 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // TMDB 검색 결과 한 건을 콘텐츠 입력 폼에 채우고 상세 정보까지 조회
        private async Task FillContentInputFromTmdbAsync(TmdbSearchResultModel result)
        {
            TitleTextBox.Text = result.Title;
            ReleaseDateTextBox.Text = result.ReleaseDate;
            TmdbIdTextBox.Text = result.TmdbId.ToString();
            TmdbScoreTextBox.Text = result.TmdbAverageRating.ToString("N1");
            OverviewTextBox.Text = result.Overview;

            PosterUrlTextBox.Text = result.PosterUrl;
            SetPosterPreview(result.PosterUrl);

            SetContentTypeComboBox(result.ContentType);

            _selectedGenres.Clear();
            RefreshSelectedGenreListBox();

            // TMDB 장르 ID와 DB 장르의 TMDB_Genre_ID를 비교해 자동 선택
            AddTmdbGenresToSelectedList(
                result.GenreIds,
                result.ContentType
            );

            AgeRatingTextBox.Text = "";
            RunningTimeTextBox.Text = "";

            try
            {
                TmdbService tmdbService = GetTmdbService();

                TmdbContentDetailModel detail =
                    await tmdbService.GetContentDetailAsync(
                        result.TmdbId,
                        result.ContentType
                    );

                AgeRatingTextBox.Text = detail.AgeRating;

                if (detail.RunningTime.HasValue)
                {
                    RunningTimeTextBox.Text = detail.RunningTime.Value.ToString();
                }
            }
            catch
            {
                AgeRatingTextBox.Text = "";
                RunningTimeTextBox.Text = "";
            }
        }

        // TMDB에서 받은 장르 ID 목록을 DB 장르 목록과 매칭해 선택 목록에 추가
        private void AddTmdbGenresToSelectedList(List<int> tmdbGenreIds, string contentType)
        {
            if (tmdbGenreIds == null || tmdbGenreIds.Count == 0)
            {
                return;
            }

            List<GenreListItemModel> allGenres = _contentMappingService.GetGenreList();

            List<GenreListItemModel> matchedGenres = allGenres
                .Where(genre =>
                    genre.TMDB_Genre_ID.HasValue
                    && tmdbGenreIds.Contains(genre.TMDB_Genre_ID.Value)
                    && genre.Content_Type.Equals(contentType, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            foreach (GenreListItemModel genre in matchedGenres)
            {
                bool alreadyExists = _selectedGenres.Any(selectedGenre =>
                    selectedGenre.Id == genre.Genre_ID
                );

                if (alreadyExists)
                {
                    continue;
                }

                _selectedGenres.Add(new SelectItemModel
                {
                    Id = genre.Genre_ID,
                    Name = $"[{genre.Content_Type}] {genre.Genre_Name}"
                });
            }

            RefreshSelectedGenreListBox();
        }

        // 포스터 URL을 이미지 미리보기 영역에 반영
        private void SetPosterPreview(string posterUrl)
        {
            PosterPreviewImage.Source = null;

            if (string.IsNullOrWhiteSpace(posterUrl))
            {
                return;
            }

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(posterUrl, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                PosterPreviewImage.Source = image;
            }
            catch
            {
                PosterPreviewImage.Source = null;
            }
        }

        // 선택한 장르를 콘텐츠에 추가
        private void AddGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (GenreComboBox.SelectedItem is not SelectItemModel selectedGenre)
            {
                MessageBox.Show("추가할 장르를 선택하세요.");
                return;
            }

            bool alreadyExists = _selectedGenres.Any(genre => genre.Id == selectedGenre.Id);

            if (alreadyExists)
            {
                MessageBox.Show("이미 추가된 장르입니다.");
                return;
            }

            _selectedGenres.Add(selectedGenre);

            RefreshSelectedGenreListBox();
        }

        // 선택 목록에서 장르 제거
        private void RemoveGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGenreListBox.SelectedItem is not SelectItemModel selectedGenre)
            {
                MessageBox.Show("제거할 장르를 선택하세요.");
                return;
            }

            _selectedGenres.Remove(selectedGenre);

            RefreshSelectedGenreListBox();
        }

        // ListBox는 같은 리스트 객체만 다시 할당하면 갱신이 안 될 수 있어 한 번 비운 뒤 다시 연결
        private void RefreshSelectedGenreListBox()
        {
            SelectedGenreListBox.ItemsSource = null;
            SelectedGenreListBox.ItemsSource = _selectedGenres;
        }

        // 선택한 OTT Provider를 콘텐츠에 추가
        private void AddOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OttProviderComboBox.SelectedItem is not SelectItemModel selectedOttProvider)
            {
                MessageBox.Show("추가할 OTT Provider를 선택하세요.");
                return;
            }

            bool alreadyExists = _selectedOttProviders.Any(ott =>
                ott.Id == selectedOttProvider.Id
            );

            if (alreadyExists)
            {
                MessageBox.Show("이미 추가된 OTT Provider입니다.");
                return;
            }

            _selectedOttProviders.Add(selectedOttProvider);

            RefreshSelectedOttProviderListBox();
        }

        // 선택 목록에서 OTT Provider 제거
        private void RemoveOttProviderButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOttProviderListBox.SelectedItem is not SelectItemModel selectedOttProvider)
            {
                MessageBox.Show("제거할 OTT Provider를 선택하세요.");
                return;
            }

            _selectedOttProviders.Remove(selectedOttProvider);

            RefreshSelectedOttProviderListBox();
        }

        // OTT 선택 목록을 화면에 다시 연결해 갱신
        private void RefreshSelectedOttProviderListBox()
        {
            SelectedOttProviderListBox.ItemsSource = null;
            SelectedOttProviderListBox.ItemsSource = _selectedOttProviders;
        }

        // 입력값을 검증한 뒤 콘텐츠와 장르/OTT 매핑을 저장
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = TitleTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(title))
                {
                    MessageBox.Show("Title은 필수입니다.");
                    TitleTextBox.Focus();
                    return;
                }

                string contentType = GetSelectedContentType();

                if (!ValidateSelectedGenresMatchContentType(contentType))
                {
                    return;
                }

                int? tmdbId = GetNullableInt(TmdbIdTextBox.Text);
                DateTime? releaseDate = GetNullableDate(ReleaseDateTextBox.Text);
                decimal? tmdbScore = GetNullableDecimal(TmdbScoreTextBox.Text);
                int? runningTime = GetNullableInt(RunningTimeTextBox.Text);

                string ageRating = AgeRatingTextBox.Text.Trim();
                string overview = OverviewTextBox.Text.Trim();
                string posterUrl = PosterUrlTextBox.Text.Trim();

                // 서비스에는 선택된 항목의 ID 목록만 전달
                List<int> genreIds = _selectedGenres
                    .Select(genre => genre.Id)
                    .ToList();

                List<int> ottIds = _selectedOttProviders
                    .Select(ott => ott.Id)
                    .ToList();

                int newContentId = _contentService.AddContent(
                    tmdbId,
                    contentType,
                    title,
                    releaseDate,
                    tmdbScore,
                    overview,
                    posterUrl,
                    runningTime,
                    ageRating,
                    genreIds,
                    ottIds
                );

                MessageBox.Show(
                    $"콘텐츠가 등록되었습니다.\n\nContent ID: {newContentId}",
                    "콘텐츠 등록 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "콘텐츠 등록 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "콘텐츠 등록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 초기화 버튼 클릭 시 입력 폼 전체 초기화
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        // 검색 결과, 입력값, 선택 목록, 포스터 미리보기를 모두 초기 상태로 되돌림
        private void ClearForm()
        {
            TmdbSearchTitleTextBox.Clear();
            TmdbSearchTypeComboBox.SelectedIndex = 0;

            _tmdbSearchResults.Clear();
            _selectedTmdbResult = null;

            TmdbResultListBox.ItemsSource = null;

            TitleTextBox.Clear();
            ContentTypeComboBox.SelectedIndex = 0;

            ReleaseDateTextBox.Clear();
            RunningTimeTextBox.Clear();
            AgeRatingTextBox.Clear();

            TmdbIdTextBox.Clear();
            TmdbScoreTextBox.Clear();

            GenreComboBox.SelectedIndex = -1;
            OttProviderComboBox.SelectedIndex = -1;

            _selectedGenres.Clear();
            _selectedOttProviders.Clear();

            RefreshSelectedGenreListBox();
            RefreshSelectedOttProviderListBox();

            PosterUrlTextBox.Clear();
            OverviewTextBox.Clear();

            PosterPreviewImage.Source = null;

            TmdbSearchTitleTextBox.Focus();
        }

        // 부모 화면에 목록으로 돌아가야 함을 알림
        private void BackToListButton_Click(object sender, RoutedEventArgs e)
        {
            BackToListRequested?.Invoke();
        }

        // ComboBoxItem 또는 직접 입력된 ComboBox 텍스트를 문자열로 가져옴
        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString() ?? "";
            }

            return comboBox.Text;
        }

        // TMDB 콘텐츠 타입 값을 화면 ComboBox 선택값으로 맞춤
        private void SetContentTypeComboBox(string contentType)
        {
            string displayText = contentType.ToUpper() == "TV" ? "TV" : "Movie";

            foreach (object item in ContentTypeComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem)
                {
                    string itemText = comboBoxItem.Content?.ToString() ?? "";

                    if (itemText.Equals(displayText, StringComparison.OrdinalIgnoreCase))
                    {
                        ContentTypeComboBox.SelectedItem = comboBoxItem;
                        return;
                    }
                }
            }

            ContentTypeComboBox.SelectedIndex = 0;
        }

        // 화면 ComboBox 선택값을 DB 저장용 콘텐츠 타입 값으로 변환
        private string GetSelectedContentType()
        {
            if (ContentTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content?.ToString() ?? "";

                if (content.Equals("Movie", StringComparison.OrdinalIgnoreCase))
                {
                    return "MOVIE";
                }

                if (content.Equals("TV", StringComparison.OrdinalIgnoreCase))
                {
                    return "TV";
                }
            }

            return "MOVIE";
        }

        // 빈 값은 null로 두고, 값이 있으면 int로 변환
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

            throw new Exception("숫자 입력값이 올바르지 않습니다: " + text);
        }

        // 빈 값은 null로 두고, 값이 있으면 decimal로 변환
        private decimal? GetNullableDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (decimal.TryParse(text.Trim(), out decimal value))
            {
                return value;
            }

            throw new Exception("소수 입력값이 올바르지 않습니다: " + text);
        }

        // 일반 날짜 형식과 yyyy-MM-dd 형식을 모두 허용해 날짜로 변환
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

            if (DateTime.TryParseExact(
                trimmedText,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                return date;
            }

            throw new Exception("날짜 형식이 올바르지 않습니다. 예: 2024-01-01");
        }

        // 선택된 장르의 콘텐츠 타입이 현재 선택한 콘텐츠 타입과 맞는지 검증
        private bool ValidateSelectedGenresMatchContentType(string contentType)
        {
            string normalizedContentType = contentType.Trim().ToUpper();

            List<GenreListItemModel> allGenres = _contentMappingService.GetGenreList();

            List<GenreListItemModel> wrongGenres = new List<GenreListItemModel>();

            foreach (var selectedGenre in _selectedGenres)
            {
                GenreListItemModel? matchedGenre = allGenres
                    .FirstOrDefault(genre => genre.Genre_ID == selectedGenre.Id);

                if (matchedGenre == null)
                {
                    continue;
                }

                if (!matchedGenre.Content_Type.Equals(
                    normalizedContentType,
                    StringComparison.OrdinalIgnoreCase))
                {
                    wrongGenres.Add(matchedGenre);
                }
            }

            if (wrongGenres.Count == 0)
            {
                return true;
            }

            string wrongGenreNames = string.Join(
                ", ",
                wrongGenres.Select(genre => $"[{genre.Content_Type}] {genre.Genre_Name}")
            );

            MessageBox.Show(
                $"콘텐츠 타입과 맞지 않는 장르가 선택되었습니다.\n\n" +
                $"콘텐츠 타입: {normalizedContentType}\n" +
                $"잘못 선택된 장르: {wrongGenreNames}",
                "장르 타입 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return false;
        }
    }
}
