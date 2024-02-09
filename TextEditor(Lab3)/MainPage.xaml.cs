using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TextEditor_Lab3_
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile currentFile; // Add this field
        private bool isTextChanged = false;
        private bool isFileSaved = false;

        public MainPage()
        {
            this.InitializeComponent();
            ShowOpenOrCreateFileDialog();
        }

        //All Buttons 
        private void OpenFileButton_Click(object sender, RoutedEventArgs e) => OpenFile();
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (isTextChanged)
            {
                AskToSaveFile();
            }

        }
        private void SaveFileButton_Click(object sender, RoutedEventArgs e) => SaveFile();
        private void SaveAsButton_Click(object sender, RoutedEventArgs e) => SaveAsFile();

        //Save, Save As, Open, New
        private void CreateNewFile()
        {
            textBox.Text = "";
            UpdateTitle();
        }
        private async void SaveFile()
        {
            if (!isFileSaved)
            {
                SaveAsFile();
            }
            else if (currentFile != null)
            {
                string textToSave = textBox.Text;
                await Windows.Storage.FileIO.WriteTextAsync(currentFile, textToSave);
                isTextChanged = false;
                ShowMessageDialog("File saved successfully!");
                UpdateTitle(); // Call the UpdateTitle method

            }
        }
        private async void SaveAsFile()
        {
            var fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            var result = await fileSavePicker.PickSaveFileAsync();
            if (result != null)
            {
                string textToSave = textBox.Text;
                await Windows.Storage.FileIO.WriteTextAsync(result, textToSave);
                ShowMessageDialog("File saved successfully!");
                isTextChanged = false;
                isFileSaved = true;
                currentFile = result; // Update currentFile with the new file
                UpdateTitle(); // Call the UpdateTitle method
            }
        }
        private async void OpenFile()
        {

            if (isTextChanged)
            {
                AskToSaveFile();
            }
            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".txt");
            var result = await fileOpenPicker.PickSingleFileAsync();

            if (result != null)
            {
                var text = await Windows.Storage.FileIO.ReadTextAsync(result);
                textBox.Text = text;
                currentFile = result; // Set the currentFile field
                isTextChanged = false;
                UpdateTitle(); // Call the UpdateTitle method


            }
        }

        // Dialogs 
        private async void ShowOpenOrCreateFileDialog()
        {
            var dialog = new MessageDialog("Vill du öppna en befintlig fil eller skapa en ny?", "Välj alternativ");
            dialog.Commands.Add(new UICommand("Öppna fil", new UICommandInvokedHandler((command) => OpenFile())));
            dialog.Commands.Add(new UICommand("Skapa ny", new UICommandInvokedHandler((command) => CreateNewFile())));
            await dialog.ShowAsync();
        }
        private async void AskToSaveFile()
        {
            var dialog = new MessageDialog("Do you want to save this file?");
            dialog.Commands.Add(new UICommand("Yes"));
            dialog.Commands.Add(new UICommand("No"));
            dialog.Commands.Add(new UICommand("Cancel"));

            IUICommand result = await dialog.ShowAsync();

            // Kontrollera vilken knapp användaren har klickat på
            if (result == dialog.Commands[0])
            {
                SaveAsFile();
                ResetTextBox();
            }
            else if (result == dialog.Commands[1])
            {
                ResetTextBox();
            }
        }
        private async void ShowMessageDialog(string message)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            await dialog.ShowAsync();
        }

        //TexBox Changed & Counters
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Hämta texten från TextBox
            string text = textBox.Text;
            isTextChanged = true;

            UpdateTitle();

            // Uppdatera antal ord
            int wordCount = CountWords(text);
            wordCountTextBlock.Text = "Antal Ord : " + wordCount;

            // Uppdatera antal tecken
            int charCount = text.Length;
            charCountTextBlock.Text = "Antal Tecken : " + charCount;

            // Uppdatera antal tecken utan mellanslag
            int charNoSpaceCount = text.Replace(" ", "").Length;
            charNoSpaceCountTextBlock.Text = "Antal Tecken Utan Mellanslag : " + charNoSpaceCount;

            // Uppdatera antal rader
            int lineCount = CountLines(text);
            lineCountTextBlock.Text = "Antal Rader : " + lineCount;
        }
        private int CountWords(string text)
        {
            // Räkna antal ord genom att splitta texten på mellanslag och räkna arrayens längd
            string[] words = text.Split(new[] {' ','\r','\n','\t'}, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
        private int CountLines(string text)
        {
            using (StringReader reader = new StringReader(text))
            {
                int lineCount = 0;
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
                return lineCount;
            }
        }

        //Drag & Drop
        private void textBox_DragOver(object sender, DragEventArgs e)
        {
            // Kontrollera om det finns data som kan droppas och om den är av typen fil
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        private async void textBox_Drop(object sender, DragEventArgs e)
        {
            // Kontrollera om det finns data som kan droppas och om den är av typen fil
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    if (storageFile != null && storageFile.FileType == ".txt")
                    {
                        // Check if Ctrl key is pressed
                        bool isCtrlPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
                        // Check if Shift key is pressed
                        bool isShiftPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

                        if (isCtrlPressed)
                        {
                            // Append text at the end
                            string textToAdd = await FileIO.ReadTextAsync(storageFile);
                            textBox.Text += textToAdd;
                        }
                        else if (isShiftPressed)
                        {
                            // Insert text at cursor position
                            string textToAdd = await FileIO.ReadTextAsync(storageFile);
                            int selectionStart = textBox.SelectionStart;
                            textBox.Text = textBox.Text.Insert(selectionStart, textToAdd);
                            textBox.SelectionStart = selectionStart + textToAdd.Length;
                        }
                        else
                        {
                            if (isTextChanged)
                            {
                                //OpenFileDaD(storageFile);
                            }
                            else
                            {
                               // OpenFileDaD(storageFile);
                            }
                        }

                        // Update currentFile and isFileSaved
                        currentFile = storageFile;
                        isFileSaved = true;

                        // Update title
                        UpdateTitle();
                    }
                    else
                    {
                        // Meddela användaren att endast .txt-filer kan droppas
                        ShowMessageDialog("Endast .txt-filer kan droppas.");
                    }
                }
            }
        }

        //Others
        private void ResetTextBox()
        {
            textBox.Text = "";
            isFileSaved = false;
            isTextChanged = false;
        }
        private void UpdateTitle()
        {
            string fileName;

            if (currentFile != null)
            {
                if (!isTextChanged)
                {
                    fileName = currentFile.Name;
                }

                else
                {
                    fileName = currentFile.Name;
                    fileName = "*" + fileName;
                }
            }
            else
            {
                fileName = "namnlös.txt";

                if (isTextChanged)
                {
                    fileName = "*" + fileName;
                }
            }

            ApplicationView.GetForCurrentView().Title = fileName;
        }

    }
}