using System;
using System.Threading;
using System.Windows.Forms;
// ReSharper disable InconsistentNaming
namespace NesEmulator
{
    /// <summary>
    /// Centrinio procesoriaus klase, atsakinga uz visu kitu moduliu darba
    /// </summary>
    public class Cpu
    {
        #region Registrai
        /// <summary>
        /// Accumulator registras, skirtas loginems operacijoms, 8bit
        /// </summary>
        public byte regA;
        /// <summary>
        /// Stack Pointer registras, laiko 8bit laisvo steko elemento adresa
        /// </summary>
        public byte regS;
        /// <summary>
        /// X registras skirtais informacijai laikyti, 8bit
        /// </summary>
        public byte regX;
        /// <summary>
        /// Y registras skirtais informacijai laikyti, 8bit
        /// </summary>
        public byte regY;
        /// <summary>
        /// Program Counter registras - aktyvus CPU adresas
        /// </summary>
        public ushort regPC;
        #endregion Registrai

        public int cmd_no; //emuliatoriaus testavimui skirtas kintamasis

        #region Zymes
        /// <summary>
        /// Break Command - nurodo ar buvo ivykes break'as
        /// </summary>
        public bool flagB;
        /// <summary>
        /// Carry Flag - nurodo ar ivyko overflow'as
        /// </summary>
        public bool flagC;
        /// <summary>
        /// Decimal Mode - nurodo ar ijungtas Binary Coded Decimal
        /// </summary>
        public bool flagD;

        /// <summary>
        /// Interrupt Disable - nurodo ar reaguoti i interuptus
        /// </summary>
        public bool flagI;
        /// <summary>
        /// Negative Flag - nurodo ar ivyko operacija kurios metu gautas neigiamas rezultatas
        /// </summary>
        public bool flagN;
        /// <summary>
        /// Overflow Flag - arimtetiniu operaciju overflow'as
        /// </summary>
        public bool flagV;
        /// <summary>
        /// Zero Flag - nurodo ar paskutines operacijos rezultatas buvo 0
        /// </summary>
        public bool flagZ;
        #endregion Zymes

        //CPU ciklai
        public int CpuCycle;

        public bool Paused; //Ar Cpu sustabdytas

        public Cpu()
        {
            Console.WriteLine("Initializing CPU...");
            regA = 0;
            regS = 0xFF;
            regX = 0;
            regY = 0;
            flagB = false;
            flagC = false;
            flagD = false;
            flagI = true;
            flagN = false;
            flagV = false;
            flagZ = false;
            Paused = false;
            regPC = Globals.Memory.Read16(0xFFFC);
        }

        #region Adresavimas
        //Adresavimo modai
        //Zero Page - budingas Motorola 6800 ir MOS 6502 procesoriams, naudojamas 8bit ilgio adresu nuskaityt is atminties
        //Absolute - pats paprasciausia nuskaitymas is atminties ties nurodytu adresu
        //Indirect - adreso adreso reiksmes nuskaitymas, t.y. rodykle

        /// <summary>
        /// Nuskaito is atminties $0000 - $00FF(ZeroPage)
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        byte ZeroPage(byte arg1)
        { 
            return Globals.Memory.Read(arg1);
        }

        /// <summary>
        /// Prided Registro X reiksme ir nuskaito is atminties ties ZeroPage
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        byte ZeroPageX(byte arg1)
        { 
            return Globals.Memory.Read((ushort)(0xFF & (arg1 + regX))); //pridejus reiksme neturi virsyti ZeroPage ribos 0xFF
        }

        /// <summary>
        /// Prided Registro Y reiksme ir nuskaito is atminties ties ZeroPage
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        byte ZeroPageY(ushort arg1)
        {
            return Globals.Memory.Read((ushort)(0xFF & (arg1 + regY))); //pridejus reiksme neturi virsyti ZeroPage ribos 0xFF
        }

        /// <summary>
        /// Pagal abu argumentus sudaro 16bit adresa ir ji nuskaito
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        byte Absolute(byte arg1, byte arg2)
        { 
            return Globals.Memory.Read(MakeAddress(arg1, arg2)); 
        }

        /// <summary>
        /// Absoliutinio adreso nuskaitymas pridedant Registra X prie adreso, su galimybe patikrinti ar Registro X pridejimas keicia pirmaji baita
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="checkPage"></param>
        /// <returns></returns>
        byte AbsoluteX(byte arg1, byte arg2, bool checkPage)
        {
            if (checkPage) //ar reikalingas patikrinimas
            {
                if ((MakeAddress(arg1, arg2) & 0xFF00) != ((MakeAddress(arg1, arg2) + regX) & 0xFF00))
                {
                    CpuCycle += 1; //Jei primasis baitas nesutampa reiks papildomo zingsnio
                }
            }
            return Globals.Memory.Read((ushort)(MakeAddress(arg1, arg2) + regX));
        }

        /// <summary>
        /// Absoliutinio adreso nuskaitymas pridedant Registra Y prie adreso, su galimybe patikrinti ar Registro Y pridejimas keicia pirmaji baita
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="checkPage"></param>
        /// <returns></returns>
        byte AbsoluteY(byte arg1, byte arg2, bool checkPage)
        {
            if (checkPage)
            {
                if ((MakeAddress(arg1, arg2) & 0xFF00) != ((MakeAddress(arg1, arg2) + regY) & 0xFF00))
                {
                    CpuCycle += 1; //Jei primasis baitas nesutampa reiks papildomo zingsnio
                }
            }
            return Globals.Memory.Read((ushort)(MakeAddress(arg1, arg2) + regY));
        }

        /// <summary>
        /// Nuskaito informacijas is adreso esancio atmintyje, pastarojo reiksme gaunama argumenta pridejus prie Registro X
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        byte IndirectX(byte arg1)
        {
            return Globals.Memory.Read(Globals.Memory.Read16((ushort)(0xff & (arg1 + regX)))); 
        }

        /// <summary>
        /// Nuskaito informacijas is adreso esancio atmintyje, pastarojo reiksme gaunama argumenta pridejus prie Registro Y, su galimybe patikrinti ar Registro Y pridejimas keicia pirmaji baita
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="checkPage"></param>
        /// <returns></returns>
        byte IndirectY(byte arg1, bool checkPage)
        {
            if (checkPage)
            {
                if ((Globals.Memory.Read16(arg1) & 0xFF00) != ((Globals.Memory.Read16(arg1) + regY) & 0xFF00))
                {
                    CpuCycle += 1; //reiks papildomo zingsnio
                }
            }
            return Globals.Memory.Read((ushort)(Globals.Memory.Read16(arg1) + regY));
        }

