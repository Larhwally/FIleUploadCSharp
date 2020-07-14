using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace UploadImage.Data
{
    public class EntityConnection
    {
        public string ConnectionString = "server=localhost;port=3306;database=fileuploaddb;user=root;password=ellnerd22";
        private MySqlConnection connection;
        private string tableName;
        private int defaultSelectLength;
        private Dictionary<string, string> tableSchema;

        public EntityConnection(string tableName)
        {
            this.defaultSelectLength = 100;
            this.tableName = tableName;
            this.loadConnection();
            this.tableSchema = getTableSchema();
        }

        private Dictionary<string, string> getTableSchema()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string query = "select column_name, data_type from information_schema.columns where table_name='" + this.tableName + "'";
            this.connection.Open();
            MySqlCommand command = new MySqlCommand(query, this.connection);

            List<Dictionary<string, object>> tempResult = BaseSelect(query);
            for (int i = 0; i < tempResult.Count; i++)
            {
                Dictionary<string, object> current = tempResult[i];
                result.Add(current["column_name"].ToString(), current["data_type"].ToString());
            }
            return result;
        }

        private List<Dictionary<string, object>> BaseSelect(string query)
        {
            if (this.connection.State == System.Data.ConnectionState.Closed)
            {
                this.connection.Open();
            }
            MySqlCommand command = new MySqlCommand(query, this.connection);
            MySqlDataReader reader = command.ExecuteReader();
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                Dictionary<string, object> tempResult = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    tempResult.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(tempResult);
            }
            reader.Close();
            this.connection.Close();
            return result;
        }

        internal MySqlConnection loadConnection()
        {
            if (this.connection == null)
            {
                this.connection = new MySqlConnection(this.ConnectionString);
            }
            else
            {
                this.connection.Close();
            }
            return this.connection;
        }

        public EntityConnection(string tableName, int defaultlength) : this(tableName)
        {
            this.defaultSelectLength = defaultlength;
        }

        //Insert image details generated from image file upload to the database
        public bool Insert(Dictionary<string, string> content)
        {
            this.connection.Open();
            string[] keys = content.Keys.ToArray<string>();
            string placeholder = GetPlaceholder(keys);
            string query = " insert into " + this.tableName + "(" + implode(keys) + ") values (" + placeholder + ")";

            MySqlCommand command = new MySqlCommand(query, this.connection);
            for (int i = 0; i < keys.Length; i++)
            {
                string currentParam = "@" + keys[i];
                string currentvalue = content[keys[i]].ToString();
                MySqlDbType dbType = getColumnType(this.tableSchema[keys[i]]);
                MySqlParameter tempParam = new MySqlParameter(currentParam, dbType);
                tempParam.Value = wrapvalue(currentvalue, dbType);
                command.Parameters.Add(tempParam);
            }
            int n = command.ExecuteNonQuery();
            this.connection.Close();
            return n > 0;
        }


        //Check image unique name on the database
        public bool CheckImage(string uniquePath)
        {
            this.connection.Open();
            bool hasRows = false;
            string query = "select * from " + this.tableName + " where imgUniquePath =  @imgUniquePath  ";
            MySqlCommand cmd = new MySqlCommand(query, this.connection);
            cmd.Parameters.AddWithValue("imgUniquePath", uniquePath);
            MySqlDataReader reader = cmd.ExecuteReader();
            hasRows = reader.HasRows;
            this.connection.Close();
            return hasRows;
            

        }

        private object wrapvalue(string currentValue, MySqlDbType dbType)
        {
            if (dbType == MySqlDbType.DateTime)
            {
                DateTime datetime = DateTime.Parse(currentValue);
                return datetime;
            }
            if (dbType == MySqlDbType.Time)
            {
                TimeSpan datetime = TimeSpan.Parse(currentValue);
                return datetime;
            }

            return currentValue;
        }

        private MySqlDbType getColumnType(string v)
        {
            v = v.ToLower();
            switch (v)
            {
                case "int":
                    return MySqlDbType.Int32;
                case "varchar":
                    return MySqlDbType.VarChar;
                case "datetime":
                    return MySqlDbType.DateTime;
                case "time":
                    return MySqlDbType.Time;
                case "decimal":
                    return MySqlDbType.Decimal;
                case "text":
                    return MySqlDbType.Text;
                case "nvarchar":
                    return MySqlDbType.LongText;
                case "Guid":
                    return MySqlDbType.Guid;
                default:
                    return MySqlDbType.JSON;
            }
        }

        private string implode(string[] keys)
        {
            string result = "";
            for (int i = 0; i < keys.Length; i++)
            {
                string currentValue = keys[i];
                result += string.IsNullOrEmpty(result) ? currentValue : "," + currentValue; //another way of writing if-then-else
            }
            return result;
        }

        private static string GetPlaceholder(string[] keys)
        {
            string result = "";
            for (int i = 0; i < keys.Length; i++)
            {
                string currentValue = "@" + keys[i];
                result += string.IsNullOrEmpty(result) ? currentValue : "," + currentValue;
            }
            return result;
        }

        public static string ToJson(string result)
        {
            var JsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return JsonResult;
        }


    }
}

