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
using LogiFrame;

namespace LCDCallOfDuty.TabPages
{
    public abstract class ServerTabPage : LCDTabPage
    {
        private bool _isRefreshButtonDown;
        private DateTime _refreshButtonDownTime;

        protected ServerTabPage(CodApp app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            App = app;
        }

        public CodApp App { get; }
        protected abstract void Refresh();

        #region Overrides of LCDContainerControl

        protected override void OnButtonDown(ButtonEventArgs e)
        {
            if (e.PreventPropagation) return;
            switch (e.Button)
            {
                case 2:
                    _isRefreshButtonDown = true;
                    _refreshButtonDownTime = DateTime.Now;
                    e.PreventPropagation = true;
                    break;
            }
            base.OnButtonDown(e);
        }

        protected override void OnButtonUp(ButtonEventArgs e)
        {
            if (e.PreventPropagation) return;
            switch (e.Button)
            {
                case 2:
                    if (!_isRefreshButtonDown) break;
                    if (DateTime.Now - _refreshButtonDownTime < TimeSpan.FromMilliseconds(500))
                        App.NextServer();

                    Refresh();
                    _isRefreshButtonDown = false;
                    break;
            }

            base.OnButtonUp(e);
        }

        #region Overrides of LCDControl

        protected override void OnVisibleChanged()
        {
            if (Visible) Refresh();

            base.OnVisibleChanged();
        }

        #endregion

        #endregion
    }
}