        /// <summary>
        /// Irasomas nurodyta reiksme(data) i nurodyta adresa(arg1), ZeroPage
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte ZeroPageWrite(byte arg1, byte data)
        {
            return Globals.Memory.Write(arg1, data);
        }

        /// <summary>
        /// Irasomas nurodyta reiksme(data) i nurodyta adresa(arg1) sudeta su Registro X reiksme, ZeroPage
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte ZeroPageXWrite(byte arg1, byte data)
        {
            return Globals.Memory.Write((ushort)(0xff & (arg1 + regX)), data);
        }

        

        /// <summary>
        /// Irasoma reiksme i adresa gaunama sujungus prima ir antra argumentus
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte AbsoluteWrite(byte arg1, byte arg2, byte data)
        {
            return Globals.Memory.Write(MakeAddress(arg1, arg2), data);
        }

        /// <summary>
        /// Irasoma reiksme i adresa gaunama sujungus prima ir antra argumentus ir prie ju pridejus Registro X reiksme
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        byte AbsoluteXWrite(byte arg1, byte arg2, byte data)
        {
            return Globals.Memory.Write((ushort)(MakeAddress(arg1, arg2) + regX), data);
        }

        /// <summary>
        /// Irasoma reiksme i adresa gaunama sujungus prima ir antra argumentus ir prie ju pridejus Registro Y reiksme
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        void AbsoluteYWrite(byte arg1, byte arg2, byte data)
        {
            Globals.Memory.Write((ushort)(MakeAddress(arg1, arg2) + regY), data);
        }

        /// <summary>
        /// Irsamo i rodykles reiksme, pradinis adresas gaunamas prie pirmo argumento pridejus Registro X reiksme
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        void IndirectXWrite(byte arg1, byte data)
        {
            Globals.Memory.Write(Globals.Memory.Read16((ushort)(0xff & (arg1 + regX))), data);
        }

        /// <summary>
        /// Irsamo i rodykles reiksme, pradinis adresas gaunamas prie pirmo argumento pridejus Registro Y reiksme
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        void IndirectYWrite(byte arg1, byte data)
        {
                Globals.Memory.Write((ushort)(Globals.Memory.Read16(arg1) + regY), data);
        }

        /// <summary>
        /// Is dvieju argumentu "suklijuojamas" adresas
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        ushort MakeAddress(byte arg1, byte arg2)
        {
            ushort address = (ushort)((arg2 << 8) | arg1); //pirmus 8bitus sudaro arg2 paslinktas i desine, likusius arg1
            return address;
        }

        #endregion Adresavimas

        #region Operacijos


        /// <summary>
        /// Break interuptas, paprastai naudojamas debuginimui kurimo metu
        /// </summary>
        void BRK()
        {
            regPC++;
            Push16((ushort)(regPC + 1));
            flagB = true;
            PushFlagsStatus();
            flagI = true;
            regPC = Globals.Memory.Read16(0xFFFE);
            CpuCycle += 7;
        }

