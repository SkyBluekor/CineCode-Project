using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CineCode.Models.User;
using CineCode.Services;

namespace CineCode.Views
{
    public partial class UserManagementView : UserControl
    {
        private int? _selectedUserId = null;
        private string _selectedUserName = "";
        private string _selectedGender = "";
        private string _selectedUserType = "";
        private string _selectedPhone = "";

        private readonly UserManagementService _userManagementService = new UserManagementService();
        private List<UserListItemModel> _users = new List<UserListItemModel>();

        // 사용자 관리 화면을 초기화하고 기본 목록 화면을 표시합니다.
        public UserManagementView()
        {
            InitializeComponent();

            ShowUserListPanel();

            LoadUserList();

            ResetAddUserForm();
            ResetEditUserForm();

            SetUserActionButtonsEnabled(false);
        }

        // 사용자 목록을 조회하고 요약 카드와 선택 상태를 초기화합니다.
        private void LoadUserList()
        {
            try
            {
                _users = _userManagementService.GetUserList();

                UserDataGrid.ItemsSource = null;
                UserDataGrid.ItemsSource = _users;

                LoadUserSummary();

                ClearSelectedUserInfo();
                SetUserActionButtonsEnabled(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "사용자 목록을 불러오는 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "사용자 목록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 현재 사용자 목록 기준으로 요약 카드 수치를 계산합니다.
        private void LoadUserSummary()
        {
            if (_users == null)
            {
                TotalUserCountText.Text = "0";
                ActiveSubscriptionUserCountText.Text = "0";
                ChildUserCountText.Text = "0";
                AdultUserCountText.Text = "0";
                return;
            }

            int totalUserCount = _users.Count;

            int actvieSubscriptionUserCount = _users.Count(user => user.UserStatus == "활성");
            int childUserCount = _users.Count(user => user.UserType == "아동");
            int aduldUserCount = _users.Count(user => user.UserType == "성인");

            TotalUserCountText.Text = totalUserCount.ToString();
            ActiveSubscriptionUserCountText.Text = actvieSubscriptionUserCount.ToString();
            ChildUserCountText.Text = childUserCount.ToString();
            AdultUserCountText.Text = aduldUserCount.ToString();
        }

        // 검색어, 성별, 구분, 상태 조건을 조합해 사용자 목록을 필터링합니다.
        private void ApplyUserFilter()
        {
            if (UserDataGrid == null
                || UserSearchTextBox == null
                || GenderFilterComboBox == null
                || UserTypeFilterComboBox == null
                || UserStatusFilterComboBox == null)
            {
                return;
            }

            if (_users == null)
            {
                UserDataGrid.ItemsSource = null;
                return;
            }

            string keyword = UserSearchTextBox.Text.Trim().ToLower();

            string genderFilter = GetSelectedComboBoxText(GenderFilterComboBox);
            string userTypeFilter = GetSelectedComboBoxText(UserTypeFilterComboBox);
            string statusFilter = GetSelectedComboBoxText(UserStatusFilterComboBox);

            // 검색어와 각 필터 조건을 모두 만족하는 사용자만 DataGrid에 표시합니다.
            List<UserListItemModel> filteredUsers = _users.Where(user =>
            {
                bool keywordMatcehd =
                    string.IsNullOrWhiteSpace(keyword)
                    || user.User_ID.ToString().Contains(keyword)
                    || (user.User_Name ?? "").ToLower().Contains(keyword)
                    || (user.User_Sex ?? "").ToLower().Contains(keyword)
                    || (user.UserType ?? "").ToLower().Contains(keyword)
                    || (user.UserStatus ?? "").ToLower().Contains(keyword)
                    || (user.User_PhoneNumber ?? "").ToLower().Contains(keyword)
                    || (user.PhoneText ?? "").Contains(keyword);

                bool genderMatched =
                    string.IsNullOrWhiteSpace(genderFilter)
                    || genderFilter.Contains("전체")
                    || user.User_Sex == genderFilter;

                bool userTypeMatched =
                    string.IsNullOrWhiteSpace(userTypeFilter)
                    || userTypeFilter.Contains("전체")
                    || user.UserType == userTypeFilter;

                bool statusMatched =
                    string.IsNullOrWhiteSpace(statusFilter)
                    || statusFilter.Contains("전체")
                    || user.UserStatus == statusFilter;

                return keywordMatcehd && userTypeMatched && statusMatched && genderMatched;
            }).ToList();

            UserDataGrid.ItemsSource = null;
            UserDataGrid.ItemsSource = filteredUsers;

            ClearSelectedUserInfo();
            SetUserActionButtonsEnabled(false);
        }

        // 사용자 검색어가 바뀌면 목록 필터를 다시 적용합니다.
        private void UserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyUserFilter();
        }

        // 사용자 필터 ComboBox가 바뀌면 목록 필터를 다시 적용합니다.
        private void UserFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyUserFilter();
        }

        // 사용자 목록에서 선택한 행 정보를 수정 폼 상태로 보관합니다.
        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserDataGrid.SelectedItem == null)
            {
                ClearSelectedUserInfo();
                SetUserActionButtonsEnabled(false);
                return;
            }

