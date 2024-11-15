using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDKursovaya
{
    public partial class RegForm : Form
    {
        public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        private OleDbConnection mC;

        public RegForm()
        {
            InitializeComponent();

            mC = new OleDbConnection(s);
            mC.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (loginBox.Text != String.Empty && passwordBox.Text != String.Empty && nameBox.Text != String.Empty && genderBox.SelectedItem != null)
            {
                OleDbCommand c = new OleDbCommand($"SELECT count(login)>0 FROM AuthorizationInfo WHERE login = '{loginBox.Text}'", mC);
                if (Convert.ToInt16(c.ExecuteScalar()) != 0)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                }
                else
                {
                    OleDbCommand cmd = new OleDbCommand($"INSERT INTO Clients(fullName, gender) VALUES('{nameBox.Text}', '{genderBox.SelectedItem.ToString()}')", mC);
                    cmd.ExecuteNonQuery();

                    cmd = new OleDbCommand("SELECT TOP 1 id FROM Clients ORDER BY id DESC", mC);
                    int id_ = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new OleDbCommand($"INSERT INTO AuthorizationInfo VALUES({id_}, '{loginBox.Text}', '{passwordBox.Text}')", mC);
                    cmd.ExecuteNonQuery();

                    ClientView cV = new ClientView(id_);
                    cV.Show();
                    this.Hide();
                }
            }
            else MessageBox.Show("Заполнены не все поля!");
        }
    }
}
