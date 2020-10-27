using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;   // для использования Process

namespace FarSync2
{
    public partial class MainForm : Form
    {
        public const string TextNoChoice = "Не выбран";
        private const string PathDefault = "D:\\Test";    // Application.StartupPath
        private const int MaxProgress = 100000; // длина линии прогресса информационной панели

        private static WorkConfiguration MainConfiguration = new WorkConfiguration();    // основной класс конфигурации
        private static bool HaveFileLog;    // есть файл логов
        private static string PathFileLog;  // полное имя файла логов
        private static int LengthNodePath;  // длина корневой директории

        public MainForm()
        {
            InitializeComponent();

            // попытаться считать конфигурационный файл в каталоге по умолчанию
            MainConfiguration.PathFileConfiguration = PathDefault + "\\farsync.cfg";
            ReadConfiguration();

            // проверить ли есть в каталоге по умолчанию файл логов
            PathFileLog = PathDefault + "\\farlog.csv";
            FileInfo fileInf = new FileInfo(PathFileLog);
            HaveFileLog = (fileInf.Exists) ? true : false;

            UpdateWindow();
        }

        // обновить все элементы диалога. Ничего больше
        private void UpdateWindow()
        {
            uxlblConfigFile.Text                = MainConfiguration.PathFileConfiguration;
            uxlblPathSource.Text                = MainConfiguration.PathSource;
            uxlblPathDestination.Text           = MainConfiguration.PathDestination;
            uxlblPathDifference.Text            = MainConfiguration.PathDifference;
            uxbtnConfigFileOpen.ForeColor       = (MainConfiguration.Status == Statuses.DontRead) ? Color.Red : SystemColors.ControlText;
            uxbtnConfigFileSave.ForeColor       = (MainConfiguration.Status == Statuses.Save) ? SystemColors.ControlText : Color.Red;
            uxbtnSourcePathSet.ForeColor        = (MainConfiguration.HavePathSource) ? SystemColors.ControlText : Color.Red;
            uxbtnSourcePathRead.ForeColor       = (MainConfiguration.ReadPathSource) ? SystemColors.ControlText : Color.Red;
            uxbtnDestinationPathSet.ForeColor   = (MainConfiguration.HavePathDestination) ? SystemColors.ControlText : Color.Red;
            uxbtnDestinationPathRead.ForeColor  = (MainConfiguration.ReadPathDestination) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferencePathSet.ForeColor    = (MainConfiguration.HavePathDifference) ? SystemColors.ControlText : Color.Red;

            uxbtnDifferenceFind.ForeColor       = (MainConfiguration.DoesDifferenceFind) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferenceUpload.ForeColor     = (MainConfiguration.DoesDifferenceUpload) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferenceDownload.ForeColor   = (MainConfiguration.DoesDifferenceDownload) ? SystemColors.ControlText : Color.Red;

            uxlblLogFile.Text = PathFileLog;
            uxbtnLogFileShow.ForeColor = (HaveFileLog) ? Color.Blue : SystemColors.ControlText;
            uxbtnLogFileDelete.ForeColor = (HaveFileLog) ? Color.Blue : SystemColors.ControlText;

            //            Application.DoEvents(); // без этой строки не обновлялся прогресс бар
        }