            LoadSelectedUserInfo();
            SetUserActionButtonsEnabled(true);
        }

        // 선택한 사용자 정보를 필드와 수정 화면 입력값에 반영합니다.
        private void LoadSelectedUserInfo()
        {
            object selectedItem = UserDataGrid.SelectedItem;

            _selectedUserId = ToNullableInt(GetValue(selectedItem, "User_ID"));
            _selectedUserName = ToStringValue(GetValue(selectedItem, "User_Name"));
            _selectedGender = ToStringValue(GetValue(selectedItem, "User_Sex"));
            _selectedUserType = ToStringValue(GetValue(selectedItem, "UserType"));
            _selectedPhone = ToStringValue(GetValue(selectedItem, "User_PhoneNumber"));

            EditUserIdText.Text = _selectedUserId?.ToString() ?? "-";
            EditGenderText.Text = string.IsNullOrWhiteSpace(_selectedGender) ? "-" : _selectedGender;
            EditUserTypeText.Text = string.IsNullOrWhiteSpace(_selectedUserType) ? "-" : _selectedUserType;

            EditUserNameTextBox.Text = _selectedUserName;
            EditPhoneTextBox.Text = _selectedPhone;
        }

        // 선택 사용자 상태를 비우고 수정 폼을 초기화합니다.
        private void ClearSelectedUserInfo()
        {
            _selectedUserId = null;
            _selectedUserName = "";
            _selectedGender = "";
            _selectedUserType = "";
            _selectedPhone = "";

            ResetEditUserForm();
        }

        // 선택 여부에 따라 수정/삭제 버튼을 활성화합니다.
        private void SetUserActionButtonsEnabled(bool isEnabled)
        {
            EditUserButton.IsEnabled = isEnabled;
            DeleteUserButton.IsEnabled = isEnabled;
        }

        // 사용자 목록 화면만 표시합니다.
        private void ShowUserListPanel()
        {
            UserListPanel.Visibility = Visibility.Visible;
            UserAddPanel.Visibility = Visibility.Collapsed;
            UserEditPanel.Visibility = Visibility.Collapsed;
        }

        // 사용자 등록 화면만 표시합니다.
        private void ShowUserAddPanel()
        {
            UserListPanel.Visibility = Visibility.Collapsed;
            UserAddPanel.Visibility = Visibility.Visible;
            UserEditPanel.Visibility = Visibility.Collapsed;
        }

        // 사용자 수정 화면만 표시합니다.
        private void ShowUserEditPanel()
        {
            UserListPanel.Visibility = Visibility.Collapsed;
            UserAddPanel.Visibility = Visibility.Collapsed;
            UserEditPanel.Visibility = Visibility.Visible;
        }

