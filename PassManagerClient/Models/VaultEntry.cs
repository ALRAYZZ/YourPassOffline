using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassManagerClient.Models
{
	public class VaultEntry
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Site { get; set; } = "";
		public string Username { get; set; } = "";
		public string Password { get; set; } = "";
	}
}
