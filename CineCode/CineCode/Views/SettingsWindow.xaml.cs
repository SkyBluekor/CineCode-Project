using System;
using System.IO;
using System.Windows;
using CineCode.Services;

namespace CineCode.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SystemSettingService _systemSettingService = new SystemSettingService();

        public SettingsWindow()
        {
            InitializeComponent();

            _systemSettingService.EnsureDefaultSettings();

            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                CsvExportPathTextBox.Text = _systemSettingService.GetCsvExportPath();

                int defaultSubscriptionDays = _systemSettingService.GetDefaultSubscriptionDays();
                DefaultSubscriptionDaysText.Text = $"{defaultSubscriptionDays}일";

                int minRatingProgress = _systemSettingService.GetMinRatingProgress();
                MinRatingProgressTextBox.Text = minRatingProgress.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"환경 설정값을 불러오는 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "환경 설정 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void BrowseCsvPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "CSV 파일을 저장할 폴더를 선택하세요."
            };

            if (!string.IsNullOrWhiteSpace(CsvExportPathTextBox.Text)
                && Directory.Exists(CsvExportPathTextBox.Text))
            {
                dialog.InitialDirectory = CsvExportPathTextBox.Text;
            }

            bool? result = dialog.ShowDialog(this);

            if (result == true)
            {
                CsvExportPathTextBox.Text = dialog.FolderName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSettings())
            {
                return;
            }

            try
            {
                string csvExportPath = CsvExportPathTextBox.Text.Trim();
                int minRatingProgress = int.Parse(MinRatingProgressTextBox.Text.Trim());

                if (!Directory.Exists(csvExportPath))
                {
                    Directory.CreateDirectory(csvExportPath);
                }

                _systemSettingService.UpdateCsvExportPath(csvExportPath);
                _systemSettingService.UpdateMinRatingProgress(minRatingProgress);

                MessageBox.Show(
                    "환경 설정이 저장되었습니다.",
                    "환경 설정 저장",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"환경 설정 저장 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "환경 설정 저장 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(CsvExportPathTextBox.Text))
            {
                MessageBox.Show("CSV 저장 경로를 입력하세요.");
                CsvExportPathTextBox.Focus();
                return false;
            }

            string csvExportPath = CsvExportPathTextBox.Text.Trim();

            try
            {
                Path.GetFullPath(csvExportPath);
            }
            catch
            {
                MessageBox.Show("CSV 저장 경로 형식이 올바르지 않습니다.");
                CsvExportPathTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MinRatingProgressTextBox.Text.Trim(), out int minRatingProgress))
            {
                MessageBox.Show("최소 평가 가능 시청률은 숫자로 입력하세요.");
                MinRatingProgressTextBox.Focus();
                return false;
            }

            if (minRatingProgress < 0 || minRatingProgress > 100)
            {
                MessageBox.Show("최소 평가 가능 시청률은 0부터 100 사이로 입력하세요.");
                MinRatingProgressTextBox.Focus();
                return false;
            }

            return true;
        }
    }
}
