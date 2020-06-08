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

        // удалить файл
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
                    int countElementes = directories.Count();    // количество файлов и директорий в обрабатываемой папке
                    countElementes += (files.Count() > 0) ? 1 : 0; // если файлы в папке есть, то добавляем ещё один элемент для прогресса чтения
                    if (countElementes == 0)
                        ShowProgress(maxBound, MaxProgress);  // в обрабатываемой директории нет ни файлов ни папок
                    else
                    {
                        stepProgress = (maxBound - minBound) / countElementes;    // размер одного шага для прогресса
                        lowBound = minBound;
                        upBound = minBound;
                        if( files.Count() > 0)
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
            }
            else
                MessageBox.Show("Директории ещё не прочитаны", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // сортировать список элементов деревьев источника и приёмника
        private void SortElements()
        {
            ElementFilesTree tempElement;
            int countElement = MainConfiguration.ListElementsFilesTree.Count;

            uxlblInfoWorkName.Text = "Идёт сортировка списка элементов...";
            for (int i = 0; i < countElement - 1; i++)
            {
                ShowProgress(i, countElement - 2);
                for (int j = i + 1; j < countElement; j++)
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

        private int Abs(int signValue)
        {
            return signValue > 0 ? signValue : -signValue;
        }

        private void SetAction()
        {
            int result; // результат сравнения
            int countElement = MainConfiguration.ListElementsFilesTree.Count;

            uxlblInfoWorkName.Text = "Идёт установка действий элементов...";
            for (int i = 0; i < countElement - 1; i++)
            {
                ShowProgress(i, countElement - 2);
                if (MainConfiguration.ListElementsFilesTree[i].IsSource == false) // если источника не было, то приемник стирается
                    MainConfiguration.ListElementsFilesTree[i].Act = Operation.Delete;
                else
                {
                    result = MainConfiguration.CompareElementsFilesTree(i, i+1); // сравнить имя с расширением, размер, время изменения, путь (всё кроме источник - приемник)
                    if (Abs(result) < 2)
                    {
                        // если файлы сходятся, то этот файл - источник и он имеет те же реквизиты, что и приемник
                        MainConfiguration.ListElementsFilesTree[i].Act = Operation.Nothing;
                        MainConfiguration.ListElementsFilesTree[i+1].Act = Operation.Nothing;
                        i++;    // пропускаем второй элемент (из приёмника)
                    }
                    else
                    {
                        if (Abs(result) == 2)
                        {
                            // следующий файл имеет базовое совпадение, но расположен в другой директории
                            if (MainConfiguration.ListElementsFilesTree[i + 1].IsSource)
                            {
                                // следующий файл источник, значит он нужен в этой папке и делаем из неё копию
                                MainConfiguration.ListElementsFilesTree[i].Act = Operation.Copy;
                                MainConfiguration.ListElementsFilesTree[i].PathOut = MainConfiguration.ListElementsFilesTree[i+1].Path;
                            }
                            else
                            {
                                // следующий файл приемник, значит он не нужен в этой папке и перемещаем его
                                MainConfiguration.ListElementsFilesTree[i].Act = Operation.Move;
                                MainConfiguration.ListElementsFilesTree[i].PathOut = MainConfiguration.ListElementsFilesTree[i+1].Path;
                            }    
                        }
                        else
                        {
                            // следующий файл не совпадает как базовый, проверяем предыдущий, который может быть сохранившимся приёмником
                            result = (i == 0) ? 10 : MainConfiguration.CompareElementsFilesTree(i, i-1); // сравнить базовое соответствие с предыдущим элементом для всех элементов, кроме первого
                            if ( Abs(result) > 2 )
                                MainConfiguration.ListElementsFilesTree[i].Act = Operation.New; // предыдущий файл тоже не совпадает, значит это новый файл
                            else
                            {
                                // есть базовое соответствие
                                if (MainConfiguration.ListElementsFilesTree[i-1].Act == Operation.Delete )
                                    MainConfiguration.ListElementsFilesTree[i].Act = Operation.Move;
                                else
                                    MainConfiguration.ListElementsFilesTree[i].Act = Operation.Copy; // предыдущий файл совпадает, значит можно скопировать
                                MainConfiguration.ListElementsFilesTree[i].PathOut = MainConfiguration.ListElementsFilesTree[i-1].Path;
                            }
                        }
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
            SetAction();
            MainConfiguration.DoesDifferenceFind = true;
            MainConfiguration.Status = Statuses.Change;
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
                Environment.Exit(0);
        }

        private void UxbtnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Хотите закончить программу?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
                CheckExit( true );
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckExit( false );
        }

    }
}
