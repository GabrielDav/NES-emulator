using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NesEmulator
{
    /// <summary>
    /// Konsoles "pultelis"
    /// </summary>
    public class JoyPad
    {
        #region Mygtukai
        bool keyUp = false;
        bool keyRight = false;
        bool keyDown = false;
        bool keyLeft = false;
        bool keyStart = false;
        bool keySelect = false;
        bool keyA = false;
        bool keyB = false;
        #endregion Mygtukai

        int call_no = 0;

        /// <summary>
        /// Uzregistruoja mygtuko nuspaudima arba atelidima
        /// </summary>
        /// <param name="keyCode">Mygtuko kodas is Windows.Forms.Keys</param>
        /// <param name="down">Ar mygtukas nuspaustas</param>
        public void CheckKey(Keys keyCode, bool down)
        {
            if (keyCode == Keys.A)
                keyA = down;
            else if (keyCode == Keys.S)
                keyB = down;
            else if (keyCode == Keys.Space)
                keyStart = down;
            else if (keyCode == Keys.Control)
                keySelect = down;
            else if (keyCode == Keys.Up)
                keyUp = down;
            else if (keyCode == Keys.Right)
                keyRight = down;
            else if (keyCode == Keys.Down)
                keyDown = down;
            else if (keyCode == Keys.Left)
                keyLeft = down;
        }

        /// <summary>
        /// Nuskaito esama pulto bukle
        /// </summary>
        /// <returns>mygtuku koda(8bit)</returns>
        public int Read()
        {
            int code = 0;
            call_no++;
            //Mygtuku kodai
            if (keyA)
                code |= 0x01; //00000001
            if (keyB)
                code |= 0x02; //00000010
            if (keySelect)
                code |= 0x04; //00000100
            if (keyStart)
                code |= 0x08; //00001000
            if (keyUp)
                code |= 0x10; //00010000
            if (keyDown)
                code |= 0x20; //00100000
            if (keyLeft)
                code |= 0x40; //01000000
            if (keyRight)
                code |= 0x80; //10000000
            return code;
            
        }


    }
}
