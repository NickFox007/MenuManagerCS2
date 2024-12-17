using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    public class ButtonsConfig
    {
        public PlayerButtons UpButton { get; set; } = PlayerButtons.Forward;
        public PlayerButtons DownButton { get; set; } = PlayerButtons.Back;
        public PlayerButtons LeftButton { get; set; } = PlayerButtons.Moveleft;
        public PlayerButtons RightButton { get; set; } = PlayerButtons.Moveright;
        public PlayerButtons SelectButton { get; set; } = PlayerButtons.Use;
        public PlayerButtons ExitButton { get; set; } = PlayerButtons.Reload;
        public PlayerButtons BackButton { get; set; } = PlayerButtons.Duck;

        public ButtonsConfig()
        {

        }
    }
}
