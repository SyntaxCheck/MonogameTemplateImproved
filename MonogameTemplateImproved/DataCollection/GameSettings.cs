using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameSettings
{
    //Database Settings
    public string ServerName { get; set; }
    public string DatabaseName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    //Game Settings
    public int WorldSize { get; set; }
    public int GridCellSize { get; set; }

    public GameSettings()
    {
    }
}