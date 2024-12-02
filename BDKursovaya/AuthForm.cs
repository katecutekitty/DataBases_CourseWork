using cursovaya;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDKursovaya
{
    public partial class AuthForm : Form
    {

        //public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        //private OleDbConnection mC;

        //public AuthForm()
        //{
        //    InitializeComponent();

        //    mC = new OleDbConnection(s);
        //    mC.Open();

        public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        private OleDbConnection mC;

        public AuthForm()
        {
            InitializeComponent();
            mC = new OleDbConnection(s);
            mC.Open();

        }

        public string returnLogin()
        {
            return loginBox.Text.ToString();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            string login = loginBox.Text;
            string password = passwordBox.Text;


            //commented by ArtemyBombastic 01.12.2024 - пока работаем без хэш паролей
            //string hashpassword = String.Empty;
            //using (MD5 md5 = MD5.Create())
            //{
            //    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
            //    byte[] hashBytes = md5.ComputeHash(inputBytes);
            //    hashpassword = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            //}

            OleDbCommand command = new OleDbCommand($"SELECT COUNT(*) FROM AuthorizationInfo WHERE login = \"{login}\"", mC);
            string existUser = command.ExecuteScalar().ToString();
            command = new OleDbCommand($"SELECT COUNT(*) FROM AuthorizationInfo WHERE login = \"{login}\" AND password = \"{password}\"", mC);
            string existUserWithPass = command.ExecuteScalar().ToString();

            if (existUser == "0")
            {
                MessageBox.Show("Вы не зарегистрированы!");
                return;
            }
            else if (existUserWithPass == "0")
            {
                MessageBox.Show("Неверный пароль!");
                return;
            }
            else
            {
                MessageBox.Show("Вы успешно вошли в систему!");
                //command = new OleDbCommand($"SELECT [Номер клиента] FROM Users WHERE Login = \"{login}\" ", Program.connectionDB);
                //int user_id = Convert.ToInt32(command.ExecuteScalar());
                command = new OleDbCommand($"SELECT status FROM AuthorizationInfo WHERE  login = \"{login}\" ", mC);
                OleDbCommand command1 = new OleDbCommand($"SELECT user_id FROM AuthorizationInfo WHERE  login = \"{login}\" ", mC);
                int root_id = Convert.ToInt32(command.ExecuteScalar());
                int user_id = Convert.ToInt32(command1.ExecuteScalar());
                //command = new OleDbCommand($"SELECT root_name FROM roots WHERE  root_id = {root_id} ", Program.connectionDB);
                //string root_name = command.ExecuteScalar().ToString();

                MessageBox.Show(root_id.ToString());

                if (root_id == 0)
                {
                    ClientView adminForm = new ClientView(1);
                    adminForm.ShowDialog();
                }
                else if (root_id == 1)
                {
                    Admin_window adminPrimeForm = new Admin_window();
                    adminPrimeForm.ShowDialog();
                }

            }
        }
    }
}
