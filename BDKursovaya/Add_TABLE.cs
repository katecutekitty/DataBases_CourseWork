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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cursovaya
{
    public partial class Add_TABLE : Form
    {
        Admin_window admin_Window;
        private OleDbConnection DbCon;
        public Add_TABLE(OleDbConnection dbCon, Admin_window admin_w)
        {
            admin_Window = admin_w;
            DbCon = dbCon;
            InitializeComponent();
            comboBox1.Enabled = false;
        }
        //имена таблиц
        public void Add_Tables_names()
        {
            comboBox1.Text = "";
            comboBox1.Items.Clear();
            DataTable tables = DbCon.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();
                if (!tableName.StartsWith("MSys"))
                    if (!tableName.StartsWith("f_6F231996"))
                        if (!tableName.StartsWith("fk_special_table"))
                            if (!tableName.StartsWith("~"))
                                comboBox1.Items.Add(tableName);
            }
        }

        //создать таблицу
        private void button1_Click(object sender, EventArgs e)
        {
            //получение списка всех таблиц из бд
            List<string> list_tables = new List<string>();
            DataTable tables = DbCon.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                string table_Name = row["TABLE_NAME"].ToString();
                if (!table_Name.StartsWith("MSys"))
                    if (!table_Name.StartsWith("f_6F231996"))
                        if (!table_Name.StartsWith("fk_special_table"))
                            if (!table_Name.StartsWith("~"))
                                list_tables.Add(table_Name);
            }

            if (textBox1.Text != null)
            {
                string tableName = textBox1.Text;
                if (list_tables.Contains(tableName) == false)
                {
                    create_table();
                    Add_Tables_names();
                }
                else
                {
                    MessageBox.Show("Таблица с таким именем уже существует.", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Название таблицы не может быть пустым.", "Ошибка");
            }
        }

        //добавить поле
        private void button2_Click(object sender, EventArgs e)
        {
            //получение списка всех таблиц из бд
            List<string> list_tables = new List<string>();
            DataTable tables = DbCon.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
            {
                string table_Name = row["TABLE_NAME"].ToString();
                if (!table_Name.StartsWith("MSys"))
                    if (!table_Name.StartsWith("f_6F231996"))
                        if (!table_Name.StartsWith("fk_special_table"))
                            if (!table_Name.StartsWith("~TM"))
                                list_tables.Add(table_Name);
            }

            if (textBox1.Text != null) //название новой таблицы не пустое
            {
                string tableName = textBox1.Text;
                if (list_tables.Contains(tableName) == true) //таблица существует
                {
                    if (textBox2.Text != null && ((radioButton1.Checked == true && radioButton2.Checked == false) || (radioButton1.Checked == false && radioButton2.Checked == true)))
                    {
                        if (listBox1.Items.Contains("key: true") == false)
                        {

                            add_fields_in_table_new();
                            textBox2.Clear();
                            radioButton1.Checked = false; radioButton2.Checked = false;
                            comboBox1.Items.Clear();
                            checkBox1.Checked = false;


                        }
                        else { MessageBox.Show("Невозможно добавить второй ключ в таблицу", "Ошибка"); checkBox1.Checked = false; }
                    }
                    else //Не заполнены оба поля или одно из двух
                    {
                        MessageBox.Show("Пожалуйста, убедитесь что вы заполнили название поля и выбрали его тип", "Ошибка");
                    }
                }
                else //таблицы не существует
                {
                    MessageBox.Show("Невозможно добавить поля, пока таблица не существует.", "Ошибка");
                }
            }
            else //textbox1 не заполнен
            {
                MessageBox.Show("Невозможно добавить поля, пока таблица не существует.", "Ошибка");
            }
        }


        //добавление поля в таблицу 
        private void add_fields_in_table_new()
        {
            string table_name = textBox1.Text;
            string field_name = textBox2.Text;
            string fiel_type;
            string child_table;
            if (radioButton1.Checked == true && checkBox1.Checked == false) //поле числовое не ключевое
            {
                fiel_type = radioButton1.Text;
                string sql = $"ALTER TABLE [{table_name}] ADD COLUMN [{field_name}] INT";
                OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                cmd.ExecuteNonQuery();

                //добавление в listbox
                add_info_in_listbox(field_name, fiel_type, "false", "false");

            }
            if (radioButton1.Checked == true && checkBox1.Checked == true) //поле числовое ключевое
            {
                if (comboBox1.SelectedIndex > -1 && comboBox1.Enabled == true)
                {
                    child_table = comboBox1.SelectedItem.ToString();
                    fiel_type = radioButton1.Text;
                    //создание поля в новой талице
                    string sql = $"ALTER TABLE [{table_name}] ADD COLUMN [{field_name}] INT PRIMARY KEY";
                    OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                    cmd.ExecuteNonQuery();

                    //создание поля в таблице связи
                    string sql_new = $"ALTER TABLE [{child_table}] ADD COLUMN [{field_name}] INT";
                    cmd = new OleDbCommand(sql_new, DbCon);
                    cmd.ExecuteNonQuery();

                    //запрос на создание связей между ними
                    string sql_link = $"ALTER TABLE [{child_table}] " +
                        $"ADD CONSTRAINT fk_{table_name}_{child_table} " +
                        $"FOREIGN KEY ([{field_name}])" +
                        $"REFERENCES [{table_name}]([{field_name}]) ";
                    cmd = new OleDbCommand(sql_link, DbCon);
                    cmd.ExecuteNonQuery();

                    //добавление в listbox
                    add_info_in_listbox(field_name, fiel_type, "true", child_table);
                    //добавление в бд
                    add_info_in_bd(table_name, child_table, $"fk_{table_name}_{child_table}", field_name);

                }
                else { MessageBox.Show("Выберите таблицу для связи с ключевым полем.", "Ошибка"); }
            }
            if (radioButton2.Checked == true && checkBox1.Checked == false) //поле текстовое не ключевое
            {
                fiel_type = radioButton1.Text;
                string sql = $"ALTER TABLE [{table_name}] ADD COLUMN [{field_name}] TEXT";
                OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                cmd.ExecuteNonQuery();

                //добавление в listbox
                add_info_in_listbox(field_name, fiel_type, "false", "false");

            }
            if (radioButton2.Checked == true && checkBox1.Checked == true) //поле текстовое ключевое
            {
                if (comboBox1.SelectedIndex > -1)
                {
                    child_table = comboBox1.SelectedItem.ToString();
                    fiel_type = radioButton1.Text;
                    //создание поля в новой талице
                    string sql = $"ALTER TABLE [{table_name}] ADD COLUMN [{field_name}] TEXT PRIMARY KEY";
                    OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                    cmd.ExecuteNonQuery();

                    //создание поля в таблице связи
                    string sql_new = $"ALTER TABLE [{child_table}] ADD COLUMN [{field_name}] TEXT";
                    cmd = new OleDbCommand(sql_new, DbCon);
                    cmd.ExecuteNonQuery();

                    //запрос на создание связей между ними
                    string sql_link = $"ALTER TABLE [{child_table}] " +
                        $"ADD CONSTRAINT fk_{table_name}_{child_table} " +
                        $"FOREIGN KEY ([{field_name}]) " +
                        $"REFERENCES [{table_name}]([{field_name}])";
                    cmd = new OleDbCommand(sql_link, DbCon);
                    cmd.ExecuteNonQuery();

                    //добавление в listbox
                    add_info_in_listbox(field_name, fiel_type, "true", child_table);
                    //добавление в бд
                    add_info_in_bd(table_name, child_table, $"fk_{table_name}_{child_table}", field_name);

                }
                else { MessageBox.Show("Выберите таблицу для связи с ключевым полем.", "Ошибка"); }
            }
        }
        //доавление в листбокс
        private void add_info_in_listbox(string fieldName, string fieldType, string fieldKey, string childTable)
        {
            listBox1.Items.Add("Field name: " + fieldName);
            listBox1.Items.Add("type: " + fieldType);
            listBox1.Items.Add("key: " + fieldKey);
            listBox1.Items.Add("link_table: " + childTable);
            listBox1.Items.Add("\n");
        }

        //доавление информации в таблицу бд
        private void add_info_in_bd(string parentTable, string childTable, string fk_name, string field_name)
        {
            string sql_quest = $"INSERT INTO fk_special_table (key_table, link_table, key_name, field_name) VALUES ('{parentTable}', '{childTable}', '{fk_name}', '{field_name}')";
            OleDbCommand cmd = new OleDbCommand(sql_quest, DbCon);
            cmd.ExecuteNonQuery();
        }

        //кнопка вернуться
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            admin_Window.Show();

        }
        //
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            listBox1.Items.Clear();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //INT
            if (radioButton1.Checked) { radioButton2.Checked = false; }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //TEXT
            if (radioButton2.Checked) { radioButton1.Checked = false; }

        }
        //наличие связи
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                comboBox1.Enabled = true;
                comboBox1.Items.Clear();
                Add_Tables_names();
            }
            if (checkBox1.Checked == false)
            {
                comboBox1.Items.Clear();
                comboBox1.Enabled = false;
            }
        }

        //создание таблицы
        private void create_table()
        {
            string tableName = textBox1.Text;
            string create_table_sql = $"CREATE TABLE {tableName}";
            OleDbCommand cmd = new OleDbCommand(create_table_sql, DbCon);
            cmd.ExecuteNonQuery();
        }


    }
}
