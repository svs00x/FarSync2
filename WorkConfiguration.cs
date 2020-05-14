using System;
using System.Collections.Generic;

namespace FarSync2
{
    public enum Statuses : byte
    {
        DontRead,
        Change,
        Save
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

        // сравнить два элемента списка: -1 i меньше j; 0 равны; 1 i больше j
        // deep = 0 это базовый уровень (flagDir + lSize + dElement + sElement); (lSize + dElement) только для файлов
        // deep = 1 это дополнительное сравнение к базовому
        // для папки добавляется sPath + (источник | приемник)
        // для файла добавляется (источник | приемник) + sPath
        public int CompareElementsFilesTree(int i, int j)
        {
            TimeSpan lastWriteTimeDifference;
            long lengthDifference;
            int result = 0;

            if (ListElementsFilesTree[i].IsFile != ListElementsFilesTree[j].IsFile)
            {
                // один элемент файл, другой директория
                result = (ListElementsFilesTree[i].IsFile) ? 1 : -1;
            }
            else
            {
                if (ListElementsFilesTree[i].IsFile)     // для файлов сравнить длины и время
                {
                    lengthDifference = ListElementsFilesTree[i].Length - ListElementsFilesTree[j].Length;    // сравнить размеры файлов
                    if (lengthDifference == 0)
                    {
                        // размеры файлов совпали, сравнить время файлов
                        lastWriteTimeDifference = ListElementsFilesTree[i].LastWriteTime - ListElementsFilesTree[j].LastWriteTime;
                        result = (lastWriteTimeDifference.TotalDays == 0) ? 0 : (lastWriteTimeDifference.TotalDays < 0) ? -1 : 1;
                    }
                    else
                        result = (lengthDifference < 0) ? -1 : 1;
                }
                if (result == 0) // пока сходится, проверяем строки: имя, относительный путь, источник
                {
                    result = string.Compare(ListElementsFilesTree[i].NameExt, ListElementsFilesTree[j].NameExt);
                    if (result == 0)
                    {
                        // имена совпали, сравниваем путь и источник-приемник
                        int resultSource = ((ListElementsFilesTree[i].IsSource) ? 1 : 2) - ((ListElementsFilesTree[j].IsSource) ? 1 : 2);
                        int resultPath = string.Compare(ListElementsFilesTree[i].Path, ListElementsFilesTree[j].Path);
                        if (ListElementsFilesTree[i].IsFile)
                        {
                            // для файлов (источник | приемник) + sPath
                            result = (resultSource == 0) ? resultPath : resultSource;
                        }
                        else
                        {
                            // для директорий sPath + (источник | приемник)
                            result = (resultPath == 0) ? resultSource : resultPath;
                        }
                    }
                }
            }
            return result;
        }

        public void ClearActElementsFilesTree() // очистить действия для элементов
        {
            for (int i = 0; i < ListElementsFilesTree.Count - 1; i++)
                ListElementsFilesTree[i].Act = Operation.Empty;
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
