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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using LCDCallOfDuty.Utils;
using LogiFrame;

namespace LCDCallOfDuty.TabPages
{
    internal class JoinServerTabPage : ServerTabPage
    {
        private readonly LCDLabel _joinLabel;
        private readonly LCDLine _line;
        private readonly LCDLabel _mapNameLabel;
        private readonly LCDLabel _menuLabel;
        private readonly LCDLabel _nextLabel;
        private readonly LCDLabel _playerCountLabel;
        private readonly LCDLabel _questionLabel;
        private readonly LCDMarquee _serverNameMarquee;
        private readonly LCDLabel _statusLabel;
        private bool _isServerJoinable;

        public JoinServerTabPage(CodApp app) : base(app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            Icon = new LCDLabel
            {
                Text = "J",
                Font = PixelFonts.Title,
                AutoSize = true
            };

            _serverNameMarquee = new LCDMarquee
            {
                Text = "Server name",
                Size = new Size(Width, 7),
                Font = PixelFonts.Title,
                Location = new Point(0, 0),
                Interval = 150,
                BreakSteps = 6
            };
            _mapNameLabel = new LCDLabel
            {
                Text = "Map name",
                Size = new Size(100, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(0, 7)
            };
            _playerCountLabel = new LCDLabel
            {
                Text = "00/00",
                TextAlign = ContentAlignment.TopRight,
                Size = new Size(50, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(Width - 50, 7)
            };
            _line = new LCDLine
            {
                Start = new Point(0, 13),
                End = new Point(Width - 1, 13)
            };
            _questionLabel = new LCDLabel
            {
                Text = "Join this server?",
                Size = new Size(Width, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(0, 17)
            };
            _joinLabel = new LCDLabel
            {
                Text = "Join",
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(Width/4, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(0, Height - 5)
            };
            _statusLabel = new LCDLabel
            {
                Text = "Status",
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(Width/4, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(Width/4, Height - 5)
            };
            _nextLabel = new LCDLabel
            {
                Text = "Next",
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(Width/4, 5),
                Font = PixelFonts.Capitals,
                Location = new Point(Width/2, Height - 5)
            };
            _menuLabel = new LCDLabel
            {
                Text = "Menu",
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(Width/4, 5),
                Font = PixelFonts.Capitals,
                Location = new Point((Width/4)*3, Height - 5)
            };

            Controls.Add(_serverNameMarquee);
            Controls.Add(_mapNameLabel);
            Controls.Add(_playerCountLabel);
            Controls.Add(_line);
            Controls.Add(_questionLabel);
            Controls.Add(_joinLabel);
            Controls.Add(_statusLabel);
            Controls.Add(_nextLabel);
            Controls.Add(_menuLabel);
        }

        #region Overrides of ServerTabPage

        protected override async void Refresh()
        {
            SuspendLayout();
            if (App.CurrentServer == null)
            {
                _serverNameMarquee.Text = "No servers were found in your favorites list!";
                _mapNameLabel.Visible = false;
                _playerCountLabel.Visible = false;
                _questionLabel.Visible = false;
                _joinLabel.Visible = false;
                _statusLabel.Visible = false;
                _nextLabel.Visible = false;
                _menuLabel.Visible = false;
                _isServerJoinable = false;
            }
            else
            {
                try
                {
                    var result = await App.CurrentServer.Query().TimeoutAfter(TimeSpan.FromMilliseconds(1000));
                    _serverNameMarquee.Text = result.Variables.Get("sv_hostname").RemoveColors();
                    _mapNameLabel.Text = result.Variables.Get("mapname").Substring(3);
                    _mapNameLabel.Visible = true;
                    _playerCountLabel.Text = $"{result.Players.Count()}/{result.Variables["sv_maxclients"]}";
                    _playerCountLabel.Visible = true;
                    _questionLabel.Visible = true;
                    _joinLabel.Visible = true;
                    _statusLabel.Visible = true;
                    _nextLabel.Visible = true;
                    _menuLabel.Visible = true;
                    _isServerJoinable = true;
                }
                catch
                {
                    _serverNameMarquee.Text = "Could not connect to server";
                    _mapNameLabel.Text = $"{App.CurrentServer.Ip}:{App.CurrentServer.Port}";
                    _mapNameLabel.Visible = true;
                    _playerCountLabel.Visible = false;
                    _questionLabel.Visible = false;
                    _joinLabel.Visible = false;
                    _statusLabel.Visible = false;
                    _nextLabel.Visible = true;
                    _menuLabel.Visible = false;
                    _isServerJoinable = false;
                }
            }
            ResumeLayout();
        }

        #region Overrides of ServerTabPage

        /// <summary>
        ///     Raises the <see cref="E:ButtonDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LogiFrame.ButtonEventArgs" /> instance containing the event data.</param>
        protected override void OnButtonDown(ButtonEventArgs e)
        {
            if (e.PreventPropagation) return;
            if (_isServerJoinable)
                switch (e.Button)
                {
                    case 0:
                        e.PreventPropagation = true;
                        var serverCachePath = App.Repository?.ServerCache?.Path;
                        if (serverCachePath == null) return;
                        var codPath = Path.GetDirectoryName(serverCachePath);
                        if (codPath == null) return;
                        var path = Path.Combine(codPath, "iw3mp.exe");
                        if (File.Exists(path))
                        {
                            Process.Start(new ProcessStartInfo(path, $"+connect {App.CurrentServer.Ip}:{App.CurrentServer.Port}")
                            {
                                WorkingDirectory = codPath
                            });
                        }
                        App.TabControl.SelectedIndex = 0;
                        return;
                    case 1:
                        e.PreventPropagation = true;
                        App.TabControl.SelectedIndex = 0;
                        return;
                }
            base.OnButtonDown(e);
        }

        #endregion

        #endregion
    }
}