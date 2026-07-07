using CineCode.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CineCode
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SystemSettingService systemSettingService = new SystemSettingService();
                systemSettingService.EnsureDefaultSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "CINECODE 실행에 필요한 DB 연결 또는 기본 설정 초기화에 실패했습니다.\n\n" +
                    "확인할 항목:\n" +
                    "1. SQL Server Express가 실행 중인지 확인\n" +
                    "2. .\\SQLEXPRESS에 CINECODEDB가 복원되어 있는지 확인\n" +
                    "3. 현재 Windows 계정에 CINECODEDB 접근 권한이 있는지 확인\n\n" +
                    "오류 내용:\n" + ex.GetBaseException().Message,
                    "CINECODE 시작 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
                return;
            }

            base.OnStartup(e);
        }
    }

}
