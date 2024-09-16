using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // Можем считать всю структуру запросов из файла с sql-запросами
using System.Threading.Tasks;

namespace DataBase_Person
{
    internal class DBWork
    {
        static private string dbname = "Fandom.db";
        static private string path = $"Data Source = {dbname};"; // вместо dbname можно вставлять относительный путь
        static private List<string> queryes = new List<string>(); // в коллекции будут хранится 3 запроса по созданию таблиц
        static private List<SQLiteCommand> commands = new List<SQLiteCommand>(); // в коллекции будут хряниться команды
        // Метод будет заполнять коллекцию queryes запросами по созданию таблиц
        static public void FillQueryes(string filename = @"/CreateDB") 
        {
            // Создаём новое регулярное выражение (шаблоны поиска для вылавливания чего-нибудь из текста)
            //Regex regex = new Regex(filename, RegexOptions.Compiled); 
            queryes = FSWork.ReadSQLFile(filename); // Запросы будут забираться из файла с sql-запросами
        }
        
        static public bool MakeQuery()
        {
            bool result = false;
            // Создаём соединение по указанному пути
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                conn.Open(); // чтобы в цикле ExecuteNonQuery
                for (int i = 0; i < queryes.Count; ++i)
                {
                    commands.Add(conn.CreateCommand()); // Для каждого запроса создаём отдельную команду
                    commands[i].CommandText = queryes[i];
                    commands[i].ExecuteNonQuery();
                }
            }                        
            result = true;
            return result;
        }
        // Метод для получения из базы набора имён мастеров
        static public List<string> GetMechanics()
        {
            List<string> result = new List<string>();
            string get_mechanics = "SELECT name  FROM Mechanic";
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = get_mechanics;
                conn.Open();
                var reader = cmd.ExecuteReader();
                if (reader.HasRows) // Если reader имеет поля
                {
                    while (reader.Read()) // Читает значения и переходит на следующие                    
                        result.Add(reader.GetString(0)); // Добавляет по строчке                    
                }
            }
            return result;
        }
        // Метод для добавления аватарки мастеру
        static public void AddAvatar(string _name, byte[] _image)
        {
            // Создаём соединение с SQL
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                SQLiteCommand command = new SQLiteCommand(conn);
                command.CommandText = @"UPDATE Mechanic SET Avatar=@Avatar " +
                    $"WHERE Name LIKE '{_name}%';";
                command.Parameters.Add(new SQLiteParameter("@Avatar", _image));
                conn.Open();
                command.ExecuteNonQuery();
            }
        }
        static public MemoryStream GetAvatar(string _name) // По имени возвращает аватар из базы
        {
            MemoryStream result = null; // ссылка для хранения результата
            byte[] _image = null; // ссылка для хранения массива байтов
            // Создаём соединение с SQL
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                // Команда на основе созданного соединения
                SQLiteCommand cmd = new SQLiteCommand(conn);
                string get_image = $"SELECT Avatar FROM Mechanic WHERE Name LIKE '{_name}%'";
                cmd.CommandText = get_image;
                conn.Open();
                // Обратились к базе и получили System.Object с запакованными бинарными данными
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0)) // Если не null, то
                            _image = (byte[])reader.GetValue(0);
                    }
                }
            }
            if (_image != null)
                result = new MemoryStream(_image);
            return result;
        }
        static public void AddData(string _newCategoryInsert, string _dbname = "Servis.db")
        {
            string path = $"Data Source={_dbname};";
            // Выделяем ресурс, который должен финализироваться по завершении
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                SQLiteCommand cmd = new SQLiteCommand(conn); // cmd - это ссылка
                //conn.ConnectionString = path;
                cmd.CommandText = _newCategoryInsert;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        static public DataSet Refresh(string _dbname = "Servis.db")
        {
            DataSet result = new DataSet();
            string path = $"Data Source={_dbname};";
            string show_all_data = "SELECT * FROM Category;"; // SQL-запрос для вывода всех данных
            // Описываем время существования соединения (создаём соединение и финализируем его)
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                conn.Open();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(show_all_data, conn); // Создаём адаптер и наполняем его данными
                adapter.Fill(result);
            }
            return result;

        }
        static public void Save(DataTable dt, out string _query, string _dbname = "Servis.db")
        {
            // Описание запросов (как раздел var в Pascale)
            string path = $"Data Source={_dbname};";
            string show_all_data = "SELECT * FROM Category;";
            using (SQLiteConnection conn = new SQLiteConnection(path))
            {
                conn.Open();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(show_all_data, conn);
                SQLiteCommandBuilder commandBulder = new SQLiteCommandBuilder(adapter); // Берёт инфу о структуре таблицы
                adapter.Update(dt); // Обновляем данные
                _query = commandBulder.GetUpdateCommand().CommandText; // Обновлённый текст
            }
        }
    }
}
