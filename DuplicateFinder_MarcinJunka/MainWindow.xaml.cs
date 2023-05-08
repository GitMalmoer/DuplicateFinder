using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using MessageBoxOptions = System.Windows.MessageBoxOptions;


namespace DuplicateFinder_MarcinJunka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.InitialDirectory = Environment.CurrentDirectory;
            DialogResult result = folderBrowserDialog.ShowDialog();


            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                txtFolderPath.Text = path;
            }
            else
            {
                MessageBox.Show("Invalid Selection");
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            lstViewAll.Items.Clear();
            lstViewDuplicates.ItemsSource = null;

            var includeSubfolders = checkIncludeSubfolders.IsChecked ?? false;
            string path = txtFolderPath.Text;


            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Invalid path");
                return;
            }

            // at the beginning checking if includesubfolders is checked 
            var files = Directory.GetFiles(path, "*.*", includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            
            // populating list with all the files. Later, on this list we are doing LINQ querries to find duplicates.
            List<FileRecord> duplicateFiles = new List<FileRecord>();

            foreach (var file in files)
            {
                // POPULATING LISTVIEW ALL FILES
                FileInfo fileInfo = new FileInfo(file);
                lstViewAll.Items.Add(file);

                FileRecord fileRecord = new FileRecord()
                {
                    Name = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    DateCreated = fileInfo.CreationTime,
                    DateModified = fileInfo.LastWriteTime,
                    Path = fileInfo.FullName,
                    Size = fileInfo.Length,
                };
                duplicateFiles.Add(fileRecord);
            }

            lblAllFilesCount.Content = lstViewAll.Items.Count;

            if (checkSize.IsChecked ?? false)
            {
                // FASTER WAY

                //duplicateFiles = duplicateFiles.GroupBy(f => f.Size).Where(g => g.Count() > 1)
                //    .SelectMany(g => g).ToList();

                // WAY USING RECORD AND LOOP
                List<FileRecord> tempList = new List<FileRecord>();

                for (int i = 0; i < duplicateFiles.Count; i++)
                {
                    for (int j = i+1; j < duplicateFiles.Count; j++)
                    {
                        // COMPARING RECORDS!
                        if (duplicateFiles[i].Size == duplicateFiles[j].Size)
                        {
                            if (j == i+1)
                            {
                                tempList.Add(duplicateFiles[i]);
                            }
                            tempList.Add(duplicateFiles[j]);
                        }
                    }
                }

                duplicateFiles = tempList.Distinct().ToList();

            }

            if (checkDateCreated.IsChecked ?? false)
            {
                duplicateFiles = duplicateFiles.GroupBy(f => f.DateCreated.AddSeconds(-new FileInfo(f.Path).CreationTime.Second).AddTicks(-(new FileInfo(f.Path).CreationTime.Ticks % TimeSpan.TicksPerMinute)))
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g).ToList();
            }

            if (checkDateModified.IsChecked ?? false)
            {
                duplicateFiles = duplicateFiles.GroupBy(f => f.DateModified)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g).ToList();
            }

            if (checkFileType.IsChecked ?? false)
            {
                duplicateFiles = duplicateFiles.GroupBy(f => f.Extension)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
                    .ToList();
            }

            lstViewDuplicates.ItemsSource = duplicateFiles.Distinct();
            lblDuplicatesCount.Content = lstViewDuplicates.Items.Count; 

        }

        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstViewDuplicates.SelectedIndex;

            if ( selectedIndex < 0)
            {
                MessageBox.Show("You need to select something from the duplicates list");
                return;
            }

            try
            {
                FileRecord selectedRecord = (FileRecord)lstViewDuplicates.Items[selectedIndex];

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to remove \n {selectedRecord.Name} ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (File.Exists(selectedRecord.Path))
                    {
                        // making sure that file has proper attributes to allow deletion.
                        File.SetAttributes(selectedRecord.Path, FileAttributes.Normal);
                        // deleting actual file
                        File.Delete(selectedRecord.Path);
                    }

                    MessageBox.Show("File has been deleted");
                    ResetAppGUI();


                }
                else
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
        }

        private void ResetAppGUI()
        {
            lstViewAll.Items.Clear();
            lstViewDuplicates.ItemsSource = null;
            lblAllFilesCount.Content = "";
            lblDuplicatesCount.Content = "";
        }
    }
}
