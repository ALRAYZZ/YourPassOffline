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
		private MainViewModel _viewModel; // Create a ViewModel instance so we can access its properties and methods
		public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(); // Link the ViewModel 
			DataContext = _viewModel; // Set the DataContext to the ViewModel

			this.MouseDown += MainWindow_MouseDown; // Add a MouseDown event handler
			this.KeyDown += MainWindow_KeyDown; // Add a KeyDown event handler
		}

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			_viewModel.ResetInactivityTimer(); // Reset the inactivity timer when a key is pressed
		}

		private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
		{
			_viewModel.ResetInactivityTimer(); // Reset the inactivity timer when the mouse is clicked
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
                var result = MessageBox.Show(message, "Entry Details", MessageBoxButton.OKCancel);

				if (result == MessageBoxResult.OK)
				{
					Clipboard.SetText(selectedEntry.Password); // Copy the password to the clipboard
                    MessageBox.Show("Password copied to clipboard", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				_viewModel.ResetInactivityTimer(); // Reset the inactivity timer when a password is copied
			}
		}
	}
}