        /// <summary>
        /// Logine operacija OR, palygina bitus is A registro su bitais is nurodytos atminties, rezultatas irasomas i registra A
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void ORA(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            switch (opCode)
            {
                case 0x09: result = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case 0x05: result = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case 0x15: result = ZeroPageX(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case 0x0D: result = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case 0x1D: result = AbsoluteX(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case 0x19: result = AbsoluteY(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case 0x01: result = IndirectX(arg1);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case 0x11: result = IndirectY(arg1, false);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
            }
            regA = (byte)(regA | result); //logine opeacija OR
            flagZ = (regA == 0); //ar Registro A reiksme 0
            flagN = ((regA & 0x80) == 0x80); //patikrina ar pirmas bitas yra 1, jei taip - atsakymas neigiamas
        }

        /// <summary>
        /// Logine operacija XOR(grieztas arba), palygina bitus is A registro su bitais is nurodytos atminties, rezultatas irasomas i registra A
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void EOR(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;

            switch (opCode)
            {
                case (0x49):
                    result = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case (0x45):
                    result = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case (0x55):
                    result = ZeroPageX(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case (0x4D):
                    result = Absolute(arg1, arg2);
                    CpuCycle += 3;
                    regPC += 3;
                    break;
                case (0x5D):
                    result = AbsoluteX(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0x59):
                    result = AbsoluteY(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0x41):
                    result = IndirectX(arg1);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case (0x51):
                    result = IndirectY(arg1, true);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
            }
            regA = (byte)(regA ^ result); //logine operacija XOR
            flagZ = (regA == 0); //ar Registro A reiksme 0
            flagN = ((regA & 0x80) == 0x80); //patikrina ar pirmas bitas yra 1, jei taip - atsakymas neigiamas
        }

        /// <summary>
        /// Pastumia nurodyta baita per viena bita i kaire, rezultata iraso i Registra A(op.kodas: 0x0A) arba i adresa kurio reiksme keicia
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void ASL(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //baito nuskaitymas
            switch (opCode)
            {
                case 0x0A: result = regA; //ASL A - naudoti Registra A
                    regA = (byte)(result << 1);
                    CpuCycle += 2;
                    regPC +=1; 
                    break; 
                case 0x06: result = ZeroPage(arg1);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
                case 0x16: result = ZeroPageX(arg1); 
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case 0x0E: result = Absolute(arg1, arg2); 
                    CpuCycle += 6;
                    regPC += 3;
                    break;
                case 0x1E: result = AbsoluteX(arg1, arg2, false);
                    CpuCycle += 7;
                    regPC += 3;
                    break;
            }

            flagC = ((result & 0x80) == 0x80); //paskutiniji bita perraso i C Zyme
            result <<= 1; //rezultatas = rzultato bitus paslinkus per viena i kaire
            flagZ = (result == 0); //ar rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //ar po paslinkimo rezultatas pasidare neigiamas?

            //pakeisto baito irasymas
            switch (opCode)
            {
                case 0x0A: return; //irasymas i Registra A jau ivykdytas
                case 0x06: ZeroPageWrite(arg1, result); break;
                case 0x16: ZeroPageXWrite(arg1, result); break;
                case 0x0E: AbsoluteWrite(arg1, arg2, result); break;
                case 0x1E: AbsoluteXWrite(arg1, arg2, result); break;
            }

        }

        /// <summary>
        /// Pastumia nurodyta baita per viena bita i desine, rezultata iraso i Registra A(op.kodas: 0x4A) arba i adresa kurio reiksme keicia
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void LSR(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //baito nuskaitymas
            switch (opCode)
            {
                case 0x4A: result = regA; 
                    regA = (byte)(result >> 1);
                    CpuCycle += 2;
                    regPC += 1;
                    break;
                case 0x46: result = ZeroPage(arg1);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
                case 0x56: result = ZeroPageX(arg1);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case 0x4E: result = Absolute(arg1, arg2);
                    CpuCycle += 6;
                    regPC += 3;
                    break;
                case 0x5E: result = AbsoluteX(arg1, arg2, false);
                    CpuCycle += 7;
                    regPC += 3;
                    break;
            }

            flagC = ((result & 0x01) == 0x01); //pirmaji bita perraso i C Zyme
            result >>= 1; //rezultatas = rzultato bitus paslinkus per viena i desine
            flagZ = (result == 0); //ar rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //ar po paslinkimo rezultatas pasidare neigiamas?

            //pakeisto baito irasymas
            switch (opCode)
            {
                case 0x4A: return; //irasymas i Registra A jau ivykdytas
                case 0x46: ZeroPageWrite(arg1, result); break;
                case 0x56: ZeroPageXWrite(arg1, result); break;
                case 0x4E: AbsoluteWrite(arg1, arg2, result); break;
                case 0x5E: AbsoluteXWrite(arg1, arg2, result); break;
            }

        }

        /// <summary>
        /// Iraso zymiu informacija i steka
        /// </summary>
        void PHP()
        {
            PushFlagsStatus();
            regPC++;
            CpuCycle += 3;
        }

        /// <summary>
        /// Jei paskutinis veiksmas grazino teigiama rezultata, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BPL(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (!flagN) //jei paskutinis rezultatas gavosi teigiamas
            {
                
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++; 
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei paskutinis veiksmas grazino neigiama rezultata, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BMI(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (flagN)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++; 
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei overflow Zyme lygi 0, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BVC(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (!flagV)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei Overflow Zyme lygi 1, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BVS(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (flagV)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei Carry Zyme lygi 1, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BCS(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (flagC)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei Carry Zyme lygi 0, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BCC(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (!flagC)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei Zero Zyme lygi 1, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BNE(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (!flagZ)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Jei Zero Zyme lygi 0, pereiti(regPC) i nurodyta vieta(arg1) atmintyje
        /// </summary>
        /// <param name="arg1"></param>
        void BEQ(byte arg1)
        {
            regPC += 2; //op.kodas ir argumentas
            if (flagZ)
            {
                if ((regPC & 0xFF00) != ((regPC + (sbyte)arg1 + 2) & 0xFF00)) //ar naujas adresas tame paciame puslapyje?
                {
                    CpuCycle++;
                }
                regPC = (ushort)(regPC + (sbyte)arg1); //naujas adresas
                CpuCycle++; //zingsnis pereinant prie naujo adreso
            }
            CpuCycle += 2;
        }

        /// <summary>
        /// Anuliuoja C Zyme(flagC)
        /// </summary>
        void CLC()
        {
            flagC = false;
            regPC++; //1 baito operacija
            CpuCycle += 2;

        }

        /// <summary>
        /// Pakeicia aktyvu CPU adresa i adresa nurodyta argumentuose, senaji adresa iraso i steka
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void JSR(byte arg1, byte arg2)
        {
            regPC += 2; //2 baitu operacija
            Push16(regPC); //Iraso aktyvu adresa i steka
            regPC = MakeAddress(arg1, arg2); //priskiriamas naujas adresas
            CpuCycle += 6;
        }

        /// <summary>
        /// Return from Subroutine - iseina is JSR
        /// </summary>
        void RTS()
        {
            regPC = Pull16();
            CpuCycle += 6;
            regPC++;
        }

        /// <summary>
        /// Logine operacija AND, palygina bitus is A registro su bitais is nurodytos atminties, rezultatas irasomas i registra A
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void AND(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaitymas
            switch (opCode)
            {
                case 0x29: result = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case 0x25: result = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case 0x35: result = ZeroPageX(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case 0x2D: result = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case 0x3D: result = AbsoluteX(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case 0x39: result = AbsoluteY(arg1, arg2, true);
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case 0x21: result = IndirectX(arg1);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case 0x31: result = IndirectY(arg1, false);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
            }
            regA = (byte)(regA & result); //logine opeacija AND
            flagZ = (regA  == 0); //ar Registro A reiksme 0
            flagN = ((regA & 0x80) == 0x80); //patikrina ar pirmas bitas yra 1, jei taip - atsakymas neigiamas
        }

        /// <summary>
        /// Baito testavimo operacija BIT, patikrina baita is nurodytos atminties
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void BIT(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            switch (opCode)
            {
                case (0x24):
                    result = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case (0x2C):
                    result = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
            }
            flagZ = ((regA & result) == 0x00); //patikrina ar registro A reiksme padauginu su gauta reiksme nesigauna 0;
            flagN = ((result & 0x80) == 0x80); //Ar neigiamas?
            flagV = ((result & 0x40) == 0x40);
        }

        /// <summary>
        /// ROL - Rotate Left, nuskaito baita is nurodytos vietos arba Registro A, paslenka visus bitus nuskaitytame baite i kaire, 1 bita pakeicia Zymes C reiksme,
        /// o Zymes C nustato 8 nuskaityto baito bitu, rezultata suraso atgal i nurodyta vieta
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void ROL(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaito baita
            switch (opCode)
            {
                case (0x2A):
                    result = regA;
                    break;
                case (0x26):
                    result = ZeroPage(arg1);
                    break;
                case (0x36):
                    result = ZeroPageX(arg1);
                    break;
                case (0x2E):
                    result = Absolute(arg1, arg2);
                    break;
                case (0x3E):
                    result = AbsoluteX(arg1, arg2, false);
                    break;
            }

            bool bit = ((result & 0x80) == 0x80);

            //paslenka baita
            result = (byte)(result << 1); //i kaire per 1 bita
            byte carry_flag = (byte)(flagC ? 1 : 0); //perkelia Zymes C reiksme i baita
            result = (byte)(result | carry_flag); //1 bita prilygina carry flag reiksmei

            flagC = bit; //irasomas nuskaityto baito 7 bitas
            flagZ = ((result & 0xFF) == 0x00); //Ar gautas rezultatas = 0?
            flagN = ((result & 0x80) == 0x80); //Ar gautas rezultatas neigiamas?

            //iraso rezultata atgal i ta pacia vieta
            switch (opCode)
            {
                case (0x2A):
                    regA = result;
                    CpuCycle += 2;
                    regPC++;
                    break;
                case (0x26):
                    ZeroPageWrite(arg1, result);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
                case (0x36):
                    ZeroPageXWrite(arg1, result);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case (0x2E):
                    AbsoluteWrite(arg1, arg2, result);
                    CpuCycle += 6;
                    regPC += 3;
                    break;
                case (0x3E):
                    AbsoluteXWrite(arg1, arg2, result);
                    CpuCycle += 7;
                    regPC += 3;
                    break;
            }
         }

        /// <summary>
        /// ROR - Rotate Right, nuskaito baita is nurodytos vietos arba Registro A, paslenka visus bitus nuskaitytame baite i desine, 8 bita pakeicia Zymes C reiksme,
        /// o Zymes C nustato 1 nuskaityto baito bitu, rezultata suraso atgal i nurodyta vieta
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void ROR(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaito baita
            switch (opCode)
            {
                case (0x6A):
                    result = regA;
                    break;
                case (0x66):
                    result = ZeroPage(arg1);
                    break;
                case (0x76):
                    result = ZeroPageX(arg1);
                    break;
                case (0x6E):
                    result = Absolute(arg1, arg2);
                    break;
                case (0x7E):
                    result = AbsoluteX(arg1, arg2, false);
                    break;
            }

            bool bit = ((result & 0x01) == 0x01); //nuskaito pirma bita pries paslinkima, kad sis niekur nedingtu

            //paslenka baita
            result = (byte)(result >> 1); //i desine per 1 bita
            if (flagC)
                result = (byte)(result | 0x80);//po paslinkimo 8 bitas = 0, bet jei Zyme C lygi 1, tai prilygina 8 bita 1
            

            flagC = bit; //irasomas nuskaityto baito 1 bitas
            flagZ = ((result & 0xFF) == 0x00); //Ar gautas rezultatas = 0?
            flagN = ((result & 0x80) == 0x80); //Ar gautas rezultatas neigiamas?

            //iraso rezultata atgal i ta pacia vieta
            switch (opCode)
            {
                case (0x6A):
                    regA = result;
                    CpuCycle += 2;
                    regPC++;
                    break;
                case (0x66):
                    ZeroPageWrite(arg1, result);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
                case (0x76):
                    ZeroPageXWrite(arg1, result);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case (0x6E):
                    AbsoluteWrite(arg1, arg2, result);
                    CpuCycle += 6;
                    regPC += 3;
                    break;
                case (0x7E):
                    AbsoluteXWrite(arg1, arg2, result);
                    CpuCycle += 7;
                    regPC += 3;
                    break;
            }
        }

        /// <summary>
        /// Iraso Registro A reiksme i steka
        /// </summary>
        void PHA()
        {
            Push8(regA);
            regPC++;
            CpuCycle += 3;
        }

        /// <summary>
        /// Nuskaito Registro A reiksme is steko
        /// </summary>
        void PLA()
        {
            regA = Pull8();
            flagZ = (regA == 0); //Ar istraukta reiksme 0?
            flagN = ((regA & 0x80) == 0x80); //Ar istraukta reiksme neigiama?
            regPC++;
            CpuCycle += 4;
        }

        /// <summary>
        /// Nuskaito zymiu reiksmes is steko
        /// </summary>
        void PLP()
        {
            PullFlagsStatus();
            regPC += 1;
            CpuCycle += 4;
        }

        /// <summary>
        /// Nustato Zyme C = 1
        /// </summary>
        void SEC()
        {
            flagC = true;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Nustato Zyme I = 1
        /// </summary>
        void SEI()
        {
            flagI = true;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Nustato Zyme D = 1
        /// </summary>
        void SED()
        {
            flagD = true;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Nustato Zyme I = 0
        /// </summary>
        void CLI()
        {
            flagI = false;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Nustato Zyme V = 0
        /// </summary>
        void CLV()
        {
            flagV = false;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Nustato Zyme D = 0
        /// </summary>
        void CLD()
        {
            flagD = false;
            CpuCycle += 2;
            regPC++;
        }

        /// <summary>
        /// Jump - perkelia aktyvu adresa i nurodyta vieta
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void JMP(byte opCode, byte arg1, byte arg2)
        {
            ushort adress = MakeAddress(arg1, arg2);
            switch (opCode)
            {
                case (0x4C):
                    regPC = adress;
                    CpuCycle += 3;
                    break;
                case (0x6C):
                    regPC = Globals.Memory.Read16(adress); //normalus atvejis
                    CpuCycle += 5;
                    break;
            }
        }

        /// <summary>
        /// Sudeda Registro A reiksme su nurodytu baitu(arg1) arba nuskaitytu baitu is nurodytos atminties(arg1, arg2), prideda Zymes C reiksme
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void ADC(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaitymas
            switch (opCode)
            {
                case (0x69):
                    result = arg1;
                    regPC += 2;
                    CpuCycle += 2;
                    break;
                case (0x65):
                    result = ZeroPage(arg1);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0x75):
                    result = ZeroPageX(arg1);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0x6D):
                    result = Absolute(arg1, arg2);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0x7D):
                    result = AbsoluteX(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0x79):
                    result = AbsoluteY(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0x61):
                    result = IndirectX(arg1);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0x71):
                    result = IndirectY(arg1, true);
                    regPC += 2;
                    CpuCycle += 5;
                    break;
            }

            uint temp32 = (uint)(regA + result + Convert.ToUInt32(flagC)); //registro A reiksme + nuskaitytas baitas is atminties + Carry Zyme(1 arba 0)

            flagC = (temp32 > 255); //Ar ivyko overflow'as?
            flagV = (((temp32 ^ regA) & (temp32 ^ result)) & 0x80) != 0; //jei 8 bitas temp32 = 1, o Registro A ir nuskaityto baito 8 bitas = 0, arba atvirksciai, Zyme V = 1
            flagZ = (temp32 & 0xFF) == 0; //Ar rezultatas = 0
            flagN = (temp32 & 0x80) == 0x80; //Ar rezultatas neigiamas

            regA = (byte)(temp32 & 0xFF); //Registras A = pradiniai Registro A reiksmei + nuskaitytam baitui + Zeymes C reiksmei, 
            //toks fancy uzrasymas naudojamas tam, kad isitikinti jog konvertavimo i baita is uint metu informacija bus paimta tik is low baito
        }

        /// <summary>
        /// Atima Registro A reiksme su nurodytu baitu(arg1) arba nuskaitytu baitu is nurodytos atminties(arg1, arg2), atima Zymes C reiksme
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void SBC(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaitymas
            switch (opCode)
            {
                case (0xE9):
                    result = arg1;
                    regPC += 2;
                    CpuCycle += 2;
                    break;
                case (0xE5):
                    result = ZeroPage(arg1);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0xF5):
                    result = ZeroPageX(arg1);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0xED):
                    result = Absolute(arg1, arg2);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xFD):
                    result = AbsoluteX(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xF9):
                    result = AbsoluteY(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xE1):
                    result = IndirectX(arg1);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0xF1):
                    result = IndirectY(arg1, true);
                    regPC += 2;
                    CpuCycle += 5;
                    break;
            }

            uint temp32 = (uint)(regA - result - Convert.ToByte(!flagC)); //registro A reiksme - nuskaitytas baitas is atminties - Carry Zyme(1 arba 0)

            flagC = !(temp32 > 255); //Ar ivyko overflow'as?
            flagV = (((result ^ regA) & (temp32 ^ regA)) & 0x80) != 0; //jei 8 bitas temp32 = 1, o Registro A ir nuskaityto baito 8 bitas = 0, arba atvirksciai, Zyme V = 1
            flagZ = (temp32 & 0xFF) == 0; //Ar rezultatas = 0
            flagN = (temp32 & 0x80) == 0x80; //Ar rezultatas neigiamas

            regA = (byte)(temp32 & 0xFF); //Registras A = pradiniai Registro A reiksmei - nuskaitytam baitui - Zeymes C reiksmei, 
            //toks fancy uzrasymas naudojamas tam, kad isitikinti jog konvertavimo i baita is uint metu informacija bus paimta tik is low baito
        }

        /// <summary>
        /// Iraso Registro A reiksme i nurodyta adresa(arg1, arg2)
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void STA(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0x85):
                    ZeroPageWrite(arg1, regA);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0x95):
                    ZeroPageXWrite(arg1, regA);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0x8D):
                    AbsoluteWrite(arg1, arg2, regA);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0x9D):
                    AbsoluteXWrite(arg1, arg2, regA);
                    regPC += 3;
                    CpuCycle += 5;
                    break;
                case (0x99):
                    AbsoluteYWrite(arg1, arg2, regA);
                    regPC += 3;
                    CpuCycle += 5;
                    break;
                case (0x81):
                    IndirectXWrite(arg1, regA);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0x91):
                    IndirectYWrite(arg1, regA);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
            }
        }

        /// <summary>
        /// Iraso Registro Y reiksme i nurodyta adresa(arg1, arg2)
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void STY(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0x84):
                    ZeroPageWrite(arg1, regY);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0x94):
                    ZeroPageXWrite(arg1, regY);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0x8C):
                    AbsoluteWrite(arg1, arg2, regY);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                
            }
        }

