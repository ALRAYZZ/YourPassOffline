using PassManagerClient.Models;
using PassManagerClient.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PassManagerClient.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private string _key = "";
		private string _newSite = "";
		private string _newUsername = "";
		private string _newPassword = "";
		private bool _isVaultOpen = false;
		private ObservableCollection<VaultEntry> _vaultEntries = new();
		private VaultService _vaultService;
		private DispatcherTimer _inactivityTimer;

		// Property bound to the Key TextBox
		public string Key
		{
			get => _key;
			set
			{
				_key = value;
				OnPropertyChanged(nameof(Key)); // Tell the UI that the Key property has changed
				ResetInactivityTimer(); // Reset the inactivity timer when the key changes
			}
		}
		public string NewSite
		{
			get => _newSite;
			set
			{
				_newSite = value;
				OnPropertyChanged(nameof(NewSite));
				ResetInactivityTimer(); // Reset the inactivity timer when the key changes
			}
		}
		public string NewUsername
		{
			get => _newUsername;
			set
			{
				_newUsername = value;
				OnPropertyChanged(nameof(NewUsername));
				ResetInactivityTimer(); // Reset the inactivity timer when the key changes
			}
		}
		public string NewPassword
		{
			get => _newPassword;
			set
			{
				_newPassword = value;
				OnPropertyChanged(nameof(NewPassword));
				ResetInactivityTimer(); // Reset the inactivity timer when the key changes
			}
		}
		public bool IsVaultOpen
		{
			get => _isVaultOpen;
			set
			{
				_isVaultOpen = value;
				OnPropertyChanged(nameof(IsVaultOpen));
				if (_isVaultOpen)
				{
					StartInactivityTimer(); // Start the inactivity timer when the vault is open
				}
				else
				{
					StopInactivityTimer(); // Stop the inactivity timer when the vault is closed
				}
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
		public ICommand CloseVaultCommand { get; }
		public ICommand SavePasswordCommand { get; }

		public MainViewModel()
		{
			OpenVaultCommand = new RelayCommand(OpenVault); // Now takes a parameter
			SavePasswordCommand = new RelayCommand(_ => SavePassword()); // No parameter needed
			CloseVaultCommand = new RelayCommand(_ => CloseVault()); // No parameter needed

			_inactivityTimer = new DispatcherTimer()
			{
				Interval = TimeSpan.FromMinutes(5)
			};
			_inactivityTimer.Tick += InactivityTimer_Tick;
		}

		public void OpenVault(object? parameter)
		{
			if (parameter is PasswordBox passwordBox)
			{
				Key = passwordBox.Password; // Grab the password from the PasswordBox
			}

			if (string.IsNullOrEmpty(Key))
			{
				// Show a message box if the key is empty
				System.Windows.MessageBox.Show("Please enter a key", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				return;
			}

			// Load the vault entries
			_vaultService = new VaultService(Key);

			if (!File.Exists(_vaultService.FilePath))
			{
				// First time opening the vault: ask the user if they want to create a new vault
				var result = System.Windows.MessageBox.Show("Vault does not exist, would you like to create a new vault?", "Vault does not exist", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
				if (result == System.Windows.MessageBoxResult.Yes)
				{
					_vaultService.SaveVault(new Vault());
					System.Windows.MessageBox.Show("Vault created successfully", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
					IsVaultOpen = true; // Enable the UI
				}
				else
				{
					return; // User chose not to create a new vault
				}
			}
			else
			{
				// Load the vault entries
				try
				{
					Vault vault = _vaultService.LoadVault();
					VaultEntries = new ObservableCollection<VaultEntry>(vault.Entries);
					IsVaultOpen = true;
					System.Windows.MessageBox.Show("Vault opened successfully", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
				}
				catch (CryptographicException)
				{
					System.Windows.MessageBox.Show("Invalid decryption key. Please check your decryption key.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				}
			}
		}
		private void CloseVault()
		{
			// Reset the key and vault entries
			Key = "";
			VaultEntries.Clear();
			IsVaultOpen = false;

			System.Windows.MessageBox.Show("Vault closed successfully", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
			ResetInactivityTimer(); // Reset the inactivity timer when a password is saved
		}
		

		// Inactivity timer implementation
		private void InactivityTimer_Tick(object? sender, EventArgs e)
		{
			if (IsVaultOpen)
			{
				CloseVault();
				MessageBox.Show("Vault closed due to inactivity", "Vault closed", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void StartInactivityTimer()
		{
			_inactivityTimer.Start();
		}

		private void StopInactivityTimer()
		{
			_inactivityTimer.Stop();
		}

		public void ResetInactivityTimer()
		{
			if (IsVaultOpen)
			{
				_inactivityTimer.Stop();
				_inactivityTimer.Start();
			}
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
		private readonly Action<object?> _execute; // Changed to take a parameter
		public RelayCommand(Action<object?> execute) => _execute = execute;
		public event EventHandler? CanExecuteChanged;
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => _execute(parameter);
	}
}
