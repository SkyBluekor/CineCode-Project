using CineCode.Data;
using CineCode.Models.Statistics;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CineCode.Services
{
    // 통계 화면의 기간, 요약, 차트, 인기 콘텐츠 데이터를 조회하는 서비스
    public class StatisticsService
    {
        // 통계 화면에서 선택 가능한 기간 목록을 조회
        public List<StatisticsPeriodSelectItemModel> GetAvailablePeriods()
        {
            List<StatisticsPeriodSelectItemModel> periods =
                new List<StatisticsPeriodSelectItemModel>();

            // 시청 기록, 평점, 구독 시작일에서 월 단위 기간을 모아 중복을 제거
            string sql = @"
                WITH PeriodSource AS
                (
                    SELECT
                        DATEFROMPARTS(
                            YEAR(WH.Watch_Ended_At),
                            MONTH(WH.Watch_Ended_At),
                            1
                        ) AS PeriodStartDate
                    FROM dbo.WatchHistory WH
                    WHERE WH.Watch_Ended_At IS NOT NULL

                    UNION

                    SELECT
                        DATEFROMPARTS(
                            YEAR(UR.Rated_At),
                            MONTH(UR.Rated_At),
                            1
                        ) AS PeriodStartDate
                    FROM dbo.UserRating UR
                    WHERE UR.Rated_At IS NOT NULL

                    UNION

                    SELECT
                        DATEFROMPARTS(
                            YEAR(US.started_at),
                            MONTH(US.started_at),
                            1
                        ) AS PeriodStartDate
                    FROM dbo.UserSubscription US
                    WHERE US.started_at IS NOT NULL
                )
                SELECT
                    PeriodStartDate
                FROM PeriodSource
                ORDER BY PeriodStartDate DESC;
            ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime startDate =
                    Convert.ToDateTime(reader["PeriodStartDate"]);

                periods.Add(
                    new StatisticsPeriodSelectItemModel
                    {
                        DisplayText = startDate.ToString("yyyy년 M월"),
                        StartDate = startDate,
                        EndDate = startDate.AddMonths(1),
                        IsAllPeriod = false
                    }
                );
            }

            if (periods.Count > 0)
            {
                // 실제 데이터가 있는 전체 범위를 "전체 기간"으로 추가
                DateTime allStartDate =
                    periods.Min(period => period.StartDate);

                DateTime allEndDate =
                    periods.Max(period => period.EndDate);

                periods.Insert(
                    0,
                    new StatisticsPeriodSelectItemModel
                    {
                        DisplayText = "전체 기간",
                        StartDate = allStartDate,
                        EndDate = allEndDate,
                        IsAllPeriod = true
                    }
                );
            }
            else
            {
                // 데이터가 없을 때도 화면 필터가 비지 않도록 기본 전체 기간을 제공
                periods.Add(
                    new StatisticsPeriodSelectItemModel
                    {
                        DisplayText = "전체 기간",
                        StartDate = new DateTime(2000, 1, 1),
                        EndDate = DateTime.Today.AddDays(1),
                        IsAllPeriod = true
                    }
                );
            }

            return periods;
        }

        // 선택 기간의 요약 지표와 이전 기간 대비 변화를 계산
        public StatisticsSummaryModel GetStatisticsSummary(
            DateTime startDate,
            DateTime endDate,
            string contentType,
            bool isAllPeriod,
            string compareType)
        {
            string normalizedContentType =
                NormalizeContentType(contentType);

            StatisticsSummaryModel currentSummary =
                GetStatisticsSummaryCore(
                    startDate,
                    endDate,
                    normalizedContentType
                );

            if (isAllPeriod)
            {
                // 전체 기간은 비교 기준이 없으므로 변화량을 0으로 고정
                currentSummary.TotalUserChangeRate = 0;
                currentSummary.WatchHourChangeRate = 0;
                currentSummary.AverageRatingChange = 0;
                currentSummary.ActiveSubscriptionRateChange = 0;

                return currentSummary;
            }

            DateTime compareStartDate;
            DateTime compareEndDate;

            // 전월/전분기/전년 중 선택된 기준에 맞는 비교 기간을 계산
            GetCompareDateRange(
                startDate,
                endDate,
                compareType,
                out compareStartDate,
                out compareEndDate
            );

            StatisticsSummaryModel previousSummary =
                GetStatisticsSummaryCore(
                    compareStartDate,
                    compareEndDate,
                    normalizedContentType
                );

            currentSummary.TotalUserChangeRate =
                CalculatePercentChange(
                    currentSummary.TotalUserCount,
                    previousSummary.TotalUserCount
                );

            currentSummary.WatchHourChangeRate =
                CalculatePercentChange(
                    currentSummary.TotalWatchHours,
                    previousSummary.TotalWatchHours
                );

            currentSummary.AverageRatingChange =
                currentSummary.AverageRatingScore
                - previousSummary.AverageRatingScore;

            currentSummary.ActiveSubscriptionRateChange =
                currentSummary.ActiveSubscriptionRate
                - previousSummary.ActiveSubscriptionRate;

            return currentSummary;
        }

        // 최근 월별 시청 활동을 콘텐츠 타입 기준으로 집계
        public List<MonthlyWatchActivityModel> GetMonthlyWatchActivity(
            string contentType,
            int monthCount = 6)
        {
            List<MonthlyWatchActivityModel> result = new List<MonthlyWatchActivityModel>();

            string normalizedContentType = NormalizeContentType(contentType);
            DateTime currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime startDate = currentMonthStart.AddMonths(-(monthCount - 1));
            DateTime endDate = currentMonthStart.AddMonths(1);

            string sql = @"
                SELECT 
                    DATEFROMPARTS(
                        YEAR(WH.Watch_Ended_At),
                        MONTH(WH.Watch_Ended_At),
                        1 ) AS MonthStartDate,
                        COUNT(*) AS WatchCount,
                        ISNULL(SUM(CAST(WH.Watch_Minutes AS DECIMAL(18,2))),0) AS TotalWatchMinutes
                FROM dbo.WatchHistory WH
                INNER JOIN dbo.Content C
                    ON WH.Content_ID = C.Content_ID
                WHERE WH.Watch_Ended_At IS NOT NULL
                  AND WH.Watch_Ended_At >= @StartDate
                  AND WH.Watch_Ended_At < @EndDate
                  AND (
                        @ContentType = 'ALL' OR C.Content_Type = @ContentType)
                GROUP BY 
                    DATEFROMPARTS(
                        YEAR(WH.Watch_Ended_At),
                        MONTH(WH.Watch_Ended_At),
                        1)
                ORDER BY MonthStartDate;
            ";

            Dictionary<DateTime, MonthlyWatchActivityModel> monthlyMap = new Dictionary<DateTime, MonthlyWatchActivityModel>();

            // 조회 결과가 없는 달도 0으로 표시되도록 먼저 월별 기본값을 채움
            for (int i = 0; i < monthCount; i++)
            {
                DateTime monthstart = startDate.AddMonths(i);

                MonthlyWatchActivityModel item = new MonthlyWatchActivityModel
                {
                    MonthText = monthstart.ToString("yyyy년 M월"),
                    ShortMonthText = monthstart.ToString("M월"),
                    WatchCount = 0,
                    TotalWatchHours = 0
                };
                monthlyMap.Add(monthstart, item);
            }

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate;

            command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = endDate;

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 10).Value = normalizedContentType;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime monthStartDate =
                    Convert.ToDateTime(reader["MonthStartDate"]);

                int watchCount =
                    Convert.ToInt32(reader["WatchCount"]);

                decimal totalWatchMinutes =
                    Convert.ToDecimal(reader["TotalWatchMinutes"]);

                if (monthlyMap.ContainsKey(monthStartDate))
                {
                    // DB에서 조회된 달만 실제 시청 수와 시간을 덮어씀
                    monthlyMap[monthStartDate].WatchCount = watchCount;
                    monthlyMap[monthStartDate].TotalWatchHours =
                        Math.Round(totalWatchMinutes / 60, 1);
                }
            }

            result =
                monthlyMap
                    .OrderBy(pair => pair.Key)
                    .Select(pair => pair.Value)
                    .ToList();

            return result;
        }

        // OTT 플랫폼별 시청 시간 점유율을 계산
        public List<OttPlatformShareModel> GetOttPlatformShare(
            int maxDisplayCount = 7)
        {
            List<OttPlatformShareModel> rawItems =
                new List<OttPlatformShareModel>();

            string sql = @"
                SELECT
                    OP.OTT_Name AS OttName,
                    COUNT(*) AS WatchCount,
                    ISNULL(SUM(CAST(WH.Watch_Minutes AS DECIMAL(18,2))), 0) AS TotalWatchMinutes
                FROM dbo.WatchHistory WH
                INNER JOIN dbo.OTTProvider OP
                    ON WH.OTT_ID = OP.OTT_ID
                WHERE WH.Watch_Ended_At IS NOT NULL
                GROUP BY
                    OP.OTT_ID,
                    OP.OTT_Name
                ORDER BY
                    TotalWatchMinutes DESC,
                    OP.OTT_Name;
            ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                OttPlatformShareModel item =
                    new OttPlatformShareModel
                    {
                        OttName = DbReaderHelper.GetString(reader, "OttName"),
                        WatchCount = DbReaderHelper.GetInt(reader, "WatchCount"),
                        TotalWatchMinutes =
                            DbReaderHelper.GetNullableDecimal(reader, "TotalWatchMinutes") ?? 0
                    };

                rawItems.Add(item);
            }

            decimal totalWatchMinutes =
                rawItems.Sum(item => item.TotalWatchMinutes);

            if (totalWatchMinutes <= 0)
            {
                return new List<OttPlatformShareModel>();
            }

            List<OttPlatformShareModel> result =
                new List<OttPlatformShareModel>();

            List<OttPlatformShareModel> sortedItems =
                rawItems
                    .OrderByDescending(item => item.TotalWatchMinutes)
                    .ThenBy(item => item.OttName)
                    .ToList();

            if (sortedItems.Count <= maxDisplayCount)
            {
                result = sortedItems;
            }
            else
            {
                // 표시 개수를 넘는 플랫폼은 "기타" 하나로 합산
                int topCount = maxDisplayCount - 1;

                result =
                    sortedItems
                        .Take(topCount)
                        .ToList();

                List<OttPlatformShareModel> otherItems =
                    sortedItems
                        .Skip(topCount)
                        .ToList();

                OttPlatformShareModel other =
                    new OttPlatformShareModel
                    {
                        OttName = "기타",
                        WatchCount = otherItems.Sum(item => item.WatchCount),
                        TotalWatchMinutes = otherItems.Sum(item => item.TotalWatchMinutes)
                    };

                result.Add(other);
            }

            string[] colors =
            {
                "#FF4B4B",
                "#3B82F6",
                "#EC4899",
                "#2563EB",
                "#06B6D4",
                "#111827",
                "#9CA3AF"
            };

            for (int i = 0; i < result.Count; i++)
            {
                // 전체 시청 시간 대비 각 OTT의 점유율과 차트 색상 지정
                result[i].ShareRate =
                    Math.Round(
                        result[i].TotalWatchMinutes / totalWatchMinutes * 100,
                        1
                    );

                result[i].ColorHex =
                    colors[i % colors.Length];
            }

            return result;
        }

        // 장르별 시청 인기도와 막대 그래프 비율을 계산
        public List<GenrePopularityModel> GetGenrePopularity(
            DateTime startDate,
            DateTime endDate,
            string contentType,
            int topCount = 5)
        {
            List<GenrePopularityModel> result = new List<GenrePopularityModel>();

            string normalizedContentType = NormalizeContentType(contentType);

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetTopGenreWatchStatistics", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate;
            command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = endDate;
            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 10).Value = normalizedContentType;
            command.Parameters.Add("@TopCount", SqlDbType.Int).Value = topCount;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                GenrePopularityModel item =
                    new GenrePopularityModel
                    {
                        Rank = DbReaderHelper.GetInt(reader, "GenreRank"),
                        GenreName = DbReaderHelper.GetString(reader, "GenreName"),
                        WatchCount = DbReaderHelper.GetInt(reader, "WatchCount"),
                        ShareRate =
                            Math.Round(
                                DbReaderHelper.GetNullableDecimal(reader, "ShareRate") ?? 0,
                                1
                            )
                    };

                result.Add(item);
            }

            int maxWatchCount = result.Count == 0
               ? 0
               : result.Max(item => item.WatchCount);

            foreach (GenrePopularityModel item in result)
            {
                // 가장 많이 시청된 장르를 100%로 보고 나머지 막대 길이를 계산
                if (maxWatchCount <= 0)
                {
                    item.BarPercent = 0;
                }
                else
                {
                    item.BarPercent =
                        Math.Round(
                            100.0 * item.WatchCount / maxWatchCount,
                            1
                        );
                }
            }

            return result;
        }

        // 인기 콘텐츠 조회에 사용할 콘텐츠 타입 필터 목록 생성
        public List<TopContentTypeFilterItemModel> GetTopContentTypeFilterItems()
        {
            List<TopContentTypeFilterItemModel> items =
                new List<TopContentTypeFilterItemModel>();

            items.Add(
                new TopContentTypeFilterItemModel
                {
                    ContentType = "ALL",
                    DisplayText = "타입 전체"
                }
            );

            items.Add(
                new TopContentTypeFilterItemModel
                {
                    ContentType = "MOVIE",
                    DisplayText = "영화"
                }
            );

            items.Add(
                new TopContentTypeFilterItemModel
                {
                    ContentType = "TV",
                    DisplayText = "TV"
                }
            );

            return items;
        }

        // 인기 콘텐츠 조회에 사용할 OTT 필터 목록 조회
        public List<TopContentOttFilterItemModel> GetTopContentOttFilterItems()
        {
            List<TopContentOttFilterItemModel> items =
                new List<TopContentOttFilterItemModel>();

            items.Add(
                new TopContentOttFilterItemModel
                {
                    OttId = null,
                    OttName = "OTT 전체"
                }
            );

            string sql = @"
        SELECT
            OTT_ID,
            OTT_Name
        FROM dbo.OTTProvider
        ORDER BY
            OTT_Name;
    ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                TopContentOttFilterItemModel item =
                    new TopContentOttFilterItemModel
                    {
                        OttId = DbReaderHelper.GetInt(reader, "OTT_ID"),
                        OttName = DbReaderHelper.GetString(reader, "OTT_Name")
                    };

                items.Add(item);
            }

            return items;
        }

        // 인기 콘텐츠 조회에 사용할 장르 필터 목록 조회
        public List<TopContentGenreFilterItemModel> GetTopContentGenreFilterItems(
            string contentType)
        {
            List<TopContentGenreFilterItemModel> items =
                new List<TopContentGenreFilterItemModel>();

            items.Add(
                new TopContentGenreFilterItemModel
                {
                    GenreId = null,
                    GenreName = "장르 전체",
                    ContentType = "ALL"
                }
            );

            string normalizedContentType =
                NormalizeContentType(contentType);

            string sql = @"
        SELECT
            Genre_ID,
            Genre_Name,
            Content_Type
        FROM dbo.Genre
        WHERE
            @ContentType = 'ALL'
            OR Content_Type = @ContentType
        ORDER BY
            Content_Type,
            Genre_Name;
    ";

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 10).Value =
                normalizedContentType;

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                TopContentGenreFilterItemModel item =
                    new TopContentGenreFilterItemModel
                    {
                        GenreId = DbReaderHelper.GetInt(reader, "Genre_ID"),
                        GenreName = DbReaderHelper.GetString(reader, "Genre_Name"),
                        ContentType = DbReaderHelper.GetString(reader, "Content_Type")
                    };

                items.Add(item);
            }

            return items;
        }

        // 검색/필터 조건에 맞는 인기 콘텐츠 랭킹 조회
        public List<TopContentRankingItemModel> GetTopContentRanking(
                DateTime startDate,
                DateTime endDate,
                string keyword,
                string contentType,
                int? ottId,
                int? genreId)
        {
            List<TopContentRankingItemModel> result =
                new List<TopContentRankingItemModel>();

            string normalizedContentType =
                NormalizeContentType(contentType);

            using SqlConnection connection =
                DbConnectionFactory.CreateConnection();

            using SqlCommand command =
                new SqlCommand("dbo.Usp_GetTopContentRanking", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value =
                startDate;

            command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value =
                endDate;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                // 검색어가 없으면 프로시저에서 전체 검색으로 처리하도록 NULL 전달
                command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value =
                    DBNull.Value;
            }
            else
            {
                command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value =
                    keyword.Trim();
            }

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 10).Value =
                normalizedContentType;

            if (ottId.HasValue)
            {
                command.Parameters.Add("@OttId", SqlDbType.Int).Value =
                    ottId.Value;
            }
            else
            {
                // OTT/장르 필터가 없으면 NULL을 전달해 전체 대상으로 조회
                command.Parameters.Add("@OttId", SqlDbType.Int).Value =
                    DBNull.Value;
            }

            if (genreId.HasValue)
            {
                command.Parameters.Add("@GenreId", SqlDbType.Int).Value =
                    genreId.Value;
            }
            else
            {
                command.Parameters.Add("@GenreId", SqlDbType.Int).Value =
                    DBNull.Value;
            }

            connection.Open();

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                TopContentRankingItemModel item =
                    new TopContentRankingItemModel
                    {
                        Rank = DbReaderHelper.GetInt(reader, "ContentRank"),

                        ContentId = DbReaderHelper.GetInt(reader, "Content_ID"),

                        Title = DbReaderHelper.GetString(reader, "Title"),

                        ContentType = DbReaderHelper.GetString(reader, "ContentType"),

                        PosterUrl = DbReaderHelper.GetString(reader, "PosterUrl"),

                        OttNames = DbReaderHelper.GetString(reader, "OttNames"),

                        GenreNames = DbReaderHelper.GetString(reader, "GenreNames"),

                        WatchCount = DbReaderHelper.GetInt(reader, "WatchCount"),

                        ViewerCount = DbReaderHelper.GetInt(reader, "ViewerCount"),

                        TotalWatchMinutes =
                            DbReaderHelper.GetNullableDecimal(reader, "TotalWatchMinutes") ?? 0,

                        AverageRatingScore =
                            Math.Round(
                                DbReaderHelper.GetNullableDecimal(reader, "AverageRatingScore") ?? 0,
                                1
                            ),

                        CompletionRate =
                            Math.Round(
                                DbReaderHelper.GetNullableDecimal(reader, "CompletionRate") ?? 0,
                                1
                            )
                    };

                result.Add(item);
            }

            return result;
        }

        // 상위 콘텐츠 요약 통계를 조회
        public List<TopContentStatisticsModel> GetTopContents(
            DateTime startDate,
            DateTime endDate,
            int topCount = 5)
        {
            List<TopContentStatisticsModel> result = new List<TopContentStatisticsModel>();

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetTopContentStatistics", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate;
            command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = endDate;
            command.Parameters.Add("@TopCount", SqlDbType.Int).Value = topCount;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                decimal totalWatchMinutes =
                    DbReaderHelper.GetNullableDecimal(reader, "TotalWatchMinutes") ?? 0;

                // DB는 분 단위로 주므로 화면 모델에서는 시간 단위로 변환
                TopContentStatisticsModel item =
                    new TopContentStatisticsModel
                    {
                        Rank = DbReaderHelper.GetInt(reader, "ContentRank"),
                        ContentId = DbReaderHelper.GetInt(reader, "Content_ID"),
                        Title = DbReaderHelper.GetString(reader, "Title"),
                        OttName = DbReaderHelper.GetString(reader, "OttName"),
                        GenreNames = DbReaderHelper.GetString(reader, "GenreNames"),
                        WatchCount = DbReaderHelper.GetInt(reader, "WatchCount"),
                        TotalWatchHours = Math.Round(totalWatchMinutes / 60, 1),
                        AverageRatingScore =
                            Math.Round(
                                DbReaderHelper.GetNullableDecimal(reader, "AverageRatingScore") ?? 0,
                                1
                            )
                    };

                result.Add(item);
            }

            return result;
        }

        // 화면 표시값을 DB 조회에 사용할 콘텐츠 타입 코드로 변환
        private string NormalizeContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return "ALL";
            }

            string value = contentType.Trim().ToUpper();

            if (value == "전체" || value == "ALL")
            {
                return "ALL";
            }

            if (value == "영화" || value == "MOVIE")
            {
                return "MOVIE";
            }

            if (value == "TV" || value == "티비")
            {
                return "TV";
            }
            return "ALL";
        }

        // 요약 카드 원본 데이터를 DB에서 조회
        private StatisticsSummaryModel GetStatisticsSummaryCore(
            DateTime startDate,
            DateTime endDate,
            string contentType)
        {
            StatisticsSummaryModel summary = new StatisticsSummaryModel();

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetStatisticsSummaryCards", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = startDate;
            command.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = endDate;
            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 10).Value = contentType;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                summary.TotalUserCount = DbReaderHelper.GetInt(reader, "TotalUserCount");
                decimal totalWatchMinutes = DbReaderHelper.GetNullableDecimal(reader, "TotalWatchMinutes") ?? 0;
                summary.TotalWatchHours = Math.Round(totalWatchMinutes / 60, 1);
                summary.AverageRatingScore = Math.Round(DbReaderHelper.GetNullableDecimal(reader, "AverageRatingScore") ?? 0, 1);
                summary.ActiveSubscriptionRate = Math.Round(DbReaderHelper.GetNullableDecimal(reader, "ActiveSubscriptionRate") ?? 0, 1);
            }
            return summary;
        }

        // 비교 기준에 따라 이전 기간 범위를 계산
        private void GetCompareDateRange(
            DateTime startDate,
            DateTime endDate,
            string compareType,
            out DateTime compareStartDate,
            out DateTime compareEndDate)
        {
            string value = compareType.Trim();

            if (value == "전년 대비")
            {
                compareStartDate = startDate.AddYears(-1);
                compareEndDate = endDate.AddYears(-1);
                return;
            }

            if (value == "전분기 대비")
            {
                compareStartDate = startDate.AddMonths(-3);
                compareEndDate = endDate.AddMonths(-3);
                return;
            }

            compareStartDate = startDate.AddMonths(-1);
            compareEndDate = endDate.AddMonths(-1);
        }

        // 이전 값 대비 현재 값의 증감률을 계산
        private decimal CalculatePercentChange(
            decimal currentValue,
            decimal previousValue)
        {
            if (previousValue == 0)
            {
                if (currentValue == 0)
                {
                    return 0;
                }

                return 100;
            }

            return Math.Round(
                ((currentValue - previousValue) / previousValue) * 100, 2);
        }
    }
}
