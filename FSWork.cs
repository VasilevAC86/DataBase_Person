using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataBase_Person
{
    internal class FSWork // Класс для работы с файловой системой
    {  
        static public string Path(string location = "myDocs") // Метод определяет локацию сохранения
        {
            switch (location)
            {
                case "myDocs":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case "Desktop":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                case "Current":
                    return Environment.CurrentDirectory;
                default:
                    return string.Empty;                    
            }           
        }

        // Метод для чтения из БД команд 
        static public List<string> ReadSQLFile(string filename, string start = "CREATE TABLE")
        {
            List<string> result = new List<string>();
            using (StreamReader sr = new StreamReader(filename)) // Поток для чтения файла
            {
                string tmp = sr.ReadToEnd(); // Из файла всё считывается в tmp
                tmp.Split(';').ToList<string>(); // Получим массив строк, разбитый по ';', засунем его сразу в лист
            }
            for (int i = 0; i < result.Count; ++i) // Для каждого эл. в List добавляем ;
            {
                result[i] += ";";
            }
            return result;
        }
        static public bool IsFileExist(string path) // Проверка файла на существование
        {
            bool result = false;
            if (File.Exists(path))
            {
                result = true;
            }
            return result;
        }
        static public byte[] GetImage() // Возвращает массив байтов
        {
            byte[] result = null;
            string filename = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG files(*.JPG)|*.jpg|All files(*,*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK) // Если пользователь нажал OK 
                filename = ofd.FileName;
            else
                return result;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                result = new byte[fs.Length];
                fs.Read(result, 0, result.Length);
            }
            return result;
        }

    }
}

