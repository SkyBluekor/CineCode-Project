using System.Data;
using CineCode.Data;
using CineCode.Models.User;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    public class UserManagementService
    {
        // 사용자 목록과 각 사용자의 구독 요약 정보를 조회
        public List<UserListItemModel> GetUserList()
        {
            List<UserListItemModel> users = new List<UserListItemModel>();

            string sql = @"
                SELECT
                    U.User_ID,
                    U.USER_NAME,
                    U.User_BirthDate,
                    U.User_PhoneNumber,
                    U.User_Sex,

                    ISNULL(S.ActiveSubscriptionCount, 0) AS SubscriptionCount,
                    S.LatestExpiredAt
                FROM dbo.AppUser U
                OUTER APPLY
                (
                    SELECT
                        COUNT(CASE 
                                WHEN US.expired_at >= CAST(GETDATE() AS DATE) 
                                THEN 1 
                                END) AS ActiveSubscriptionCount,

                        MAX(US.expired_at) AS LatestExpiredAt
                    FROM dbo.Usersubscription US
                    WHERE US.User_ID = U.User_ID
                ) S
                ORDER BY U.User_ID DESC;
            ";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                UserListItemModel user = new UserListItemModel
                {
                    User_ID = DbReaderHelper.GetInt(reader, "User_ID"),
                    User_Name = DbReaderHelper.GetString(reader, "User_Name"),
                    User_BirthDate = DbReaderHelper.GetDateTime(reader, "User_BirthDate"),
                    User_PhoneNumber = DbReaderHelper.GetString(reader, "User_PhoneNumber"),
                    User_Sex = DbReaderHelper.GetString(reader, "User_Sex").Trim(),

                    SubscriptionCount = DbReaderHelper.GetInt(reader, "SubscriptionCount"),
                    LatestExpiredAt = DbReaderHelper.GetNullableDateTime(reader, "LatestExpiredAt")
                };

                users.Add(user);
            }

            return users;
        }

        // 입력값을 검증한 뒤 신규 사용자를 등록하고 생성된 사용자 ID를 반환
        public int AddUser(
            string userName,
            DateTime birthDate,
            string phoneNumber,
            string userSex)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new Exception("사용자 이름은 필수입니다.");
            }

            if (birthDate.Date > DateTime.Today)
            {
                throw new Exception("생년월일은 오늘 이후의 날짜일 수 없습니다.");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new Exception("전화번호는 필수입니다.");
            }

            if (string.IsNullOrWhiteSpace(userSex))
            {
                throw new Exception("성별은 필수 입니다.");
            }

            string sql = @"
                insert into dbo.AppUser
                (
                    USER_NAME,
                    User_BirthDate,
                    User_PhoneNumber,
                    User_Sex
                )
                output inserted.User_ID
                values
                (
                    @User_Name,
                    @User_BirthDate,
                    @User_PhoneNumber,
                    @User_Sex
                );";

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@User_Name", SqlDbType.NVarChar, 20).Value = userName.Trim();
            command.Parameters.Add("@User_BirthDate", SqlDbType.Date).Value = birthDate.Date;
            command.Parameters.Add("@User_PhoneNumber", SqlDbType.NVarChar, 40).Value = phoneNumber.Trim();
            command.Parameters.Add("@User_Sex", SqlDbType.NChar, 4).Value = userSex.Trim();

            connection.Open();

            int newUserID = Convert.ToInt32(command.ExecuteScalar());
            return newUserID;
        }
    }
}

