using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Drawing.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cursovaya
{
    public partial class Delete_Form5 : Form
    {
        Admin_window admin_Window;
        private OleDbConnection DbCon;
        public Delete_Form5(OleDbConnection dbCon, Admin_window admin_w)
        {
            DbCon = dbCon;
            admin_Window = admin_w;
            InitializeComponent();
            Add_Tables_names();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) { Add_Field_values(); }
            else { listView1.Items.Clear(); }
        }
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
                            if (!tableName.StartsWith("~TM"))
                                comboBox1.Items.Add(tableName);
            }
        }
        private void Add_Field_values()
        {
            listView1.Clear();
            if (comboBox2.SelectedItem != null)
            {
                listView1.Columns.Add("");
                string table_name = comboBox1.SelectedItem.ToString();
                string field_name = comboBox2.SelectedItem.ToString();
                string values = $"SELECT {field_name} FROM {table_name}";
                OleDbCommand cmd = new OleDbCommand(values, DbCon);
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader[0].ToString());
                    listView1.Items.Add(item);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null) //выбрана таблица
            {
                string tableName = comboBox1.SelectedItem.ToString();
                string result = $"Таблица: {tableName}";

                if (comboBox2.SelectedItem != null) //выбрано поле
                {
                    string columnName = comboBox2.SelectedItem.ToString();
                    result = $"Таблица: {tableName}\n Поле: {columnName}";

                    if (listView1.SelectedItems.Count != 0) //выбрано значение(Я)
                    {
                        string items = "";
                        foreach (ListViewItem item in listView1.SelectedItems)
                        {
                            items += item.Text.ToString() + ", ";
                        }
                        //string itemName = listView1.SelectedItems.ToString();
                        result = $"Таблица: {tableName}\n Поле: {columnName}\n Значение: {items}";
                    }
                }
                //Подтверждение удаления
                DialogResult dial_result = MessageBox.Show($"Подтвердите удаление\n\n{result}", "Delete window", MessageBoxButtons.YesNo);
                if (dial_result == DialogResult.Yes)
                {//Действие при Yes

                    //TABLE
                    if (comboBox1.SelectedItem != null && comboBox2.SelectedItem == null && listView1.SelectedItems.Count == 0)
                    {
                        if (RowExists(tableName))
                        {
                            DialogResult dialog = MessageBox.Show("Данная таблица содержит внешние ключи.\nВсе равно удалить?", "Внимание.", MessageBoxButtons.OKCancel);
                            if (dialog == DialogResult.OK)
                            {
                                delete_keys(tableName);
                                string del_table_sql = $"DROP TABLE [{tableName}]";
                                comboBox1.Items.Clear();
                                OleDbCommand cmd = new OleDbCommand(del_table_sql, DbCon);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string del_table_sql = $"DROP TABLE [{tableName}]";
                            comboBox1.Items.Clear();
                            OleDbCommand cmd = new OleDbCommand(del_table_sql, DbCon);
                            cmd.ExecuteNonQuery();
                        }
                        Add_Tables_names();
                    }
                    //TABLE and FIELD
                    if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null && listView1.SelectedItems.Count == 0)
                    {
                        string column_name = comboBox2.SelectedItem.ToString();
                        DataTable primaryKey = DbCon.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] { null, null, tableName });
                        DataRow prim_key_Row = primaryKey.Rows.Cast<DataRow>().FirstOrDefault(row => row["COLUMN_NAME"].ToString().Equals(column_name, StringComparison.OrdinalIgnoreCase));
                        if (prim_key_Row != null)
                        {
                            string keyColumnName = prim_key_Row["COLUMN_NAME"].ToString();
                            string constraintName = prim_key_Row["PK_NAME"].ToString();
                            string dropConstraintQuery = $"ALTER TABLE {tableName} DROP CONSTRAINT {constraintName}";
                            OleDbCommand cmd = new OleDbCommand(dropConstraintQuery, DbCon);
                            cmd.ExecuteNonQuery();
                        }
                        string del_field_sql = $"ALTER TABLE {tableName} DROP COLUMN {column_name}";
                        OleDbCommand cmd_del_field = new OleDbCommand(del_field_sql, DbCon);
                        cmd_del_field.ExecuteNonQuery();
                        Add_Fields_names();

                    }
                    //TABLE and FIELD and VALUES
                    if (comboBox1.SelectedItem != null & comboBox2.SelectedItem != null & listView1.SelectedItems.Count > 0)
                    {
                        string column_name = comboBox2.SelectedItem.ToString();
                        foreach (ListViewItem item in listView1.SelectedItems)
                        {
                            if (int.TryParse(item.Text.ToString(), out int intvalue))
                            {
                                string sql = $"DELETE FROM {tableName} WHERE {column_name} = {intvalue}";
                                OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                string sql = $"DELETE FROM {tableName} WHERE {column_name} = '{item.Text}'";
                                OleDbCommand cmd = new OleDbCommand(sql, DbCon);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        Add_Field_values();
                    }
                } // конец if (dial_result == DialogResult.Yes)
            }
        }

        private void delete_keys(string tableName)
        {
            DataTable dt = GetRowData(tableName); //получение всех строчек, содержащих удаляемую таблицу
            foreach (DataRow drrows in dt.Rows)
            {
                string key_table = drrows[0].ToString();
                string link_table = drrows[1].ToString();
                string fk_name = drrows[2].ToString();
                string field_name = drrows[3].ToString();
                string sql_quest = $"ALTER TABLE [{link_table}] DROP CONSTRAINT {fk_name}";
                OleDbCommand cmd = new OleDbCommand(sql_quest, DbCon);
                cmd.ExecuteNonQuery();

                string sql_drop_field = $"ALTER TABLE [{link_table}] DROP COLUMN [{field_name}]";
                cmd = new OleDbCommand(sql_drop_field, DbCon);
                cmd.ExecuteNonQuery();

                DeleteRow(tableName);
            }
        }

        //Проверка наличия ключа у таблицы
        private bool RowExists(string tableName)
        {
            string sql_query = $"SELECT COUNT(*) FROM fk_special_table WHERE link_table = '{tableName}' OR key_table = '{tableName}'";
            OleDbCommand cmd = new OleDbCommand(sql_query, DbCon);
            object result = cmd.ExecuteScalar();
            int rowCount = result == null ? 0 : Convert.ToInt32(result);
            return rowCount > 0;
        }

        //получение строк с удаляемой таблицей
        private DataTable GetRowData(string tableName)
        {
            DataTable result_table = new DataTable();
            string sql_get = $"SELECT * FROM fk_special_table WHERE link_table = '{tableName}' OR key_table = '{tableName}'";
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql_get, DbCon);
            adapter.Fill(result_table);
            return result_table;
        }
        //удаление данных о ключах удал. таблицы
        private void DeleteRow(string tableName)
        {
            string sql_del = $"DELETE FROM fk_special_table WHERE link_table = '{tableName}' OR key_table = '{tableName}'";
            OleDbCommand cmd = new OleDbCommand(sql_del, DbCon);
            cmd.ExecuteNonQuery();
        }
        public void Add_Fields_names()
        {
            comboBox2.Text = "";
            comboBox2.Items.Clear();
            if (comboBox1.SelectedItem != null)
            {
                string tableName = comboBox1.SelectedItem.ToString();
                DataTable columns = DbCon.GetSchema("Columns", new string[] { null, null, tableName, null });
                foreach (DataRow row in columns.Rows)
                {
                    string columnName = row["COLUMN_NAME"].ToString();
                    comboBox2.Items.Add(columnName);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                comboBox2.Items.Clear();
                Add_Fields_names();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex > -1)
            {
                //очистка listview
                listView1.Items.Clear();
                if (checkBox1.Checked) { Add_Field_values(); }
            }
        }


    }
}