        /// <summary>
        /// Iraso Registro X reiksme i nurodyta adresa(arg1, arg2)
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void STX(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0x86):
                    ZeroPageWrite(arg1, regX);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0x96):
                    ZeroPageXWrite(arg1, regX);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0x8E):
                    AbsoluteWrite(arg1, arg2, regX);
                    regPC += 3;
                    CpuCycle += 4;
                    break;

            }
        }

        /// <summary>
        /// Atima 1 is Registro Y
        /// </summary>
        void DEY()
        {
            regY--;
            flagZ = (regY == 0x00); //Ar nauja Registro Y reiksme = 0
            flagN = ((regY & 0x80) == 0x80); //Ar nauja Registro Y reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro X reiksme i Registra A
        /// </summary>
        void TXA()
        {
            regA = regX;
            flagZ = (regA == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regA & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro Y reiksme i Registra A
        /// </summary>
        void TYA()
        {
            regA = regY;
            flagZ = (regA == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regA & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro A reiksme i Registra Y
        /// </summary>
        void TAY()
        {
            regY = regA;
            flagZ = (regY == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regY & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro A reiksme i Registra X
        /// </summary>
        void TAX()
        {
            regX = regA;
            flagZ = (regX == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regX & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro S reiksme i Registra X
        /// </summary>
        void TSX()
        {
            regX = regS;
            flagZ = (regX == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regX & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Perkelia Registro X reiksme i Registra S
        /// </summary>
        void TXS()
        {
            regS = regX;
            flagZ = (regS == 0x00); //Ar nauja Registro A reiksme = 0
            flagN = ((regS & 0x80) == 0x80); //Ar nauja Registro A reiksme neigiama
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Uzkrauna Registro Y reiksme is nurodyto baito(arg1) arba is nuskaitytos atiminties(arg1, arg2) baito
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void LDY(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0xA0):
                    regY = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case (0xA4):
                    regY = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case (0xB4):
                    regY = ZeroPageX(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case (0xAC):
                    regY = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0xBC):
                    regY = AbsoluteX(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
            }
            flagZ = (regY == 0); //Ar gauta reiksme lygi 0
            flagN = ((regY & 0x80) == 0x80); //Ar gauta reiksme neigiama
        }

        /// <summary>
        /// Uzkrauna Registro A reiksme is nurodyto baito(arg1) arba is nuskaitytos atiminties(arg1, arg2) baito
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void LDA(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0xA9):
                    regA = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case (0xA5):
                    regA = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case (0xB5):
                    regA = ZeroPageX(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case (0xAD):
                    regA = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0xBD):
                    regA = AbsoluteX(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0xB9):
                    regA = AbsoluteY(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0xA1):
                    regA = IndirectX(arg1);
                    CpuCycle += 6;
                    regPC += 2;
                    break;
                case (0xB1):
                    regA = IndirectY(arg1, true);
                    CpuCycle += 5;
                    regPC += 2;
                    break;
            }
            flagZ = (regA == 0); //Ar gauta reiksme lygi 0
            flagN = ((regA & 0x80) == 0x80); //Ar gauta reiksme neigiama
        }

        /// <summary>
        /// Uzkrauna Registro X reiksme is nurodyto baito(arg1) arba is nuskaitytos atiminties(arg1, arg2) baito
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void LDX(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case (0xA2):
                    regX = arg1;
                    CpuCycle += 2;
                    regPC += 2;
                    break;
                case (0xA6):
                    regX = ZeroPage(arg1);
                    CpuCycle += 3;
                    regPC += 2;
                    break;
                case (0xB6):
                    regX = ZeroPageY(arg1);
                    CpuCycle += 4;
                    regPC += 2;
                    break;
                case (0xAE):
                    regX = Absolute(arg1, arg2);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
                case (0xBE):
                    regX = AbsoluteY(arg1, arg2, true);
                    CpuCycle += 4;
                    regPC += 3;
                    break;
            }
            flagZ = (regX == 0); //Ar gauta reiksme lygi 0
            flagN = ((regX & 0x80) == 0x80); //Ar gauta reiksme neigiama
        }

        /// <summary>
        /// Palygina duota reiksme(arg1) arba nuskaityta reiksme is atminties(arg1, arg2) su Registru Y
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void CPY(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            switch (opCode)
            {
                case (0xC0):
                    result = arg1;
                    regPC += 2;
                    CpuCycle += 2;
                    break;
                case (0xC4):
                    result = ZeroPage(arg1);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0xCC):
                    result = Absolute(arg1, arg2);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
            }
            flagC = (regY >= result); //Ar ivyko overlowas?
            result = (byte)(regY - result);
            flagZ = (result == 0); //Ar rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //Ar rezultatas neigiamas?

        }

        /// <summary>
        /// Palygina duota reiksme(arg1) arba nuskaityta reiksme is atminties(arg1, arg2) su Registru X
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void CPX(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            switch (opCode)
            {
                case (0xE0):
                    result = arg1;
                    regPC += 2;
                    CpuCycle += 2;
                    break;
                case (0xE4):
                    result = ZeroPage(arg1);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0xEC):
                    result = Absolute(arg1, arg2);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
            }
            flagC = (regX >= result); //Ar ivyko overlowas?
            result = (byte)(regX - result);
            flagZ = (result == 0); //Ar rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //Ar rezultatas neigiamas?

        }

        /// <summary>
        /// Palygina duota reiksme(arg1) arba nuskaityta reiksme is atminties(arg1, arg2) su Registru A
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void CMP(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            switch (opCode)
            {
                case (0xC9):
                    result = arg1;
                    regPC += 2;
                    CpuCycle += 2;
                    break;
                case (0xC5):
                    result = ZeroPage(arg1);
                    regPC += 2;
                    CpuCycle += 3;
                    break;
                case (0xD5):
                    result = ZeroPageX(arg1);
                    regPC += 2;
                    CpuCycle += 4;
                    break;
                case (0xCD):
                    result = Absolute(arg1, arg2);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xDD):
                    result = AbsoluteX(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xD9):
                    result = AbsoluteY(arg1, arg2, true);
                    regPC += 3;
                    CpuCycle += 4;
                    break;
                case (0xC1):
                    result = IndirectX(arg1);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0xD1):
                    result = IndirectY(arg1, true);
                    regPC += 2;
                    CpuCycle += 5;
                    break;
            }
            flagC = (regA >= result); //Ar ivyko overlowas?
            result = (byte)(regA - result);
            flagZ = (result == 0); //Ar rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //Ar rezultatas neigiamas?

        }

        /// <summary>
        /// Nuskaito baita is nurodytas atminties(arg1, arg2), sumazina ji vienetu ir iraso atgal
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void DEC(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaitymas
            switch (opCode)
            {
                case (0xC6):
                    result = ZeroPage(arg1);
                    break;
                case (0xD6):
                    result = ZeroPageX(arg1);
                    break;
                case (0xCE):
                    result = Absolute(arg1, arg2);
                    break;
                case (0xDE):
                    result = AbsoluteX(arg1, arg2, false);
                    break;
            }
            result--; //sumazina 1
            flagZ = (result == 0); //Ar gautas rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //Ar gautas rezultatas neigiamas?
            //irasymas atgal
            switch (opCode)
            {
                case (0xC6):
                    ZeroPageWrite(arg1, result);
                    regPC += 2;
                    CpuCycle += 5;
                    break;
                case (0xD6):
                    ZeroPageXWrite(arg1, result);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0xCE):
                    AbsoluteWrite(arg1, arg2, result);
                    regPC += 3;
                    CpuCycle += 6;
                    break;
                case (0xDE):
                    AbsoluteXWrite(arg1, arg2, result);
                    regPC += 3;
                    CpuCycle += 7;
                    break;
            }
        }

        /// <summary>
        /// Nuskaito baita is nurodytas atminties(arg1, arg2), padidina ji vienetu ir iraso atgal
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        void INC(byte opCode, byte arg1, byte arg2)
        {
            byte result = 0x00;
            //nuskaitymas
            switch (opCode)
            {
                case (0xE6):
                    result = ZeroPage(arg1);
                    break;
                case (0xF6):
                    result = ZeroPageX(arg1);
                    break;
                case (0xEE):
                    result = Absolute(arg1, arg2);
                    break;
                case (0xFE):
                    result = AbsoluteX(arg1, arg2, false);
                    break;
            }
            result++; //padidina 1
            flagZ = (result == 0); //Ar gautas rezultatas lygus 0?
            flagN = ((result & 0x80) == 0x80); //Ar gautas rezultatas neigiamas?
            //irasymas atgal
            switch (opCode)
            {
                case (0xE6):
                    result = ZeroPageWrite(arg1, result);
                    regPC += 2;
                    CpuCycle += 5;
                    break;
                case (0xF6):
                    result = ZeroPageXWrite(arg1, result);
                    regPC += 2;
                    CpuCycle += 6;
                    break;
                case (0xEE):
                    result = AbsoluteWrite(arg1, arg2, result);
                    regPC += 3;
                    CpuCycle += 6;
                    break;
                case (0xFE):
                    result = AbsoluteXWrite(arg1, arg2, result);
                    regPC += 3;
                    CpuCycle += 7;
                    break;
            }
        }

        /// <summary>
        /// Prideda vieneta prie Registro Y
        /// </summary>
        void INY()
        {
            regY++;
            flagZ = (regY == 0); //Ar naujas rezultatas lygus 0?
            flagN = ((regY & 0x80) == 0x80); //Ar naujas rezultatas neigiamas?
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Prideda vieneta prie Registro X
        /// </summary>
        void INX()
        {
            regX++;
            flagZ = (regX == 0); //Ar naujas rezultatas lygus 0?
            flagN = ((regX & 0x80) == 0x80); //Ar naujas rezultatas neigiamas?
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Atima vieneta is Registro X
        /// </summary>
        void DEX()
        {
            regX--;
            flagZ = (regX == 0); //Ar naujas rezultatas lygus 0?
            flagN = ((regX & 0x80) == 0x80); //Ar naujas rezultatas neigiamas?
            regPC++;
            CpuCycle += 2;
        }

        /// <summary>
        /// Tusciau komanda
        /// </summary>
        void NOP()
        {
            regPC++;
            CpuCycle += 2;
        }

        #endregion Operacijos

        #region StekoOperacijos

        /// <summary>
        /// Iraso 8bitu informacija i steck'a ir paslenka Resgistra S zemiau
        /// </summary>
        /// <param name="value">8bitu reiksme</param>
        public void Push8(byte value)
        {
            Globals.Memory.Write((ushort)(0x100 + regS), value);//0x100 - pradinis stack'o adresas, Registras S nurodo esama registro pozicija
            regS--;//paslenka registro pozicija per 8bitus
        }

        /// <summary>
        /// Paslenka Registra S auksciau ir nuskaito 8bitu informacija is steck'o
        /// </summary>
        /// <returns>8bitu reiksme</returns>
        public byte Pull8()
        {
            regS++;
            return Globals.Memory.Read((ushort)(0x100 + regS)); //0x100 steko pradzia + steko elemento adresas
        }

        /// <summary>
        /// Iraso 16bitu informacija i steck'a du kartus irasant po 8 bitus - pirmiausia kairiji po to desiniji baitus
        /// </summary>
        /// <param name="value">16bit reiksme</param>
        public void Push16(ushort value)
        {
            Push8((byte)((value & 0xFF00) >> 8)); //pasalina desiniji baita ir paslenka kairiji baita i jo vieta, taip sudaromas vienas baitas is dvieju
            Push8((byte)(value & 0xFF)); //irasomasis desinysis baitas
        }

        /// <summary>
        /// Nuskaito 16bitu informacija is steck'o du kartus istraukiant po 8 bitus ir juos suklijuojant i ushort
        /// </summary>
        /// <returns></returns>
        public ushort Pull16()
        {
            byte data1 = Pull8();
            byte data2 = Pull8();

            ushort data = (ushort)((data2 << 8) | data1);

            return data;
        }

        /// <summary>
        /// Grazina zymiu reiksmes surasytas i 8 bitus (surasymo tvarka: N, V, 1, B, D, I, Z, C)
        /// </summary>
        /// <returns>Zymiu reiksmes surasytos i 8 bitus</returns>
        public byte GetFlagsStatus()
        {
            byte status = 0x00;
            if (flagN)
                status = (byte)(status | 0x80); //status | 10000000
            if (flagV)
                status = (byte)(status | 0x40); //status | 01000000
            status = (byte)(status | 0x20);     //status | 00100000
            if (flagB)
                status = (byte)(status | 0x10); //status | 00010000
            if (flagD)
                status = (byte)(status | 0x08); //status | 00001000
            if (flagI)
                status = (byte)(status | 0x04); //status | 00000100
            if (flagZ)
                status = (byte)(status | 0x02); //status | 00000010
            if (flagC)
                status = (byte)(status | 0x01); //status | 00000001
            return status;
        }

        /// <summary>
        /// Iraso zymiu reiksmes i steka
        /// </summary>
        public void PushFlagsStatus()
        {
            Push8(GetFlagsStatus());
        }

        /// <summary>
        /// Nuskaito zymiu reiksmes is steko
        /// </summary>
        public void PullFlagsStatus()
        {
            byte status = Pull8();
            flagN = ((status & 0x80) == 0x80); //10000000
            flagV = ((status & 0x40) == 0x40); //01000000
            flagB = ((status & 0x10) == 0x10); //00010000
            flagD = ((status & 0x08) == 0x08); //00001000
            flagI = ((status & 0x04) == 0x04); //00000100
            flagZ = ((status & 0x02) == 0x02); //00000010
            flagC = ((status & 0x01) == 0x01); //00000001
        }

        /// <summary>
        /// Istraukia zymiu reiksmes is steko, po to istraukia aktyvuji adresa is steko
        /// </summary>
        public void RTI()
        {
            PullFlagsStatus();
            regPC = Pull16();
            CpuCycle += 6;
        }

        #endregion StekoOperacijos

        /// <summary>
        /// Operacijos iskvietimas pagal atitinkama Op koda
        /// </summary>
        /// <param name="opCode">Operacijos kodas</param>
        /// <param name="arg1">pirmas parametras(8bit)</param>
        /// <param name="arg2">antras parametras(8bit)</param>
        public void Disassemble(byte opCode, byte arg1, byte arg2)
        {
            switch (opCode)
            {
                case 0x00:
                    BRK();
                    break;
                case 0x1d:
                case 0x19:
                case 0x15:
                case 0x11:
                case 0x0d:
                case 0x09:
                case 0x05:
                case 0x01:
                    ORA(opCode, arg1, arg2);
                    break;
                case 0x1E:
                case 0x0E:
                case 0x16:
                case 0x06:
                case 0x0a:
                    ASL(opCode, arg1, arg2);
                    break;
                case 0x08:
                    PHP();
                    break;
                case 0x10:
                    BPL(arg1);
                    break;
                case 0x18:
                    CLC();
                    break;
                case 0x20:
                    JSR(arg1, arg2);
                    break;
                case 0x3D:
                case 0x39:
                case 0x35:
                case 0x31:
                case 0x2D:
                case 0x29:
                case 0x25:
                case 0x21:
                    AND(opCode, arg1, arg2);
                    break;
                case 0x2C:
                case 0x24:
                    BIT(opCode, arg1, arg2);
                    break;
                case 0x3E:
                case 0x36:
                case 0x2E:
                case 0x2A:
                case 0x26:
                    ROL(opCode, arg1, arg2);
                    break;
                case 0x28:
                    PLP();
                    break;
                case 0x30:
                    BMI(arg1);
                    break;
                case 0x38:
                    SEC();
                    break;
                case 0x40:
                    RTI();
                    break;
                case 0x5D:
                case 0x59:
                case 0x55:
                case 0x51:
                case 0x4D:
                case 0x49:
                case 0x45:
                case 0x41:
                    EOR(opCode, arg1, arg2);
                    break;
                case 0x5E:
                case 0x56:
                case 0x4E:
                case 0x4A:
                case 0x46:
                    LSR(opCode, arg1, arg2);
                    break;
                case 0x48:
                    PHA();
                    break;
                case 0x6C:
                case 0x4C:
                    JMP(opCode, arg1, arg2);
                    break;
                case 0x50:
                    BVC(arg1);
                    break;
                case 0x58:
                    CLI();
                    break;
                case 0x60:
                    RTS();
                    break;
                case 0x7D:
                case 0x79:
                case 0x75:
                case 0x71:
                case 0x6D:
                case 0x69:
                case 0x65:
                case 0x61:
                    ADC(opCode, arg1, arg2);
                    break;
                case 0x7E:
                case 0x76:
                case 0x6E:
                case 0x6A:
                case 0x66:
                    ROR(opCode, arg1, arg2);
                    break;
                case 0x68:
                    PLA();
                    break;
                case 0x70:
                    BVS(arg1);
                    break;
                case 0x78:
                    SEI();
                    break;
                case 0x9D:
                case 0x99:
                case 0x95:
                case 0x91:
                case 0x8D:
                case 0x85:
                case 0x81:
                    STA(opCode, arg1, arg2);
                    break;
                case 0x94:
                case 0x8C:
                case 0x84:
                    STY(opCode, arg1, arg2);
                    break;
                case 0x96:
                case 0x8E:
                case 0x86:
                    STX(opCode, arg1, arg2);
                    break;
                case 0x88:
                    DEY();
                    break;
                case 0x8A:
                    TXA();
                    break;
                case 0x90:
                    BCC(arg1);
                    break;
                case 0x98:
                    TYA();
                    break;
                case 0x9A:
                    TXS();
                    break;
                case 0xBC:
                case 0xB4:
                case 0xAC:
                case 0xA4:
                case 0xA0:
                    LDY(opCode, arg1, arg2);
                    break;
                case 0xBD:
                case 0xB9:
                case 0xB5:
                case 0xB1:
                case 0xAD:
                case 0xA9:
                case 0xA5:
                case 0xA1:
                    LDA(opCode, arg1, arg2);
                    break;
                case 0xBE:
                case 0xB6:
                case 0xAE:
                case 0xA6:
                case 0xA2:
                    LDX(opCode, arg1, arg2);
                    break;
                case 0xA8:
                    TAY();
                    break;
                case 0xAA:
                    TAX();
                    break;
                case 0xB0:
                    BCS(arg1);
                    break;
                case 0xB8:
                    CLV();
                    break;
                case 0xBA:
                    TSX();
                    break;
                case 0xCC:
                case 0xC4:
                case 0xC0:
                    CPY(opCode, arg1, arg2);
                    break;
                case 0xDD:
                case 0xD9:
                case 0xD5:
                case 0xD1:
                case 0xCD:
                case 0xC9:
                case 0xC5:
                case 0xC1:
                    CMP(opCode, arg1, arg2);
                    break;
                case 0xDE:
                case 0xD6:
                case 0xCE:
                case 0xC6:
                    DEC(opCode, arg1, arg2);
                    break;
                case 0xC8:
                    INY();
                    break;
                case 0xCA:
                    DEX();
                    break;
                case 0xD0:
                    BNE(arg1);
                    break;
                case 0xD8:
                    CLD();
                    break;
                case 0xEC:
                case 0xE4:
                case 0xE0:
                    CPX(opCode, arg1, arg2);
                    break;
                case 0xFD:
                case 0xF9:
                case 0xF5:
                case 0xF1:
                case 0xED:
                case 0xE9:
                case 0xE5:
                case 0xE1:
                    SBC(opCode, arg1, arg2);
                    break;
                case 0xFE:
                case 0xF6:
                case 0xEE:
                case 0xE6:
                    INC(opCode, arg1, arg2);
                    break;
                case 0xE8:
                    INX();
                    break;
                case 0xF0:
                    BEQ(arg1);
                    break;
                case 0xF8:
                    SED();
                    break;
                default:
                    NOP();
                    break;
            }
        }

        /// <summary>
        /// Metodas atsakingas uz procesoriaus veikimo emuliavima, pagrindinis emulatoriaus ciklas
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Starting CPU...");
            //Ciklas tesis kol nebus priverstinai nutrauktas
            while (true)
            {
                byte opCode = Globals.Memory.Read(regPC); //Nuskaitomas operacijos kodas is aktyvaus adreso
                cmd_no++; //ivykdytu komandu skaiciuoklis
                if (Main.MainFrm == null)
                    break;

                if (!Paused) //Jei CPU darbas nesulaikytas
                {
                    //Nuskaitomi argumentai, ju gali ir nereiketi, bet tai issiaiskins Disassemble metodas
                    byte arg1 = Globals.Memory.Read((ushort)(regPC + 1)); //Nuskaitomas pirmas argumentas
                    byte arg2 = Globals.Memory.Read((ushort)(regPC + 2)); //Nuskaitomas antras argumentas
                    Disassemble(opCode, arg1, arg2); //Ivyksta operacijos kodo dekodavimas ir pati CPU operacija
                    
                }
                else
                {
                    Thread.Sleep(100); //CPU darbas sulaikytas, laukia 100ms ir vel prades cikla
                    continue; //Pradeti cikla is naujo
                }
                if (CpuCycle >= 113) //113 yra NTSC ciklu skaicius per PPU eilutes skanavima
                {
                    if (Globals.Ppu.ScanLine()) //??
                    {
                        flagB = false;
                        Push16(regPC);
                        PushFlagsStatus();
                        flagI = true;
                        regPC = Globals.Memory.Read16(0XFFFA);
                    }
                    

                    CpuCycle = 0; //Anuliuojamas CPU ciklu skaicius
                    if (Main.MainFrm.showInfo)
                    {
                        Main.MainFrm.InfoFB.Text = Convert.ToByte(flagB).ToString();
                        Main.MainFrm.InfoFC.Text = Convert.ToByte(flagC).ToString();
                        Main.MainFrm.InfoFD.Text = Convert.ToByte(flagD).ToString();
                        Main.MainFrm.InfoFI.Text = Convert.ToByte(flagI).ToString();
                        Main.MainFrm.InfoFN.Text = Convert.ToByte(flagN).ToString();
                        Main.MainFrm.InfoFV.Text = Convert.ToByte(flagV).ToString();
                        Main.MainFrm.InfoFlagZ.Text = Convert.ToByte(flagZ).ToString();
                        Main.MainFrm.InfoRegA.Text = regA.ToString();
                        Main.MainFrm.InfoRegS.Text = regS.ToString();
                        Main.MainFrm.InfoRegX.Text = regX.ToString();
                        Main.MainFrm.InfoRegY.Text = regY.ToString();
                        Main.MainFrm.InfoRegPC.Text = regPC.ToString();
                    }
                    

                }
                Application.DoEvents(); //perpiesimai reagavimas i mygtukus ir pan.
                
            }
        }

    }
}
// ReSharper restore InconsistentNaming