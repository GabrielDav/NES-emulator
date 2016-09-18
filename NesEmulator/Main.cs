using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NesEmulator
{
    public partial class Main : Form
    {
        public static Main MainFrm;
        public bool showInfo = false;

        public Main()
        {
            InitializeComponent();
            MainFrm = this;
            if (!showInfo)
            {
                panel2.Enabled = false;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            
            
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            try
            {
                
                Globals.Memory = new MemoryMaper();
                Globals.Memory.LoadRom("rom1.nes");
                Globals.Ppu = new Ppu(panel1);
                Globals.Storage = new Storage();
   
                Globals.Cpu = new Cpu();
                Globals.JoyPad = new JoyPad();
                Globals.debuger = new DebugManager();
                Globals.Cpu.Run();
                Console.WriteLine("Application closed.");
            }
            catch (Exception ex)
            {

                Program.ShowError(ex.Message + Environment.NewLine + ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Status:");
                Console.ResetColor();
                Console.WriteLine("Flag B: " + Globals.Cpu.flagB);
                Console.WriteLine("Flag C: " + Globals.Cpu.flagC);
                Console.WriteLine("Flag D: " + Globals.Cpu.flagD);
                Console.WriteLine("Flag I: " + Globals.Cpu.flagI);
                Console.WriteLine("Flag N: " + Globals.Cpu.flagN);
                Console.WriteLine("Flag V: " + Globals.Cpu.flagV);
                Console.WriteLine("Flag Z: " + Globals.Cpu.flagZ);
                Console.WriteLine("Register A: " + Globals.Cpu.regA);
                Console.WriteLine("Register S: " + Globals.Cpu.regS);
                Console.WriteLine("Register X: " + Globals.Cpu.regX);
                Console.WriteLine("Register Y: " + Globals.Cpu.regY);
                Console.WriteLine("Register PC: " + Globals.Cpu.regPC);
                Console.WriteLine("Cpu Cycle: " + Globals.Cpu.CpuCycle);
                Console.WriteLine("Command number: " + Globals.Cpu.cmd_no);
                this.Close();
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            //Patikrina ar buvo paspaustas mygtukas priklausantis "pulteliui", jei taip - tai uzregistruoti
            Globals.JoyPad.CheckKey(e.KeyCode, true);
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Patikrina ar buvo atleistas mygtukas priklausantis "pulteliui", jei taip - tai isregistruoti
            Globals.JoyPad.CheckKey(e.KeyCode, false);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainFrm = null;
        }
    }
}
