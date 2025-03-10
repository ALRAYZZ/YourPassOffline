using PassManagerClient.Models;
using PassManagerClient.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PassManagerClient
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // Link the ViewModel 
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			// Get the selected item
            var listBox = sender as ListBox;
			if (listBox?.SelectedItem is VaultEntry selectedEntry)
			{
                // Create a custom message box with the details
                var message = $"Site: {selectedEntry.Site}\nUsername: {selectedEntry.Username}\nPassword: {selectedEntry.Password}";

				// Show the message box with a "Copy Password" button
                var result = System.Windows.MessageBox.Show(message, "Entry Details", MessageBoxButton.OKCancel);

				if (result == MessageBoxResult.OK)
				{
					Clipboard.SetText(selectedEntry.Password); // Copy the password to the clipboard
                    System.Windows.MessageBox.Show("Password copied to clipboard", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}
	}
}