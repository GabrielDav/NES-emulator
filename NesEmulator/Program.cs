using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NesEmulator
{
    class Program
    {


        public static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ResetColor();
            Console.WriteLine(message);
            
            
        }

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Main MainForm = new Main();
            Application.Run(MainForm);
            Console.ReadKey();
        }

    }

    public enum Mirroring {HORIZONTAL, VERTICAL}

    public static class Globals
    {
        public static MemoryMaper Memory;
        public static Cpu Cpu;
        public static Ppu Ppu;
        public static DebugManager debuger;
        public static JoyPad JoyPad;
        public static Storage Storage;
    }
}
