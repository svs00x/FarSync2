using System;
using System.Collections.Generic;
// using System.Text;
// using System.Windows.Forms;

namespace FarSync2
{
    public enum Statuses : byte
    {
        DontRead,   // файл конфигурации не прочитан
        Change,     // файл нуждается в сохранении
        Save        // файл сохранен
    }

    [Serializable]
    class WorkConfiguration
    {
        public string PathFileConfiguration;  // главный конфигурационный файл
        public string PathSource;  // путь, где лежит папка источник
        public string PathDestination;  // путь, где лежит папка приёмник "d:\Temp\Удалить\Different"
        public string PathDifference;  // путь, где лежит папка с обновленными файлами

        public long LenghDifferenceTotal;   // размер файлов в папке изменений

        public bool HaveFileConfiguration; // выбран главный конфигурационный файл
        public bool HavePathSource; // выбран путь к источнику
        public bool ReadPathSource; // прочитан источник
        public bool HavePathDestination; // выбран путь к приемнику
        public bool ReadPathDestination; // прочитан приемник
        public bool HavePathDifference; // выбран путь к папке различий
        public bool DoesDifferenceFind; // различия определены
        public bool DoesDifferenceUpload; // различия выгружены
        public bool DoesDifferenceDownload; // различия загружены
        public Statuses Status; // статус конфигурации (не прочитан, изменен, сохранен)

        public List<ElementFilesTree> ListElementsFilesTree;  // список файлов источника, приёмника и изменений

        // основной класс
        public WorkConfiguration()
        {
            PathFileConfiguration = MainForm.TextNoChoice;
            PathSource = MainForm.TextNoChoice;
            PathDestination = MainForm.TextNoChoice;
            PathDifference = MainForm.TextNoChoice;

            LenghDifferenceTotal = 0;
            ListElementsFilesTree = new List<ElementFilesTree>(); // создаем узел списка элементов

            HaveFileConfiguration = false;
            HavePathSource = false;
            ReadPathSource = false;
            HavePathDestination = false;
            ReadPathDestination = false;
            HavePathDifference = false;
            DoesDifferenceFind = false;
            DoesDifferenceUpload = false;
            DoesDifferenceDownload = false;
            Status = Statuses.DontRead;
        }

        // сравнить два элемента списка
        // на входе 2 индекса сравнения и глубина сравнения:
        // compareDeep = 0 для директорий выдаст результат, только если сравнивать с файлом
        // compareDeep = 0 это базовый уровень, только для файлов имя, с расширением + размер + дата и время изменения
        // compareDeep = 1 это средний уровень, к базовому уровню добавляется относительный путь
        // compareDeep = 2 это максимальный уровень, к среднему уровню добавляется относительный путь
        // на выходе:
        // 0 - одинаковы, такого быть не может
        // 1 - различаются только источник - приёмник
        // 2 - различаются пути и один элемент источник - другой приёмник
        // 3 - различаются пути и оба элемента источники
        // 4 - различаются пути и оба элемента приёмники
        // 5 - различаются имена с расширением
        // 6 - различаются время файла
        // 7 - различаются длины
        // 8 - различаются директория и файл
        public int CompareElementsFilesTree(int i, int j)
        {
            TimeSpan lastWriteTimeDifference;
            long lengthDifference;
            int result = 0;

            if (ListElementsFilesTree[i].IsFile != ListElementsFilesTree[j].IsFile)
            {
                // один элемент файл, другой директория
                result = 8 * ( (ListElementsFilesTree[i].IsFile) ? 1 : -1 );
            }
            else
            {
                if (ListElementsFilesTree[i].IsFile)     // для файлов сравнить длины и время
                {
                    lengthDifference = ListElementsFilesTree[i].Length - ListElementsFilesTree[j].Length;    // сравнить размеры файлов
                    if (lengthDifference != 0)
                        result = 7 * ( (lengthDifference > 0) ? 1 : -1 );
                    else
                    {
                        // размеры файлов совпали, сравнить время файлов
                        lastWriteTimeDifference = ListElementsFilesTree[i].LastWriteTime - ListElementsFilesTree[j].LastWriteTime;
                        result = 6 * ( (lastWriteTimeDifference.TotalDays == 0) ? 0 : (lastWriteTimeDifference.TotalDays > 0) ? 1 : -1 );
                    }
                    if (result == 0) // для файлов проверяем имена файлов
                    {
                        result = string.Compare(ListElementsFilesTree[i].NameExt, ListElementsFilesTree[j].NameExt);
                        result = 5 * ( (result == 0) ? 0 : ((result > 0) ? 1 : -1) );
                    }
                }
                if (result == 0) // пока сходится, проверяем относительный путь и источник
                {
                    // сравниваем источник или приёмник. Результат может быть: -1 или 0 или 1
                    int resultIsSource = ((ListElementsFilesTree[i].IsSource) ? 1 : 2) - ((ListElementsFilesTree[j].IsSource) ? 1 : 2);
                    result = string.Compare(ListElementsFilesTree[i].Path, ListElementsFilesTree[j].Path);
                    if (result == 0)
                        result = resultIsSource; // результат может быть: -1 или 0 или 1; 0 - строки полностью совпадают - ошибка
                    else
                    {
                        result = ( result > 0 ) ? 1 : -1;
                        if (resultIsSource == 0)
                            result *= ((ListElementsFilesTree[i].IsSource) ? 3 : 4);
                        else
                            result *= 2;
                    }
                }
            }
            return result;
        }

        public void ClearActElementsFilesTree() // очистить действия для элементов
        {
            foreach (ElementFilesTree workElement in ListElementsFilesTree)
                workElement.Act = Operation.Empty;
        }

        public void ClearElementsFilesTree(bool isClearDirection) // чистит элементы для соответствующего направления (источник или приемник)
        {
            // чистим список для данного типа
            for (int i = ListElementsFilesTree.Count - 1; i >= 0; i--)
            {
                if (ListElementsFilesTree[i].IsSource == isClearDirection)
                    ListElementsFilesTree.RemoveAt(i);
            }
        }

    }
}
