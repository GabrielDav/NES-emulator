namespace NesEmulator
{
    public class Storage
    {
        public uint Head;
        public byte LowByte;
        public byte HighByte;

        public byte[] Data;

        public Storage()
        {
            Data = new byte[0xFFFF];
        }

        public void WriteLowByte(byte value)
        {
            LowByte = value;
        }

        public void WriteHighByte(byte value)
        {
            HighByte = value;
            Head = (uint)(Globals.Memory.Read16((ushort)((ushort)((ushort)(8 << HighByte) & LowByte) + 2)) << 16) & Globals.Memory.Read16((ushort)((ushort)(HighByte << 8) & LowByte));
        }

        public byte ReadByte()
        {
            byte result = Data[Head];
            Head++;
            return result;
        }

        public void WriteByte(byte value)
        {
            Data[Head] = value;
            Head++;
        }

    }
}
