using System.Net.Http;
using System.Text.Json;
using CineCode.Models.Tmdb;

namespace CineCode.Services
{
    public class TmdbService
    {
        // TMDB API와 포스터 이미지 주소의 기본 경로
        private const string BaseUrl = "https://api.themoviedb.org/3";
        private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w185";

        // 서비스 전체에서 재사용할 HTTP 클라이언트
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey;

        // 환경변수에서 TMDB API 키를 읽어 서비스 사용 준비
        public TmdbService()
        {
            string? apKey = Environment.GetEnvironmentVariable("TMDB_API_KEY");

            if (string.IsNullOrWhiteSpace(apKey))
            {
                throw new InvalidOperationException(
                    "TMDB_API_KEY 환경변수가 설정되지 않았습니다.\n" +
                    "README 안내에 따라 windows 환경변수를 먼저 등록하세요."
                );
            }

            _apiKey = apKey;
        }

        // 사용자가 입력한 키워드로 영화 또는 TV 콘텐츠를 TMDB에서 검색
        public async Task<List<TmdbSearchResultModel>> SearchContentAsync(string keyword, string contentType)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("검색 제목을 입력하세요.");
            }

            string normalizedContentType = NormalizeContentType(contentType);

            string endpoint = normalizedContentType == "MOVIE"
                ? "/search/movie"
                : "/search/tv";

