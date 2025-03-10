using PassManagerClient.Models;
using PassManagerClient.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace PassManagerClient.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private string _key = "";
		private string _newSite = "";
		private string _newUsername = "";
		private string _newPassword = "";
		private ObservableCollection<VaultEntry> _vaultEntries = new();
		private VaultService _vaultService;

		// Property bound to the Key TextBox
		public string Key
		{
			get => _key;
			set
			{
				_key = value;
				OnPropertyChanged(nameof(Key)); // Tell the UI that the Key property has changed
			}
		}

		public string NewSite
		{
			get => _newSite;
			set
			{
				_newSite = value;
				OnPropertyChanged(nameof(NewSite));
			}
		}

		public string NewUsername
		{
			get => _newUsername;
			set
			{
				_newUsername = value;
				OnPropertyChanged(nameof(NewUsername));
			}
		}

		public string NewPassword
		{
			get => _newPassword;
			set
			{
				_newPassword = value;
				OnPropertyChanged(nameof(NewPassword));
			}
		}

		public ObservableCollection<VaultEntry> VaultEntries
		{
			get => _vaultEntries;
			set
			{
				_vaultEntries = value;
				OnPropertyChanged(nameof(VaultEntries));
			}
		}

		// Command bound to the OpenVault button, we need matching XAML for this to work Command="{Binding OpenVaultCommand}"
		public ICommand OpenVaultCommand { get; }
		public ICommand SavePasswordCommand { get; }

		public MainViewModel()
		{
			OpenVaultCommand = new RelayCommand(OpenVault); // Set the OpenVault method as the command's action
			SavePasswordCommand = new RelayCommand(SavePassword); // Set the SavePassword method as the command's action
		}

		private void OpenVault()
		{
			if (string.IsNullOrEmpty(Key))
			{
				// Show a message box if the key is empty
				System.Windows.MessageBox.Show("Please enter a key", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				return;
			}

			// Load the vault entries
			_vaultService = new VaultService(Key);
			Vault vault = _vaultService.LoadVault();
			VaultEntries = new ObservableCollection<VaultEntry>(vault.Entries);
			System.Windows.MessageBox.Show("Vault opened successfully", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
		}

		private void SavePassword()
		{
			if (string.IsNullOrEmpty(Key) || _vaultService == null)
			{
				System.Windows.MessageBox.Show("Please open a vault first", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				return;
			}

			if (string.IsNullOrEmpty(NewSite) || string.IsNullOrEmpty(NewUsername) || string.IsNullOrEmpty(NewPassword))
			{
				System.Windows.MessageBox.Show("Please fill in all fields", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				return;
			}

			var newEntry = new VaultEntry()
			{
				Site = NewSite,
				Username = NewUsername,
				Password = NewPassword
			};

			VaultEntries.Add(newEntry);
			_vaultService.SaveVault(new Vault { Entries = VaultEntries.ToList() });

			NewSite = "";
			NewUsername = "";
			NewPassword = "";
			System.Windows.MessageBox.Show("Password saved successfully", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
		}
		// INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}

	// Simple command implementation
	public class RelayCommand : ICommand
	{
		private readonly Action _execute; // What to do when the command is executed
		public RelayCommand(Action execute) => _execute = execute;
		public event EventHandler? CanExecuteChanged; // Not used in this example
		public bool CanExecute(object? parameter) => true; // Always clickable for now
		public void Execute(object? parameter) => _execute(); // Execute the action
	}
}