        // прочитать конфигурационный файл
        private bool ReadConfiguration()
        {
            bool result = false;
            MainConfiguration.Status = Statuses.DontRead;
            BinaryFormatter formatter = new BinaryFormatter();
            FileInfo fileInf = new FileInfo(MainConfiguration.PathFileConfiguration);
            if (fileInf.Exists)
            {
                using (FileStream fs = File.OpenRead(MainConfiguration.PathFileConfiguration))
                {
                    try
                    {
                        // считать сохранненый файл конфигурации
                        MainConfiguration = (WorkConfiguration)formatter.Deserialize(fs);
                        if (MainConfiguration.PathFileConfiguration == fileInf.FullName)
                        {
                            MainConfiguration.Status = Statuses.Save; // только что прочитан, сохранять не надо
                        }
                        else
                        {
                            // файл конфигурации был переименован и восстановилось старое имя файла
                            MainConfiguration.PathFileConfiguration = fileInf.FullName;
                            MainConfiguration.Status = Statuses.Change; // имя файла конфигурации поменялось. Нужно сохранение
                        }
                        result = true;
                    }
                    catch
                    {
                        MessageBox.Show("Файл конфигурации не загружен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return result;
        }

        // сохранить конфигурационный файл
        private bool SaveConfiguration()
        {
            bool result = false;
            DeleteFile(MainConfiguration.PathFileConfiguration); // удалить файл конфигурации, не умеет уменьшать размер
            using (FileStream fs = new FileStream(MainConfiguration.PathFileConfiguration, FileMode.OpenOrCreate))
            {
                MainConfiguration.Status = Statuses.Save; // конфигурация сохранена
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, MainConfiguration);      // сохранить в файл текущую конфигурацию
            }
            FileInfo fileInf = new FileInfo(MainConfiguration.PathFileConfiguration);
            if (fileInf.Exists)
                result = true;
            else
                MessageBox.Show("Не удалось сохранить конфигурацию.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return result;
        }

        // записать сообщение об ошибки в лог файл. Если лог файла нет, то создать его
        private void WriteLogMessage(string logOperation, string logMessage)
        {
            string fullText = string.Format("{0:dd.MM.yyy HH:mm:ss};{1};{2}\r\n", DateTime.Now, logOperation, logMessage);
            File.AppendAllText(PathFileLog, fullText, Encoding.GetEncoding("Windows-1251"));
            HaveFileLog = true;
        }

        // обновить полосу прогресса и вывести текущую долю выполнения
        private void ShowProgress(int valCur, int valMax)
        {
            uxproInfoProcent.Minimum = 0;
            if ((valMax > 0) && (valCur > 0))
            {
                uxproInfoProcent.Maximum = valMax;
                uxproInfoProcent.Value = (valMax > valCur) ? valCur : valMax;   // защита от ошибки, если текущий процент больше 100%
                double curProcent = (double)valCur / (double)valMax;
                uxlblInfoProcent.Text = curProcent.ToString("P2");
            }
            else
            {
                uxproInfoProcent.Maximum = 1;
                uxproInfoProcent.Value = 0;
                uxlblInfoProcent.Text = "0.00%";
            }
            uxproInfoProcent.Refresh();
            uxlblInfoProcent.Refresh();
            Application.DoEvents(); // без этой строки не обновлялся прогресс бар
        }

        // выбрать файл конфигурации. !!!!!!!!!!!!!!!! можно разложить предыдущий конфигурационный файл на путь и имя !!!!!!!!!!!!!
        private void UxbtnConfigFileOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Считать файл конфигурации";
            openFileDialog1.Filter = "config files (*.cfg)|*.cfg|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            // если уже открыт какой-то файл конфигурации, то новый предлагается открыть на основе старого
            FileInfo fileInf = new FileInfo(MainConfiguration.PathFileConfiguration);
            if (fileInf.Exists)
            {
                openFileDialog1.InitialDirectory = fileInf.DirectoryName;
                openFileDialog1.FileName = fileInf.Name;
            }
            else
            {
                openFileDialog1.InitialDirectory = PathDefault;
                openFileDialog1.FileName = "farsync.cfg";
            }

            if ((openFileDialog1.ShowDialog() == DialogResult.OK) && (MainConfiguration.PathFileConfiguration != openFileDialog1.FileName))
            {
                // открыть диалог выбора файла предварительно сохраненной конфигурации
                // файл выбран и он не тот же, что текущий
                MainConfiguration.PathFileConfiguration = openFileDialog1.FileName;
                if (ReadConfiguration() == false)
                    MainConfiguration.PathFileConfiguration = fileInf.FullName; // если чтение не получилось, то осталась конфигурация старого файла
                UpdateWindow();
            }
        }

        // сохранить файл конфигурации
        private void UxbtnConfigFileSave_Click(object sender, EventArgs e)
        {
            SaveConfiguration();
            UpdateWindow();
        }

        // сохранить файл конфигурации с другим именем
        private void UxbtnConfigFileSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Сохранить файл конфигурации";
            saveFileDialog1.InitialDirectory = PathDefault;
            saveFileDialog1.FileName = "farsync.cfg";
            saveFileDialog1.Filter = "config files (*.cfg)|*.cfg|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.OverwritePrompt = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)    // показать диалог для выбора имени файла, в котором сохранится текущая синхронизация
            {
                // записать текущие установки в данные класса
                MainConfiguration.PathFileConfiguration = saveFileDialog1.FileName;
                SaveConfiguration();
                UpdateWindow();
            }
        }

        // просмотреть лог файл
        private void UxbtnLogFileShow_Click(object sender, EventArgs e)
        {
            FileInfo fileInf = new FileInfo(PathFileLog);
            if (fileInf.Exists)
            {
                Process.Start(PathFileLog);
            }
        }

        // удалить лог файл
        private void UxbtnLogFileDelete_Click(object sender, EventArgs e)
        {
            bool success = DeleteFile(PathFileLog);
            if (success)
            {
                HaveFileLog = false;
                UpdateWindow();
            }
            else
                MessageBox.Show("Файл логов не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // выбрать директорию для всех типов (источник, приемник, разница)
        private void SetDirectory(int indexDirectory)
        {
            // 0 = source; 1 - destinition; 2 - difference
            string [] oldPath = new string[3] {MainConfiguration.PathSource, MainConfiguration.PathDestination, MainConfiguration.PathDifference };
            folderBrowserDialog1.SelectedPath = oldPath[indexDirectory];

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (oldPath[indexDirectory] != folderBrowserDialog1.SelectedPath)
                {
                    switch (indexDirectory)
                    {
                        case 0:
                            MainConfiguration.PathSource = folderBrowserDialog1.SelectedPath;
                            MainConfiguration.ReadPathSource = false;
                            break;
                        case 1:
                            MainConfiguration.PathDestination = folderBrowserDialog1.SelectedPath;
                            MainConfiguration.ReadPathDestination = false;
                            break;
                        case 2:
                            MainConfiguration.PathDifference = folderBrowserDialog1.SelectedPath;
                            break;
                    }
                    MainConfiguration.Status = Statuses.Change;
                    MainConfiguration.DoesDifferenceFind = false;
                    MainConfiguration.DoesDifferenceDownload = false;
                    MainConfiguration.DoesDifferenceUpload = false;
                }
                switch (indexDirectory)
                {
                    case 0:
                        MainConfiguration.HavePathSource = true;
                        break;
                    case 1:
                        MainConfiguration.HavePathDestination = true;
                        break;
                    case 2:
                        MainConfiguration.HavePathDifference = true;
                        break;
                }
                UpdateWindow();
            }
        }

        // выбрать путь к источнику
        private void UxbtnSourcePathSet_Click(object sender, EventArgs e)
        {
            SetDirectory(0);
        }

        // выбрать путь к приемнику
        private void UxbtnDestinationPathSet_Click(object sender, EventArgs e)
        {
            SetDirectory(1);
        }

        // выбрать путь к разнице
        private void UxbtnDifferencePathSet_Click(object sender, EventArgs e)
        {
            SetDirectory(2);
        }

        // узел рекурсивного процесса по чтению каталога
        private void ReadOneDirectory(string path, bool isSource, int minBound, int maxBound)
        {
            const string nameProcedure = "Прочитать содержимое папки";

            double lowBound, upBound; // нижняя и верхняя границы для текущей папки для вывода линии прогресса
            double stepProgress; // шаг для вывода линии прогресса

            if (Directory.Exists(path) == true)
            {
                uxlblInfoWorkName.Text = "Обрабатывается папка: " + path;
                uxlblInfoWorkName.Refresh();

                try
                {
                    string[] directories = Directory.GetDirectories(path);      // список файлов и директорий в обрабатываемой папке
                    string[] files = Directory.GetFiles(path);                  // список файлов и директорий в обрабатываемой папке
                    int amountOfElementses = directories.Count();    // количество файлов и директорий в обрабатываемой папке
                    amountOfElementses += (files.Count() > 0) ? 1 : 0; // если файлы в папке есть, то добавляем ещё один элемент для прогресса чтения
                    if (amountOfElementses == 0)
                        ShowProgress(maxBound, MaxProgress);  // в обрабатываемой директории нет ни файлов ни папок
                    else
                    {
                        stepProgress = (maxBound - minBound) / amountOfElementses;    // размер одного шага для прогресса
                        lowBound = minBound;
                        upBound = minBound;
                        if (files.Count() > 0)
                        {
                            foreach (string file in files)
                            {
                                FileInfo fileInf = new FileInfo(file);     // считать аттрибуты файла
                                if (fileInf.Exists)
                                    MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(fileInf, isSource, LengthNodePath));         // добавить к списку новый элемент файловой директории
                            }
                            upBound = lowBound + stepProgress;
                            ShowProgress((int)Math.Round(upBound), MaxProgress);  // прогресс в один шаг для всех файлов
                        }

                        foreach (string directory in directories)
                        {
                            DirectoryInfo dirInf = new DirectoryInfo(directory);    // считать аттрибуты директории
                            if (dirInf.Exists)
                            {
                                MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(dirInf, isSource, LengthNodePath));         // добавить к списку новый элемент файловой директории
                                // задать нижнюю и верхнюю границы для чтения одной вложенной папки
                                lowBound = upBound;
                                upBound = lowBound + stepProgress;
                                ReadOneDirectory(directory, isSource, (int)Math.Round(lowBound), (int)Math.Round(upBound));    // рекурсивный вызов чтения вложенной директории
                            }
                        }
                    }
                }
                catch
                {
                    WriteLogMessage(nameProcedure, "Ошибка при чтении директории: " + path);
                    MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(path, isSource, LengthNodePath));
                }

            }
            else
            {
                WriteLogMessage(nameProcedure, "Не найдена папка: " + path);
            }
        }

