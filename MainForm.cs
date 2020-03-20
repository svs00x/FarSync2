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
        private const int MaxProgress = 100000; // длина линии прогресса информационной панели

        private static WorkConfiguration MainConfiguration = new WorkConfiguration();    // основной класс конфигурации
        private static bool HaveFileLog;
        private static string PathFileLog;

        private static int LengthNodePath;

        public MainForm()
        {
            InitializeComponent();

            // попытаться считать конфигурационный файл в каталоге запуска
            MainConfiguration.PathFileConfiguration = Application.StartupPath + "\\farsync.cfg";
            ReadConfiguration();

            // проверить ли есть в каталоге запуска файл логов
            PathFileLog = Application.StartupPath + "\\farlog.csv";
            FileInfo fileInf = new FileInfo(PathFileLog);
            HaveFileLog = (fileInf.Exists) ? true : false;

            UpdateWindow();
        }

        private bool DeleteFile(string nameFile)
        {
            bool result = false;
            FileInfo fileInf = new FileInfo(nameFile);
            if (fileInf.Exists)
            {
                try
                {
                    System.IO.File.Delete(nameFile);
                    result = true;
                } catch {}
            }
            return result;
        }

        private void WriteLogMessage(string logOperation, string logMessage)
        {
            string fullText = string.Format("{0:dd.MM.yyy HH:mm:ss};{1};{2}\r\n", DateTime.Now, logOperation, logMessage);
            File.AppendAllText(PathFileLog, fullText, Encoding.GetEncoding("Windows-1251"));
            HaveFileLog = true;
        }

        private void UpdateWindow()
        {
            uxlblConfigFile.Text = MainConfiguration.PathFileConfiguration;
            uxlblPathSource.Text = MainConfiguration.PathSource;
            uxlblPathDestination.Text = MainConfiguration.PathDestination;
            uxlblPathDifference.Text = MainConfiguration.PathDifference;
            uxbtnConfigFileOpen.ForeColor = (MainConfiguration.Status == Statuses.DontRead) ? Color.Red : SystemColors.ControlText;
            uxbtnConfigFileSave.ForeColor = (MainConfiguration.Status == Statuses.Save) ? SystemColors.ControlText : Color.Red;
            uxbtnSourcePathSet.ForeColor = (MainConfiguration.HavePathSource) ? SystemColors.ControlText : Color.Red;
            uxbtnSourcePathRead.ForeColor = (MainConfiguration.ReadPathSource) ? SystemColors.ControlText : Color.Red;
            uxbtnDestinationPathSet.ForeColor = (MainConfiguration.HavePathDestination) ? SystemColors.ControlText : Color.Red;
            uxbtnDestinationPathRead.ForeColor = (MainConfiguration.ReadPathDestination) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferencePathSet.ForeColor = (MainConfiguration.HavePathDifference) ? SystemColors.ControlText : Color.Red;

            uxbtnDifferenceFind.ForeColor = (MainConfiguration.DoesDifferenceFind) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferenceUpload.ForeColor = (MainConfiguration.DoesDifferenceUpload) ? SystemColors.ControlText : Color.Red;
            uxbtnDifferenceDownload.ForeColor = (MainConfiguration.DoesDifferenceDownload) ? SystemColors.ControlText : Color.Red;

            uxlblLogFile.Text = PathFileLog;
            uxbtnLogFileShow.ForeColor = (HaveFileLog) ? Color.Blue : SystemColors.ControlText;
            uxbtnLogFileDelete.ForeColor = (HaveFileLog) ? Color.Blue : SystemColors.ControlText;

            //            Application.DoEvents(); // без этой строки не обновлялся прогресс бар
        }

        private void ShowProgress(int valCur, int valMax)
        {
            uxproInfoProcent.Minimum = 0;
            double curProcent = 0;
            if ((valMax > 0) && (valCur > 0))
            {
                uxproInfoProcent.Maximum = valMax;
                uxproInfoProcent.Value = valCur;
                curProcent = (double)valCur / (double)valMax;
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
                        WriteLogMessage("Открытие конфигурации", "Ошибка при чтении файла конфигурации");
                    }
                }
            }
            return result;
        }

        private bool SaveConfiguration()
        {
            bool result = false;
            DeleteFile(MainConfiguration.PathFileConfiguration); // удалить файл конфигурации, не умеет уменьшать размер
            using (FileStream fs = new FileStream(MainConfiguration.PathFileConfiguration, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, MainConfiguration);      // сохранить в файл текущую конфигурацию
                MainConfiguration.Status = Statuses.Save; // конфигурация сохранена
            }
            FileInfo fileInf = new FileInfo(MainConfiguration.PathFileConfiguration);
            if (fileInf.Exists)
                result = true;
            else
                WriteLogMessage("Сохранить конфигурацию", "Не удалось сохранить конфигурацию в файл: " + MainConfiguration.PathFileConfiguration);
            return result;
        }

        private void ReadOneDirectory(string path, bool isSource, int minBound, int maxBound)
        {
            const string nameProcedure = "Прочитать содержимое папки";

            double lowBound, upBound; // нижняя и верхняя границы для текущей папки для вывода линии прогресса
            double stepProgress; // шаг для вывода линии прогресса
            string[] directories, files;   // список файлов и директорий в обрабатываемой папке
            int countElementes = 0;    // количество файлов и директорий в обрабатываемой папке

            if (Directory.Exists(path) == false)
            {
                WriteLogMessage(nameProcedure, "Не найдена папка: " + path);
            }
            else
            {
                uxlblInfoWorkName.Text = "Обрабатывается папка: " + path;
                uxlblInfoWorkName.Refresh();

                try
                {
                    directories = Directory.GetDirectories(path);
                    files = Directory.GetFiles(path);
                    countElementes = directories.Count(); // количество директорий в обрабатываемой папке
                    countElementes += (files.Count() > 0) ? 1 : 0; // если файлы в папке есть, то добавляем ещё один элемент для прогресса чтения
                    if (countElementes == 0)
                        ShowProgress(maxBound, MaxProgress);  // в обрабатываемой директории нет ни файлов ни папок
                    else
                    {
                        stepProgress = (maxBound - minBound) / countElementes;    // размер одного шага для прогресса
                        lowBound = minBound;
                        upBound = lowBound + stepProgress;
                        ShowProgress((int)Math.Round(upBound), MaxProgress);  // прогресс в один шаг для всех файлов

                        foreach (string file in files)
                        {
                            FileInfo fileInf = new FileInfo(file);     // считать аттрибуты файла
                            if (fileInf.Exists)
                                MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(isSource, fileInf, LengthNodePath));         // добавить к списку новый элемент файловой директории
                        }

                        foreach (string directory in directories)
                        {
                            DirectoryInfo dirInf = new DirectoryInfo(directory);    // считать аттрибуты директории
                            if (dirInf.Exists)
                            {
                                MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(isSource, dirInf, LengthNodePath));         // добавить к списку новый элемент файловой директории
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
                    MainConfiguration.ListElementsFilesTree.Add(new ElementFilesTree(isSource, path, LengthNodePath));
                }

            }
        }

        private bool FillDirectory(string path, bool isSource)
        {
            bool result = false;
            DirectoryInfo dirInf = new DirectoryInfo(path);
            if (dirInf.Exists)
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
                WriteLogMessage("Прочитать папку", "Не найдена папка: " + path);
            return result;
        }

        private void SortElements()
        {
            ElementFilesTree tempElement;
            int currentElement = 0;    // текущая итеррация
            int countElement = MainConfiguration.ListElementsFilesTree.Count - 1;

            uxlblInfoWorkName.Text = "Идёт сортировка списка элементов...";
            for (int i = 0; i < countElement; i++)
            {
                ShowProgress(++currentElement, countElement);
                for (int j = i + 1; j < countElement + 1; j++)
                {
                    int keySort = MainConfiguration.CompareElementsFilesTree(i, j);
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

        private void uxbtnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void uxbtnConfigFileOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Считать файл конфигурации";
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.FileName = "farsync.cfg";
            openFileDialog1.Filter = "config files (*.cfg)|*.cfg|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)    // открыть диалог выбора файла предварительно сохраненной конфигурации
            {
                // файл выбран
                MainConfiguration.PathFileConfiguration = openFileDialog1.FileName;
                ReadConfiguration();
                UpdateWindow();
            }
        }

        private void uxbtnConfigFileSave_Click(object sender, EventArgs e)
        {
            SaveConfiguration();
            UpdateWindow();
        }

        private void uxbtnConfigFileSaveAs_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Сохранить файл конфигурации";
            saveFileDialog1.InitialDirectory = Application.StartupPath;
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

        private void uxbtnLogFileShow_Click(object sender, EventArgs e)
        {
            FileInfo fileInf = new FileInfo(PathFileLog);
            if (fileInf.Exists)
            {
                Process.Start(PathFileLog);
            }
        }

        private void uxbtnLogFileDelete_Click(object sender, EventArgs e)
        {
            bool success = DeleteFile(PathFileLog);
            if (success)
            {
                System.IO.File.Delete(PathFileLog);
                HaveFileLog = false;
                UpdateWindow();
            }
            else
                WriteLogMessage("Удалить файл логов", "Неудача при попытке удаления");
        }

        private void uxbtnSourcePathSet_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = MainConfiguration.PathSource;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (MainConfiguration.PathSource != folderBrowserDialog1.SelectedPath)
                {
                    MainConfiguration.PathSource = folderBrowserDialog1.SelectedPath;
                    MainConfiguration.Status = Statuses.Change;
                    MainConfiguration.DoesDifferenceFind = false;
                    MainConfiguration.DoesDifferenceDownload = false;
                    MainConfiguration.DoesDifferenceUpload = false;
                    MainConfiguration.ReadPathSource = false;
                }
                MainConfiguration.HavePathSource = true;
                UpdateWindow();
            }
        }

        private void uxbtnDestinationPathSet_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = MainConfiguration.PathDestination;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (MainConfiguration.PathDestination != folderBrowserDialog1.SelectedPath)
                {
                    MainConfiguration.PathDestination = folderBrowserDialog1.SelectedPath;
                    MainConfiguration.Status = Statuses.Change;
                    MainConfiguration.DoesDifferenceFind = false;
                    MainConfiguration.DoesDifferenceDownload = false;
                    MainConfiguration.DoesDifferenceUpload = false;
                    MainConfiguration.ReadPathDestination = false;
                }
                MainConfiguration.HavePathDestination = true;
                UpdateWindow();
            }
        }

        private void uxbtnDifferencePathSet_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = MainConfiguration.PathDifference;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (MainConfiguration.PathDifference != folderBrowserDialog1.SelectedPath)
                {
                    MainConfiguration.PathDifference = folderBrowserDialog1.SelectedPath;
                    MainConfiguration.Status = Statuses.Change;
                    MainConfiguration.DoesDifferenceFind = false;
                    MainConfiguration.DoesDifferenceDownload = false;
                    MainConfiguration.DoesDifferenceUpload = false;
                }
                MainConfiguration.HavePathDifference = true;
                UpdateWindow();
            }
        }

        private void uxbtnSourcePathRead_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.HavePathSource == false)
                uxbtnSourcePathSet_Click(sender, e);
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

        private void uxbtnDestinationPathRead_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.HavePathDestination == false)
                uxbtnDestinationPathSet_Click(sender, e);
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

        private void uxbtnExportCSV_Click(object sender, EventArgs e)
        {
            if (MainConfiguration.ListElementsFilesTree.Count > 0)
            {
                saveFileDialog1.Title = "Экспортировать данные в файл";
                saveFileDialog1.InitialDirectory = Application.StartupPath;
                saveFileDialog1.FileName = "farsync.csv";
                saveFileDialog1.Filter = "config files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.Default))
                    {
                        int curItem = 0;
                        sw.WriteLine(MainConfiguration.ListElementsFilesTree[0].GetAsCSV(true));
                        foreach (ElementFilesTree element in MainConfiguration.ListElementsFilesTree)
                        {
                            sw.WriteLine(element.GetAsCSV(false));
                            curItem++;
                            ShowProgress(curItem, MainConfiguration.ListElementsFilesTree.Count);
                        }
                        ShowProgress(0, 0);
                    }
                }
            }
            else
                WriteLogMessage("Экспорт в csv", "Директории ещё не прочитаны");
        }

        private void uxbtnDifferenceFind_Click(object sender, EventArgs e)
        {
            SortElements();
            MainConfiguration.DoesDifferenceFind = true;
            MainConfiguration.Status = Statuses.Change;
            UpdateWindow();
        }
    }
}
