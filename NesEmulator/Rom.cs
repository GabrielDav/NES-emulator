using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NesEmulator
{
    public class Rom
    {
        public byte[][] PrgRoms;
        public byte[][] ChrRoms;
        public uint PrgPages;
        public uint ChrPages;
        public uint[] current_prg_rom_page = new uint[8];
        public uint[] current_chr_rom_page = new uint[8];

        //-----Maperio adresu isdestymai-----
        public int mapper4_commandNumber;
        public int mapper4_prgAddressSelect;
        public int mapper4_chrAddressSelect;


        /// <remarks>
        /// ROM nuskaitymas ir uzkrovimas i atminti
        /// </remarks>
        public void Load(string path)
        {
            try
            {
                Console.WriteLine("Loading ROM...");
                Console.WriteLine();
                FileStream reader = File.OpenRead(path);
                Console.WriteLine("Reading headers...");
                byte[] headers = new byte[16];
                reader.Read(headers, 0, 16);
                PrgPages = headers[4];
                Console.WriteLine("PRG pages: " + PrgPages);
                if (PrgPages > 0)
                {
                    Console.WriteLine("Reading PRG pages...");
                    PrgRoms = new byte[PrgPages * 4][];
                    for (int i = 0; i < PrgPages * 4; i++)
                    {
                        PrgRoms[i] = new byte[4096];
                        reader.Read(PrgRoms[i], 0, 4096);
                    }
                }
                ChrPages = headers[5];
                Console.WriteLine("CHR pages: " + ChrPages);
                if (ChrPages > 0)
                {
                    Console.WriteLine("Reading CHR pages...");
                    ChrRoms = new byte[ChrPages * 8][];
                    for (int i = 0; i < ChrPages * 8; i++)
                    {
                        ChrRoms[i] = new byte[1024];
                        reader.Read(ChrRoms[i], 0, 1024);
                    }
                }
                Console.WriteLine();
                Console.WriteLine("ROM loaded successfully");

            }
            catch (Exception e)
            {
                Program.ShowError(e.Message);
            }
        }

        /// <remarks>
        /// Informacijos nuskaitymas is PRG, kadangi procesorius paduoda ROM'o adresus reikia juos sumazinti iki atitinkamo indekso(juk krovimas i kekviena PRG prasidejo nuo 0-nio baito)
        /// </remarks>
        public byte ReadFromPrg(ushort address)
        {
            byte returnvalue;
            if (address < 0x9000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[0]][address - 0x8000];
            }
            else if (address < 0xA000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[1]][address - 0x9000];
            }
            else if (address < 0xB000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[2]][address - 0xA000];
            }
            else if (address < 0xC000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[3]][address - 0xB000];
            }
            else if (address < 0xD000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[4]][address - 0xC000];
            }
            else if (address < 0xE000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[5]][address - 0xD000];
            }
            else if (address < 0xF000)
            {
                returnvalue = PrgRoms[current_prg_rom_page[6]][address - 0xE000];
            }
            else
            {
                returnvalue = PrgRoms[current_prg_rom_page[7]][address - 0xF000];
            }
            return returnvalue;
        }

        /// <summary>
        /// Informacijos nuskaitymas is CHR
        /// </summary>
        /// <param name="address">16bit adresas</param>
        /// <returns></returns>
        public byte ReadFromChr(ushort address)
        {
            byte returnvalue = 0xff;

            if (address < 0x400)
            {
                returnvalue = ChrRoms[current_chr_rom_page[0]][address];
            }
            else if (address < 0x800)
            {
                returnvalue = ChrRoms[current_chr_rom_page[1]][address - 0x400];
            }
            else if (address < 0xC00)
            {
                returnvalue = ChrRoms[current_chr_rom_page[2]][address - 0x800];
            }
            else if (address < 0x1000)
            {

                returnvalue = ChrRoms[current_chr_rom_page[3]][address - 0xC00];
            }
            else if (address < 0x1400)
            {
                returnvalue = ChrRoms[current_chr_rom_page[4]][address - 0x1000];
            }
            else if (address < 0x1800)
            {
                returnvalue = ChrRoms[current_chr_rom_page[5]][address - 0x1400];
            }
            else if (address < 0x1C00)
            {
                returnvalue = ChrRoms[current_chr_rom_page[6]][address - 0x1800];
            }
            else
            {

                returnvalue = ChrRoms[current_chr_rom_page[7]][address - 0x1C00];
            }
            return returnvalue;
        }

        #region PuslapiuPerjungimas

        public void Switch16kPrgRom(int start, int area)
        {
            int i;
            switch (PrgPages)
            {
                case (2): start = (start & 0x7); break;
                case (4): start = (start & 0xf); break;
                case (8): start = (start & 0x1f); break;
                case (16): start = (start & 0x3f); break;
                case (31): start = (start & 0x7f); break;
                case (32): start = (start & 0x7f); break;
                case (64): start = (start & 0xff); break;
                case (128): start = (start & 0x1ff); break;
            }
            for (i = 0; i < 4; i++)
            {
                current_prg_rom_page[4 * area + i] = (uint)(start + i);
            }
        }

        public void Switch8kPrgRom(int start, int area)
        {
            int i;
            switch (PrgPages)
            {
                case (2): start = (start & 0x7); break;
                case (4): start = (start & 0xf); break;
                case (8): start = (start & 0x1f); break;
                case (16): start = (start & 0x3f); break;
                case (32): start = (start & 0x7f); break;
                case (64): start = (start & 0xff); break;
                case (128): start = (start & 0x1ff); break;
            }
            for (i = 0; i < 2; i++)
            {
                current_prg_rom_page[2 * area + i] = (uint)(start + i);
            }
        }
 
        public void Switch8kChrRom(int start)
        {
            int i;
            switch (PrgPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
            }
            for (i = 0; i < 8; i++)
            {
                current_chr_rom_page[i] = (uint)(start + i);
            }
        }

        public void Switch2kChrRom(int start, int area)
        {
            int i;
            switch (ChrPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
            }
            for (i = 0; i < 2; i++)
            {
                current_chr_rom_page[2 * area + i] = (uint)(start + i);
            }
        }

        public void Switch1kChrRom(int start, int area)
        {
            switch (ChrPages)
            {
                case (2): start = (start & 0xf); break;
                case (4): start = (start & 0x1f); break;
                case (8): start = (start & 0x3f); break;
                case (16): start = (start & 0x7f); break;
                case (32): start = (start & 0xff); break;
                case (64): start = (start & 0x1ff); break;
            }
            current_chr_rom_page[area] = (uint)(start);
        }

        #endregion PuslapiuPerjungimas

        /// <summary>
        /// Write operacija neiraso atminties i ROM(read only), bet nurodo kaip turi buti perjungti ROM puslapiai
        /// </summary>
        /// <param name="address">16bit</param>
        /// <param name="data">8bit</param>
        public void Write(ushort address, byte data)
        {
            if (address == 0x8000)
            {
                mapper4_commandNumber = data & 0x7;
                mapper4_prgAddressSelect = data & 0x40;
                mapper4_chrAddressSelect = data & 0x80;
            }
            else if (address == 0x8001)
            {
                if (mapper4_commandNumber == 0)
                {

                    data = (byte)(data - (data % 2));
                    if (mapper4_chrAddressSelect == 0)
                        Switch2kChrRom(data, 0);
                    else
                        Switch2kChrRom(data, 2);
                }
                else if (mapper4_commandNumber == 1)
                {

                    data = (byte)(data - (data % 2));
                    if (mapper4_chrAddressSelect == 0)
                    {
                        Switch2kChrRom(data, 1);
                    }
                    else
                    {
                        Switch2kChrRom(data, 3);
                    }
                }
                else if (mapper4_commandNumber == 2)
                {

                    data = (byte)(data & (ChrPages * 8 - 1));
                    if (mapper4_chrAddressSelect == 0)
                    {
                        Switch1kChrRom(data, 4);
                    }
                    else
                    {
                        Switch1kChrRom(data, 0);
                    }
                }
                else if (mapper4_commandNumber == 3)
                {

                    if (mapper4_chrAddressSelect == 0)
                    {
                        Switch1kChrRom(data, 5);
                    }
                    else
                    {
                        Switch1kChrRom(data, 1);
                    }
                }
                else if (mapper4_commandNumber == 4)
                {

                    if (mapper4_chrAddressSelect == 0)
                    {
                        Switch1kChrRom(data, 6);
                    }
                    else
                    {
                        Switch1kChrRom(data, 2);
                    }
                }
                else if (mapper4_commandNumber == 5)
                {

                    if (mapper4_chrAddressSelect == 0)
                    {
                        Switch1kChrRom(data, 7);
                    }
                    else
                    {
                        Switch1kChrRom(data, 3);
                    }
                }
                else if (mapper4_commandNumber == 6)
                {
                    if (mapper4_prgAddressSelect == 0)
                    {
                        Switch8kPrgRom(data * 2, 0);
                    }
                    else
                    {
                        Switch8kPrgRom(data * 2, 2);
                    }
                }
                else if (mapper4_commandNumber == 7)
                {

                    Switch8kPrgRom(data * 2, 1);
                }

                if (mapper4_prgAddressSelect == 0)
                { 
                    Switch8kPrgRom((int)((PrgPages * 4) - 2) * 2, 2);
                }
                else
                { Switch8kPrgRom((int)((PrgPages * 4) - 2) * 2, 0); }
                Switch8kPrgRom((int)((PrgPages * 4) - 1) * 2, 3);
            }
            else if (address == 0xA000)
            {
                if ((data & 0x1) != 0)
                    throw new Exception("not implemented");
            }
            else if (address == 0xA001)
            {
                throw new Exception("not implemented");
            }
        }
    }
}
