using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NesEmulator
{
    public class MemoryMaper
    {
        /// <remarks>
        /// RAM atmintis, NES konsoleje - 2048 baitu
        /// </remarks>
        public byte[] Ram;
        /// <remarks>
        /// Save RAM atmintis, skirta saugoti informacijai tarp zaidimu, NES konsoleje - 8192 baitu
        /// </remarks>
        public byte[] SRam;
        public Rom Rom;
        public uint[] CurrentPrgPages;
        public uint[] CurrentChrPages;
        byte enableJoyPad = 0;
        int joyData = 0;

        public MemoryMaper()
        {
            Rom = new Rom();
            Ram = new byte[2048];
            SRam = new byte[8192];
        }

        /// <remarks>
        /// ROM nuskaitymas ir uzkrovimas i atminti
        /// </remarks>
        public void LoadRom(string path)
        {
            Rom.Load(path);
            try
            {
                Console.WriteLine("Assigning current Prg pages...");
                Rom.Switch16kPrgRom((int)(Rom.PrgPages - 1) * 4, 1);
                Rom.Switch8kChrRom(0);
            }
            catch (Exception e)
            {
                Program.ShowError(e.Message);
            }
        }  

        /// <summary>
        /// 16bitu nuskaitymas
        /// </summary>
        /// <param name="address">16bitu adresas($0000-$00FF ZeroPage, $0100-$01FF Stack'as, $0200-$07FF RAM, $2000-$2010 PPU, $4000-$4010 APU, $4010-$4016 Controler, $6000-$8000 SRam)</param>
        /// <returns>16bitu nuskaityta informacija</returns>
        public ushort Read16(ushort address)
        {
            byte data_1 = 0;
            byte data_2 = 0;
            if (address < 0x2000) //RAM
            {
                //"atkerpa" viska kas perzengia ramu dydi - 2047(7FF)
                data_1 = Ram[(address & 0x07FF)];
                data_2 = Ram[(address & 0x07FF) + 1];
            }
            else if (address < 0x8000) //SRam
            {
                //SRam - taip pat nuo 0-inio baito
                data_1 = SRam[address - 0x6000];
                data_2 = SRam[(address - 0x6000) + 1];
            }
            else //Prg - skaitymas is ROM'o
            {
                data_1 = Rom.ReadFromPrg(address);
                data_2 = Rom.ReadFromPrg((ushort)(address + 1));
            }
            return (ushort)((data_2 << 8) | data_1); //paslenka data_2 per 8 bitus ir i sekancius suraso data_1 baita
        }

        /// <summary>
        /// 8 bitu nuskaitymas
        /// </summary>
        /// <param name="adress">16bitu adresas($0000-$00FF ZeroPage, $0100-$01FF Stack'as, $0200-$07FF RAM, $2000-$2010 PPU, $2020-$2030 Storage, $4000-$4010 APU, $4010-$4016 Controler, $6000-$8000 SRam)</param>
        /// <returns>8bitu nuskaityta informacija</returns>
        public byte Read(ushort adress)
        {
            if (adress < 0x2000) //RAM
            {
                //"atkerpa" viska kas perzengia ramu dydi - 2047(7FF)
                return Ram[(adress & 0x07FF)];
            }
            if (adress < 0x6000) //IO
            {
                switch (adress)
                {
                    case 0x2002:
                        return Globals.Ppu.Read2002();
                    case 0x2004:
                        throw new Exception("PPU register 2004 not supported");
                    case 0x2007:
                        throw new Exception("PPU register 2007 not supported");
                    case 0x2023:
                        return Globals.Storage.ReadByte();
                    case 0x4017:
                    case 0x4016:
                    case 0x4015:
                        if (adress == 0x4015)
                            Program.ShowError("Can't read APU module. APU is not implemented"); //APU nenaudojamas
                        if (adress == 0x4016)
                        {
                            //Pultelio informacija talpinama i joyData
                            byte v = (byte)(0x40 | (joyData & 1));
                            joyData = (joyData >> 1) | 0x80;
                            return v;
                        }
                        if (adress == 0x4017)
                            return 88; //Zappri's ir antras Joypad'as - nenaudojami, graizinama reiksme pagal nutylejima
                        break;
                }
            }
            else if (adress < 0x8000) //SRam
            {
                return SRam[adress - 0x6000]; //SRam - nuo 0-inio baito
            }
            else //Prg - skaitymas is ROM
            {
                return Rom.ReadFromPrg(adress);
            }
            return 0;
        }

        /// <summary>
        /// 8bitu irasymas i atminti
        /// </summary>
        /// <param name="address">16bitu adresas($0000-$00FF ZeroPage, $0100-$01FF Stack'as, $0200-$07FF RAM, $2000-$2010 PPU, $2020-$2030 Storage, $4000-$4010 APU, $4010-$4016 Controler, $6000-$8000 SRam)</param>
        /// <param name="value">8bitu reiksme</param>
        /// <returns>0xff - operacija ivykdita</returns>
        public byte Write(ushort address, byte value)
        {
            if (address < 0x2000) //nuo $0000 iki $07FF RAM'ai
            {
                Ram[address & 0x07FF] = value; //"atkerpa" adresa perzengianti ramu dydi - 2047(07FF)
            }
            else if (address < 0x4000)
            {
                switch (address)
                {
                    case 0x2000:
                        Globals.Ppu.Write2000(value);
                        break;
                    case 0x2001:
                        Globals.Ppu.Write2001(value);
                        break;
                    case 0x2004:
                        throw new Exception("PPU register 2004 not supported");
                    case 0x2005:
                        Globals.Ppu.Write2005(value);
                        break;
                    case 0x2006:
                        Globals.Ppu.Write2006(value);
                        break;
                    case 0x2007:
                        Globals.Ppu.Write2007(value);
                        break;
                    case 0x2020:
                        Globals.Storage.WriteLowByte(value);
                        break;
                    case 0x2021:
                        Globals.Storage.WriteHighByte(value);
                        break;
                    case 0x2025:
                        Globals.Storage.WriteByte(value);
                        break;
                }
            }
            else if (address < 0x6000)
            {
                //cia turetu buti APU skaitymas 
                
                switch (address)
                {
                    case 0x4014:
                        Globals.Ppu.Write4014(value);
                        break;
                    case 0x4016:
                        if ((enableJoyPad == 1) & ((value & 1) == 0))
                        {
                            joyData = Globals.JoyPad.Read();
                        }
                        enableJoyPad = (byte)(value & 1);
                        break;
                }
            }
            else if (address < 0x8000)
            {
                //SRAM
                throw new Exception("not implemented");//o i SRAM sitas emulatorius nerasys :P
            }
            else
            {
                Rom.Write(address, value);
            }
            return 1; //Operacija ivykdyta
        }

    }
}
