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
    public partial class Add_Lines : Form
    {
        Admin_window admin_Window;
        private OleDbConnection DbCon;
        int count = 0;
        public Add_Lines(OleDbConnection dbCon, Admin_window admin_w)
        {
            admin_Window = admin_w;
            DbCon = dbCon;
            InitializeComponent();
            Add_Tables_names();
        }
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

        public void Add_Fields_names()
        {
            string tableName = comboBox1.SelectedItem.ToString();
            string sql_quest = $"SELECT * FROM [{tableName}] where 1=0";
            OleDbCommand cmd = new OleDbCommand(sql_quest, DbCon);
            OleDbDataReader reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
            var schemTable = reader.GetSchemaTable()
;
            //DataTable columns = DbCon.GetSchema("Columns", new string[] { null, null, tableName, null });
            foreach (DataRow row in schemTable.Rows)
            {
                string columnName = row["ColumnName"].ToString();
                listBox1.Items.Add(columnName);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                if (comboBox1.SelectedIndex > -1)
                {
                    listBox1.Items.Clear();
                    Add_Fields_names();
                }
            }
            else { MessageBox.Show("Вы не можете изменить таблицу, пока список добавляемых значений заполнен в соответствии текущей таблицы", "Ошибка"); }

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

        private void button2_Click(object sender, EventArgs e)
        {
            string tableName = comboBox1.SelectedItem.ToString();
            Dictionary<string, string> columnName_Type = Type_of_Colmns(tableName);
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string columnName = listBox1.Items[i].ToString();
                string value = listBox2.Items[i].ToString();
                if (i == 0)
                {
                    //Проверка типа поля добавляемого значения
                    string check_value = columnName_Type[columnName];
                    if (check_value == "string") //если текст
                    {
                        string sql_add = $"INSERT INTO [{tableName}] ([{columnName}]) VALUES ('{value}')";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();

                    }
                    else //если число
                    {
                        string sql_add = $"INSERT INTO [{tableName}] ({columnName}) VALUES ({value})";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string zero_field = listBox1.Items[0].ToString();
                    string zero_value = listBox2.Items[0].ToString();
                    string check_value = columnName_Type[columnName];
                    string check_zero_value = columnName_Type[zero_field];
                    if (check_value == "string" && check_zero_value == "string") //если оба текст
                    {
                        string sql_add = $"UPDATE [{tableName}] SET [{columnName}] = '{value}' WHERE [{zero_field}] = '{zero_value}'";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();
                    }
                    if (check_value == "string" && check_zero_value == "int") //если текст и число
                    {
                        string sql_add = $"UPDATE [{tableName}] SET [{columnName}] = '{value}' WHERE [{zero_field}] = {zero_value}";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();
                    }
                    if (check_value == "int" && check_zero_value == "int") //если число и число
                    {
                        string sql_add = $"UPDATE [{tableName}] SET [{columnName}] = {value} WHERE [{zero_field}] = {zero_value}";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();
                    }
                    if (check_value == "int" && check_zero_value == "string") //если число и текст
                    {
                        string sql_add = $"UPDATE [{tableName}] SET [{columnName}] = {value} WHERE [{zero_field}] = '{zero_value}'";
                        OleDbCommand cmd = new OleDbCommand(sql_add, DbCon);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            MessageBox.Show("Запись добавлена в таблицу", "Уведомление");
            listBox2.Items.Clear();
            count = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            count = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (count <= listBox1.Items.Count - 1)
            {
                string tableName = comboBox1.SelectedItem.ToString();
                string columnName = listBox1.Items[count].ToString();
                string adding_value = textBox1.Text;
                //Список полей и их типов
                Dictionary<string, string> columnName_Type = Type_of_Colmns(tableName);

                //Проверка на содержание данного поля в словаре
                if (columnName_Type.ContainsKey(columnName) == true)
                {
                    //Проверка типа поля добавляемого значения
                    string value = columnName_Type[columnName];

                    if (value == "string") //если текст
                    {
                        listBox2.Items.Add(adding_value);
                        count++;
                    }
                    else //если число
                    {
                        //Проверка нового значения на преобразование в числовое
                        if (int.TryParse(adding_value, out int convert))
                        {
                            listBox2.Items.Add(convert);
                            count++;
                        }
                        else
                        {
                            MessageBox.Show("Ваше значение не соответствует числовому типу", "Ошибка");
                            textBox1.Clear();
                        }
                    }
                    textBox1.Clear();
                }
            }
            else { MessageBox.Show("Количество добавляемых значений не может привышать количество полей таблицы", "Ошибка"); }
            textBox1.Clear();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            admin_Window.Show();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                if (comboBox1.SelectedIndex > -1)
                {
                    listBox1.Items.Clear();
                    Add_Fields_names();
                }
            }
            else { MessageBox.Show("Вы не можете изменить таблицу, пока список добавляемых значений заполнен в соответствии текущей таблицы", "Ошибка"); }

        }
    }
}
