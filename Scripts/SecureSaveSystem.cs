using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

namespace SPACE_UTIL
{
	/// <summary>
	/// Secure save system with tamper detection via checksums and optional encryption
	/// </summary>
	public static class SecureSaveSystem
	{
		// Secret key for checksum - CHANGE THIS TO YOUR OWN VALUE
		// This prevents users from recalculating valid checksums after tampering
		private const string CHECKSUM_SALT = "YourUniqueGameSecretKey_ChangeThis_2024";

		// For encryption (optional) - should be 16 characters for AES-128
		private const string ENCRYPTION_KEY = "MyGame2024Secret"; // CHANGE THIS!

		/// <summary>
		/// Save data with tamper protection (checksum)
		/// </summary>
		public static bool SaveSecure(object dataType, string jsonContent, bool encrypt = false)
		{
			if (string.IsNullOrEmpty(jsonContent))
			{
				Debug.LogError("[SecureSave] Cannot save empty content");
				return false;
			}

			try
			{
				string filePath = GetFilePath(dataType.ToString());
				EnsureDirectoryExists(filePath);

				// Optional encryption
				string dataToSave = encrypt ? Encrypt(jsonContent) : jsonContent;

				// Generate checksum of the original data (before encryption)
				string checksum = GenerateChecksum(jsonContent);

				// Create secure wrapper
				SecureDataWrapper wrapper = new SecureDataWrapper
				{
					version = 1,
					encrypted = encrypt,
					checksum = checksum,
					data = dataToSave,
					timestamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
				};

				// Save as JSON
				string wrapperJson = JsonUtility.ToJson(wrapper, true);
				File.WriteAllText(filePath, wrapperJson);

				// Create backup
				CreateBackup(filePath);

				Debug.Log($"[SecureSave] Saved: {filePath} (Encrypted: {encrypt})".colorTag("lime"));
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[SecureSave] Error saving: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Load data with tamper detection
		/// Returns null if file is tampered or corrupted
		/// </summary>
		public static string LoadSecure(object dataType, out bool wasTampered)
		{
			wasTampered = false;
			string filePath = GetFilePath(dataType.ToString());

			if (!File.Exists(filePath))
			{
				Debug.LogWarning($"[SecureSave] File not found: {filePath}");
				return null;
			}

			try
			{
				// Load wrapper
				string wrapperJson = File.ReadAllText(filePath);
				SecureDataWrapper wrapper = JsonUtility.FromJson<SecureDataWrapper>(wrapperJson);

				if (wrapper == null)
				{
					Debug.LogError("[SecureSave] Failed to parse wrapper structure");
					return TryLoadBackup(dataType, out wasTampered);
				}

				// Decrypt if needed
				string decryptedData = wrapper.encrypted ? Decrypt(wrapper.data) : wrapper.data;

				if (decryptedData == null)
				{
					Debug.LogError("[SecureSave] Decryption failed");
					return TryLoadBackup(dataType, out wasTampered);
				}

				// Verify checksum - THIS IS THE TAMPER DETECTION
				string calculatedChecksum = GenerateChecksum(decryptedData);

				if (calculatedChecksum != wrapper.checksum)
				{
					Debug.LogError("[SecureSave] CHECKSUM MISMATCH - File has been tampered with!");
					Debug.LogError($"Expected: {wrapper.checksum}");
					Debug.LogError($"Calculated: {calculatedChecksum}");

					wasTampered = true;

					// Try to load backup
					return TryLoadBackup(dataType, out wasTampered);
				}

				Debug.Log($"[SecureSave] Loaded and verified: {filePath}".colorTag("lime"));
				return decryptedData;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[SecureSave] Error loading: {ex.Message}");
				return TryLoadBackup(dataType, out wasTampered);
			}
		}

		/// <summary>
		/// Load game data and deserialize to type T
		/// Returns default T instance if file doesn't exist or parsing fails
		/// </summary>
		public static T LoadSecure<T>(object dataType, out bool wasTampered) where T : new()
		{
			string json = LoadSecure(dataType, out wasTampered);
			if (string.IsNullOrEmpty(json) == false)
				return JsonUtility.FromJson<T>(json);
			else
				return new T();
		}


		/// <summary>
		/// Generates SHA256 checksum with salt to prevent users from recalculating valid checksums
		/// </summary>
		private static string GenerateChecksum(string data)
		{
			// Add salt to prevent users from easily generating valid checksums
			string saltedData = CHECKSUM_SALT + data + CHECKSUM_SALT;

			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(saltedData);
				byte[] hash = sha256.ComputeHash(bytes);

				// Convert to hex string
				StringBuilder sb = new StringBuilder();
				foreach (byte b in hash)
				{
					sb.Append(b.ToString("x2"));
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Simple AES encryption (optional - adds extra security layer)
		/// </summary>
		private static string Encrypt(string plainText)
		{
			try
			{
				using (Aes aes = Aes.Create())
				{
					// Use fixed key (in production, use KeyDerivation from password)
					aes.Key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY.PadRight(16).Substring(0, 16));
					aes.IV = new byte[16]; // Fixed IV (not secure for sensitive data, but OK for game saves)

					ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

					using (MemoryStream ms = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
						{
							byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
							cs.Write(plainBytes, 0, plainBytes.Length);
							cs.FlushFinalBlock();

							byte[] encrypted = ms.ToArray();
							return Convert.ToBase64String(encrypted);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[SecureSave] Encryption failed: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Decrypt AES encrypted data
		/// </summary>
		private static string Decrypt(string cipherText)
		{
			try
			{
				using (Aes aes = Aes.Create())
				{
					aes.Key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY.PadRight(16).Substring(0, 16));
					aes.IV = new byte[16];

					ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

					byte[] cipherBytes = Convert.FromBase64String(cipherText);

					using (MemoryStream ms = new MemoryStream(cipherBytes))
					{
						using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
						{
							using (StreamReader sr = new StreamReader(cs))
							{
								return sr.ReadToEnd();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[SecureSave] Decryption failed: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Creates a backup of the save file
		/// </summary>
		private static void CreateBackup(string filePath)
		{
			try
			{
				string backupPath = filePath + ".backup";
				File.Copy(filePath, backupPath, overwrite: true);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[SecureSave] Backup failed: {ex.Message}");
			}
		}

		/// <summary>
		/// Attempts to load backup file if main file fails
		/// </summary>
		private static string TryLoadBackup(object dataType, out bool wasTampered)
		{
			wasTampered = false;
			string backupPath = GetFilePath(dataType.ToString()) + ".backup";

			if (!File.Exists(backupPath))
			{
				Debug.LogWarning("[SecureSave] No backup file available");
				return null;
			}

			Debug.LogWarning("[SecureSave] Attempting to load backup...");

			try
			{
				string wrapperJson = File.ReadAllText(backupPath);
				SecureDataWrapper wrapper = JsonUtility.FromJson<SecureDataWrapper>(wrapperJson);

				if (wrapper == null)
					return null;

				string decryptedData = wrapper.encrypted ? Decrypt(wrapper.data) : wrapper.data;

				if (decryptedData == null)
					return null;

				// Verify backup checksum
				string calculatedChecksum = GenerateChecksum(decryptedData);

				if (calculatedChecksum != wrapper.checksum)
				{
					Debug.LogError("[SecureSave] Backup is also corrupted!");
					wasTampered = true;
					return null;
				}

				Debug.Log("[SecureSave] Backup loaded successfully!".colorTag("lime"));
				return decryptedData;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[SecureSave] Backup load failed: {ex.Message}");
				return null;
			}
		}

		// Helper methods
		private static string GetFilePath(string fileName)
		{
			string saveDir = Path.Combine(Application.dataPath, "LOG", "GameData");
			return Path.Combine(saveDir, $"{fileName}.sav");
		}

		private static void EnsureDirectoryExists(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}
	}

	/// <summary>
	/// Wrapper for secure save data
	/// </summary>
	[Serializable]
	public class SecureDataWrapper
	{
		public int version;           // For future format changes
		public bool encrypted;        // Whether data is encrypted
		public string checksum;       // SHA256 hash for tamper detection
		public string data;           // Actual JSON data (encrypted or plain)
		public string timestamp;      // When it was saved
	}
}