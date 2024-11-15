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
        public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        private OleDbConnection mC;

        public AuthForm()
        {
            InitializeComponent();

            mC = new OleDbConnection(s);
            mC.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OleDbCommand cmd = new OleDbCommand($"SELECT count(login)>0 FROM AuthorizationInfo WHERE login = '{loginBox.Text}' AND hashedPassword = '{passwordBox.Text}'", mC);
            if (Convert.ToInt32(cmd.ExecuteScalar()) != 0)
            {
                cmd = new OleDbCommand($"SELECT user_id FROM AuthorizationInfo WHERE login = '{loginBox.Text}'", mC);
                int id_ = Convert.ToInt32(cmd.ExecuteScalar());

                ClientView cV = new ClientView(id_);
                cV.Show();
                this.Hide();
            }
            else MessageBox.Show("Неверное имя пользователя или пароль!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RegForm rF = new RegForm();

            rF.Show();
            this.Hide();
        }
    }
}
