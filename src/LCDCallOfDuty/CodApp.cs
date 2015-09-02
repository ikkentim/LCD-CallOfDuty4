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

using System.Diagnostics;
using System.Linq;
using LCDCallOfDuty.ModernWarfare;
using LCDCallOfDuty.Properties;
using LCDCallOfDuty.TabPages;
using LogiFrame;

namespace LCDCallOfDuty
{
    public class CodApp : LCDApp
    {
        public CodApp() : base("Call of Duty 4: Modern Warfare", true, true, false)
        {
            TabControl.TabPages.Add(new ServerInfoTabPage(this));
            TabControl.TabPages.Add(new JoinServerTabPage(this));
            TabControl.SelectedIndex = 0;
            TabControl.Menu.Button3Task = LCDTabMenuButtonTask.Close;
            Controls.Add(TabControl);
        }

        public ServerRepository Repository { get; } = new ServerRepository();
        public LCDTabControl TabControl { get; } = new LCDTabControl();

        public Server CurrentServer
        {
            get
            {
                var servers = Repository.Servers.ToArray();

                if (Settings.Default.SelectedIndex >= servers.Length || Settings.Default.SelectedIndex < 0)
                {
                    Settings.Default.SelectedIndex = 0;
                    Settings.Default.Save();
                }

                return servers.Length == 0 ? null : servers[Settings.Default.SelectedIndex];
            }
        }

        public void NextServer()
        {
            Repository.Reload();
            var serverCount = Repository.Servers.Count();

            if (serverCount == 0)
                Settings.Default.SelectedIndex = -1;
            else
                Settings.Default.SelectedIndex = (Settings.Default.SelectedIndex + 1)%serverCount;

            Settings.Default.Save();
        }

        public void PreviousServer()
        {
            Repository.Reload();
            var serverCount = Repository.Servers.Count();

            if (serverCount == 0)
                Settings.Default.SelectedIndex = -1;
            else
                Settings.Default.SelectedIndex = Settings.Default.SelectedIndex == 0
                    ? serverCount - 1
                    : Settings.Default.SelectedIndex - 1;

            Settings.Default.Save();
        }

        #region Overrides of LCDContainerControl

        /// <summary>
        ///     Raises the <see cref="E:ButtonDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LogiFrame.ButtonEventArgs" /> instance containing the event data.</param>
        protected override void OnButtonDown(ButtonEventArgs e)
        {
            base.OnButtonDown(e);

            if (e.Button == 3 && !e.PreventPropagation)
            {
                TabControl.ShowMenu();
            }
        }
        
        #endregion
    }
}