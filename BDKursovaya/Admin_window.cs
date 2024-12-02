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

namespace cursovaya
{
    public partial class Admin_window : Form
    {
        public static string s = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=BeautySalon2.mdb;";
        private OleDbConnection DbCon;
        private Delete_Form5 Delete_Form5;
        private Add_TABLE Add_TABLE_Form;
        private Add_Lines Add_LINES_Form;
        public Admin_window()
        {
            InitializeComponent();
            DbCon = new OleDbConnection(s);
            DbCon.Open();
            entering_text();
            Add_Tables_names();
            add_setting();
        }
        //Выбор режима
        private void add_setting()
        {
            comboBox4.Items.Add("Таблица");
            comboBox4.Items.Add("Запись");
        }
        //Закрытие БД
        private void Form4_FormClosing(object sender, FormClosedEventArgs e)
        {
            DbCon.Close();
        }
        //начальный текст таблицы
        private void entering_text()
        {
            listView1.Columns.Add("Выберите таблицу для отображения информации").Width = 300;
        }
        //имена таблиц
        public void Add_Tables_names()
        {
            comboBox1.Items.Clear();
            DataTable tables = DbCon.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();
                if (!tableName.StartsWith("MSys"))
                    if (!tableName.StartsWith("f_6F231996"))
                        if (!tableName.StartsWith("fk_special_table"))
                            if (!tableName.StartsWith("~TM"))
                                comboBox1.Items.Add(tableName);
            }
        }

        
        //название полей
        public void Add_Fields_names()
        {
            comboBox2.Items.Clear();
            string tableName = comboBox1.SelectedItem.ToString();
            DataTable columns = DbCon.GetSchema("Columns", new string[] { null, null, tableName, null });
            foreach (DataRow row in columns.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();
                comboBox2.Items.Add(columnName);
            }
        }

