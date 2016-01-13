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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LCDCallOfDuty.ModernWarfare;
using LCDCallOfDuty.Utils;
using LogiFrame;

namespace LCDCallOfDuty.TabPages
{
    public class ServerInfoTabPage : ServerTabPage
    {
        private readonly LCDLine _line;
        private readonly LCDLabel _mapNameLabel;
        private readonly LCDLabel _playerCountLabel;
        private readonly List<LCDLabel> _playerLabels = new List<LCDLabel>();
        private readonly Timer _refreshTimer;
        private readonly LCDMarquee _serverNameMarquee;
        private Player[] _players;
        private int _scrollPos;

        public ServerInfoTabPage(CodApp app) : base(app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            Icon = new LCDLabel
            {
                Text = "S",
                Font = PixelFonts.Title,
                AutoSize = true
            };

            _refreshTimer = new Timer {Interval = 30000};
            _refreshTimer.Tick += (sender, args) => Refresh();

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

            Controls.Add(_serverNameMarquee);
            Controls.Add(_mapNameLabel);
            Controls.Add(_playerCountLabel);
            Controls.Add(_line);
        }

        private void DrawPlayers()
        {
            SuspendLayout();
            RemovePlayers();

            if (_players == null || _players.Length == 0)
            {
                ResumeLayout();
                return;
            }

            var playerCount = _players.Length;
            if (_scrollPos*4 >= playerCount)
                _scrollPos = 0;

            var players = _players.Skip(_scrollPos*4).Take(4).ToArray();
            var n = Math.Min(4, players.Length);
            for (var y = 0; y < n; y++)
            {
                var player = players[y];
                var yp = 17 + y*6;
                var nameLabel = new LCDLabel
                {
                    Text = player.Name.RemoveColors(),
                    Size = new Size(75, 5),
                    Font = PixelFonts.Capitals,
                    Location = new Point(0, yp)
                };
                _playerLabels.Add(nameLabel);
                Controls.Add(nameLabel);

                var scoreLabel = new LCDLabel
                {
                    Text = player.Score.ToString(),
                    TextAlign = ContentAlignment.TopRight,
                    Size = new Size(25, 5),
                    Font = PixelFonts.Capitals,
                    Location = new Point(100, yp)
                };
                _playerLabels.Add(scoreLabel);
                Controls.Add(scoreLabel);

                var pingLabel = new LCDLabel
                {
                    Text = player.Ping.ToString(),
                    TextAlign = ContentAlignment.TopRight,
                    Size = new Size(20, 5),
                    Font = PixelFonts.Capitals,
                    Location = new Point(Width - 20, yp)
                };
                _playerLabels.Add(pingLabel);
                Controls.Add(pingLabel);
            }

            ResumeLayout();
        }

        private void RemovePlayers()
        {
            foreach (var l in _playerLabels)
                Controls.Remove(l);
            _playerLabels.Clear();
        }

        private bool IsBot(string name)
        {
            var startsWithBot = name.StartsWith("bot");
            var numbers = name.Skip(3).All(c => c >= '0' && c <= '9');
            return startsWithBot && numbers;
        }

        private int TryParseInt(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : 0;
        }

        #region Overrides of ServerTabPage

        protected override async void Refresh()
        {
            SuspendLayout();
            if (App.CurrentServer == null)
            {
                RemovePlayers();

                _serverNameMarquee.Text = "No servers were found in your favorites list!";
                _mapNameLabel.Visible = false;
                _playerCountLabel.Visible = false;
            }
            else
            {
                try
                {
                    var result = await App.CurrentServer.Query().TimeoutAfter(TimeSpan.FromMilliseconds(1000));
                    var players = result.Players.Where(p => !IsBot(p.Name)).ToArray();
                    var playerCount = players.Length;
                    var botCount = result.Players.Count() - playerCount;

                    _serverNameMarquee.Text = result.Variables.Get("sv_hostname").RemoveColors();
                    _mapNameLabel.Text = result.Variables.Get("mapname").Substring(3);
                    _mapNameLabel.Visible = true;
                    _playerCountLabel.Text = $"{playerCount}/{TryParseInt(result.Variables["sv_maxclients"]) - botCount}";
                    _playerCountLabel.Visible = true;

                    _players = players.ToArray();
                    DrawPlayers();
                }
                catch
                {
                    RemovePlayers();

                    _serverNameMarquee.Text = "Could not connect to server";
                    _mapNameLabel.Text = $"{App.CurrentServer.Ip}:{App.CurrentServer.Port}";
                    _playerCountLabel.Visible = false;
                }
            }
            ResumeLayout();
        }

        #endregion

        #region Overrides of LCDControl

        protected override void OnVisibleChanged()
        {
            if (_refreshTimer != null)
                _refreshTimer.Enabled = Visible;

            base.OnVisibleChanged();
        }

        protected override void OnButtonPress(ButtonEventArgs e)
        {
            switch (e.Button)
            {
                case 0:
                    _scrollPos--;
                    if (_scrollPos < 0)
                        _scrollPos = _players?.Length/4 - (_players?.Length%4 > 0 ? 0 : 1) ?? 0;
                    DrawPlayers();
                    e.PreventPropagation = true;
                    break;
                case 1:
                    _scrollPos = (_scrollPos + 1)%
                                 (Math.Max(1, (int) Math.Ceiling((float) (_players?.Length ?? 1)/4)));
                    DrawPlayers();
                    e.PreventPropagation = true;
                    break;
            }
            base.OnButtonPress(e);
        }

        #endregion
    }
}