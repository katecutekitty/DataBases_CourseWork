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
    public partial class ClientView : Form
    {
        public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        private OleDbConnection mC;
        private int currentId = 1;

        public ClientView()
        {
            InitializeComponent();
            mC = new OleDbConnection(s);
            mC.Open();

            OleDbCommand comm = new OleDbCommand("UPDATE Bookings SET totalPrice = IIf([masterСat] = 'I', [price],IIf([masterСat]= 'II', [price]*2,IIf([masterСat]= 'III', [price]*3,[price]*4)))", mC);
            comm.ExecuteNonQuery();

            OleDbCommand cmd = new OleDbCommand("SELECT * FROM Services", mC);
            var l = cmd.ExecuteReader();
            comboBox1.Items.Clear();
            while (l.Read()) comboBox1.Items.Add(l[0].ToString());
            l.Close();

            RefreshList();
        }

        private int getPrice(string serviceTitle)
        {
            int price = Convert.ToInt32(new OleDbCommand($"SELECT price from Services WHERE title = '{serviceTitle}'", mC).ExecuteScalar());
            return price;
        }

        private Tuple<int, string> getIdAndCat(string master)
        {
            OleDbCommand c = new OleDbCommand($"SELECT id, category FROM Masters WHERE fullName = '{master}'", mC);
            var l = c.ExecuteReader();
            Tuple<int, string> result = Tuple.Create(0, "");
            while (l.Read())
            {
                result = Tuple.Create(Convert.ToInt32(l[0]), l[1].ToString());
            }
            l.Close();
            return result;
        }

        private void RefreshPrice()
        {
            int currentPrice = 0;
            if (comboBox1.SelectedItem != null) {
                currentPrice = getPrice(comboBox1.SelectedItem.ToString());
                if (comboBox2.SelectedItem != null)
                {
                    string category = getIdAndCat(comboBox2.SelectedItem.ToString()).Item2;
                    if (category == "II")
                    {
                        currentPrice *= 2;
                    }
                    if (category == "III")
                    {
                        currentPrice *= 3;
                    }
                    if (category == "Высшая") { currentPrice *= 4; }
                }
            }
            textBox1.Text = currentPrice.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Не заполнено одно или несколько полей: выбор мастера и/или выбор услуги");
            }
            else
            {
                var selectedService = comboBox1.SelectedItem.ToString();
                var selectedMaster = comboBox2.SelectedItem.ToString();

                var masterArgs = getIdAndCat(selectedMaster);

                int selectedMasterId = masterArgs.Item1;
                string selectedMasterCategory = masterArgs.Item2;

                int servicePrice = getPrice(selectedService);

                string selectedDate = dateTimePicker1.Value.Date.ToString();

                OleDbCommand cmd = new OleDbCommand($"INSERT INTO Bookings (client_id, master_id, service_title, insertion_date, appointment_date_time, price, masterСat) VALUES ({currentId}, {selectedMasterId}, '{selectedService}', '{selectedDate}', '{selectedDate}', {servicePrice}, '{selectedMasterCategory}')",mC);
                cmd.ExecuteNonQuery();
            }
            RefreshList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedService = comboBox1.SelectedItem.ToString();

            OleDbCommand cmd = new OleDbCommand($"SELECT * FROM Masters LEFT JOIN Services ON Masters.specialization = Services.category WHERE Services.title = '{selectedService}'", mC);

            var l = cmd.ExecuteReader();
            comboBox2.Items.Clear();
            while (l.Read()) comboBox2.Items.Add(l[1].ToString());
            l.Close();

            RefreshPrice();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPrice();
        }

        private void RefreshList()
        {
            OleDbCommand cmd = new OleDbCommand($"SELECT Bookings.*, Masters.fullName FROM Bookings LEFT JOIN Masters ON Bookings.master_id = Masters.id WHERE client_id = {currentId} AND appointment_date_time > '{DateTime.Now.ToString()}'", mC);
            var l = cmd.ExecuteReader();
            listBox1.Items.Clear();
            while (l.Read())
            {
                listBox1.Items.Add(l[3].ToString() + ' ' + l[9].ToString() + ' ' + l[8].ToString() + ' ' + l[5].ToString());
            }
            l.Close();
        }
    }
}
