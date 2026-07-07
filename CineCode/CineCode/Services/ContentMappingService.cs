using System.Data;
using CineCode.Data;
using CineCode.Models.Tmdb;
using Microsoft.Data.SqlClient;
using CineCode.Models.Mapping;


namespace CineCode.Services
{
    // 콘텐츠와 장르/OTT 연결 관리, 장르/OTT 기본 데이터 관리를 담당하는 서비스
    public class ContentMappingService
    {
        // 매핑 관리 화면 상단 요약 정보를 조회
        public ContentMappingSummaryModel GetContentMappingSummary()
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetContentMappingSummary", connection);

            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                ContentMappingSummaryModel summary = new ContentMappingSummaryModel
                {
                    RegisteredGenreCount = DbReaderHelper.GetInt(reader, "RegisteredGenreCount"),
                    OttProviderCount = DbReaderHelper.GetInt(reader, "OttProviderCount"),
                    UnmappedContentCount = DbReaderHelper.GetInt(reader, "UnmappedContentCount")
                };

                return summary;
            }

            throw new Exception("Content Mapping 요약 정보를 가져오지 못했습니다.");
        }

        // 장르/OTT 매핑 상태를 확인할 콘텐츠 목록을 조회
        public List<ContentMappingContentItemModel> GetContentMappingContentList()
        {
            List<ContentMappingContentItemModel> contents = new List<ContentMappingContentItemModel>();

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetContentMappingContentList", connection);

            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ContentMappingContentItemModel content = new ContentMappingContentItemModel
                {
                    Content_ID = DbReaderHelper.GetInt(reader, "Content_ID"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    Title = DbReaderHelper.GetString(reader, "Title"),

                    GenreCount = DbReaderHelper.GetInt(reader, "GenreCount"),
                    OttCount = DbReaderHelper.GetInt(reader, "OttCount"),

                    MappingStatus = DbReaderHelper.GetString(reader, "MappingStatus"),
                    MappingSortOrder = DbReaderHelper.GetInt(reader, "MappingSortOrder")
                };

                contents.Add(content);
            }

            return contents;
        }

        // 선택한 콘텐츠에 연결 가능한 장르 목록과 현재 연결 여부를 조회
        public List<ContentMappingGenreItemModel> GetContentMappingGenreList(int contentId, string contentType)
        {
            List<ContentMappingGenreItemModel> genres = new List<ContentMappingGenreItemModel>();

            string sql = @"
        SELECT
            G.Genre_ID,
            G.TMDB_Genre_ID,
            G.Genre_Name,
            G.Content_Type,
            CASE
                WHEN CG.Content_ID IS NULL THEN CAST(0 AS BIT)
                ELSE CAST(1 AS BIT)
            END AS IsLinked
        FROM dbo.Genre G
        LEFT JOIN dbo.ContentGenre CG
            ON G.Genre_ID = CG.Genre_ID
           AND CG.Content_ID = @Content_ID
        WHERE G.Content_Type = @Content_Type
        ORDER BY
            CASE WHEN CG.Content_ID IS NULL THEN 1 ELSE 0 END,
            G.Genre_Name;
    ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@Content_Type", SqlDbType.VarChar, 10).Value = contentType;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ContentMappingGenreItemModel genre = new ContentMappingGenreItemModel
                {
                    Genre_ID = DbReaderHelper.GetInt(reader, "Genre_ID"),
                    TMDB_Genre_ID = DbReaderHelper.GetNullableInt(reader, "TMDB_Genre_ID"),
                    Genre_Name = DbReaderHelper.GetString(reader, "Genre_Name"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    IsLinked = Convert.ToBoolean(reader["IsLinked"])
                };

                genres.Add(genre);
            }

            return genres;
        }

        // 콘텐츠와 장르를 연결
        public void LinkGenreToContent(int contentId, int genreId)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_LinkContentGenre", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreId;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 콘텐츠와 장르 연결을 해제
        public void UnlinkGenreFromContent(int contentId, int genreId)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_UnlinkContentGenre", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreId;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 선택한 콘텐츠에 연결 가능한 OTT 목록과 현재 연결 여부를 조회
        public List<ContentMappingOttItemModel> GetContentMappingOttList(int contentId)
        {
            List<ContentMappingOttItemModel> otts = new List<ContentMappingOttItemModel>();

            string sql = @"
        SELECT
            OP.OTT_ID,
            OP.OTT_NAME,
            OP.monthly_price,
            CASE
                WHEN COP.Content_ID IS NULL THEN CAST(0 AS BIT)
                ELSE CAST(1 AS BIT)
            END AS IsLinked
        FROM dbo.OTTProvider OP
        LEFT JOIN dbo.ContentOTTProvider COP
            ON OP.OTT_ID = COP.OTT_ID
           AND COP.Content_ID = @Content_ID
        ORDER BY
            CASE WHEN COP.Content_ID IS NULL THEN 1 ELSE 0 END,
            OP.OTT_NAME;
    ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ContentMappingOttItemModel ott = new ContentMappingOttItemModel
                {
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),
                    OTT_NAME = DbReaderHelper.GetString(reader, "OTT_NAME"),
                    monthly_price = DbReaderHelper.GetInt(reader, "monthly_price"),
                    IsLinked = Convert.ToBoolean(reader["IsLinked"])
                };

                otts.Add(ott);
            }

            return otts;
        }

        // 콘텐츠와 OTT 플랫폼을 연결
        public void LinkOttToContent(int contentId, int ottId)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_LinkContentOTTProvider", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = ottId;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 콘텐츠와 OTT 플랫폼 연결을 해제
        public void UnlinkOttFromContent(int contentId, int ottId)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_UnlinkContentOTTProvider", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = ottId;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // TMDB에서 받은 장르 목록을 DB 장르 테이블에 반영
        public int ImportTmdbGenres(List<TmdbGenreModel> tmdbGenres)
        {
            int insertedCount = 0;

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            foreach (TmdbGenreModel genre in tmdbGenres)
            {
                using SqlCommand command = new SqlCommand("dbo.Usp_ImportTmdbGenre", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TMDB_Genre_ID", genre.id);
                command.Parameters.AddWithValue("@Content_Type", genre.ContentType);
                command.Parameters.AddWithValue("@Genre_Name", genre.name);

                SqlParameter insertParameter = new SqlParameter("@Inserted", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(insertParameter);

                command.ExecuteNonQuery();

                bool inserted = Convert.ToBoolean(insertParameter.Value);

                if (inserted)
                {
                    insertedCount++;
                }
            }

            return insertedCount;
        }

        // 장르 관리 목록을 조회
        public List<GenreListItemModel> GetGenreList()
        {
            List<GenreListItemModel> genres = new List<GenreListItemModel>();

            string sql = @"select  
                            G.Genre_ID,
                            G.TMDB_Genre_ID, 
                            G.Content_Type,
                            G.Genre_Name, 
                            count(CG.Content_ID) AS LinkedContentCount  
                        from Genre G
                        left join dbo.ContentGenre CG 
                            On G.Genre_ID = CG.Genre_ID
                        Group by 
                            G.Genre_ID,
                            G.TMDB_Genre_ID,
                            G.Content_Type,
                            G.Genre_Name 
                        order by
                            G.Content_Type,
                            G.Genre_Name;";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                GenreListItemModel genre = new GenreListItemModel
                {
                    Genre_ID = DbReaderHelper.GetInt(reader, "Genre_ID"),
                    TMDB_Genre_ID = DbReaderHelper.GetNullableInt(reader, "TMDB_Genre_ID"),
                    Content_Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    Genre_Name = DbReaderHelper.GetString(reader, "Genre_Name"),
                    LinkedContentCount = DbReaderHelper.GetInt(reader, "LinkedContentCount")
                };

                genres.Add(genre);
            }

            return genres;
        }

        // 새 장르를 추가
        public void AddGenre(int? tmdbGenreId, string contentType, string genreName)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddOrUpdateGenre", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Genre_ID", DBNull.Value);

            if (tmdbGenreId.HasValue)
            {
                command.Parameters.AddWithValue("@TMDB_Genre_ID", tmdbGenreId.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@TMDB_Genre_ID", DBNull.Value);
            }

            command.Parameters.AddWithValue("@Content_Type", contentType);
            command.Parameters.AddWithValue("@Genre_Name", genreName);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // 기존 장르 정보를 수정
        public void UpdateGenre(int genreId, int? tmdbGenreId, string contentType, string genreName)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddOrUpdateGenre", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Genre_ID", genreId);

            if (tmdbGenreId.HasValue)
            {
                command.Parameters.AddWithValue("@TMDB_Genre_ID", tmdbGenreId.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@TMDB_Genre_ID", DBNull.Value);
            }

            command.Parameters.AddWithValue("@Content_Type", contentType);
            command.Parameters.AddWithValue("@Genre_Name", genreName);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // 장르와 연결 데이터를 함께 삭제하고 결과를 반환
        public GenreDeleteResultModel DeleteGenreWithRelations(int genreId, bool forceDeleteRelations)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_DeleteGenreWithRelations", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreId;
            command.Parameters.Add("@ForceDeleteRelations", SqlDbType.Bit).Value = forceDeleteRelations;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new GenreDeleteResultModel
                {
                    Genre_ID = DbReaderHelper.GetInt(reader, "Genre_ID"),
                    Delete_ContentGenre_Count = DbReaderHelper.GetInt(reader, "Delete_ContentGenre_Count"),
                    Delete_Genre_Count = DbReaderHelper.GetInt(reader, "Delete_Genre_Count"),
                    Message = DbReaderHelper.GetString(reader, "Message")
                };
            }

            throw new Exception("장르 삭제 결과를 가져오지 못했습니다.");
        }

        // OTT 플랫폼 관리 목록을 조회
        public List<OttProviderListItemModel> GetOttProviderList()
        {
            List<OttProviderListItemModel> ottProviders = new List<OttProviderListItemModel>();

            string sql = @"
        SELECT
            OP.OTT_ID,
            OP.OTT_NAME,
            OP.monthly_price,

            COUNT(DISTINCT COP.Content_ID) AS LinkedContentCount,
            COUNT(DISTINCT US.Subscription_ID) AS SubscriptionCount,
            COUNT(DISTINCT WH.Watch_ID) AS WatchHistoryCount
        FROM dbo.OTTProvider OP
        LEFT JOIN dbo.ContentOTTProvider COP
            ON OP.OTT_ID = COP.OTT_ID
        LEFT JOIN dbo.Usersubscription US
            ON OP.OTT_ID = US.OTT_ID
        LEFT JOIN dbo.WatchHistory WH
            ON OP.OTT_ID = WH.OTT_ID
        GROUP BY
            OP.OTT_ID,
            OP.OTT_NAME,
            OP.monthly_price
        ORDER BY
            OP.OTT_NAME;
    ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                OttProviderListItemModel ottProvider = new OttProviderListItemModel
                {
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),
                    OTT_NAME = DbReaderHelper.GetString(reader, "OTT_NAME"),
                    monthly_price = DbReaderHelper.GetInt(reader, "monthly_price"),
                    LinkedContentCount = DbReaderHelper.GetInt(reader, "LinkedContentCount"),
                    SubscriptionCount = DbReaderHelper.GetInt(reader, "SubscriptionCount"),
                    WatchHistoryCount = DbReaderHelper.GetInt(reader, "WatchHistoryCount")
                };

                ottProviders.Add(ottProvider);
            }

            return ottProviders;
        }

        // 새 OTT 플랫폼을 추가
        public void AddOttProvider(string ottName, int monthlyPrice)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddOrUpdateOTTProvider", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = DBNull.Value;
            command.Parameters.Add("@OTT_NAME", SqlDbType.NVarChar, 20).Value = ottName;
            command.Parameters.Add("@monthly_price", SqlDbType.Int).Value = monthlyPrice;

            connection.Open();
            command.ExecuteNonQuery();
        }

        // 기존 OTT 플랫폼 정보를 수정
        public void UpdateOttProvider(int ottId, string ottName, int monthlyPrice)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_AddOrUpdateOTTProvider", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = ottId;
            command.Parameters.Add("@OTT_NAME", SqlDbType.NVarChar, 20).Value = ottName;
            command.Parameters.Add("@monthly_price", SqlDbType.Int).Value = monthlyPrice;

            connection.Open();
            command.ExecuteNonQuery();
        }

        // OTT 플랫폼과 연결 데이터를 함께 삭제하고 결과를 반환
        public OttProviderDeleteResultModel DeleteOttProviderWithRelations(int ottId, bool forceDeleteRelations)
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_DeleteOTTProviderWithRelations", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = ottId;
            command.Parameters.Add("@ForceDeleteRelations", SqlDbType.Bit).Value = forceDeleteRelations;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                OttProviderDeleteResultModel result = new OttProviderDeleteResultModel
                {
                    OTT_ID = DbReaderHelper.GetInt(reader, "OTT_ID"),

                    Delete_WatchHistory_Count =
                        DbReaderHelper.GetInt(reader, "Delete_WatchHistory_Count"),

                    Delete_UserSubscription_Count =
                        DbReaderHelper.GetInt(reader, "Delete_UserSubscription_Count"),

                    Delete_ContentOTTProvider_Count =
                        DbReaderHelper.GetInt(reader, "Delete_ContentOTTProvider_Count"),

                    Delete_OTTProvider_Count =
                        DbReaderHelper.GetInt(reader, "Delete_OTTProvider_Count"),

                    Message = DbReaderHelper.GetString(reader, "Message")
                };

                return result;
            }

            throw new Exception("OTT 제공자 삭제 결과를 가져오지 못했습니다.");
        }
    }
}
