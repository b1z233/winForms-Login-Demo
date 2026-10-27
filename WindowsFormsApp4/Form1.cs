using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //窗体加载时，检查有没有记住的账号
            LoadSaveUsername();


        }
        private void LoadSaveUsername()
        {
            string savedUsername = Registry.CurrentUser.OpenSubKey(@"Software\MyLoginApp")?.GetValue("LastUser")?.ToString();
            if (!string.IsNullOrEmpty(savedUsername))
            {
                txtUser.Text = savedUsername;
                chkRemeber.Checked = true;
                txtPwd.Focus();

            }
        }
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string Username = txtUser.Text.Trim();
            string Password = txtPwd.Text;

            if(string.IsNullOrWhiteSpace(Username) || Username == "请输入用户名")
            {
                ShowError("用户名不能为空！");
                txtUser.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password == "请输入密码")
            {
                ShowError("密码不能为空！");
                txtPwd.Focus();
                return;
            }
            //界面登陆状态
            btnLogin.Enabled = false;   //禁用登录按钮
            btnLogin.Text = "正在登陆...";
            lblMsg.Text = "";

            //模拟登陆延迟
            await Task.Delay(800);

            //调用数据源验证
            var account = UserAccount.MockDatabase.Login(Username, Password);

            if(account != null)
            {
                //登陆成功
                lblMsg.ForeColor = Color.Green;
                lblMsg.Text = $"欢迎，{account.Username}！角色：{account.Role}";
                //记住用户名
                if (chkRemeber.Checked)
                {
                    Registry.CurrentUser.CreateSubKey(@"Software\MyLoginApp").SetValue("LastUser", account.Username);
                }
                else
                {
                    Registry.CurrentUser.DeleteSubKey(@"Software\MyLoginApp",false);
                }
            //角色跳转
            if(account.Role == "Admin")
                {
                    //跳转到管理员界面 实际跳转不这样
                    MessageBox.Show("跳转到管理员界面...");

                }
                else if (account.Role == "User")
                {
                    //跳转到用户界面
                    MessageBox.Show("跳转到用户界面...");

                }
            }
            else
            {
                //登陆失败
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "用户名或密码错误！";
                txtPwd.Clear();
                txtPwd.Focus();
            }
            //恢复界面状态
            btnLogin.Enabled = true;
            btnLogin.Text = "登录";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //显示错误信息
        private void ShowError(string message)
        {
            lblMsg.ForeColor = Color.Red;
            lblMsg.Text = message;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetPlaceholder(txtUser, "请输入用户名");
            SetPlaceholder(txtPwd, "请输入密码",true);
        }
        private void SetPlaceholder(TextBox textBox, string placeholder, bool isPasswordBox = false)
        {
            //初始状态为灰色，显示提示词
            textBox.ForeColor = Color.Gray;
            textBox.Text = placeholder;
            //密码框，初始状态先把掩码去掉，否则显示******
            
            if(isPasswordBox)
            {
                textBox.PasswordChar = '\0';
            }
            //获得焦点时，清空提示词
            textBox.Enter += (s, e) =>
            {
                //判断内容是否为提示词
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                    //如果是密码框则变成**
                    if (isPasswordBox)
                    {
                        textBox.PasswordChar = chkShowPwd.Checked ?'\0':'*';
                    }
                }
            };
            //失去焦点
            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.ForeColor = Color.Gray;
                    textBox.Text = placeholder;
                    //如果是密码框则去掉掩码
                    if (isPasswordBox) textBox.PasswordChar = '\0';
                }
            };
        }

        private void chkRemeber_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void chkShowPwd_CheckedChanged(object sender, EventArgs e)
        {
            // 如果当前密码框里显示的还是占位符，那就什么都不做，保持占位符状态
            if (txtPwd.Text == "请输入密码")
            {
                return;
            }

            // 如果勾选了“显示密码”，掩码设为空（显示明文）；否则设为 * 号
            if (chkShowPwd.Checked)
            {
                txtPwd.PasswordChar = '\0'; // 显示明文
            }
            else
            {
                txtPwd.PasswordChar = '*';  // 恢复星号
            }

            // 细节：切换显示状态后，把光标重新聚焦到密码框，方便继续输入
            txtPwd.Focus();
        }
    }
    public class UserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }


        public static class MockDatabase
        {
            private static List<UserAccount> users = new List<UserAccount>
            {
                new UserAccount { Username = "admin", Password = "123456", Role = "Admin" },
                new UserAccount { Username = "user1", Password = "123456", Role = "User" }
            };
            public static UserAccount Login(string username,string password)
            {
                return users.FirstOrDefault(u => u.Username == username && u.Password == password);
            }
        }
    }
}