        // обновить дерево в конфигурационном файле для каталога источника или приёмника (запуск рекурсивного процесса)
        private bool FillDirectory(string path, bool isSource)
        {
            bool result = false;
            if (Directory.Exists(path) == true)
            {
                if (MainConfiguration.ListElementsFilesTree.Count > 0)
                    MainConfiguration.ClearElementsFilesTree(isSource); // очистить сохраненные данные папки
                LengthNodePath = path.Length;   // длина строки пути корневого каталога
                ReadOneDirectory(path, isSource, 0, MaxProgress);   // запуск рекурсивной функции по чтению дерева директорий
                // вывести информацию, что процесс завершён
                uxlblInfoWorkName.Text = "Чтение папки: <" + path + "> окончено";
                ShowProgress(0, 0);
                result = true;
            }
            else
                MessageBox.Show("Не найдена папка <" + path + ">", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return result;
        }

        // прочитать директорию источник
        private void UxbtnSourcePathRead_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.HavePathSource == false)
                UxbtnSourcePathSet_Click(sender, e);    // если путь источник не выбран, то попробовать его выбрать
            if (MainConfiguration.HavePathSource)
            {
                bool isReadDirectory = FillDirectory(MainConfiguration.PathSource, true);
                if (isReadDirectory)
                {
                    MainConfiguration.ReadPathSource = true;
                    MainConfiguration.Status = Statuses.Change;
                    UpdateWindow();
                }
            }
        }

