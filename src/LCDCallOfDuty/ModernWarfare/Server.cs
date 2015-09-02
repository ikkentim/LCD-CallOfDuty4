// LCDCallOfDuty
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LCDCallOfDuty.Utils;

namespace LCDCallOfDuty.ModernWarfare
{
    public class Server
    {
        public Server(IPAddress ip, int port)
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));
            Ip = ip;
            Port = port;
        }

        public IPAddress Ip { get; }
        public int Port { get; }

        public async Task<ServerQueryResults> Query()
        {
            var getStatusCommand = GetCommand("getstatus");
            var statusResponseCommand = GetCommand("statusResponse", 0xa);

            var client = new UdpClient(Ip.ToString(),Port);
            client.Send(getStatusCommand, getStatusCommand.Length);

            var result = await client.ReceiveAsync().TimeoutAfter(new TimeSpan(0, 0, 0, 1));
            var buffer = result.Buffer;

            if (!buffer.Take(statusResponseCommand.Length).SequenceEqual(statusResponseCommand))
                return null;

            var status = Encoding.ASCII.GetString(buffer, statusResponseCommand.Length,
                buffer.Length - statusResponseCommand.Length).Split('\n');

            return
                new ServerQueryResults(
                    status.First().Trim('\\').Split('\\').Group(2).ToDictionary(e => e.First(), e => e.Last()),
                    status.Skip(1)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Split(new[] {' '}, 3))
                        .Where(p => p.Length == 3)
                        .Select(p => new Player(p[2].Trim('"'), TryParseInt(p[1], -1), TryParseInt(p[0], -1)))
                        .ToArray());
        }

        private static int TryParseInt(string s, int defaultValue)
        {
            int result;
            return int.TryParse(s, out result) ? result : defaultValue;
        }

        private static byte[] GetCommand(string command, byte terminator = 0x00)
        {
            return
                Enumerable.Repeat((byte) 0xff, 4).ToArray()
                    .Concat(Encoding.ASCII.GetBytes(command))
                    .Concat(new[] {terminator})
                    .ToArray();
        }
    }
}