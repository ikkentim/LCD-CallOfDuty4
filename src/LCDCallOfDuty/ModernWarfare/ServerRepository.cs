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

using System.Collections.Generic;
using System.Linq;
using CodServerCache;

namespace LCDCallOfDuty.ModernWarfare
{
    public class ServerRepository
    {
        public ServerRepository()
        {
            Reload();
        }

        public Cod4ServerCache ServerCache { get; } = Cod4ServerCache.DetectCache();
        public IEnumerable<Server> Servers { get; private set; }

        public void Reload()
        {
            if (ServerCache == null) return;

            Servers = ServerCache.FavoriteServers.Select(s => new Server(s.Ip, s.Port)).ToArray();
        }
    }
}