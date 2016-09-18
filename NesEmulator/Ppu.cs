using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NesEmulator
{
    public unsafe class Ppu
    {
        public int CurrentScanLine;
        public int ScanlinesToCut;
        

        /// <summary>
        /// Zyme nurodanti, ar siuo metu PPU perpiesineja vaizda
        /// </summary>
        public bool vBlank = false;

        
        public int VRAMAddressIncrement = 1;
        //-----Piesimo kintamieji-------
        public byte HScroll = 0;
        public int VScroll = 0;
        public int VBits = 0;
        public int TileY = 0;
        public bool PPUToggle = false;
        public byte ReloadBits2000 = 0;
        public int ScanlineOfVBLANK = 0;
        public bool ExecuteNMIonVBlank = false;
        int* numPtr;
        int[] Buffer;

        //---- PPU atmintis-----
        public ushort VRAMAddress = 0;
        public int ScanlinesPerFrame = 0;
        public ushort VRAMTemp = 0;
        public byte VRAMReadBuffer = 0;
        public byte[] VRAM; //PPU ramai
        public byte[] SPRRAM; //Animaciju stekas(RAM)
        int PatternTableAddressBackground = 0;

        //Ekranas
        Bitmap bmp;
        Control surface;
        Graphics GR;
        BitmapData bmpData;
        int Screen_W = 0;
        int Screen_H = 0;
        int Screen_X = 0;
        int Screen_Y = 0;
        int _Scanlines = 0;
        
        

        /// <summary>
        /// PAL spalvu palete
        /// </summary>
        static int[] Palette = new int[] 
        { 
        0x808080, 0xbb, 0x3700bf, 0x8400a6, 0xbb006a, 0xb7001e, 0xb30000, 0x912600, 
        0x7b2b00, 0x3e00, 0x480d, 0x3c22, 0x2f66, 0, 0x50505, 0x50505, 
        0xc8c8c8, 0x59ff, 0x443cff, 0xb733cc, 0xff33aa, 0xff375e, 0xff371a, 0xd54b00,
        0xc46200, 0x3c7b00, 0x1e8415, 0x9566, 0x84c4, 0x111111, 0x90909, 0x90909, 
        0xffffff, 0x95ff, 0x6f84ff, 0xd56fff, 0xff77cc, 0xff6f99, 0xff7b59, 0xff915f,
        0xffa233, 0xa6bf00, 0x51d96a, 0x4dd5ae, 0xd9ff, 0x666666, 0xd0d0d, 0xd0d0d, 
        0xffffff, 0x84bfff, 0xbbbbff, 0xd0bbff, 0xffbfea, 0xffbfcc, 0xffc4b7, 0xffccae,
        0xffd9a2, 0xcce199, 0xaeeeb7, 0xaaf7ee, 0xb3eeff, 0xdddddd, 0x111111, 0x111111
        };
        /// <summary>
        /// Nurodo ar piesti animacijas
        /// </summary>
        private bool SpriteVisibility;
        /// <summary>
        /// nurodo ar piesti backgrounda
        /// </summary>
        private bool BackgroundVisibility;


        public Ppu(Control control)
        {
            Console.WriteLine("Initializing PPU...");
            _Scanlines = 224;
            ScanlinesToCut = 8;
            Buffer = new int[256 * _Scanlines];
            ScanlinesPerFrame = 261;
      
            ScanlineOfVBLANK = 244;
            VRAM = new byte[0x2000];
            SPRRAM = new byte[0x100];
            surface = control;
             bmp = new Bitmap(256, 224);
            UpdateSize(0, 0, surface.Width + 1, surface.Height + 1);
        }

        public void UpdateSize(int X, int Y, int W, int H)
        {
            Screen_W = W;
            Screen_H = H;
            Screen_X = X;
            Screen_Y = Y;
            GR = surface.CreateGraphics();
            GR.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            GR.Clear(Color.Black);
        }

        public void DrawPixel(int x, int y, int color)
        {

                if (y >= ScanlinesToCut & y < (_Scanlines + ScanlinesToCut))
                {
                    int liner = ((y - ScanlinesToCut) * 256) + x;
                    Buffer[liner] = color;
                }
        }

        /// <summary>
        /// Nupiesia viena linija
        /// </summary>
        /// <returns></returns>
        public bool ScanLine()
        {


            if (CurrentScanLine < 240)
            {
                //Isvaloma linija pries piesima
                for (int i = 0; i < 256; i++)
                {
                    int clr = 0x000000; //Pasirodo backgound spalva nera tik naudojama pradzioja o po to uzpaisoma kita spalva, o is tikro turbut taupant DrawPixel iskvietima,
                    //backgroundo pixeliai paliekami jei imanoma. Spalva RGB(0,0,0) - juoda
                    DrawPixel(i, CurrentScanLine, clr);
                }

                //Piesiamios animacijos ir backgroundas
         
                if (BackgroundVisibility)
                    RenderBackground();
                if (SpriteVisibility)
                    RenderSprites();

                
            }
            

            if (CurrentScanLine == 240) //240 liniju, isijungia PPU refreshas
            {
                vBlank = true;
            }

            CurrentScanLine++; //linija nuskanuota

            if (CurrentScanLine == ScanlinesPerFrame) //260 liniju, piesiamas visas vaizdas, anuliuojamas liniju skaicius
            {
                RenderFrame();
                CurrentScanLine = 0;
            }
         

            return ((CurrentScanLine == ScanlineOfVBLANK) & ExecuteNMIonVBlank);

        }


        /// <summary>
        /// Animaciju piesimas
        /// Animacijos sudetis(4 baitai):
        ///     0 : Y pozicija virsutinio kairiojo kampo - 1
        ///     1 : Animacijos id PPU atmintyje 
        ///     3 : 
        /// </summary>
        void RenderSprites()
        {
            int _LineToDraw = 0;
            //eina per animaciju steka
            //1 sprite'as 4 baitai, isviso yra 64 spritai
            for (int i = 252; i >= 0; i = i - 4)
            {
                int PixelColor = 0;
                byte YCoordinate = (byte)(SPRRAM[i] + 1);
                //Patikrina ar animacija patenka i piesiama linija
                if ((YCoordinate <= CurrentScanLine) &&
                    YCoordinate+16 > CurrentScanLine)
                {
                    //Animaciju piesimas
                    byte SpriteId = SPRRAM[i + 1]; //gauna animacijos ID
                    if ((SPRRAM[i + 2] & 0x80) != 0x80) //tikrina nuo kurios vietos pradeti piesti animacija
                        _LineToDraw = CurrentScanLine - YCoordinate;
                    else
                        _LineToDraw = YCoordinate + 15 - CurrentScanLine;
                    int SpriteOffset = 0;

                    if (_LineToDraw < 8)
                    {
                        //Virsutine puse
                        if ((SpriteId % 2) == 0)
                            SpriteOffset = 0x0000 + (SpriteId) * 16;
                        else
                            SpriteOffset = 0x1000 + (SpriteId - 1) * 16;
                    }
                    else
                    {
                        //Apatine puse
                        _LineToDraw -= 8;
                        if ((SpriteId % 2) == 0)
                            SpriteOffset = 0x0000 + (SpriteId + 1) * 16;
                        else
                            SpriteOffset = 0x1000 + (SpriteId) * 16;
                    }
                    
                    //Nuskaitoma informacija is ROM'o
                    var TileData1 = Globals.Memory.Rom.ReadFromChr((ushort)(SpriteOffset + _LineToDraw));
                    var TileData2 = Globals.Memory.Rom.ReadFromChr((ushort)(SpriteOffset + _LineToDraw + 8));

                    var PaletteUpperBits = (byte)((SPRRAM[i + 2] & 0x3) << 2);
                    
                    //Piesiami animacijos pikseliai
                    for (var j = 0; j < 8; j++)
                    {
                        if ((SPRRAM[i + 2] & 0x40) == 0x40)
                        {
                            PixelColor = PaletteUpperBits + (((TileData2 & (1 << (j))) >> (j)) << 1) +
                                ((TileData1 & (1 << (j))) >> (j));
                        }
                        else
                        {
                            PixelColor = PaletteUpperBits + (((TileData2 & (1 << (7 - j))) >> (7 - j)) << 1) +
                                ((TileData1 & (1 << (7 - j))) >> (7 - j));
                        }
                        if ((PixelColor % 4) == 0) continue;
                        if ((SPRRAM[i + 3] + j) < 256)
                        {
                            DrawPixel((SPRRAM[i + 3]) + j,
                                      CurrentScanLine,
                                      Palette[(0x3f & VRAM[0x1F10 + PixelColor])]);
                        }
                    }


                }
            }
        }

        /// <summary>
        /// Backgroundo piesimas
        /// </summary>
        void RenderBackground()
        {
            int nameTableAddress = 0;
            if (ReloadBits2000 == 0)
                nameTableAddress = 0x2000;
            else if (ReloadBits2000 == 1)
                nameTableAddress = 0x2400;
            else if (ReloadBits2000 == 2)
                nameTableAddress = 0x2800;
            else if (ReloadBits2000 == 3)
                nameTableAddress = 0x2C00;
            for (int vScrollSide = 0; vScrollSide < 2; vScrollSide++)
            {
                int virtualScanline = CurrentScanLine + VScroll;
                if (virtualScanline < 0)
                    virtualScanline = 0;
                int nameTableBase = nameTableAddress;
                int startColumn = 0;
                int endColumn = 0;
                if (vScrollSide == 0)
                {
                    if (virtualScanline >= 240)
                    {
                        if (nameTableAddress == 0x2000)
                            nameTableBase = 0x2800;
                        else if (nameTableAddress == 0x2400)
                            nameTableBase = 0x2C00;
                        else if (nameTableAddress == 0x2800)
                            nameTableBase = 0x2000;
                        else if (nameTableAddress == 0x2C00)
                            nameTableBase = 0x2400;
                        virtualScanline -= 240;
                    }
                    startColumn = HScroll / 8;
                    endColumn = 32;
                }
                else
                {
                    if (virtualScanline >= 240)
                    {
                        if (nameTableAddress == 0x2000)
                            nameTableBase = 0x2C00;
                        else if (nameTableAddress == 0x2400)
                            nameTableBase = 0x2800;
                        else if (nameTableAddress == 0x2800)
                            nameTableBase = 0x2400;
                        else if (nameTableAddress == 0x2C00)
                            nameTableBase = 0x2000;
                        virtualScanline -= 240;
                    }
                    else
                    {
                        if (nameTableAddress == 0x2000)
                            nameTableBase = 0x2400;
                        else if (nameTableAddress == 0x2400)
                            nameTableBase = 0x2000;
                        else if (nameTableAddress == 0x2800)
                            nameTableBase = 0x2C00;
                        else if (nameTableAddress == 0x2C00)
                            nameTableBase = 0x2800;
                    }
                    startColumn = 0;
                    endColumn = (HScroll / 8) + 1;
                }
            

                    if (nameTableBase == 0x2800)
                        nameTableBase = 0x2000;
                    else if (nameTableBase == 0x2C00)
                        nameTableBase = 0x2400;

                for (int currentTileColumn = startColumn; currentTileColumn < endColumn;
                    currentTileColumn++)
                {
                    

                    //gaunamas "detales" ID
                    int tileNumber = VRAM[((nameTableBase - 0x2000) + ((virtualScanline / 8) * 32) + currentTileColumn)];

                    //Pagal ID gaunamas title pradzios adresas
                    int tileDataOffset = PatternTableAddressBackground + (tileNumber * 16);

                    //Uzkrauna "detale" is ROM'o
                    int tiledata1 = Globals.Memory.Rom.ReadFromChr((ushort)(tileDataOffset + (virtualScanline % 8)));
                    int tiledata2 = Globals.Memory.Rom.ReadFromChr((ushort)(tileDataOffset + (virtualScanline % 8) + 8));

                    //Nustatomas paletes high baitas
                    int paletteHighBits = VRAM[((nameTableBase - 0x2000 +
                        0x3c0 + (((virtualScanline / 8) / 4) * 8) + (currentTileColumn / 4)))];
                    paletteHighBits = (byte)(paletteHighBits >> ((4 * (((virtualScanline / 8) % 4) / 2)) +
                        (2 * ((currentTileColumn % 4) / 2))));
                    paletteHighBits = (byte)((paletteHighBits & 0x3) << 2);

                    //Piesiama "detale"
                    int startTilePixel = 0;
                    int endTilePixel = 0;
                    if (vScrollSide == 0)
                    {
                        if (currentTileColumn == startColumn)
                        {
                            startTilePixel = HScroll % 8;
                            endTilePixel = 8;
                        }
                        else
                        {
                            startTilePixel = 0;
                            endTilePixel = 8;
                        }
                    }
                    else
                    {
                        if (currentTileColumn == endColumn)
                        {
                            startTilePixel = 0;
                            endTilePixel = HScroll % 8;
                        }
                        else
                        {
                            startTilePixel = 0;
                            endTilePixel = 8;
                        }
                    }

                    for (int i = startTilePixel; i < endTilePixel; i++)
                    {
                        int pixelColor = paletteHighBits + (((tiledata2 & (1 << (7 - i))) >> (7 - i)) << 1) +
                            ((tiledata1 & (1 << (7 - i))) >> (7 - i));

                        if ((pixelColor % 4) != 0)
                        {
                            if (vScrollSide == 0)
                            {
                                DrawPixel((8 * currentTileColumn) - HScroll + i, CurrentScanLine, Palette[(0x3f & VRAM[0x1f00 + pixelColor])]);
                            }
                            else
                            {
                                if (((8 * currentTileColumn) + (256 - HScroll) + i) < 256)
                                {
                                    DrawPixel((8 * currentTileColumn) + (256 - HScroll) + i, CurrentScanLine, Palette[(0x3f & VRAM[0x1f00 + pixelColor])]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RenderFrame()
        {

                bmpData = bmp.LockBits(new Rectangle(0, 0, 256, _Scanlines),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                //Console.WriteLine(Globals.Cpu.cmd_no);
                numPtr = (int*)bmpData.Scan0;
                for (int i = 0; i < Buffer.Length; i++)
                {
                    numPtr[i] = Buffer[i];
                }
                bmp.UnlockBits(bmpData);
                //Piesimas
                GR.DrawImage(bmp, Screen_X, Screen_Y, Screen_W, Screen_H);
                


        }

        #region PPUregistrai

        /// <summary>
        /// Nustatomi PPU kintamieji
        /// </summary>
        /// <param name="Value"></param>
        public void Write2000(byte Value)
        {
            ExecuteNMIonVBlank = (Value & 0x80) == 0x80;
            VRAMAddressIncrement = ((Value & 0x04) != 0) ? 32 : 1;
            ReloadBits2000 = (byte)(Value & 0x3);
            VRAMTemp = (ushort)(VRAMTemp | ((Value & 0x3) << 10));
        }

        /// <summary>
        /// Nustatomos PPU zymes
        /// </summary>
        /// <param name="Value"></param>
        public void Write2001(byte Value)
        {
  
            SpriteVisibility = (Value & 0x10) != 0;
            BackgroundVisibility = (Value & 0x8) != 0;
  
        }

        /// <summary>
        /// Nuskaito vBlank zyme ir ji atjungia. Taip pat sitas registras atsakingas uz animacijos atvaizdavimo prioriteta,
        /// bei tikrinima ar neperzengta maksimali animaciju per linija riba(8). Pastarosios funkcijos nepadarytos
        /// </summary>
        /// <returns></returns>
        public byte Read2002()
        {
            byte returnedValue = 0;
            // VBlank
            if (vBlank)
                returnedValue = (byte)(returnedValue | 0x80);
            vBlank = false;
            PPUToggle = true;
            return returnedValue;
        }

        /// <summary>
        /// Nustatomi scroll'ai
        /// </summary>
        /// <param name="Value"></param>
        public void Write2005(byte Value)
        {
            if (PPUToggle)
            {
                HScroll = Value;
                VRAMTemp = (ushort)(VRAMTemp | ((Value & 0xF8) >> 3));
            }
            else
            {
                if (CurrentScanLine >= 240)
                {
                    VScroll = Value;
                    if (VScroll > 239)
                        VScroll = 0;
                }
                VRAMTemp = (ushort)(VRAMTemp | ((Value & 0xF8) << 2));
                VRAMTemp = (ushort)(VRAMTemp | ((Value & 0x3) << 12));
            }
            PPUToggle = !PPUToggle;
        }

        /// <summary>
        /// Nurodomas PPU atminties adresas kuri norima nuskaityti
        /// </summary>
        /// <param name="Value"></param>
        public void Write2006(byte Value)
        {
            if (PPUToggle)
            {
                VRAMTemp = (ushort)((VRAMTemp & 0x00FF) | ((Value & 0x3F) << 8));
            }
            else
            {
                VRAMTemp = (ushort)((VRAMTemp & 0x7F00) | Value);
                VRAMAddress = VRAMTemp;
            }
            
            TileY = ((VRAMTemp & 0x7000) >> 12);
            HScroll = (byte)(((Value & 0x1F) << 3));
            if (CurrentScanLine < 240)
            {
                    VScroll = ((VRAMTemp & 0x03E0) >> 5);
                    VScroll = ((VScroll * 8) - CurrentScanLine);
                    VScroll += (TileY + 1);
            }
            else
            {
                VScroll = 0;
            }

            ReloadBits2000 = (byte)((VRAMTemp & 0x0C00) >> 10);
            if (CurrentScanLine >= 240)
                ReloadBits2000 = (byte)((VRAMTemp & 0x0C00) >> 10);
            PPUToggle = !PPUToggle;
        }

        /// <summary>
        /// Iraso arba nuskaito VRAM
        /// </summary>
        /// <param name="Value"></param>
        public void Write2007(byte Value)
        {
            int ADD = VRAMAddress;
            if (ADD >= 0x4000)
                ADD -= 0x4000;
            else if (ADD >= 0x3F20 & ADD < 0x4000)
                ADD -= 0x20;
            if (ADD < 0x2000)
            {
                Globals.Memory.Rom.Write((ushort)ADD, Value);
            }
            else if ((ADD >= 0x2000) && (ADD < 0x3F00))
            {
                if (ADD >= 0x3000)
                    ADD -= 0x1000;
                int vr = (ADD & 0x2C00);
                if (vr == 0x2000)
                    VRAM[ADD - 0x2000] = Value;
                else if (vr == 0x2400)
                    VRAM[ADD - 0x2000] = Value;
                else if (vr == 0x2800)
                {
                    VRAM[ADD - 0x800 - 0x2000] = Value;
                    VRAM[ADD - 0x2000] = Value;
                }
                else if (vr == 0x2C00)
                {
                    VRAM[(ADD - 0x800) - 0x2000] = Value;
                    VRAM[ADD - 0x2000] = Value;
                }

            }
            else if ((ADD >= 0x3F00) && (ADD < 0x3F20))
            {
                VRAM[ADD - 0x2000] = Value;
                if ((ADD & 0x7) == 0)
                {
                    VRAM[(ADD - 0x2000) ^ 0x10] = (byte)(Value & 0x3F);
                }
            }
            VRAMAddress += (ushort)VRAMAddressIncrement;
        }

        /// <summary>
        /// Nurodo uzkrauti Sprite Steka(Ramus) is ROM naujam kadrui
        /// </summary>
        /// <param name="Value"></param>
        public void Write4014(byte Value)
        {
            int i;
            if (CurrentScanLine >= 240)
            {
                for (i = 0; i < 0x100; i++)
                {
                    SPRRAM[i] = Globals.Memory.Read((ushort)((Value * 0x100) + i));
                    Globals.Cpu.CpuCycle += 2;
                }
            }
        }

        #endregion PPUregistrai
    }
}