            string url =
                $"{BaseUrl}{endpoint}" +
                $"?api_key={_apiKey}" +
                $"&language=ko-KR" +
                $"&query={Uri.EscapeDataString(keyword)}" +
                $"&include_adult=false" +
                $"&page=1";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("results", out JsonElement resultsElement))
            {
                return new List<TmdbSearchResultModel>();
            }

            List<TmdbSearchResultModel> results = new List<TmdbSearchResultModel>();

            foreach (JsonElement item in resultsElement.EnumerateArray())
            {
                TmdbSearchResultModel result = ConvertToSearchResult(item, normalizedContentType);
                results.Add(result);
            }

            return results;
        }

        // 콘텐츠 유형에 맞는 TMDB 장르 목록 조회
        public async Task<List<TmdbGenreModel>> GetGenreLIstAsync(string contentType)
        {
            string normalizedContetType = NormalizeContentType(contentType);

            string endpoint = normalizedContetType == "MOVIE"
                ? "/genre/movie/list"
                : "/genre/tv/list";

            string url =
                $"{BaseUrl}{endpoint}" +
                $"?api_key={_apiKey}" +
                $"&language=ko-KR";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;
            List<TmdbGenreModel> genres = new List<TmdbGenreModel>();

            if (!root.TryGetProperty("genres", out JsonElement genresElement))
            {
                return genres;
            }

            foreach (JsonElement item in genresElement.EnumerateArray())
            {
                TmdbGenreModel genre = new TmdbGenreModel
                {
                    id = GetInt(item, "id"),
                    name = GetString(item, "name"),
                    ContentType = normalizedContetType
                };
                genres.Add(genre);
            }

            return genres;
        }

        // 선택한 TMDB 콘텐츠의 러닝타임과 연령등급 상세 정보 조회
        public async Task<TmdbContentDetailModel> GetContentDetailAsync(int tmdbId, string contentType)
        {
            string normalizedContentType = NormalizeContentType(contentType);

            TmdbContentDetailModel detail = new TmdbContentDetailModel
            {
                TmdbId = tmdbId,
                ContentType = normalizedContentType,
                RunningTime = null,
                AgeRating = ""
            };

            if (normalizedContentType == "MOVIE")
            {
                detail.RunningTime = await GetMovieRunningTimeAsync(tmdbId);
                detail.AgeRating = await GetMovieAgeRatingAsync(tmdbId);
            }
            else
            {
                detail.RunningTime = await GetTvRunningTimeAsync(tmdbId);
                detail.AgeRating = await GetTvAgeRatingAsync(tmdbId);
            }

            return detail;
        }

        // TMDB 검색 결과 JSON 한 건을 화면에서 사용할 검색 결과 모델로 변환
        private TmdbSearchResultModel ConvertToSearchResult(JsonElement item, string contentType)
        {
            int tmdbId = GetInt(item, "id");

            string title = contentType == "MOVIE"
                ? GetString(item, "title")
                : GetString(item, "name");

            string releaseDate = contentType == "MOVIE"
                ? GetString(item, "release_date")
                : GetString(item, "first_air_date");

            decimal tmdbAverageRating = GetDecimal(item, "vote_average");
            string overview = GetString(item, "overview");
            string posterPath = GetString(item, "poster_path");

            List<int> genreIds = GetGenreIds(item);

            string posterUrl = "";

            if (!string.IsNullOrWhiteSpace(posterPath))
            {
                posterUrl = PosterBaseUrl + posterPath;
            }

            return new TmdbSearchResultModel
            {
                TmdbId = tmdbId,
                ContentType = contentType,
                Title = title,
                ReleaseDate = releaseDate,
                TmdbAverageRating = tmdbAverageRating,
                Overview = overview,
                PosterPath = posterPath,
                PosterUrl = posterUrl,
                GenreIds = genreIds
            };
        }

        // 화면에서 넘어온 콘텐츠 유형 값을 TMDB API에서 사용할 값으로 통일
        private string NormalizeContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return "MOVIE";
            }

            string value = contentType.Trim().ToUpper();

            if (value == "MOVIE" || value == "MOVIES" || value == "영화")
            {
                return "MOVIE";
            }

            if (value == "TV" || value == "드라마" || value == "시리즈")
            {
                return "TV";
            }

            return "MOVIE";
        }

        // 검색 결과에 포함된 장르 ID 배열을 숫자 목록으로 변환
        private List<int> GetGenreIds(JsonElement item)
        {
            List<int> genreIds = new List<int>();

            if (!item.TryGetProperty("genre_ids", out JsonElement genreIdsElement))
            {
                return genreIds;
            }

            if (genreIdsElement.ValueKind != JsonValueKind.Array)
            {
                return genreIds;
            }

            foreach (JsonElement genreIdElement in genreIdsElement.EnumerateArray())
            {
                if (genreIdElement.ValueKind == JsonValueKind.Number)
                {
                    genreIds.Add(genreIdElement.GetInt32());
                }
            }

            return genreIds;
        }

        // 영화 상세 API에서 러닝타임 조회
        private async Task<int?> GetMovieRunningTimeAsync(int tmdbId)
        {
            string url =
                $"{BaseUrl}/movie/{tmdbId}" +
                $"?api_key={_apiKey}" +
                $"&language=ko-KR";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            int runningTime = GetInt(root, "runtime");

            if (runningTime <= 0)
            {
                return null;
            }

            return runningTime;
        }

        // TV 상세 API에서 첫 번째 유효한 에피소드 러닝타임 조회
        private async Task<int?> GetTvRunningTimeAsync(int tmdbId)
        {
            string url =
                $"{BaseUrl}/tv/{tmdbId}" +
                $"?api_key={_apiKey}" +
                $"&language=ko-KR";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("episode_run_time", out JsonElement runTimeElement))
            {
                return null;
            }

            if (runTimeElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (JsonElement item in runTimeElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Number)
                {
                    int runningTime = item.GetInt32();

                    if (runningTime > 0)
                    {
                        return runningTime;
                    }
                }
            }

            return null;
        }

        // 영화 개봉 정보에서 한국 등급을 먼저 찾고, 없으면 미국 등급 조회
        private async Task<string> GetMovieAgeRatingAsync(int tmdbId)
        {
            string url =
                $"{BaseUrl}/movie/{tmdbId}/release_dates" +
                $"?api_key={_apiKey}";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("results", out JsonElement resultsElement))
            {
                return "";
            }

            string krRating = FindMovieCertification(resultsElement, "KR");

            if (!string.IsNullOrWhiteSpace(krRating))
            {
                return krRating;
            }

            string usRating = FindMovieCertification(resultsElement, "US");

            if (!string.IsNullOrWhiteSpace(usRating))
            {
                return usRating;
            }

            return "";
        }

        // 국가 코드에 맞는 영화 등급 문자열 검색
        private string FindMovieCertification(JsonElement resultsElement, string countryCode)
        {
            foreach (JsonElement countryItem in resultsElement.EnumerateArray())
            {
                string isoCode = GetString(countryItem, "iso_3166_1");

                if (!isoCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!countryItem.TryGetProperty("release_dates", out JsonElement releaseDatesElement))
                {
                    return "";
                }

                if (releaseDatesElement.ValueKind != JsonValueKind.Array)
                {
                    return "";
                }

                foreach (JsonElement releaseItem in releaseDatesElement.EnumerateArray())
                {
                    string certification = GetString(releaseItem, "certification");

                    if (!string.IsNullOrWhiteSpace(certification))
                    {
                        return certification;
                    }
                }
            }

            return "";
        }

        // TV 등급 정보에서 한국 등급을 먼저 찾고, 없으면 미국 등급 조회
        private async Task<string> GetTvAgeRatingAsync(int tmdbId)
        {
            string url =
                $"{BaseUrl}/tv/{tmdbId}/content_ratings" +
                $"?api_key={_apiKey}";

            string json = await _httpClient.GetStringAsync(url);

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("results", out JsonElement resultsElement))
            {
                return "";
            }

            string krRating = FindTvRating(resultsElement, "KR");

            if (!string.IsNullOrWhiteSpace(krRating))
            {
                return krRating;
            }

            string usRating = FindTvRating(resultsElement, "US");

            if (!string.IsNullOrWhiteSpace(usRating))
            {
                return usRating;
            }

            return "";
        }

        // 국가 코드에 맞는 TV 등급 문자열 검색
        private string FindTvRating(JsonElement resultsElement, string countryCode)
        {
            foreach (JsonElement countryItem in resultsElement.EnumerateArray())
            {
                string isoCode = GetString(countryItem, "iso_3166_1");

                if (!isoCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string rating = GetString(countryItem, "rating");

                if (!string.IsNullOrWhiteSpace(rating))
                {
                    return rating;
                }
            }

            return "";
        }

        // JSON 속성이 없거나 null이면 빈 문자열로 처리
        private string GetString(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out JsonElement value))
            {
                return "";
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return "";
            }

            return value.GetString() ?? "";
        }

        // JSON 숫자 속성을 int로 안전하게 읽기
        private int GetInt(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out JsonElement value))
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt32();
            }

            return 0;
        }

        // JSON 숫자 속성을 decimal로 안전하게 읽기
        private decimal GetDecimal(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out JsonElement value))
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDecimal();
            }

            return 0;
        }
    }
}
