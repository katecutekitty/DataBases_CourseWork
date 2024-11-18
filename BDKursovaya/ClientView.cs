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
        private int currentId;

        bool Is_it_start = true;

        public ClientView(int cntClientId)
        {
            
            InitializeComponent();
            mC = new OleDbConnection(s);
            mC.Open();

            currentId = cntClientId;
            //dateTimePicker1.Value= DateTime.Now.AddDays(1);

            //OleDbCommand comm = new OleDbCommand("UPDATE (Bookings INNER JOIN Masters ON Bookings.master_id = Masters.id INNER JOIN Services ON Bookings.service_title = Services.title SET totalPrice = IIf([Masters.category] = 'I', [Services.price],IIf([Masters.category]= 'II', [Services.price]*2,IIf([Masters.category]= 'III', [Services.price]*3,[Services.price]*4)))", mC);
            //comm.ExecuteNonQuery();

            OleDbCommand cmd = new OleDbCommand("SELECT * FROM Services", mC);
            var l = cmd.ExecuteReader();
            comboBox1.Items.Clear();
            while (l.Read()) comboBox1.Items.Add(l[0].ToString());
            l.Close();

            RefreshList();
            
            dateTimePicker1.Value = DateTime.Now.AddDays(1);
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

        //created by BombasticFantastic
        private string[] Get_list_of_bool_from_base(System.Data.OleDb.OleDbDataReader l)
        {
            string[] splited_text = l[4].ToString().Split(new char[] { ',' });
            return splited_text;
        }

        //created by BombasticFantastic
        private bool FreeDayCheck(System.Data.OleDb.OleDbDataReader l)
        {
            int dateTimePicker1_day = DateTime.Parse(dateTimePicker1.Text.ToString()).Day;
            int between_today_and_date = dateTimePicker1_day-DateTime.Now.Day-1;
            string[] days = Get_list_of_bool_from_base(l);
            if (days[between_today_and_date] == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
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

                var selectedDate = dateTimePicker1.Value.Date;

                //
                //OleDbCommand cmd = new OleDbCommand($"INSERT INTO Bookings (id, client_id, master_id, service_title, appointment_date_time, totalPrice) VALUES (1, 1, 1, 1, 1)", mC);

                OleDbCommand cmd = new OleDbCommand($"INSERT INTO Bookings (client_id, master_id, service_title, appointment_date_time, totalPrice) VALUES ({currentId}, {selectedMasterId}, '{selectedService}', '{selectedDate}', {servicePrice})",mC);
                cmd.ExecuteNonQuery();
            }
            RefreshList();
        }

        private void RefreshMasters() 
        {
            var selectedService = comboBox1.SelectedItem.ToString();

            //OleDbCommand cmd = new OleDbCommand($"SELECT * FROM Masters LEFT JOIN Services ON Masters.specialization = Services.category WHERE Services.title = '{selectedService}'", mC);
            OleDbCommand cmd = new OleDbCommand($"SELECT * FROM Masters LEFT JOIN Services ON Masters.spec_id = Services.category_id WHERE Services.title = '{selectedService}'", mC);


            var l = cmd.ExecuteReader();
            comboBox2.Items.Clear();
            while (l.Read())
            {
                //if (FreeDayCheck(l))
                if (FreeDayCheck(l))
                {
                    comboBox2.Items.Add(l[1].ToString());
                }
                //comboBox2.Items.Add(FreeDayCheck(l).ToString());
            }
            l.Close();

            
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMasters();
            RefreshPrice();
        }

        //private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var selectedService = comboBox1.SelectedItem.ToString();

        //    //OleDbCommand cmd = new OleDbCommand($"SELECT * FROM Masters LEFT JOIN Services ON Masters.specialization = Services.category WHERE Services.title = '{selectedService}'", mC);
        //    OleDbCommand cmd = new OleDbCommand($"SELECT * FROM Masters LEFT JOIN Services ON Masters.spec_id = Services.category_id WHERE Services.title = '{selectedService}'", mC);


        //    var l = cmd.ExecuteReader();
        //    comboBox2.Items.Clear();
        //    while (l.Read())
        //    {
        //        //if (FreeDayCheck(l))
        //        if (FreeDayCheck(l))
        //        {
        //            comboBox2.Items.Add(l[1].ToString());
        //        }
        //        //comboBox2.Items.Add(FreeDayCheck(l).ToString());
        //    }
        //    l.Close();

        //    RefreshPrice();
        //}



        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPrice();
        }

        private void RefreshList()
        {
            OleDbCommand cmd = new OleDbCommand($"SELECT Bookings.*, Masters.fullName FROM Bookings LEFT JOIN Masters ON Bookings.master_id = Masters.id WHERE client_id = {currentId}", mC);
            var l = cmd.ExecuteReader();
            listBox1.Items.Clear();
            while (l.Read())
            {
                listBox1.Items.Add(l[0].ToString() + ' ' + l[1].ToString() + ' ' + l[3].ToString() + ' ' + l[4].ToString());
            }
            l.Close();
            //RefreshMasters();
        }

        private void RefreshListSort()
        {
            OleDbCommand cmd = new OleDbCommand($"SELECT Bookings.*, Masters.fullName FROM Bookings LEFT JOIN Masters ON Bookings.master_id = Masters.id WHERE client_id = {currentId} ORDER BY appointment_date_time ASC", mC);
            var l = cmd.ExecuteReader();
            listBox1.Items.Clear();
            while (l.Read())
            {
                listBox1.Items.Add(l[0].ToString() + ' ' + l[1].ToString() + ' ' + l[3].ToString() + ' ' + l[4].ToString());
            }
            l.Close();
        }

        private void RefreshPrice()
        {
            int currentPrice = 0;
            if (comboBox1.SelectedItem != null)
            {
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0) {
                string selectedBooking = listBox1.SelectedItems[0].ToString();
                int _id = Convert.ToInt32(selectedBooking.Substring(0, selectedBooking.IndexOf(' ')));
                OleDbCommand cmd = new OleDbCommand($"DELETE FROM Bookings WHERE id = {_id}", mC);
                cmd.ExecuteNonQuery();

                RefreshList();
            }
            else button1.Enabled = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                RefreshListSort();
            }
            else RefreshList();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (Is_it_start)
            {
                Is_it_start = false;
            }
            else
            {
                int dateTimePicker1_day = DateTime.Parse(dateTimePicker1.Text.ToString()).Day;
                int between_today_and_date = dateTimePicker1_day - DateTime.Now.Day - 1;
                if ((DateTime.Parse(dateTimePicker1.Text.ToString()).Day - (DateTime.Now.Day)) <= 7)
                {
                    RefreshMasters();
                }
                else
                {
                    comboBox2.Items.Clear();
                    MessageBox.Show("Максимальное время записи от сегодняшнего дня - неделя");
                }
            }

            
        }

        
    }
}
