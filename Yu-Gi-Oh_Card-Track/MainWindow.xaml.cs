using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Card_Library_FM_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDir = Environment.CurrentDirectory;
        private Dictionary<int, CardData> cards = new Dictionary<int, CardData>();
        OpenFileDialog fileDialog = new OpenFileDialog();
        FileSystemWatcher watcher = new FileSystemWatcher();
        string memCardPath;
        Dispatcher dispatcher;
        bool processing = false;
        int totalCards = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadImages();
            this.Cards.ItemsSource = cards;
            fileDialog.FileOk += FileDialog_FileOk;
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void LoadImages()
        {
            foreach (string file in Directory.EnumerateFiles($"{currentDir}/icons", "*.PNG"))
            {
                FileInfo fileinfo = new FileInfo(file);
                int id = 0;
                int.TryParse(fileinfo.Name.Replace(fileinfo.Extension, ""), out id);
                cards.Add(id, new CardData
                {
                    ImageData = fileinfo.FullName,
                    Opacity = 1,
                });
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            fileDialog.ShowDialog();
        }

        private void FileDialog_FileOk(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender == null) return;
            var file = sender as OpenFileDialog;
            txtMemCard.Text = Path.GetFileName(file.FileName);
            memCardPath = file.FileName;
            watcher.Path = Path.GetDirectoryName(file.FileName);
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                         | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            _ = ReadMCAsync();
        }

        private async Task ReadMCAsync()
        {
            if(memCardPath == null) return;
            processing = true;
            await Task.Factory.StartNew(() =>
            {
                FileStream fileStream = new FileStream(memCardPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024,
                    (FileOptions)0x20000000 | FileOptions.WriteThrough & FileOptions.SequentialScan);
                MemoryStream ms = new MemoryStream();
                fileStream.CopyTo(ms);
                List<int>? list = ReadMemCard.Read(ms);
                if (list == null) return;
                totalCards = list.Count;
                foreach (var card in list)
                {
                    cards[card].Opacity = 0.5;
                }
                cards = cards.OrderByDescending(x => x.Value.Opacity).ThenBy(x => x.Key).ToDictionary();
                dispatcher.Invoke(() =>
                {
                    this.Cards.ItemsSource = cards;
                    this.txtCont.Content = $"{totalCards} / 722";
                    //this.Cards.Items.Refresh();
                });
            });
            processing = false;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (source == null) return;
            if (!processing)
            {
                _ = ReadMCAsync();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < cards.Count; i++) {
                cards[i].Opacity = 1;
            }
            cards = cards.OrderBy(x => x.Key).ToDictionary();
            totalCards = 0;
            dispatcher.Invoke(() =>
            {
                this.Cards.ItemsSource = cards;
                this.txtCont.Content = $"{totalCards} / 722";
                //this.Cards.Items.Refresh();
            });
            processing = false;
        }
    }

    class CardData
    {
        public string ImageData { get; set; }
        public double Opacity { get; set; }
    }
}