using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NesEmulator
{
    public class DebugManager
    {
        public List<string> data = new List<string>();

        public void AddString(string cmd)
        {
            Cpu cpu = Globals.Cpu;
            string flags = Convert.ToByte(cpu.flagB).ToString() + Convert.ToByte(cpu.flagC).ToString() +
                Convert.ToByte(cpu.flagD).ToString() + Convert.ToByte(cpu.flagI).ToString() + Convert.ToByte(cpu.flagN).ToString() +
                Convert.ToByte(cpu.flagV).ToString() + Convert.ToByte(cpu.flagZ).ToString();
            string registers = cpu.regA.ToString() + " " + cpu.regS.ToString() + " " + cpu.regX.ToString() + " " + cpu.regY.ToString() +
                " " + cpu.regPC.ToString();
            data.Add(cmd + " " + flags + " " + registers + " " + cpu.CpuCycle + " " + cpu.cmd_no);
        }

        public void Dump(string fname)
        {
            TextWriter writer = new StreamWriter(fname);
            writer.WriteLine(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                writer.WriteLine(data[i]);
            }
            writer.Close();
        }

        public bool Check(string fname)
        {
            TextReader reader = new StreamReader(fname);
            List<string> data2 = new List<string>();
            int len = Convert.ToInt32(reader.ReadLine());
            for (int i = 0; i < len; i++)
            {
                data2.Add(reader.ReadLine());
            }
            for (int i = 0; i < data2.Count; i++)
            {
                if (data[i] != data2[i])
                    throw new Exception("data check failure at line " + (i + 2) + ", source\"" + data[i] + "\" not equal destination\"" + data2[i] + "\"");
            }
            reader.Close();
            return true;
        }
    }
}
