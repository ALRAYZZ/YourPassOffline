using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassManagerClient.Models
{
	public class Vault
	{
		public List<VaultEntry> Entries { get; set; } = new();
	}
}