        // 사용자 등록 화면으로 이동합니다.
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAddUserForm();
            ShowUserAddPanel();
        }

        // 등록 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromAddUserButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserListPanel();
        }

        // 사용자 등록 입력값을 초기화합니다.
        private void ResetAddUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAddUserForm();
        }

        // 등록 입력값을 검증한 뒤 사용자를 추가합니다.
        private void SaveAddUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAddUserForm())
            {
                return;
            }

            string userName = AddUserNameTextBox.Text.Trim();
            string sex = GetSelectedComboBoxText(AddGenderComboBox);
            string phone = AddPhoneTextBox.Text.Trim();
            DateTime birthDate = AddBirthDatePicker.SelectedDate!.Value.Date;
            string userType = AddUserTypeText.Text.Trim();

            try
            {
                _userManagementService.AddUser(
                    userName,
                    birthDate,
                    phone,
                    sex);

                MessageBox.Show(
                    "사용자가 등록되었습니다.",
                    "사용자 등록완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ResetAddUserForm();
                LoadUserList();
                ShowUserListPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "사용자 등록 중 오류가 발생했습니다.\n\n" + ex.Message,
                    "사용자 등록 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 생년월일이 바뀌면 아동/성인 구분을 다시 계산합니다.
        private void AddBirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddBirthDatePicker.SelectedDate == null)
            {
                AddUserTypeText.Text = "-";
                return;
            }

            DateTime birthDate = AddBirthDatePicker.SelectedDate.Value;

            AddUserTypeText.Text = GetUserTypeByBirthDate(birthDate);
        }

        // 등록 화면 입력값을 기본 상태로 되돌립니다.
        private void ResetAddUserForm()
        {
            AddUserNameTextBox.Clear();
            AddGenderComboBox.SelectedIndex = -1;
            AddPhoneTextBox.Clear();

            AddBirthDatePicker.SelectedDate = null;
            AddUserTypeText.Text = "-";
        }

        // 사용자 등록에 필요한 필수값과 휴대전화 형식을 검증합니다.
        private bool ValidateAddUserForm()
        {
            if (string.IsNullOrWhiteSpace(AddUserNameTextBox.Text))
            {
                MessageBox.Show("이름을 입력하세요.");
                AddUserNameTextBox.Focus();
                return false;
            }

            if (AddGenderComboBox.SelectedItem == null)
            {
                MessageBox.Show("성별을 선택하세요.");
                AddGenderComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(AddPhoneTextBox.Text))
            {
                MessageBox.Show("휴대전화를 입력하세요.");
                AddPhoneTextBox.Focus();
                return false;
            }

            if (AddBirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("생년월일을 선택하세요.");
                AddBirthDatePicker.Focus();
                return false;
            }

            if (AddUserTypeText.Text == "-")
            {
                MessageBox.Show("생년월일을 확인해 구분을 계산해야 합니다.");
                AddBirthDatePicker.Focus();
                return false;
            }

            string phone = AddPhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("휴대전화를 입력하세요.");
                AddPhoneTextBox.Focus();
                return false;
            }

            if (!phone.All(char.IsDigit))
            {
                MessageBox.Show("휴대전화는 숫자만 입력하세요.");
                AddPhoneTextBox.Focus();
                return false;
            }

            if (phone.Length < 10 || phone.Length > 11)
            {
                MessageBox.Show("휴대전화는 10~11자리로 입력하세요.");
                AddPhoneTextBox.Focus();
                return false;
            }

            return true;
        }

        // 선택한 사용자 정보를 수정 화면에 불러옵니다.
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show("수정할 사용자를 선택하세요.");
                return;
            }

            ResetEditUserForm();
            LoadSelectedUserInfo();

            ShowUserEditPanel();
        }

        // 수정 화면에서 목록 화면으로 돌아갑니다.
        private void BackFromEditUserButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUserListPanel();
        }

        // 수정 입력값을 선택 사용자 원본 값으로 되돌립니다.
        private void ResetEditUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null)
            {
                ResetEditUserForm();
                return;
            }

            LoadSelectedUserInfo();
        }

        // 수정 입력값을 검증하고 수정 저장 흐름을 실행합니다.
        private void SaveEditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show("수정할 사용자를 선택하세요.");
                return;
            }

            if (!ValidateEditUserForm())
            {
                return;
            }

            string userName = EditUserNameTextBox.Text.Trim();
            string phone = EditPhoneTextBox.Text.Trim();

            MessageBox.Show(
                "여기서 나중에 사용자 수정 UPDATE 또는 프로시저를 호출하면 됩니다.\n\n" +
                $"User_ID: {_selectedUserId}\n" +
                $"이름: {userName}\n" +
                $"휴대전화: {phone}",
                "사용자 수정",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            ShowUserListPanel();
        }

        // 수정 화면의 읽기 전용 값과 입력값을 비웁니다.
        private void ResetEditUserForm()
        {
            EditUserIdText.Text = "-";
            EditGenderText.Text = "-";
            EditUserTypeText.Text = "-";

            EditUserNameTextBox.Clear();
            EditPhoneTextBox.Clear();
        }

        // 사용자 수정에 필요한 필수값을 검증합니다.
        private bool ValidateEditUserForm()
        {
            if (string.IsNullOrWhiteSpace(EditUserNameTextBox.Text))
            {
                MessageBox.Show("이름을 입력하세요.");
                EditUserNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditPhoneTextBox.Text))
            {
                MessageBox.Show("휴대전화를 입력하세요.");
                EditPhoneTextBox.Focus();
                return false;
            }

            return true;
        }

        // 선택한 사용자의 삭제 확인 흐름을 실행합니다.
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUserId == null)
            {
                MessageBox.Show("삭제할 사용자를 선택하세요.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"User ID {_selectedUserId} 사용자를 삭제하시겠습니까?",
                "사용자 삭제",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            MessageBox.Show(
                "여기서 나중에 DELETE 또는 삭제 프로시저를 호출하면 됩니다.",
                "사용자 삭제",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // ComboBoxItem 기반 필터와 일반 Text 기반 입력 모두에서 선택 문자열을 가져옵니다.
        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox == null)
            {
                return "";
            }

            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString()?.Trim() ?? "";
            }

            return comboBox.Text?.Trim() ?? "";
        }

        // DataRowView와 일반 모델 객체 양쪽에서 같은 방식으로 값을 꺼냅니다.
        private object GetValue(object item, string columnName)
        {
            if (item == null)
            {
                return null;
            }

            if (item is DataRowView rowView)
            {
                if (rowView.Row.Table.Columns.Contains(columnName))
                {
                    return rowView[columnName];
                }

                return null;
            }

            PropertyInfo property = item.GetType().GetProperty(columnName);

            if (property == null)
            {
                return null;
            }

            return property.GetValue(item);
        }

        // null, DB null, 문자열 숫자를 모두 nullable int로 변환합니다.
        private int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            return null;
        }

        // null 또는 DB null을 빈 문자열로 변환합니다.
        private string ToStringValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            return value.ToString() ?? "";
        }

        // 생년월일 기준으로 만 나이를 계산해 아동/성인을 구분합니다.
        private string GetUserTypeByBirthDate(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age >= 19 ? "성인" : "아동";
        }

        // 휴대전화 입력칸에 숫자만 입력되도록 제한합니다.
        private void PhoneNumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // 휴대전화 입력칸에 붙여넣는 값도 숫자만 허용합니다.
        private void PhoneNumberOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (!text.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
