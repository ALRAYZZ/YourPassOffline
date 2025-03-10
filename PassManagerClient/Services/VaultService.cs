using PassManagerClient.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PassManagerClient.Services
{
	public class VaultService
	{
		private readonly byte[] _key;
		private readonly string _filePath;
		public string FilePath => _filePath;

		public VaultService(string key)
		{
			_filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PassManagerClientOffline","vault.pmco");
			string? directory = Path.GetDirectoryName(_filePath);
			if (directory != null)
			{
				Directory.CreateDirectory(directory);
			}
			_key = DeriveKey(key);
		}

		public Vault LoadVault()
		{
			if (!File.Exists(_filePath))
			{
				return new Vault();
			}

			try
			{
				byte[] encryptedData = File.ReadAllBytes(_filePath); // Read the encrypted data
				string json = DecryptData(encryptedData); // Decrypt the data
				return JsonSerializer.Deserialize<Vault>(json) ?? new Vault(); // Deserialize the decrypted data
			}
			catch
			{
				// If anything goes wrong, return an empty vault
				throw new CryptographicException("Failed to decrypt the vault. Please check you decryption key.");
			}
		}
		public void SaveVault(Vault vault)
		{
			var json = JsonSerializer.Serialize(vault); // Serialize the vault
			byte[] encryptedData = EncryptData(json); // Encrypt the data
			File.WriteAllBytes(_filePath, encryptedData); // Write the encrypted data to the file
		}
		private byte[] DeriveKey(string userKey)
		{
			// Use PBKDF2 with a fixed salt for simplicity (could store salt in file later)
			byte[] salt = Encoding.UTF8.GetBytes("SecureVaultSalt"); // Create a salt 16 bytes long
			using var pbkdf2 = new Rfc2898DeriveBytes(userKey, salt, 10000, HashAlgorithmName.SHA256);
			return pbkdf2.GetBytes(32);
		}
		private byte[] EncryptData(string plainText)
		{
			using var aes = Aes.Create(); // Create an AES enctyption provider instance
			aes.Key = _key;
			aes.GenerateIV(); // Generate a random initialization vectorbyte
			byte[] iv = aes.IV;

			using var encryptor = aes.CreateEncryptor(); // Create an encryptor
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
			byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

			byte[] result = new byte[iv.Length + cipherBytes.Length];
			Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
			Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);

			return result;
		}
		private string DecryptData(byte[] encryptedData)
		{
			if (encryptedData.Length < 16)
			{
				throw new CryptographicException("Encrypted data too short to contain IV");
			}
			byte[] iv = new byte[16];
			byte[] cipherBytes = new byte[encryptedData.Length - iv.Length];
			Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
			Buffer.BlockCopy(encryptedData, iv.Length, cipherBytes, 0, cipherBytes.Length);

			using var aes = Aes.Create();
			aes.Key = _key;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor();
			byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
			return Encoding.UTF8.GetString(decryptedBytes);
		}
	}
}
