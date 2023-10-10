using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgram
{
    public class ClientRequest
    {
        public string Command { get; set; } = null!;
        public string Data { get; set; } = null!;
    }
}
