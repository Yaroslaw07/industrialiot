using Industrialiot.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrialiot.Agent
{
    internal static class Menu
    {
        public static void DisplayMenu()
        {
            Console.WriteLine(@"Menu after succefull start
1 - Emergency Stop
2 - Reset Error Status
0 - Stop Program");
        }

        public static async Task Execute(int feature, DeviceManager manager)
        {
            switch (feature)
            {
                case 0:
                    {
                        manager.Stop();
                        Console.WriteLine("Agent is stopped");
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Bad operation");
                    }
                    break;
            }
        }

        internal static int ReadInput()
        {
            var keyPressed = System.Console.ReadKey();
            var isParsed = int.TryParse(keyPressed.KeyChar.ToString(), out int result);
            return isParsed ? result : -1;
        }
    }
}
