using System.Data;
using CineCode.Data;
using CineCode.Model.DashBoard;
using Microsoft.Data.SqlClient;

namespace CineCode.Services
{
    // 대시보드 화면에 필요한 요약 데이터를 조회하는 서비스
    public class DashboardService
    {
        // 대시보드 상단 카드와 알림 영역에 표시할 집계 데이터를 가져옴
        public DashboardModel GetDashboardData()
        {
            DashboardModel dashboard = new DashboardModel();

            using SqlConnection connection = DbConnectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand("dbo.Usp_GetDashboardSummary", connection);

            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                // 전체 현황 카운트
                dashboard.TotalContentCount = DbReaderHelper.GetInt(reader, "TotalContentCount");
                dashboard.TotalUserCount = DbReaderHelper.GetInt(reader, "TotalUserCount");
                dashboard.ActiveSubscriptionCount = DbReaderHelper.GetInt(reader, "ActiveSubscriptionCount");
                dashboard.OttProviderCount = DbReaderHelper.GetInt(reader, "OttProviderCount");

                // 관리자가 확인해야 하는 미연결/만료 임박 항목 카운트
                dashboard.UnmappedGenreContentCount = DbReaderHelper.GetInt(reader, "UnmappedGenreContentCount");
                dashboard.UnmappedOttContentCount = DbReaderHelper.GetInt(reader, "UnmappedOttContentCount");
                dashboard.ExpiringSubscriptionCount = DbReaderHelper.GetInt(reader, "ExpiringSubscriptionCount");
            }

            return dashboard;
        }
    }
}
