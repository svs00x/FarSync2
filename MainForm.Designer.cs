namespace FarSync2
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.uxlblLogFile = new System.Windows.Forms.Label();
            this.uxlblConfigFile = new System.Windows.Forms.Label();
            this.uxbtnConfigFileSaveAs = new System.Windows.Forms.Button();
            this.uxbtnLogFileDelete = new System.Windows.Forms.Button();
            this.uxbtnConfigFileSave = new System.Windows.Forms.Button();
            this.uxbtnLogFileShow = new System.Windows.Forms.Button();
            this.uxbtnConfigFileOpen = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.uxbtnExportCSV = new System.Windows.Forms.Button();
            this.uxbtnDifferenceDownload = new System.Windows.Forms.Button();
            this.uxbtnDifferenceFind = new System.Windows.Forms.Button();
            this.uxlblPathDifference = new System.Windows.Forms.Label();
            this.uxbtnDifferenceUpload = new System.Windows.Forms.Button();
            this.uxlblPathDestination = new System.Windows.Forms.Label();
            this.uxlblPathSource = new System.Windows.Forms.Label();
            this.uxbtnDestinationPathRead = new System.Windows.Forms.Button();
            this.uxbtnSourcePathRead = new System.Windows.Forms.Button();
            this.uxbtnDifferencePathSet = new System.Windows.Forms.Button();
            this.uxbtnDestinationPathSet = new System.Windows.Forms.Button();
            this.uxbtnSourcePathSet = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.uxlblInfoWorkName = new System.Windows.Forms.Label();
            this.uxproInfoProcent = new System.Windows.Forms.ProgressBar();
            this.uxlblInfoProcent = new System.Windows.Forms.Label();
            this.uxbtnExit = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Конфигурация:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Файл логов:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.uxlblLogFile);
            this.groupBox1.Controls.Add(this.uxlblConfigFile);
            this.groupBox1.Controls.Add(this.uxbtnConfigFileSaveAs);
            this.groupBox1.Controls.Add(this.uxbtnLogFileDelete);
            this.groupBox1.Controls.Add(this.uxbtnConfigFileSave);
            this.groupBox1.Controls.Add(this.uxbtnLogFileShow);
            this.groupBox1.Controls.Add(this.uxbtnConfigFileOpen);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 106);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(761, 93);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Пути к файлам:";
            // 
            // uxlblLogFile
            // 
            this.uxlblLogFile.AutoSize = true;
            this.uxlblLogFile.Location = new System.Drawing.Point(341, 58);
            this.uxlblLogFile.Name = "uxlblLogFile";
            this.uxlblLogFile.Size = new System.Drawing.Size(62, 13);
            this.uxlblLogFile.TabIndex = 2;
            this.uxlblLogFile.Text = "Не выбран";
            // 
            // uxlblConfigFile
            // 
            this.uxlblConfigFile.AutoSize = true;
            this.uxlblConfigFile.Location = new System.Drawing.Point(341, 28);
            this.uxlblConfigFile.Name = "uxlblConfigFile";
            this.uxlblConfigFile.Size = new System.Drawing.Size(62, 13);
            this.uxlblConfigFile.TabIndex = 2;
            this.uxlblConfigFile.Text = "Не выбран";
            // 
            // uxbtnConfigFileSaveAs
            // 
            this.uxbtnConfigFileSaveAs.Location = new System.Drawing.Point(259, 25);
            this.uxbtnConfigFileSaveAs.Name = "uxbtnConfigFileSaveAs";
            this.uxbtnConfigFileSaveAs.Size = new System.Drawing.Size(75, 23);
            this.uxbtnConfigFileSaveAs.TabIndex = 1;
            this.uxbtnConfigFileSaveAs.Text = "как...";
            this.uxbtnConfigFileSaveAs.UseVisualStyleBackColor = true;
            this.uxbtnConfigFileSaveAs.Click += new System.EventHandler(this.uxbtnConfigFileSaveAs_Click);
            // 
            // uxbtnLogFileDelete
            // 
            this.uxbtnLogFileDelete.Location = new System.Drawing.Point(178, 54);
            this.uxbtnLogFileDelete.Name = "uxbtnLogFileDelete";
            this.uxbtnLogFileDelete.Size = new System.Drawing.Size(75, 23);
            this.uxbtnLogFileDelete.TabIndex = 1;
            this.uxbtnLogFileDelete.Text = "Удалить";
            this.uxbtnLogFileDelete.UseVisualStyleBackColor = true;
            this.uxbtnLogFileDelete.Click += new System.EventHandler(this.uxbtnLogFileDelete_Click);
            // 
            // uxbtnConfigFileSave
            // 
            this.uxbtnConfigFileSave.Location = new System.Drawing.Point(178, 25);
            this.uxbtnConfigFileSave.Name = "uxbtnConfigFileSave";
            this.uxbtnConfigFileSave.Size = new System.Drawing.Size(75, 23);
            this.uxbtnConfigFileSave.TabIndex = 1;
            this.uxbtnConfigFileSave.Text = "Сохранить";
            this.uxbtnConfigFileSave.UseVisualStyleBackColor = true;
            this.uxbtnConfigFileSave.Click += new System.EventHandler(this.uxbtnConfigFileSave_Click);
            // 
            // uxbtnLogFileShow
            // 
            this.uxbtnLogFileShow.Location = new System.Drawing.Point(97, 53);
            this.uxbtnLogFileShow.Name = "uxbtnLogFileShow";
            this.uxbtnLogFileShow.Size = new System.Drawing.Size(75, 23);
            this.uxbtnLogFileShow.TabIndex = 1;
            this.uxbtnLogFileShow.Text = "Показать";
            this.uxbtnLogFileShow.UseVisualStyleBackColor = true;
            this.uxbtnLogFileShow.Click += new System.EventHandler(this.uxbtnLogFileShow_Click);
            // 
            // uxbtnConfigFileOpen
            // 
            this.uxbtnConfigFileOpen.Location = new System.Drawing.Point(97, 25);
            this.uxbtnConfigFileOpen.Name = "uxbtnConfigFileOpen";
            this.uxbtnConfigFileOpen.Size = new System.Drawing.Size(75, 23);
            this.uxbtnConfigFileOpen.TabIndex = 1;
            this.uxbtnConfigFileOpen.Text = "Открыть";
            this.uxbtnConfigFileOpen.UseVisualStyleBackColor = true;
            this.uxbtnConfigFileOpen.Click += new System.EventHandler(this.uxbtnConfigFileOpen_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.uxbtnExportCSV);
            this.groupBox2.Controls.Add(this.uxbtnDifferenceDownload);
            this.groupBox2.Controls.Add(this.uxbtnDifferenceFind);
            this.groupBox2.Controls.Add(this.uxlblPathDifference);
            this.groupBox2.Controls.Add(this.uxbtnDifferenceUpload);
            this.groupBox2.Controls.Add(this.uxlblPathDestination);
            this.groupBox2.Controls.Add(this.uxlblPathSource);
            this.groupBox2.Controls.Add(this.uxbtnDestinationPathRead);
            this.groupBox2.Controls.Add(this.uxbtnSourcePathRead);
            this.groupBox2.Controls.Add(this.uxbtnDifferencePathSet);
            this.groupBox2.Controls.Add(this.uxbtnDestinationPathSet);
            this.groupBox2.Controls.Add(this.uxbtnSourcePathSet);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(11, 205);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(762, 129);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Пути к директориям:";
            // 
            // uxbtnExportCSV
            // 
            this.uxbtnExportCSV.Location = new System.Drawing.Point(260, 86);
            this.uxbtnExportCSV.Name = "uxbtnExportCSV";
            this.uxbtnExportCSV.Size = new System.Drawing.Size(75, 23);
            this.uxbtnExportCSV.TabIndex = 1;
            this.uxbtnExportCSV.Text = "Экспорт";
            this.uxbtnExportCSV.UseVisualStyleBackColor = true;
            this.uxbtnExportCSV.Click += new System.EventHandler(this.uxbtnExportCSV_Click);
            // 
            // uxbtnDifferenceDownload
            // 
            this.uxbtnDifferenceDownload.Location = new System.Drawing.Point(260, 55);
            this.uxbtnDifferenceDownload.Name = "uxbtnDifferenceDownload";
            this.uxbtnDifferenceDownload.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDifferenceDownload.TabIndex = 1;
            this.uxbtnDifferenceDownload.Text = "Загрузить";
            this.uxbtnDifferenceDownload.UseVisualStyleBackColor = true;
            // 
            // uxbtnDifferenceFind
            // 
            this.uxbtnDifferenceFind.Location = new System.Drawing.Point(179, 86);
            this.uxbtnDifferenceFind.Name = "uxbtnDifferenceFind";
            this.uxbtnDifferenceFind.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDifferenceFind.TabIndex = 1;
            this.uxbtnDifferenceFind.Text = "Обработать";
            this.uxbtnDifferenceFind.UseVisualStyleBackColor = true;
            this.uxbtnDifferenceFind.Click += new System.EventHandler(this.uxbtnDifferenceFind_Click);
            // 
            // uxlblPathDifference
            // 
            this.uxlblPathDifference.AutoSize = true;
            this.uxlblPathDifference.Location = new System.Drawing.Point(342, 91);
            this.uxlblPathDifference.Name = "uxlblPathDifference";
            this.uxlblPathDifference.Size = new System.Drawing.Size(62, 13);
            this.uxlblPathDifference.TabIndex = 2;
            this.uxlblPathDifference.Text = "Не выбран";
            // 
            // uxbtnDifferenceUpload
            // 
            this.uxbtnDifferenceUpload.Location = new System.Drawing.Point(260, 25);
            this.uxbtnDifferenceUpload.Name = "uxbtnDifferenceUpload";
            this.uxbtnDifferenceUpload.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDifferenceUpload.TabIndex = 1;
            this.uxbtnDifferenceUpload.Text = "Выгрузить";
            this.uxbtnDifferenceUpload.UseVisualStyleBackColor = true;
            // 
            // uxlblPathDestination
            // 
            this.uxlblPathDestination.AutoSize = true;
            this.uxlblPathDestination.Location = new System.Drawing.Point(342, 60);
            this.uxlblPathDestination.Name = "uxlblPathDestination";
            this.uxlblPathDestination.Size = new System.Drawing.Size(62, 13);
            this.uxlblPathDestination.TabIndex = 2;
            this.uxlblPathDestination.Text = "Не выбран";
            // 
            // uxlblPathSource
            // 
            this.uxlblPathSource.AutoSize = true;
            this.uxlblPathSource.Location = new System.Drawing.Point(342, 30);
            this.uxlblPathSource.Name = "uxlblPathSource";
            this.uxlblPathSource.Size = new System.Drawing.Size(62, 13);
            this.uxlblPathSource.TabIndex = 2;
            this.uxlblPathSource.Text = "Не выбран";
            // 
            // uxbtnDestinationPathRead
            // 
            this.uxbtnDestinationPathRead.Location = new System.Drawing.Point(179, 55);
            this.uxbtnDestinationPathRead.Name = "uxbtnDestinationPathRead";
            this.uxbtnDestinationPathRead.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDestinationPathRead.TabIndex = 1;
            this.uxbtnDestinationPathRead.Text = "Считать";
            this.uxbtnDestinationPathRead.UseVisualStyleBackColor = true;
            this.uxbtnDestinationPathRead.Click += new System.EventHandler(this.uxbtnDestinationPathRead_Click);
            // 
            // uxbtnSourcePathRead
            // 
            this.uxbtnSourcePathRead.Location = new System.Drawing.Point(179, 25);
            this.uxbtnSourcePathRead.Name = "uxbtnSourcePathRead";
            this.uxbtnSourcePathRead.Size = new System.Drawing.Size(75, 23);
            this.uxbtnSourcePathRead.TabIndex = 1;
            this.uxbtnSourcePathRead.Text = "Считать";
            this.uxbtnSourcePathRead.UseVisualStyleBackColor = true;
            this.uxbtnSourcePathRead.Click += new System.EventHandler(this.uxbtnSourcePathRead_Click);
            // 
            // uxbtnDifferencePathSet
            // 
            this.uxbtnDifferencePathSet.Location = new System.Drawing.Point(98, 86);
            this.uxbtnDifferencePathSet.Name = "uxbtnDifferencePathSet";
            this.uxbtnDifferencePathSet.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDifferencePathSet.TabIndex = 1;
            this.uxbtnDifferencePathSet.Text = "Выбрать";
            this.uxbtnDifferencePathSet.UseVisualStyleBackColor = true;
            this.uxbtnDifferencePathSet.Click += new System.EventHandler(this.uxbtnDifferencePathSet_Click);
            // 
            // uxbtnDestinationPathSet
            // 
            this.uxbtnDestinationPathSet.Location = new System.Drawing.Point(98, 55);
            this.uxbtnDestinationPathSet.Name = "uxbtnDestinationPathSet";
            this.uxbtnDestinationPathSet.Size = new System.Drawing.Size(75, 23);
            this.uxbtnDestinationPathSet.TabIndex = 1;
            this.uxbtnDestinationPathSet.Text = "Выбрать";
            this.uxbtnDestinationPathSet.UseVisualStyleBackColor = true;
            this.uxbtnDestinationPathSet.Click += new System.EventHandler(this.uxbtnDestinationPathSet_Click);
            // 
            // uxbtnSourcePathSet
            // 
            this.uxbtnSourcePathSet.Location = new System.Drawing.Point(98, 25);
            this.uxbtnSourcePathSet.Name = "uxbtnSourcePathSet";
            this.uxbtnSourcePathSet.Size = new System.Drawing.Size(75, 23);
            this.uxbtnSourcePathSet.TabIndex = 1;
            this.uxbtnSourcePathSet.Text = "Выбрать";
            this.uxbtnSourcePathSet.UseVisualStyleBackColor = true;
            this.uxbtnSourcePathSet.Click += new System.EventHandler(this.uxbtnSourcePathSet_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Изменения:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Приемник:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Источник:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.uxlblInfoWorkName);
            this.groupBox4.Controls.Add(this.uxproInfoProcent);
            this.groupBox4.Controls.Add(this.uxlblInfoProcent);
            this.groupBox4.Location = new System.Drawing.Point(12, 13);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(761, 87);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Информационная панель:";
            // 
            // uxlblInfoWorkName
            // 
            this.uxlblInfoWorkName.AutoSize = true;
            this.uxlblInfoWorkName.Location = new System.Drawing.Point(9, 56);
            this.uxlblInfoWorkName.Name = "uxlblInfoWorkName";
            this.uxlblInfoWorkName.Size = new System.Drawing.Size(128, 13);
            this.uxlblInfoWorkName.TabIndex = 2;
            this.uxlblInfoWorkName.Text = "Работа не выполняется";
            // 
            // uxproInfoProcent
            // 
            this.uxproInfoProcent.Location = new System.Drawing.Point(97, 20);
            this.uxproInfoProcent.Name = "uxproInfoProcent";
            this.uxproInfoProcent.Size = new System.Drawing.Size(658, 23);
            this.uxproInfoProcent.TabIndex = 1;
            // 
            // uxlblInfoProcent
            // 
            this.uxlblInfoProcent.AutoSize = true;
            this.uxlblInfoProcent.Location = new System.Drawing.Point(6, 30);
            this.uxlblInfoProcent.Name = "uxlblInfoProcent";
            this.uxlblInfoProcent.Size = new System.Drawing.Size(36, 13);
            this.uxlblInfoProcent.TabIndex = 0;
            this.uxlblInfoProcent.Text = "0.00%";
            // 
            // uxbtnExit
            // 
            this.uxbtnExit.Location = new System.Drawing.Point(604, 353);
            this.uxbtnExit.Name = "uxbtnExit";
            this.uxbtnExit.Size = new System.Drawing.Size(163, 23);
            this.uxbtnExit.TabIndex = 1;
            this.uxbtnExit.Text = "Закончить программу";
            this.uxbtnExit.UseVisualStyleBackColor = true;
            this.uxbtnExit.Click += new System.EventHandler(this.uxbtnExit_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 392);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.uxbtnExit);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "FarSync 2.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button uxbtnConfigFileSaveAs;
        private System.Windows.Forms.Button uxbtnConfigFileSave;
        private System.Windows.Forms.Button uxbtnLogFileShow;
        private System.Windows.Forms.Button uxbtnConfigFileOpen;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button uxbtnDestinationPathRead;
        private System.Windows.Forms.Button uxbtnSourcePathRead;
        private System.Windows.Forms.Button uxbtnDifferencePathSet;
        private System.Windows.Forms.Button uxbtnDestinationPathSet;
        private System.Windows.Forms.Button uxbtnSourcePathSet;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button uxbtnDifferenceDownload;
        private System.Windows.Forms.Button uxbtnDifferenceUpload;
        private System.Windows.Forms.Button uxbtnExportCSV;
        private System.Windows.Forms.Button uxbtnDifferenceFind;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label uxlblInfoWorkName;
        private System.Windows.Forms.ProgressBar uxproInfoProcent;
        private System.Windows.Forms.Label uxlblInfoProcent;
        private System.Windows.Forms.Button uxbtnExit;
        private System.Windows.Forms.Label uxlblLogFile;
        private System.Windows.Forms.Label uxlblConfigFile;
        private System.Windows.Forms.Label uxlblPathDifference;
        private System.Windows.Forms.Label uxlblPathDestination;
        private System.Windows.Forms.Label uxlblPathSource;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button uxbtnLogFileDelete;
    }
}

