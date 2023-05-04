using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NVL_UltrasoundLab
{
    public partial class INITForm1 : Form
    {
        StreamWriter sw;
        bool Chek;
        public double l, D, F, Vcore, t1, t2, E, v, c1,c2, M;


        public INITForm1()
        {
            InitializeComponent();

            try
            {
                serialPort1.Open();
            }catch(Exception e)
            {
                MessageBox.Show("Ошибка связи! переподключите устройства и перезагрузите программу\n\n" + e.Message);
            }

        }

        private void textBox7_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox7);
        }

        private void textBox6_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox6);
        }

        private void textBox5_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox5);
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox4);
        }

        private void textBox8_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox8);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox2.Focus();
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox3.Focus();
            }
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox7.Focus();
                textBox7.Select(0, textBox7.Text.Length);
            }
        }

        private void textBox7_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox7.Text != "")
                {
                    if (textBox6.Text != "")
                    {
                      
                        Vcore = Math.PI * D * D / 4 * l / 1000;
                        textBox4.Text = "" + Vcore;

                    }
                }
                
                textBox6.Focus();
                textBox6.Select(0, textBox6.Text.Length);
            }
        }

        private void textBox6_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if(textBox7.Text!="")
                    {
                    if (textBox6.Text != "")
                    {
                        l = double.Parse(textBox6.Text);
                        D = double.Parse(textBox7.Text);
                        Vcore = Math.Round(Math.PI * D * D / 4 * l / 1000,4);
                        textBox4.Text = "" + Vcore;

                    }
                }
                 textBox14.Focus(); textBox14.Select(0, textBox14.Text.Length);
            }
        }

        private void textBox4_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox8.Focus();
                textBox8.Select(0, textBox8.Text.Length);
            }
        }

        private void textBox8_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox7.Text != "")
                {
                    if (textBox8.Text != "")
                    {
                        double tt = Properties.Settings.Default.TDelayProd;
                        int t = (int)((double.Parse(textBox7.Text)/(double.Parse(textBox8.Text)) * 1000)/10)*10;
                        textBox12.Text = "" + t;
                    }

                }

                textBox9.Focus();
                textBox9.Select(0, textBox9.Text.Length);

            }
        }

        private void textBox9_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox7.Text != "")
            {
                if (textBox9.Text != "")
                {
                    double tt = Properties.Settings.Default.TDelayPop;
                    double t = (int)((double.Parse(textBox7.Text) / (double.Parse(textBox9.Text) - tt) * 1000) / 10) * 10;
                    textBox13.Text = "" + t;
                }

            }
            StartCalculation();
        }

        private void textBox9_Leave(object sender, EventArgs e)
        {
            test_textBox(textBox9);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Owner = this;
            f1.Show();
           // t1 = f1.GetTp();
           // t2 = f1.GetTs();
           // f1.Dispose();
           // if (t1 != 0) textBox9.Text = "" + t1;
           // if (t2 != 0) textBox8.Text = "" + t2;
        }


        byte[] g_uiCRC16tableHi = {
  0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
  0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
  0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
  0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
  0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
  0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
  0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
  0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
  0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
  0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
  0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
  0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
  0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
  0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
  0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
  0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
  0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
  0x40
};
        byte[] g_uiCRC16tableLo = {
  0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4,
  0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
  0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
  0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
  0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
  0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
  0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
  0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
  0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
  0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
  0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
  0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
  0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
  0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
  0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
  0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
  0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80, 0x40
};

        //———————————————————————
        ushort CRC16(ushort usCRC, byte[] pBuffer, int uiSize)
        {
            //const byte pucBuffer =  reinterpret_cast <const unsigned char*>(pBuffer);
            byte ucHi = (byte)((usCRC >> 8) & 0xFF);
            byte ucLo = (byte)(usCRC & 0xFF);
            byte uIndex; int i = 0;
            while (uiSize-- != 0)
            {
                uIndex = (byte)(ucHi ^ pBuffer[i]); i++;
                ucHi = (byte)(ucLo ^ g_uiCRC16tableHi[uIndex]);
                ucLo = g_uiCRC16tableLo[uIndex];
            }
            return (ushort)(ucHi << 8 | ucLo);
        }
        private void timerWriteTenz_Tick(object sender, EventArgs e)
        {
            byte[] tmp = new byte[8] { 0x03, 0x03, 0x00, 0x0E, 0x00, 0x0a, 0xa5, 0xec };
            ushort tet = CRC16(0xFFFF, new byte[] { 0x03, 0x03, 0x00, 0x0E, 0x00, 0x0a }, 6);
            //  UInt16 tst = ModRTU_CRC(tmp,6);
            tmp[7] = (byte)tet; tet >>= 8;
            tmp[6] = (byte)tet;
            try
            {
                serialPort1.Write(tmp, 0, 8);
            } catch(Exception ee)
            {
                timerWriteTenz.Enabled = false;
                MessageBox.Show("Тензометрический датчик не найдет.");
               
            }
        }

        double resultat = 0;
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[25];
            serialPort1.Read(buf, 0, 25);
            if (buf[2] + 3 < 25)
            {
                ushort tet = CRC16(0xFFFF, buf, buf[2] + 3);
                ushort tst = (ushort)((buf[23] << 8) | buf[24]);
                if (tet == tst)
                {
                    int b = buf[2];
                    byte[] res = new byte[4];
                    res[3] = buf[14 + 3];
                    res[2] = buf[15 + 3];
                    res[1] = buf[12 + 3];
                    res[0] = buf[13 + 3];

                    resultat = Math.Round(((Math.Round((BitConverter.ToSingle(res, 0)) / (4.2 * 2.85), 4)) * 1000 - 1.7) * 9.81, 2);

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void timerUpdateTenz_Tick(object sender, EventArgs e)
        {
           textBox5.Text = "" + resultat;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            StartCalculation();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sw == null)
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Chek = Properties.Settings.Default.chek;
            saveFileDialog1.Title = "Укажите место сохранения новой БД";
            saveFileDialog1.FileName = DateTime.Now.ToString("dd_MM_HH_mm_ss") + ".csv";
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                sw = new StreamWriter(saveFileDialog1.FileName, true, Encoding.UTF8);
                string str = ""; ;
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    str += dataGridView1.Columns[i].HeaderText.ToString() + ";";
                sw.WriteLine(str);
                this.Activate();
                textBox1.Focus();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (sw != null)
            {
                string str = textBox1.Text + ";" + textBox2.Text + ";" + textBox3.Text + ";" + textBox7.Text + ";" + textBox6.Text + ";" + textBox4.Text + ";" + textBox14.Text + ";" + textBox8.Text + ";" + textBox9.Text + ";"+ textBox5.Text + ";" + textBox12.Text + ";" + textBox13.Text + ";" + textBox10.Text + ";" + textBox11.Text + ";";
                sw.WriteLine(str);
                dataGridView1.Rows.Add(textBox1.Text, textBox2.Text, textBox3.Text, textBox7.Text, textBox6.Text, textBox4.Text, textBox14.Text,textBox8.Text, textBox9.Text, textBox5.Text, textBox12.Text, textBox13.Text, textBox10.Text, textBox11.Text);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            sw.Close();
            serialPort1.Close();
           
            Properties.Settings.Default.Save();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
                   }

        private void test_textBox(TextBox tb)
        {
            string str = tb.Text;
            if (str != "")
            {
                if (!double.TryParse(str, out l))
                {
                    str = str.Replace(".", ",");
                    textBox7.Text = str;
                    if (!double.TryParse(str, out l)) { MessageBox.Show("Неправильно введено число"); tb.Focus(); }
                }
            }
        }


        private void StartCalculation()
        {
            if (textBox7.Text != "")
            {
                l = Math.Round(double.Parse(textBox7.Text), 4);
            }

            if (textBox6.Text != "")
            {
                D = Math.Round(double.Parse(textBox6.Text), 4);
            }

            if (textBox5.Text != "")
            {
                F = Math.Round(double.Parse(textBox5.Text), 4);
            }

            if (textBox14.Text != "")
            {
              M = Math.Round(double.Parse(textBox14.Text), 4);
            }

            if (textBox8.Text != "")
            {
                t1 = Math.Round(double.Parse(textBox8.Text), 4);
            }

            if (textBox9.Text != "")
            {
                t2 = Math.Round(double.Parse(textBox9.Text), 4);
            }

            
           
            //скорость продольных
            c1 = (int) (Math.Round(l / (t1) * 1000, 0));
            textBox12.Text = "" + c1;
            //скорость поперечных
            c2= (int)(Math.Round(l / (t2) * 1000, 0));
            textBox13.Text = "" + c2;


            //коэффициент пуасона
            double otn = (c1 / c2) ;
            //v = 1.299;
           // v = Math.Round((otn*otn-2)/(2*(otn*otn-1)), 3);
           

            //модуль Юнга
            Vcore = (l / 100.0) * Math.PI * (D / 2.0 / 100.0) * (D / 2.0 / 100.0);
            textBox4.Text = "" + Math.Round(Vcore*1000,2);
           double po = Math.Round((M) / Vcore, 0);
            // v = 0;
            //  for (v = 0; E < 197; v += 0.1)
            //  {
            // double E = Math.Round(c1 * c1 * po * (1 + v) * (1 - 2 * v) / (1 - v) / 1000000, 2);
           

            v = Math.Round(0.5*(1+(1/(1-((c1/c2)*(c1/c2))))),3);
            double E = Math.Round(c1 * c1 * po *(1+v)*(1-2*v)/(1-v)/ 100000000000, 2);
            // }
            textBox11.Text = "" + v;
            textBox10.Text =""+ E;

            
        }

    }
}
