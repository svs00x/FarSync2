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
        private const string PathDefault = @"D:\Test";    // Application.StartupPath
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
            HaveFileLog = (File.Exists(PathFileLog)) ? true : false;

            UpdateWindow();
        }

        //**************************  БАЗОВЫЕ ФУНКЦИИ  ****************************************

        private int Abs(int signValue)
        // вернуть абсолютное значение (число по модулю)
        {
            return signValue > 0 ? signValue : -signValue;
        }

        private bool CreateDirectory(string directoryPath)
        // создать директорию
        {
            bool result = true;
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    result = true;
                }
                catch (Exception ex)
                {
                    ReportABag("Ошибка при создании директории: <" + directoryPath + ">\n" + ex.Message, 0);
                    result = false;
                }
            }
            return result;
        }

        private bool DeleteDirectory(string directoryPath, bool isRecurse = false)
        // удалить директорию. По умолчанию только директорию без вложений. Если true, то все директории с вложениями
        {
            bool result = false;
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, isRecurse);  // удалять папку без вложений, для проверки
                    result = true;
                }
                catch (Exception ex)
                {
                    ReportABag("Ошибка при удалении директории: <" + directoryPath + ">\n" + ex.Message, 0);
                }
            }
            else
                ReportABag("Не найдена папка: <" + directoryPath + ">", 0);
            return result;
        }

        private bool CopyMoveFile(string sourceFileFullName, string destinitionFileFullName, bool isCopy)
        // копировать файл
        {
            bool result = false;
            if (File.Exists(sourceFileFullName))
            {
                try
                {
                    // запрещается переписывание существующего файла, его надо было сначала удалить
                    if (isCopy)
                        File.Copy(sourceFileFullName, destinitionFileFullName);
                    else
                        File.Move(sourceFileFullName, destinitionFileFullName);
                    result = true;
                }
                catch (Exception ex)
                {
                    ReportABag("Не удалось " + (isCopy ? "скопировать" : "переместить") + " файл <" + sourceFileFullName + ">: " + ex.Message, 0);
                }
            }
            else
                ReportABag("Не найден файл <" + sourceFileFullName + ">", 0);
            return result;
        }

        private bool DeleteFile(string deleteFileFullName)
        // удалить файл
        {
            bool result = false;
            if (File.Exists(deleteFileFullName))
            {
                try
                {
                    File.Delete(deleteFileFullName);
                    result = true;
                }
                catch (Exception ex)
                {
                    ReportABag("Не удалось удалить файл <" + deleteFileFullName + "> : " + ex.Message, 0, "Удаление файла");
                }
            }
            else
                ReportABag("Не найден файл <" + deleteFileFullName + ">", 0, "Удаление файла");
            return result;
        }

        //******************************** РАБОТА С ЭКРАННОЙ ФОРМОЙ И ВЫВОД ИНФОРМАЦИИ **********************************

        private void UpdateWindow()
        // обновить все элементы диалога. Ничего больше
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
            uxbtnLogFileShow.ForeColor = (HaveFileLog) ? SystemColors.ControlText : Color.Red;
            uxbtnLogFileDelete.ForeColor = (HaveFileLog) ? SystemColors.ControlText : Color.Red;

            //            Application.DoEvents(); // без этой строки не обновлялся прогресс бар
        }

        private void ReportABag(string logMessage, int directReport = 2, string logOperation = "")
        // вывести или сохранить сообщение об ошибке. По умолчанию выводится на экран
        // directReport = 1 - выводить в лог файл
        // directReport = 2 - выводить сообщение на экран
        // directReport = 0 - выводить как указано в пользовательской форме
        {
            bool flagReportToLog = uxchkIsLog.Checked; // по умолчанию выводится как указал пользователь в форме
            switch (directReport)
            {
                case 1:
                    flagReportToLog = true;
                    break;
                case 2:
                    flagReportToLog = false;
                    break;
            }
            string TextReport = "";
            if (flagReportToLog)
            {
                TextReport = string.Format("{0:dd.MM.yyy HH:mm:ss};{1};{2}\r\n", DateTime.Now, logOperation, logMessage);
                File.AppendAllText(PathFileLog, TextReport, Encoding.GetEncoding("Windows-1251"));
                HaveFileLog = true;
            }
            else
            {
                if (logOperation != "")
                    TextReport = "Процедура " + logOperation + "\n";
                TextReport += logMessage;
                MessageBox.Show(TextReport, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowProgress(int valCur, int valMax)
        // обновить полосу прогресса и вывести текущую долю выполнения
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

        //******************************** РАБОТА С КОНФИГУРАЦИОННЫМ ФАЙЛОМ **********************************

        private bool ReadConfiguration()
        // прочитать конфигурационный файл
        {
            bool result = false;
            string currentNameFileConfiguration = MainConfiguration.PathFileConfiguration;
            MainConfiguration.Status = Statuses.DontRead;
            BinaryFormatter formatter = new BinaryFormatter();
            if ( File.Exists(MainConfiguration.PathFileConfiguration) )
            {
                using (FileStream fs = File.OpenRead(MainConfiguration.PathFileConfiguration))
                {
                    try
                    {
                        // считать сохранненый файл конфигурации
                        MainConfiguration = (WorkConfiguration)formatter.Deserialize(fs);
                        if (MainConfiguration.PathFileConfiguration == currentNameFileConfiguration)
                            MainConfiguration.Status = Statuses.Save; // только что прочитан, сохранять не надо
                        else
                        {
                            // файл конфигурации был переименован и восстановилось старое имя файла
                            MainConfiguration.PathFileConfiguration = currentNameFileConfiguration;
                            MainConfiguration.Status = Statuses.Change; // имя файла конфигурации поменялось. Нужно сохранение
                        }
                        result = true;
                    }
                    catch
                    {
                        ReportABag("Файл конфигурации не загружен");
                    }
                }
            }
            return result;
        }

        private bool SaveConfiguration()
        // сохранить конфигурационный файл
        {
            bool result = false;
            if (File.Exists(MainConfiguration.PathFileConfiguration))
                DeleteFile(MainConfiguration.PathFileConfiguration); // удалить файл конфигурации, не умеет уменьшать размер
            using (FileStream fs = new FileStream(MainConfiguration.PathFileConfiguration, FileMode.OpenOrCreate))
            {
                MainConfiguration.Status = Statuses.Save; // конфигурация сохранена
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, MainConfiguration);      // сохранить в файл текущую конфигурацию
            }
            if (File.Exists(MainConfiguration.PathFileConfiguration))
                result = true;
            else
                ReportABag("Не удалось сохранить конфигурацию.");
            return result;
        }

        private void UxbtnConfigFileOpen_Click(object sender, EventArgs e)
        // выбрать файл конфигурации. !!!!!!!!!!!!!!!! можно разложить предыдущий конфигурационный файл на путь и имя !!!!!!!!!!!!!
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

        private void UxbtnConfigFileSave_Click(object sender, EventArgs e)
        // сохранить файл конфигурации
        {
            SaveConfiguration();
            UpdateWindow();
        }

        private void UxbtnConfigFileSaveAs_Click(object sender, EventArgs e)
        // сохранить файл конфигурации с другим именем
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

        //********************************** РАБОТА С ФАЙЛОМ ЛОГОВ ********************************

        private void UxbtnLogFileShow_Click(object sender, EventArgs e)
        // просмотреть лог файл
        {
            if (File.Exists(MainConfiguration.PathFileConfiguration))
                Process.Start(PathFileLog);
        }

        private void UxbtnLogFileDelete_Click(object sender, EventArgs e)
        // удалить лог файл
        {
            if (DeleteFile(PathFileLog))
            {
                HaveFileLog = false;
                UpdateWindow();
            }
            else
                ReportABag("Файл логов не найден");
        }

        //********************************** ВЫБРАТЬ КАТАЛОГИ ********************************

        private string SetDirectory(string oldPath)
        // выбрать директорию для всех типов (источник, приемник, разница)
        {
            folderBrowserDialog1.SelectedPath = oldPath;

            string result = "";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (folderBrowserDialog1.SelectedPath != oldPath)
                {
                    result = folderBrowserDialog1.SelectedPath;
                    MainConfiguration.Status = Statuses.Change;
                    MainConfiguration.DoesDifferenceFind = false;
                    MainConfiguration.DoesDifferenceDownload = false;
                    MainConfiguration.DoesDifferenceUpload = false;
                }
            }
            return result;
        }

        private void UxbtnSourcePathSet_Click(object sender, EventArgs e)
        // выбрать путь к источнику
        {
            string result = SetDirectory(MainConfiguration.PathSource);
            if (result != "")
            {
                MainConfiguration.PathSource = result;
                MainConfiguration.HavePathSource = true;
                MainConfiguration.ReadPathSource = false;
                UpdateWindow();
            }
        }

        private void UxbtnDestinationPathSet_Click(object sender, EventArgs e)
        // выбрать путь к приемнику
        {
            string result = SetDirectory(MainConfiguration.PathDestination);
            if (result != "")
            {
                MainConfiguration.PathDestination = result;
                MainConfiguration.HavePathDestination = true;
                MainConfiguration.ReadPathDestination = false;
                UpdateWindow();
            }
        }

        private void UxbtnDifferencePathSet_Click(object sender, EventArgs e)
        // выбрать путь к разнице
        {
            string result = SetDirectory(MainConfiguration.PathDifference);
            if (result != "")
            {
                MainConfiguration.PathDifference = result;
                MainConfiguration.HavePathDifference = true;
                UpdateWindow();
            }
        }

        //******************************** ЧТЕНИЕ КАТАЛОГОВ **********************************

        private void ReadOneDirectory(string path, bool isSource, int minBound, int maxBound)
        // узел рекурсивного процесса по чтению каталога
        {
            const string nameProcedure = "Прочитать содержимое папки";

            double lowBound, upBound; // нижняя и верхняя границы для текущей папки для вывода линии прогресса
            double stepProgress; // шаг для вывода линии прогресса

            if (Directory.Exists(path) == true)
            {
                uxlblInfoWorkName.Text = "Обрабатывается папка: " + path;
                uxlblInfoWorkName.Refresh();

                // прочитать файлы и папки в обрабатываемой директории
                try
                {
                    string[] directories = Directory.GetDirectories(path);      // список директорий в обрабатываемой папке
                    string[] files = Directory.GetFiles(path);                  // список файлов в обрабатываемой папке
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
                    ReportABag("Ошибка при чтении директории: " + path, 0, nameProcedure);
                    MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(path, isSource, LengthNodePath));
                }
            }
            else
                ReportABag("Не найдена папка: " + path, 0, nameProcedure);
        }

        private bool FillDirectory(string path, bool isSource)
        // обновить дерево в конфигурационном файле для каталога источника или приёмника (запуск рекурсивного процесса)
        {
            bool result = false;
            if (Directory.Exists(path))
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
                ReportABag("Не найдена папка <" + path + ">");
            return result;
        }

        private void UxbtnSourcePathRead_Click(object sender, EventArgs e)
        // прочитать директорию источник
        {
            if (MainConfiguration.HavePathSource == false)
                UxbtnSourcePathSet_Click(sender, e);    // если путь источник не выбран, то попробовать его выбрать
            if (MainConfiguration.HavePathSource)
            {
                if (FillDirectory(MainConfiguration.PathSource, true))
                {
                    MainConfiguration.ReadPathSource = true;
                    MainConfiguration.Status = Statuses.Change;
                    UpdateWindow();
                }
            }
        }

        private void UxbtnDestinationPathRead_Click(object sender, EventArgs e)
        // прочитать директорию приёмник
        {
            if (MainConfiguration.HavePathDestination == false)
                UxbtnDestinationPathSet_Click(sender, e);   // если путь приемник не выбран, то попробовать его выбрать
            if (MainConfiguration.HavePathDestination)
            {
                if (FillDirectory(MainConfiguration.PathDestination, false))
                {
                    MainConfiguration.ReadPathDestination = true;
                    MainConfiguration.Status = Statuses.Change;
                    UpdateWindow();
                }
            }
        }

        //********************************** ОБРАБОТАТЬ И ПОКАЗАТЬ ДАННЫЕ ********************************

        private void SortElements()
        // сортировать список элементов деревьев источника и приёмника
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

        private void SetAction()
        // установить действие для каждого элемента
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
                        MainConfiguration.ListElementsFilesTree[i].Act = Operation.Nothing;         // элемент источник
                        MainConfiguration.ListElementsFilesTree[i + 1].Act = Operation.Nothing;     // элемент приёмник
                        ListDestinitionIndexes.AddIndex(i + 1, false);  // зафиксировать, что этот базовый элемент встречался в приёмнике. Но не выставлять для связки в пару
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

        private void UxbtnDifferenceFind_Click(object sender, EventArgs e)
        // найти различия в директориях
        {
            MainConfiguration.ClearActElementsFilesTree();  // очистить действия для заполненных данных
            SortElements();     // сортировать элементы данных
            SetAction();        // установить действия для элементов данных
            MainConfiguration.DoesDifferenceFind = true;
            MainConfiguration.Status = Statuses.Change;
            UpdateWindow();
        }

        private void UxbtnExportCSV_Click(object sender, EventArgs e)
        // экспортировать прочитанные каталоги в CSV файл для анализа
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
                        ReportABag("Ошибка при открытии файла: " + ex.Message);
                    }
                }
            }
            else
                ReportABag("Директории ещё не прочитаны");
        }

        //********************************* РАБОТА С КАТАЛОГАМИ (ВЫГРУЗКА-ЗАГРУЗКА) *********************************

        private void UxbtnUpload_Click(object sender, EventArgs e)
        // выгрузить изменения в папку различий
        {
            bool flagOK = true;
            DirectoryInfo dirDifference = new DirectoryInfo(MainConfiguration.PathDifference);  // папка различий
            // удалить папку с различиями, чтобы не чистить
            if (Directory.Exists(MainConfiguration.PathDifference))
                flagOK = DeleteDirectory(MainConfiguration.PathDifference, true);  // true - удалить всю папку, включая вложения
            // создать новую папку различий
            if (flagOK)
                flagOK = CreateDirectory(MainConfiguration.PathDifference);
            if (!flagOK)
            {
                string textABag = "Отсутствует папка для копирования различий <";
                if (Directory.Exists(MainConfiguration.PathDifference))
                    textABag = "Нет полного доступа к папке для копирования различий <";
                ReportABag(textABag + MainConfiguration.PathDifference + ">");
            }
            else
            {
                if (!Directory.Exists(MainConfiguration.PathSource))
                {
                    ReportABag("Отсутствует папка источник <" + MainConfiguration.PathSource + ">");
                    flagOK = false;
                }
                else
                {
                    flagOK = true;
                    foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                    {
                        if (workElement.IsFile && workElement.Act == Operation.New)
                        {
                            // только для новых файлов
                            if (CreateDirectory(MainConfiguration.PathSource + workElement.Path))
                                flagOK = flagOK && CopyMoveFile(MainConfiguration.PathSource + workElement.Path + workElement.NameExt,
                                                            MainConfiguration.PathDifference + workElement.PathOut + workElement.NameExt, true);
                            else
                                flagOK = false;
                        }
                    }
                }
            }
            MainConfiguration.DoesDifferenceUpload = flagOK;
            UpdateWindow();
        }

        private void UxbtnDownload_Click(object sender, EventArgs e)
        // загрузить изменения и обновить папку приёмник
        {
            bool flagOK = true;
            if (Directory.Exists(MainConfiguration.PathDestination))
            {
                // создать новые папки
                foreach (ElementFilesTree workElement in MainConfiguration.ListElementsFilesTree)
                {
                    // действие "Новый" возможно только для источника
                    if (!workElement.IsFile && workElement.IsSource && workElement.Act == Operation.New)
                        flagOK = flagOK && CreateDirectory(MainConfiguration.PathDestination + workElement.Path);
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
                for (int i = MainConfiguration.ListElementsFilesTree.Count - 1; i >= 0; i--)
                {
                    if (MainConfiguration.ListElementsFilesTree[i].IsFile == false &&
                        MainConfiguration.ListElementsFilesTree[i].Act == Operation.Delete)
                    {
                        flagOK = flagOK && DeleteDirectory(MainConfiguration.PathDestination + MainConfiguration.ListElementsFilesTree[i].Path);
                    }
                }
                // добавить новые, перенесенные файлы
                if (Directory.Exists(MainConfiguration.PathDifference))
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
                    ReportABag("Отсутствует папка переноса <" + MainConfiguration.PathDifference + ">");
                    flagOK = false;
                }
            }
            else
            {
                ReportABag("Отсутствует папка для обновления <" + MainConfiguration.PathDestination + ">");
                flagOK = false;
            }
            MainConfiguration.DoesDifferenceDownload = flagOK;
            UpdateWindow();
        }

        //********************************* ЗАКРЫТИЕ ПРОГРАММЫ *********************************

        private void CheckExit(bool needExit)
        // проверка, что конфигурация сохранена при выходе из программы
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
        // выход по нажатию кнопки Выход
        {
            DialogResult result = MessageBox.Show("Хотите закончить программу?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
                CheckExit( true );
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        // выход по закрытию главного окна приложения
        {
            CheckExit(false);
        }

    }
}
