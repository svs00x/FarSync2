using System;
using System.IO;

namespace FarSync2
{
    public enum Operation : byte
    {
        Empty,
        Nothing,
        Delete,
        New,
        Copy,
        Move
    }

    [Serializable]
    class ElementFilesTree
    {
        public bool IsSource;   // Тип элемента: true - источник; false - приемник
        public bool IsFile;     // Файл - Да, Директория - Нет
        public string Path;     // путь к элементу, без пути к узлу и без имени файла
        public string NameExt;  // имя элемента + расширение
        public DateTime LastWriteTime;   // Время изменения элемента
        public long Length;       // Размер файла
        public string PathOut;  // Путь куда положить
        public Operation Act;   // Действие (перечисление)

        public ElementFilesTree(bool isSourceSet, DirectoryInfo dirInf, int lengthNodePath)
        {
            // заполнить элемент - директория
            IsSource = isSourceSet;
            IsFile = false;
            Path = dirInf.FullName.Substring(lengthNodePath, dirInf.FullName.Length - lengthNodePath);
            NameExt = "";
            LastWriteTime = dirInf.LastWriteTime;
            Length = 0;
            PathOut = "";
            Act = Operation.Empty;
        }

        public ElementFilesTree(bool isSourceSet, FileInfo fileInf, int lengthNodePath)
        {
            // заполнить элемент - файл
            IsSource = isSourceSet;
            IsFile = true;
            Path = fileInf.FullName.Substring(lengthNodePath, fileInf.FullName.Length - lengthNodePath - fileInf.Name.Length);
            NameExt = fileInf.Name;
            LastWriteTime = fileInf.LastWriteTime;
            Length = fileInf.Length;
            PathOut = "";
            Act = Operation.Empty;
        }

        public ElementFilesTree(bool isSourceSet, string pathName, int lengthNodePath)
        {
            // заполнить элемент - ошибка чтения
            IsSource = isSourceSet;
            IsFile = false;
            Path = pathName.Substring(lengthNodePath);
            NameExt = "Ошибка чтения";
            Length = -1;
            PathOut = "";
            Act = Operation.Empty;
        }

        public string GetAsCSV(bool isHeader)   // вывести элемент в формате файла CSV
        {
            string ansKey;
            if (isHeader)
            {
                ansKey = "Направление;"
                    + "Тип;"
                    + "Путь;"
                    + "Имя;"
                    + "Время;"
                    + "Длина;"
                    + "Путь куда;"
                    + "Действие";
            }
            else
            {
                string sAct;
                switch (Act)
                {
                    case Operation.Empty:
                        sAct = "Не определено";
                        break;
                    case Operation.Nothing:
                        sAct = "Сходится";
                        break;
                    case Operation.Delete:
                        sAct = "Удалить";
                        break;
                    case Operation.New:
                        sAct = "Новый";
                        break;
                    case Operation.Copy:
                        sAct = "Копировать";
                        break;
                    case Operation.Move:
                        sAct = "Переместить";
                        break;
                    default:
                        sAct = "Ошибка";
                        break;
                }
                ansKey = (IsSource ? "Источник" : "Приёмник") + ";"
                    + (IsFile ? "Файл" : "Папка") + ";"
                    + Path + ";"
                    + NameExt + ";"
                    + LastWriteTime.ToString("g") + ";"
                    + Length.ToString() + ";"
                    + PathOut + ";"
                    + sAct;
            }
            return ansKey;
        }
    }
}
