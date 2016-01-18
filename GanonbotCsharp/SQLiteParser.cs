using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace GanonbotCsharp
{
    class SQLiteParser
    {
        string configLocation;
        public SQLiteParser()
        {
            configLocation = Directory.GetCurrentDirectory()+"\\";
        }
        public List<List<string>> readSQLite(string table, string value, string value2 = "")
        {
            if(!Directory.Exists(configLocation))
            {
                Directory.CreateDirectory(configLocation);
            }

            string strPath, strDb;
            strPath = string.Empty;
            List<List<string>> configOptions = new List<List<string>>();
            if (File.Exists(configLocation + "\\GanonbotConfiguration.sqlite"))
            {
                strPath = configLocation + "\\GanonbotConfiguration.sqlite";
            }
            if (string.Empty == strPath)
            {
                return new List<List<string>>() { new List<string>(){""}};
            }
            try
            {
                strDb = "Data Source=" + strPath;
                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        if (value2 != "")
                        {
                            cmd.CommandText = "SELECT " + value + "," + value2 + " FROM " + table;
                        }
                        else
                        {
                            cmd.CommandText = "SELECT " + value + " FROM " + table;
                        } 
                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tempValue = reader.GetString(0);
                                if (value2 != "")
                                {
                                    string tempValue2;
                                    if (table == "timeoutWords")
                                    {
                                        tempValue2 = reader.GetInt32(1).ToString();
                                    }
                                    else
                                    {
                                        tempValue2 = reader.GetString(1);
                                    }
                                    
                                    if (!tempValue.Equals(string.Empty))
                                    {
                                        configOptions.Add(new List<string> { tempValue, tempValue2 });
                                    }
                                }
                                else
                                {
                                    configOptions.Add(new List<string> { tempValue });
                                }
                            }
                            reader.Dispose();
                            reader.Close();
                        }
                        conn.Dispose();
                        SQLiteConnection.ClearAllPools();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
            catch (Exception)
            {

            }
            return configOptions;
        }

        public bool writeSQLite(List<string> content, string table)
        {
            if (!File.Exists(configLocation + "\\GanonbotConfiguration.sqlite"))
            {
                createDatabaseFile();
            }
            string fileLocation = configLocation + "\\GanonbotConfiguration.sqlite";
            
            deleteTableContent(table);
            foreach (string word in content)
            {
                string[] temp = word.Split('|');
                if (table == "commands" && temp.Length>1)
                {
                    insertIntoTable(table, temp[0].Trim(), temp[1].Trim());
                }
                else if(table == "timeoutWords" && temp.Length>1)
                {
                    insertIntoTable(table, temp[0].Trim(), temp[1].Trim());
                }
                else if (table == "timeoutWords" && temp.Length == 1)
                {
                    insertIntoTable(table, temp[0].Trim(), "600");
                }
                else
                {
                    insertIntoTable(table, temp[0]);
                }

            }
            return true;
        }

        public bool addToSQLite(string table, string value1, string value2 = "")
        {
            if (!File.Exists(configLocation + "\\GanonbotConfiguration.sqlite"))
            {
                createDatabaseFile();
            }
            string fileLocation = configLocation + "\\GanonbotConfiguration.sqlite";
            if (table == "commands")
            {
                insertIntoTable(table, value1, value2);
                return true;
            }
            else if (table == "timeoutWords" && value2 == "")
            {
                insertIntoTable(table, value1, value2);
                return true;
            }
            else if (table == "timeoutWords")
            {
                insertIntoTable(table, value1, "600");
                return true;
            }
            else if (table == "bannedWords")
            {
                insertIntoTable(table, value1);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void createDatabaseFile()
        {
            SQLiteConnection.CreateFile("GanonbotConfiguration.sqlite");
            executeQuery("create table bannedWords (name varchar(500))");
            executeQuery("create table timeoutWords (name varchar(500), duration int)");
            executeQuery("create table commands (command varchar(30), response varchar(500))");

        }

        public bool removeValue(string table, string value)
        {
            if (table == "commands")
            {
                removeFromTable(table,"command",value);
                return true;
            }
            else if (table == "timeoutWords")
            {
                removeFromTable(table,"name",value);
                return true;
            }
            else if (table == "bannedWords")
            {
                removeFromTable(table,"name",value);
                return true;
            }
            else
            {
                return false;
            }
        }



        private void insertIntoTable(string table, string value1, string value2 = null)
        {
            if (value2 == null)
            {
                executeQuery("insert into " + table + "(name) values ('" + value1 + "')");
            }
            else if (table == "commands")
            {
                executeQuery("insert into " + table + "(command, response) values ('" + value1 + "','" + value2 + "')");
            }
            else
            {
                executeQuery("insert into " + table + "(name, duration) values ('" + value1 + "','" + value2 + "')");
            }
        }

        private void removeFromTable(string table, string attribute, string value)
        {
            executeQuery("DELETE FROM " + table + " WHERE " + attribute + " = '" + value+"'");
        }

        private void deleteTableContent(string table)
        {
            executeQuery("DELETE FROM " + table);
        }

        private void executeQuery(string query)
        {
            var m_dbConnection = new SQLiteConnection("Data Source=GanonbotConfiguration.sqlite;Version=3;");
            m_dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
            m_dbConnection.Dispose();
            
        }
    }
}
