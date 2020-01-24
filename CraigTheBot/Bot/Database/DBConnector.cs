using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace CraigTheBot.Bot.Database
{
    internal class DBConnector
    {
        private SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=C:/Bot/Database/CraigDB.db");
        private static DBConnector _instance;
        private SQLiteDataAdapter adapter;

        private static readonly object LockObject = new object();


        private DBConnector()
        {
        }

        public static DBConnector Instance
        {
            get
            {
                lock (LockObject)
                {
                    return _instance ?? (_instance = new DBConnector());
                }
            }
        }

        public int GetRows(string SQLCommand)
        {
            int i = 0;
            try
            {
                m_dbConnection.Open();

                SQLiteCommand sqlite_cmd;
                sqlite_cmd = m_dbConnection.CreateCommand();
                sqlite_cmd.CommandText = SQLCommand;
                i = sqlite_cmd.ExecuteNonQuery();

                m_dbConnection.Close();
                return i;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                m_dbConnection.Close();
                return 0;
            }
        }

        public DataTable GetDBObjects(string SQLCommand)
        {
            DataTable table = new DataTable();

            SQLiteCommand sqlite_cmd;
            sqlite_cmd = m_dbConnection.CreateCommand();
            sqlite_cmd.CommandText = SQLCommand;

            adapter = new SQLiteDataAdapter(sqlite_cmd);
            adapter.Fill(table);

            table.TableName = "Table";

            return table;
        }

        public List<string> GetDBData(string SQLCommand)
        {
            List<string> output = new List<string>();

            try
            {
                m_dbConnection.Open();
                SQLiteDataReader sqlite_datareader;
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = m_dbConnection.CreateCommand();
                sqlite_cmd.CommandText = SQLCommand;

                sqlite_datareader = sqlite_cmd.ExecuteReader();

                Console.WriteLine($"Command: {SQLCommand}");

                while (sqlite_datareader.Read())
                {
                    int i = 0;
                    while (true)
                    {
                        try
                        {
                            string temp = "";

                            try
                            {
                                temp = sqlite_datareader.GetString(i).ToString();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                try
                                {
                                    temp = sqlite_datareader.GetInt32(i).ToString();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    break;
                                }
                            }

                            output.Add(temp);
                            i++;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
                m_dbConnection.Close();
            }
            catch (Exception e)
            {
                output = new List<string>();
                output.Add("I'm sorry, I can't read from the database");
                Console.WriteLine(e.Message);
                m_dbConnection.Close();
            }

            return output;
        }

        public bool ExecuteCommand(string SQLCommand)
        {
            try
            {
                m_dbConnection.Open();

                SQLiteCommand sqlite_cmd;
                sqlite_cmd = m_dbConnection.CreateCommand();
                sqlite_cmd.CommandText = SQLCommand;
                sqlite_cmd.ExecuteNonQuery();

                m_dbConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                m_dbConnection.Close();
                return false;
            }
        }
    }
}