        /// <summary>
        /// Показать информацию о выбранной таблице
        /// </summary>
        public void Show_Table_Info()
        {
            string tableName = comboBox1.SelectedItem.ToString();
            string aqlQuery = $"SELECT * FROM [{tableName}]";
            OleDbCommand cmd = new OleDbCommand(aqlQuery, DbCon);
            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            // Очищаем ListView перед загрузкой новых данных
            listView1.Items.Clear();
            listView1.Columns.Clear();

            // Заполняем ListView колонками из DataTable
            foreach (DataColumn column in dataTable.Columns)
            {
                listView1.Columns.Add(column.ColumnName, 120);
            }

            // Заполняем ListView данными из DataTable
            foreach (DataRow row in dataTable.Rows)
            {
                ListViewItem item = new ListViewItem(row[0].ToString());

                for (int i = 1; i < dataTable.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                listView1.Items.Add(item);
            }
        }
        
        

        
        /// <summary>
        /// Поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > -1)
            {
                comboBox3.Items.Clear();
                //Add_Items_names();
                Get_all_from_line();
            }
        }
        /// <summary>
        /// Метод проверки поля на его тип
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> Type_of_Colmns(string tableName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            DataTable columns = DbCon.GetSchema("Columns", new string[] { null, null, tableName, null });
            foreach (DataRow row in columns.Rows)
            {
                string field_Name = row["COLUMN_NAME"].ToString();
                int columnType = (int)row["DATA_TYPE"];
                if (columnType == 3)
                {
                    result[field_Name] = "int";
                }
                if (columnType == 130 || columnType == 10)
                {
                    result[field_Name] = "string";
                }
            }
            return result;
        }
        /// <summary>
        /// Таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                comboBox2.Items.Clear();
                Show_Table_Info();
                //Add_Fields_names();
                get_id_field();
            }
        }

        /// <summary>
        /// Редакция значения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            
        }



        private void get_id_field()
        {
            comboBox2.Items.Clear();
            string tableName = comboBox1.SelectedItem.ToString();
            string column_id_Name = listView1.Columns[0].Text;
            OleDbCommand cmd = new OleDbCommand($"SELECT [{column_id_Name}] FROM [{tableName}];", DbCon);
            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comboBox2.Items.Add(reader[column_id_Name].ToString());
            }
        }
        private void Get_all_from_line()
        {
            comboBox3.Items.Clear();
            comboBox3.SelectedIndex = -1;
            string tableName = comboBox1.SelectedItem.ToString();
            string id_of_item = comboBox2.SelectedItem.ToString();
            string column_id_Name = listView1.Columns[0].Text;
            if (int.TryParse(id_of_item, out int id))
            {
                string sql_quest = $"SELECT * FROM [{tableName}] WHERE [{column_id_Name}] = {id}";
                OleDbCommand cmd = new OleDbCommand(sql_quest, DbCon);
                OleDbDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                        comboBox3.Items.Add(r[i].ToString());
                }
            }
            else
            {
                string sql_quest = $"SELECT * FROM [{tableName}] WHERE [{column_id_Name}] = '{id_of_item}'";
                OleDbCommand cmd = new OleDbCommand(sql_quest, DbCon);
                OleDbDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                        comboBox3.Items.Add(r[i].ToString());
                }
            }
        }


        //Добавление
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != -1)
            {
                if (comboBox4.SelectedIndex == 0)
                {
                    if (Add_TABLE_Form== null || Add_TABLE_Form.IsDisposed)
                    {
                        Add_TABLE_Form = new Add_TABLE(DbCon, this);
                    }
                    Add_TABLE_Form.Show();
                    this.Hide();
                }
                if (comboBox4.SelectedIndex == 1)
                {
                    if (Add_LINES_Form == null || Add_LINES_Form.IsDisposed)
                    {
                        Add_LINES_Form = new Add_Lines(DbCon, this);
                    }
                    Add_LINES_Form.Show();
                    this.Hide();
                }
            }
            else { MessageBox.Show("Выберите объект, который желаете добавить, из списка ниже", "Ошибка"); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Delete_Form5 == null || Delete_Form5.IsDisposed)
            {
                Delete_Form5 = new Delete_Form5(DbCon, this);
            }
            Delete_Form5.Show();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //MessageBox.Show($"{comboBox3.SelectedIndex}");
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null && comboBox3.SelectedItem != null && textBox1.Text != null)
            {
                string tableName = comboBox1.SelectedItem.ToString();
                string id_of_item = comboBox2.SelectedItem.ToString();
                string id_field = listView1.Columns[0].Text;
                //string old_value = comboBox3.SelectedItem.ToString();
                string new_value = textBox1.Text;
                string columnName = listView1.Columns[comboBox3.SelectedIndex].Text;

                //Список полей и их типов
                Dictionary<string, string> columnName_Type = Type_of_Colmns(tableName);

                //Проверка на содержание данного поля в словаре
                if (columnName_Type.ContainsKey(columnName) == true)
                {
                    //Проверка типа поля заменяемого значения
                    string value = columnName_Type[columnName];
                    //если текст
                    if (value == "string")
                    {
                        //Проверка id записи на тип данных
                        if (int.TryParse(id_of_item, out int convert_id))
                        {
                            string updateSQL = $"UPDATE [{tableName}] SET [{columnName}] = '{textBox1.Text}' WHERE [{id_field}] = {convert_id}";
                            OleDbCommand cmd = new OleDbCommand(updateSQL, DbCon);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            string updateSQL = $"UPDATE [{tableName}] SET [{columnName}] = '{textBox1.Text}' WHERE [{id_field}] = '{id_of_item}'";
                            OleDbCommand cmd = new OleDbCommand(updateSQL, DbCon);
                            cmd.ExecuteNonQuery();
                        }
                    } //если число
                    else
                    {
                        //Проверка id записи на преобразование в числовое
                        //если числовое
                        if (int.TryParse(id_of_item, out int convert_id))
                        {
                            //Проверка нового значения на преобразование в числовое
                            if (int.TryParse(new_value, out int convert))
                            {
                                string updateSQL = $"UPDATE [{tableName}] SET [{columnName}] = {convert} WHERE [{id_field}] = {convert_id}";
                                OleDbCommand cmd = new OleDbCommand(updateSQL, DbCon);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                MessageBox.Show("Ваше значение не соответствует числовому типу", "Ошибка");
                                textBox1.Clear();
                            }
                        } //если текстовое
                        else
                        {
                            //Проверка нового значения на преобразование в числовое
                            if (int.TryParse(new_value, out int convert))
                            {
                                string updateSQL = $"UPDATE [{tableName}] SET [{columnName}] = {convert} WHERE [{id_field}] = '{convert_id}'";
                                OleDbCommand cmd = new OleDbCommand(updateSQL, DbCon);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                MessageBox.Show("Ваше значение не соответствует числовому типу", "Ошибка");
                                textBox1.Clear();
                            }
                        }

                    }
                    Show_Table_Info();
                    Get_all_from_line();
                    textBox1.Clear();
                }
            }
            else
            {
                MessageBox.Show("Убедитесь, что все значения указаны", "Ошибка");
            }
        }


        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > -1)
            {
                comboBox3.Items.Clear();
                //Add_Items_names();
                Get_all_from_line();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