        // прочитать директорию приёмник
        private void UxbtnDestinationPathRead_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.HavePathDestination == false)
                UxbtnDestinationPathSet_Click(sender, e);   // если путь приемник не выбран, то попробовать его выбрать
            if (MainConfiguration.HavePathDestination)
            {
                bool isReadDirectory = FillDirectory(MainConfiguration.PathDestination, false);
                if (isReadDirectory)
                {
                    MainConfiguration.ReadPathDestination = true;
                    MainConfiguration.Status = Statuses.Change;
                    UpdateWindow();
                }
            }
        }

        // экспортировать прочитанные каталоги в CSV файл для анализа
        private void UxbtnExportCSV_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.ListElementsFilesTree.Count > 0)
            {
                saveFileDialog1.Title = "Экспортировать данные в файл";
                saveFileDialog1.InitialDirectory = PathDefault;
                saveFileDialog1.FileName = "farsync.csv";
                saveFileDialog1.Filter = "config files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.Default))
                        {
                            uxlblInfoWorkName.Text = "Экспортируются данные ...";
                            int curItem = 0;
                            sw.WriteLine(MainConfiguration.ListElementsFilesTree[0].GetAsCSV(true));
                            foreach (ElementFilesTree element in MainConfiguration.ListElementsFilesTree)
                            {
                                sw.WriteLine(element.GetAsCSV(false));
                                curItem++;
                                ShowProgress(curItem, MainConfiguration.ListElementsFilesTree.Count);
                            }
                            uxlblInfoWorkName.Text = "Прочитанные данные экспортированы";
                            ShowProgress(0, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при открытии файла: " + ex.Message);
                    }
                }
            }
            else
                MessageBox.Show("Директории ещё не прочитаны", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // вернуть абсолютное значение (число по модулю)
        private int Abs(int signValue)
        {
            return signValue > 0 ? signValue : -signValue;
        }

        // сортировать список элементов деревьев источника и приёмника
        private void SortElements()
        {
            ElementFilesTree tempElement;
            int amountOfElements = MainConfiguration.ListElementsFilesTree.Count;

            uxlblInfoWorkName.Text = "Идёт сортировка списка элементов...";
            for (int i = 0; i < amountOfElements - 1; i++)
            {
                ShowProgress(i, amountOfElements - 2);
                for (int j = i + 1; j < amountOfElements; j++)
                {
                    int keySort = MainConfiguration.CompareElementsFilesTree(i, j); // максимальная глубина сравнения до пути и источника-приемника
                    if (keySort > 0)
                    {
                        tempElement = MainConfiguration.ListElementsFilesTree[i];
                        MainConfiguration.ListElementsFilesTree[i] = MainConfiguration.ListElementsFilesTree[j];
                        MainConfiguration.ListElementsFilesTree[j] = tempElement;
                    }
                }
            }
            uxlblInfoWorkName.Text = "Сортировка списка элементов закончена...";
            ShowProgress(0, 0);
        }

        // установить действие для каждого элемента
        private void SetAction()
        {
            int result; // результат сравнения
            bool isFile; // текущее сравнение файла или папки
            int amountOfElements = MainConfiguration.ListElementsFilesTree.Count;
            ListIndexesSameFiles ListSourceIndexes = new ListIndexesSameFiles();
            ListIndexesSameFiles ListDestinitionIndexes = new ListIndexesSameFiles();

            uxlblInfoWorkName.Text = "Идёт установка действий элементов...";
            for (int i = 0; i <= amountOfElements; i++)   // amountOfElements = 3 {0, 1, 2} => i = 0, 1, 2, <3 - нет>
            {
                ShowProgress(i, amountOfElements);
                if (i == amountOfElements) // отсутствующий элемент
                {
                    isFile = true;
                    result = 9; // заведомо больше результата сравнения
                }
                else
                {
                    isFile = MainConfiguration.ListElementsFilesTree[i].IsFile;
                    if (i == amountOfElements - 1) // последний элемент, сравнивать со следующим не нужно, просто записываем для создания пары
                        result = (MainConfiguration.ListElementsFilesTree[i].IsSource) ? 3 : 4;
                    else    // сравнить имя с расширением, размер, время изменения, путь
                        result = Abs(MainConfiguration.CompareElementsFilesTree(i, i + 1));
                }
                if (isFile)
                {
                    if (result == 1)
                    {
                        // 1 - различаются только источник - приёмник
                        MainConfiguration.ListElementsFilesTree[i].Act = Operation.Nothing;
                        MainConfiguration.ListElementsFilesTree[i + 1].Act = Operation.Nothing;
                        ListDestinitionIndexes.AddIndex(i, false);  // зафиксировать, что этот базовый элемент встречался в приёмнике. Но не выставлять для связки в пару
                    }
                    else if (result < 9)
                    {
                        // выставить элемент для связки в пару
                        if (MainConfiguration.ListElementsFilesTree[i].Act == Operation.Empty)
                        {
                            if (MainConfiguration.ListElementsFilesTree[i].IsSource)
                                ListSourceIndexes.AddIndex(i, true);    // предложить для пары
                            else
                                ListDestinitionIndexes.AddIndex(i, true);    // предложить для пары
                        }
                    }
                    if (result > 4)
                    {
                        // базовый элемент поменялся, запустить обработку увязки элементов для пар
                        int indexDestinitionElement;
                        int indexSourceElement;
                        int j = 1; // начинаем со второго элемента, т.к. первый для копий
                        while (j < ListSourceIndexes.AmountIndexes || j < ListDestinitionIndexes.AmountIndexes)
                        {
                            if (j < ListSourceIndexes.AmountIndexes && j < ListDestinitionIndexes.AmountIndexes)
                            {
                                // в приемнике есть свободный элемент для перемещения
                                indexSourceElement = ListSourceIndexes.ListIndexes[j];
                                indexDestinitionElement = ListDestinitionIndexes.ListIndexes[j];
                                // прописываем в источнике откуда переместить файл
                                MainConfiguration.ListElementsFilesTree[indexSourceElement].Act = Operation.Move;
                                MainConfiguration.ListElementsFilesTree[indexSourceElement].PathOut =
                                MainConfiguration.ListElementsFilesTree[indexDestinitionElement].Path;
                                // прописываем в приёмнике куда переместить файл
                                MainConfiguration.ListElementsFilesTree[indexDestinitionElement].Act = Operation.Move;
                                MainConfiguration.ListElementsFilesTree[indexDestinitionElement].PathOut =
                                MainConfiguration.ListElementsFilesTree[indexSourceElement].Path;
                            }
                            else if (j <= ListSourceIndexes.AmountIndexes)
                            {
                                indexSourceElement = ListSourceIndexes.ListIndexes[j];
                                // есть потребность в файлах, а в приёмнике свободных файлов больше нет
                                if (ListDestinitionIndexes.AmountIndexes > 0)
                                {
                                    // есть такой базовый файл в папке приёмника. Копируем его в нужное поле
                                    indexDestinitionElement = ListDestinitionIndexes.ListIndexes[0];
                                    MainConfiguration.ListElementsFilesTree[indexSourceElement].Act = Operation.Copy;
                                    MainConfiguration.ListElementsFilesTree[indexSourceElement].PathOut =
                                    MainConfiguration.ListElementsFilesTree[indexDestinitionElement].PathOut; // прописываем в приёмнике куда переместить файл
                                }
                                else
                                {
                                    // нет такого базового файла в папке приёмника. Создаём новый в папке переноса
                                    MainConfiguration.ListElementsFilesTree[indexSourceElement].Act = Operation.New;
                                    MainConfiguration.ListElementsFilesTree[indexSourceElement].PathOut =
                                    MainConfiguration.ListElementsFilesTree[indexSourceElement].Path; // относительный путь тот же самый
                                }
                            }
                            else if (j <= ListDestinitionIndexes.AmountIndexes)
                            {
                                indexDestinitionElement = ListDestinitionIndexes.ListIndexes[j];
                                MainConfiguration.ListElementsFilesTree[indexDestinitionElement].Act = Operation.Delete;
                            }
                            j++;
                        }
                        ListSourceIndexes.ResetListIndexesSameFiles();
                        ListDestinitionIndexes.ResetListIndexesSameFiles();
                    }
                }
                else
                {
                    // проверка для папок
                    result = Abs(MainConfiguration.CompareElementsFilesTree(i, i + 1));
                    if (result == 1)
                    {
                        // папки одинаковые ничего не делать
                        MainConfiguration.ListElementsFilesTree[i].Act = Operation.Nothing;
                        MainConfiguration.ListElementsFilesTree[i + 1].Act = Operation.Nothing;
                        i++; // пропустить следующий элемент, как уже обработанный.
                    }
                    else
                    {
                        // папки разные. Для источника создать новый, для приёмника удалить
                        if (MainConfiguration.ListElementsFilesTree[i].IsSource)
                            MainConfiguration.ListElementsFilesTree[i].Act = Operation.New;
                        else
                            MainConfiguration.ListElementsFilesTree[i].Act = Operation.Delete;
                    }
                }
            }
            uxlblInfoWorkName.Text = "Установка действий закончена...";
            ShowProgress(0, 0);
        }

        // найти различия в директориях
        private void UxbtnDifferenceFind_Click(object sender, EventArgs e)
        {
            SortElements();
            MainConfiguration.ClearActElementsFilesTree();
            SetAction();
            MainConfiguration.DoesDifferenceFind = true;
            MainConfiguration.Status = Statuses.Change;
            UpdateWindow();
        }

        // удалить файл
        private bool DeleteFile(string deleteFileFullName)
        {
            bool result = false;
            FileInfo fileDelete = new FileInfo(deleteFileFullName);
            if (fileDelete.Exists)
            {
                try
                {
                    fileDelete.Delete();
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось удалить файл <" + deleteFileFullName + "> : " + ex.Message);
                }
            }
            else
                MessageBox.Show("Не найден файл <" + deleteFileFullName + ">");
            return result;
        }

        // копировать файл
        private bool CopyMoveFile(string sourceFileFullName, string destinitionFileFullName, bool isCopy)
        {
            bool result = false;
            FileInfo fileCopyMove = new FileInfo(sourceFileFullName);
            if (fileCopyMove.Exists)
            {
                try
                {
                    if (isCopy)
                        fileCopyMove.CopyTo(destinitionFileFullName, false);
                    else
                        fileCopyMove.MoveTo(destinitionFileFullName);
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось " + (isCopy ? "скопировать" : "переместить") + " файл <" + sourceFileFullName + ">: " + ex.Message);
                }
            }
            else
                MessageBox.Show("Не найден файл <" + sourceFileFullName + ">");
            return result;
        }

        // создать директорию
        private bool CreateDirectory(DirectoryInfo dirRoot, string subPath)
        {
            bool result = false;
            try
            {
                if (subPath == "")
                    dirRoot = Directory.CreateDirectory(dirRoot.FullName);
                else
                    dirRoot.CreateSubdirectory(subPath.Trim('\\'));
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании директории: <" + subPath + ">\n" + ex.Message);
            }
            return result;
        }

        // удалить директорию. По умолчанию только директорию без вложений. Если true, то все директории с вложениями
        private bool DeleteDirectory(DirectoryInfo dirRoot, bool isRecurse = false)
        {
            bool result = false;
            if (dirRoot.Exists)
            {
                try
                {
                    dirRoot.Delete(isRecurse);  // удалять папку без вложений, для проверки
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении директории: <" + dirRoot.FullName + ">\n" + ex.Message);
                }
            }
            else
                MessageBox.Show("Не найдена папка: <" + dirRoot.FullName + ">");
            return result;
        }

        // выгрузить изменения в папку различий
        private void UxbtnUpload_Click(object sender, EventArgs e)
        {
            bool flagOK = false;
            DirectoryInfo dirDifference = new DirectoryInfo(MainConfiguration.PathDifference);  // папка различий
            // удалить папку с различиями, чтобы не чистить
            if (!dirDifference.Exists)
                flagOK = true;
            else if (DeleteDirectory(dirDifference, true))  // true - удалить всю папку, включая вложения
                flagOK = true;
            // создать новую папку различий
            if (flagOK)
                flagOK = CreateDirectory(dirDifference, "");
            if (!flagOK)
                MessageBox.Show("Отсутствует папка для копирования различий <" + MainConfiguration.PathDifference + ">");
            else
            {
                DirectoryInfo dirSource = new DirectoryInfo(MainConfiguration.PathSource);  // папка исходная
                if (dirSource.Exists)
                {
                    flagOK = true;
                    foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                    {
                        if (workElement.IsFile && workElement.Act == Operation.New)
                        {
                            // только для новых файлов
                            if (CreateDirectory(dirDifference, workElement.Path))
                            {
                                if (CopyMoveFile(MainConfiguration.PathSource + workElement.Path + workElement.NameExt,
                                                 MainConfiguration.PathDifference + workElement.PathOut + workElement.NameExt, true) == false)
                                    flagOK = false;
                            }
                            else
                                flagOK = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Отсутствует папка источник <" + MainConfiguration.PathSource + ">");
                    flagOK = false;
                }
            }
            MainConfiguration.DoesDifferenceUpload = flagOK;
            UpdateWindow();
        }

        // загрузить изменения и обновить папку приёмник
        private void UxbtnDownload_Click(object sender, EventArgs e)
        {
            bool flagOK = true;
            DirectoryInfo dirDestinition = new DirectoryInfo(MainConfiguration.PathDestination);  // папка различий
            if (dirDestinition.Exists)
            {
                // создать новые папки
                foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                {
                    // действие "Новый" возможно только для источника
                    if (!workElement.IsFile && workElement.IsSource && workElement.Act == Operation.New)
                        flagOK = flagOK && CreateDirectory(dirDestinition, workElement.Path);
                }
                // удалить лишние файлы (не папки)
                foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                {
                    if (workElement.IsFile && workElement.Act == Operation.Delete)
                        flagOK = flagOK && DeleteFile(MainConfiguration.PathDestination + workElement.Path + workElement.NameExt);
                }
                // переместить нужные файлы
                foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                {
                    // перемещать только элементы источника, т.к. приёмник дублирует действие
                    if (workElement.IsSource && workElement.Act == Operation.Move)
                        flagOK = flagOK && CopyMoveFile(MainConfiguration.PathDestination + workElement.PathOut + workElement.NameExt,
                                     MainConfiguration.PathDestination + workElement.Path + workElement.NameExt, false);
                }
                // копировать нужные файлы из перемещённых
                foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                {
                    // действие "Копировать" возможно только для источника
                    if (workElement.IsSource && workElement.Act == Operation.Copy)
                        flagOK = flagOK && CopyMoveFile(MainConfiguration.PathDestination + workElement.PathOut + workElement.NameExt,
                                     MainConfiguration.PathDestination + workElement.Path + workElement.NameExt, true);
                }
                // удалить лишние папки
                DirectoryInfo dirDelete;
                for (int i = MainConfiguration.ListElementsFilesTree.Count - 1; i >= 0; i--)
                {
                    if (MainConfiguration.ListElementsFilesTree[i].IsFile == false &&
                        MainConfiguration.ListElementsFilesTree[i].Act == Operation.Delete)
                    {
                        dirDelete = new DirectoryInfo(MainConfiguration.PathDestination + MainConfiguration.ListElementsFilesTree[i].Path);
                        flagOK = flagOK && DeleteDirectory(dirDelete);
                    }
                }
                // добавить новые, перенесенные файлы
                DirectoryInfo dirDifference = new DirectoryInfo(MainConfiguration.PathDifference);  // папка различий
                if (dirDifference.Exists)
                {
                    // копировать новые файлы из папки переноса
                    foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                    {
                        // действие "Новый" возможно только для источника
                        if (workElement.IsFile && workElement.IsSource && workElement.Act == Operation.New)
                            flagOK = flagOK && CopyMoveFile(MainConfiguration.PathDifference + workElement.PathOut + workElement.NameExt,
                                         MainConfiguration.PathDestination + workElement.Path + workElement.NameExt, true);
                    }
                }
                else
                {
                    MessageBox.Show("Отсутствует папка переноса <" + MainConfiguration.PathDifference + ">");
                    flagOK = false;
                }
            }
            else
            {
                MessageBox.Show("Отсутствует папка для обновления <" + MainConfiguration.PathDestination + ">");
                flagOK = false;
            }
            MainConfiguration.DoesDifferenceDownload = flagOK;
            UpdateWindow();
        }

        // выход из программы
        private void CheckExit(bool needExit)
        {
            bool canExit = true;
            if (MainConfiguration.Status != Statuses.Save)
            {
                DialogResult result = MessageBox.Show("Конфигурация не сохранена. Сохранить?", "Вопрос", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                switch (result)
                {
                    case DialogResult.Yes:  // сначала сохранить конфигурацию
                        canExit = SaveConfiguration();  // если сохранения не произошло, то выход из программы запрещён
                        break;
                    case DialogResult.No:   // можно выходить
                        canExit = true;
                        break;
                    case DialogResult.Cancel:   // выход отменен
                        canExit = false;
                        break;
                }
            }
            if (needExit && canExit)
                Application.Exit();    // Environment.Exit(0);
        }

        private void UxbtnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Хотите закончить программу?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
                CheckExit( true );
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckExit(false);
        }

    }
}
