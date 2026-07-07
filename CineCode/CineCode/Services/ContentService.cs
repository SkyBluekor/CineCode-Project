using System.Data;
using CineCode.Data;
using CineCode.Models.Content;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    // 콘텐츠 목록 조회, 등록, 수정, 삭제를 담당하는 서비스
    public class ContentService
    {
        // 콘텐츠 관리 화면에 표시할 콘텐츠 목록을 조회
        public List<ContentListItemModel> GetContentList()
        {
            List<ContentListItemModel> contents = new List<ContentListItemModel>();

            // OUTER APPLY로 콘텐츠마다 연결된 OTT 개수를 함께 계산
            string sql = @"
        SELECT
            C.Content_ID,
            C.Title,
            C.Content_Type,
            C.Release_Date,
            C.TMDB_Average_Rating,
            C.Age_Rating,
            C.Poster_URL,
            C.OverView,
            C.Running_Time,
            ISNULL(OC.OttCount, 0) AS OttCount
        FROM dbo.Content C
        OUTER APPLY
        (
            SELECT COUNT(*) AS OttCount
            FROM dbo.ContentOTTProvider COP
            WHERE COP.Content_ID = C.Content_ID
        ) OC
        ORDER BY C.Content_ID DESC;
    ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // DB 값을 화면 표시용 모델로 변환
                ContentListItemModel content = new ContentListItemModel
                {
                    ContentID = DbReaderHelper.GetInt(reader, "Content_ID"),
                    Title = DbReaderHelper.GetString(reader, "Title"),
                    Type = DbReaderHelper.GetString(reader, "Content_Type"),
                    ReleaseDate = DbReaderHelper.GetDateText(reader, "Release_Date"),
                    TmdbScore = DbReaderHelper.GetDecimalText(reader, "TMDB_Average_Rating"),
                    AgeRating = DbReaderHelper.GetString(reader, "Age_Rating"),
                    OttCount = DbReaderHelper.GetInt(reader, "OttCount"),

                    PosterUrl = DbReaderHelper.GetString(reader, "Poster_URL"),
                    Overview = DbReaderHelper.GetString(reader, "OverView"),
                    RunningTime = reader["Running_Time"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["Running_Time"])
                };

                contents.Add(content);
            }

            return contents;
        }

        // 새 콘텐츠를 등록하고 선택된 장르/OTT 연결까지 함께 저장
        public int AddContent(
            int? tmdbId,
            string contentType,
            string title,
            DateTime? releaseDate,
            decimal? tmdbAverageRating,
            string overview,
            string posterUrl,
            int? runningTime,
            string ageRating,
            List<int> genreIds,
            List<int> ottIds
        )
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();

            connection.Open();

            // 콘텐츠 등록과 장르/OTT 연결을 하나의 작업으로 묶기 위한 트랜잭션
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                int newContentId;

                using (SqlCommand command = new SqlCommand("dbo.Usp_AddContent", connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@TMDB_ID", SqlDbType.Int).Value =
                        tmdbId.HasValue ? tmdbId.Value : DBNull.Value;

                    command.Parameters.Add("@Content_Type", SqlDbType.VarChar, 10).Value =
                        contentType;

                    command.Parameters.Add("@Title", SqlDbType.NVarChar, 50).Value =
                        title;

                    command.Parameters.Add("@Release_Date", SqlDbType.Date).Value =
                        releaseDate.HasValue ? releaseDate.Value.Date : DBNull.Value;

                    command.Parameters.Add("@TMDB_Average_Rating", SqlDbType.Decimal).Value =
                        tmdbAverageRating.HasValue ? tmdbAverageRating.Value : DBNull.Value;

                    // DB decimal(3,1)에 맞춰 TMDB 평점 자리수를 지정
                    command.Parameters["@TMDB_Average_Rating"].Precision = 3;
                    command.Parameters["@TMDB_Average_Rating"].Scale = 1;

                    // 빈 문자열은 DB NULL로 저장
                    command.Parameters.Add("@OverView", SqlDbType.NVarChar).Value =
                        string.IsNullOrWhiteSpace(overview) ? DBNull.Value : overview;

                    command.Parameters.Add("@Poster_URL", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(posterUrl) ? DBNull.Value : posterUrl;

                    command.Parameters.Add("@Running_Time", SqlDbType.Int).Value =
                        runningTime.HasValue ? runningTime.Value : DBNull.Value;

                    command.Parameters.Add("@Age_Rating", SqlDbType.NVarChar, 5).Value =
                        string.IsNullOrWhiteSpace(ageRating) ? DBNull.Value : ageRating;

                    using SqlDataReader reader = command.ExecuteReader();

                    if (!reader.Read())
                    {
                        throw new Exception("콘텐츠 등록 결과를 가져오지 못했습니다.");
                    }

                    newContentId = DbReaderHelper.GetInt(reader, "New_Content_ID");
                }

                // 새 콘텐츠와 선택된 장르들을 연결
                foreach (int genreId in genreIds)
                {
                    using SqlCommand genreCommand = new SqlCommand(@"
                INSERT INTO dbo.ContentGenre
                (
                    Content_ID,
                    Genre_ID
                )
                VALUES
                (
                    @Content_ID,
                    @Genre_ID
                );
            ", connection, transaction);

                    genreCommand.Parameters.Add("@Content_ID", SqlDbType.Int).Value = newContentId;
                    genreCommand.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreId;

                    genreCommand.ExecuteNonQuery();
                }

                // 새 콘텐츠와 선택된 OTT 플랫폼들을 연결
                foreach (int ottId in ottIds)
                {
                    using SqlCommand ottCommand = new SqlCommand(@"
                INSERT INTO dbo.ContentOTTProvider
                (
                    Content_ID,
                    OTT_ID
                )
                VALUES
                (
                    @Content_ID,
                    @OTT_ID
                );
            ", connection, transaction);

                    ottCommand.Parameters.Add("@Content_ID", SqlDbType.Int).Value = newContentId;
                    ottCommand.Parameters.Add("@OTT_ID", SqlDbType.Int).Value = ottId;

                    ottCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                return newContentId;
            }
            catch
            {
                // 중간에 실패하면 콘텐츠/매핑 저장을 모두 되돌림
                transaction.Rollback();
                throw;
            }
        }

        // 콘텐츠 기본 정보와 TMDB 평점/연령 등급까지 수정
        public void UpdateContent(
            int contentId,
            string title,
            DateTime? releaseDate,
            decimal? tmdbAverageRating,
            string overview,
            string posterUrl,
            int? runningTime,
            string ageRating
        )
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_UpdateContent", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;

            command.Parameters.Add("@Title", SqlDbType.NVarChar, 50).Value = title;

            command.Parameters.Add("@Release_Date", SqlDbType.Date).Value =
                releaseDate.HasValue ? releaseDate.Value.Date : DBNull.Value;

            SqlParameter ratingParameter = command.Parameters.Add(
                "@TMDB_Average_Rating",
                SqlDbType.Decimal
            );

            // DB decimal(3,1)에 맞춰 TMDB 평점 자리수를 지정
            ratingParameter.Precision = 3;
            ratingParameter.Scale = 1;
            ratingParameter.Value =
                tmdbAverageRating.HasValue ? tmdbAverageRating.Value : DBNull.Value;

            // 빈 문자열은 DB NULL로 저장
            command.Parameters.Add("@OverView", SqlDbType.NVarChar).Value =
                string.IsNullOrWhiteSpace(overview) ? DBNull.Value : overview;

            command.Parameters.Add("@Poster_URL", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(posterUrl) ? DBNull.Value : posterUrl;

            command.Parameters.Add("@Running_Time", SqlDbType.Int).Value =
                runningTime.HasValue ? runningTime.Value : DBNull.Value;

            command.Parameters.Add("@Age_Rating", SqlDbType.NVarChar, 5).Value =
                string.IsNullOrWhiteSpace(ageRating) ? DBNull.Value : ageRating;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 평점/연령 등급 없이 콘텐츠 기본 정보만 수정하는 간단 수정용 메서드
        public void UpdateContent(
            int contentId,
            string title,
            DateTime? releaseDate,
            string overview,
            string posterUrl,
            int? runningTime
        )
        {
            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_UpdateContent", connection);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            command.Parameters.Add("@Content_ID", System.Data.SqlDbType.Int).Value = contentId;
            command.Parameters.Add("@Title", System.Data.SqlDbType.NVarChar, 50).Value = title;

            command.Parameters.Add("@Release_Date", System.Data.SqlDbType.Date).Value =
                releaseDate.HasValue ? releaseDate.Value.Date : DBNull.Value;

            command.Parameters.Add("@OverView", System.Data.SqlDbType.NVarChar).Value =
                string.IsNullOrWhiteSpace(overview) ? DBNull.Value : overview;

            command.Parameters.Add("@Poster_URL", System.Data.SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(posterUrl) ? DBNull.Value : posterUrl;

            command.Parameters.Add("@Running_Time", System.Data.SqlDbType.Int).Value =
                runningTime.HasValue ? runningTime.Value : DBNull.Value;

            connection.Open();

            command.ExecuteNonQuery();
        }

        // 선택된 콘텐츠들을 관련 데이터와 함께 삭제
        public void DeleteContents(List<int> contentIds)
        {
            if (contentIds == null || contentIds.Count == 0)
            {
                throw new Exception("삭제할 콘텐츠가 없습니다.");
            }

            using SqlConnection connection = DbConnectionFactory.CreateConnection();

            connection.Open();

            foreach (int contentId in contentIds)
            {
                using SqlCommand command = new SqlCommand("dbo.Usp_DeleteContentWithRelation", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Content_ID", SqlDbType.Int).Value = contentId;

                command.ExecuteNonQuery();
            }
        }

    }
}
