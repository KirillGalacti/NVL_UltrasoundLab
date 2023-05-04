using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.Visa;
using ZedGraph;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace NVL_UltrasoundLab
{
    public partial class Form1 : Form, IDisposable
    {
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x5345, 0x1234);


        private MessageBasedSession mbSession;
        private MessageBasedSession mbSessionGen;
        PointPairList list = new PointPairList();
        PointPairList list2 = new PointPairList();
        PointPairList list3Synh = new PointPairList();
        PointPairList list4Synh = new PointPairList();
        private LineItem myCurve;
        private LineItem myCurve1;
        byte[] str;
        byte[] str2;
        byte[] str3;
        int sdvig = 20;
        byte[] strSynh1;
        byte[] strSynh2;
        byte[] strSynh3;
        //string Tdiv = "";
        double DTdivStep = 0.01;
        bool bit = false;
        double ys = 1;
        INITForm1 main;
        double k;
        double delta = 3;
        double DeltaY = 3.56;
        double initDelayY;
        char[] charsCh1;
        char[] charsCh2;


        public Form1()
        {

            InitializeComponent();

            zedGraphControl1.GraphPane.XAxis.Title.Text = "Время, 10^(-6)c";
            zedGraphControl1.GraphPane.YAxis.Title.Text = "";
            zedGraphControl1.GraphPane.Title.Text = "";
            //zedGraphControl1.GraphPane.IsBoundedRanges = true;

            myCurve = zedGraphControl1.GraphPane.AddCurve("", list, Color.Red, SymbolType.None);
            myCurve1 = zedGraphControl1.GraphPane.AddCurve("", list3Synh, Color.Blue, SymbolType.None);
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -100.0;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 100.0;
            myCurve.Line.Width = 1;
            myCurve1.Line.Width = 1;
            zedGraphControl1.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphControl1.GraphPane.YAxis.MajorGrid.IsVisible = true;

            // !!!
            // Включаем отображение сетки напротив крупных рисок по оси X
            zedGraphControl1.GraphPane.XAxis.MajorGrid.IsVisible = true;

            // Задаем вид пунктирной линии для крупных рисок по оси X:
            // Длина штрихов равна 10 пикселям, ... 
            zedGraphControl1.GraphPane.XAxis.MajorGrid.DashOn = 10;

            // затем 5 пикселей - пропуск
            zedGraphControl1.GraphPane.XAxis.MajorGrid.DashOff = 5;


            // Включаем отображение сетки напротив крупных рисок по оси Y
            zedGraphControl1.GraphPane.YAxis.MajorGrid.IsVisible = true;

            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            zedGraphControl1.GraphPane.YAxis.MajorGrid.DashOn = 10;
            zedGraphControl1.GraphPane.YAxis.MajorGrid.DashOff = 5;


            // Включаем отображение сетки напротив мелких рисок по оси X
            zedGraphControl1.GraphPane.YAxis.MinorGrid.IsVisible = true;

            // Задаем вид пунктирной линии для крупных рисок по оси Y: 
            // Длина штрихов равна одному пикселю, ... 
            zedGraphControl1.GraphPane.YAxis.MinorGrid.DashOn = 1;

            // затем 2 пикселя - пропуск
            zedGraphControl1.GraphPane.YAxis.MinorGrid.DashOff = 2;

            // Включаем отображение сетки напротив мелких рисок по оси Y
            zedGraphControl1.GraphPane.XAxis.MinorGrid.IsVisible = true;

            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            zedGraphControl1.GraphPane.XAxis.MinorGrid.DashOn = 1;
            zedGraphControl1.GraphPane.XAxis.MinorGrid.DashOff = 2;

            DeltaY = Properties.Settings.Default.DeltaY;
            textBox3.Text = "" + (DeltaY * 0.2);

            zedGraphControl1.AxisChange();

            zedGraphControl1.Refresh();

            textBox1.Text = "" + Properties.Settings.Default.TDelayProd;
            textBox2.Text = "" + Properties.Settings.Default.TDelayPop;

            main = this.Owner as INITForm1;

        }

        public double GetTp()
        {
            if (textBox6.Text != "") return double.Parse(textBox6.Text);
            return 0;
        }

        public double GetTs()
        {
            if (textBox8.Text != "") return double.Parse(textBox8.Text);
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            using (var rmSession = new ResourceManager())
            {
                IEnumerable<string> resources = rmSession.Find("(ASRL|GPIB|TCPIP|USB)?*");
                try
                {
                    foreach (string s in resources)
                    {

                        if (s.Contains("0xF4EC::0xEE3A"))
                        {
                            mbSession = (MessageBasedSession)rmSession.Open(s);
                        }
                        if (s.Contains("0x0400::0x09C4"))
                        {
                            mbSessionGen = (MessageBasedSession)rmSession.Open(s);
                        }
                        Console.WriteLine(s);
                    }
                }
                catch (InvalidCastException)
                {
                    MessageBox.Show("Resource selected must be a message-based session");
                }
                catch (Exception exp)
                {

                    MessageBox.Show(exp.Message);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mbSession != null)
            {
                try
                {
                    mbSession.RawIO.Write("C1:WF? ALL\n");
                    if (mbSession.SendEndEnabled)
                    {
                        try
                        {
                            str = mbSession.RawIO.Read(0x2000);
                            str2 = mbSession.RawIO.Read(0x3140);

                            str3 = new byte[str.Length + str2.Length];
                            Array.Copy(str, str3, str.Length);
                            Array.Copy(str2, 0, str3, str.Length, str2.Length);

                            /* mbSession.RawIO.Write("TDIV?\n");
                             if (mbSession.SendEndEnabled)
                             {
                                 try
                                 {
                                     str3 = mbSession.RawIO.Read(15);
                                     var startPos = 5;
                                     var endPos = 12;
                                     var subset = new byte[endPos - startPos + 1];
                                     Array.Copy(str3, startPos, subset, 0, endPos - startPos + 1);
                                     Tdiv = (System.Text.Encoding.UTF8.GetString(subset).TrimEnd('\0'));
                                     Double.TryParse(Tdiv, out DTdivStep);
                                     DTdivStep = Math.Round((0.025) , 5);


                                 }
                                 catch (Exception exp)
                                 {
                                     button3.Text = "Включить";
                                     timerOSC.Enabled = false;
                                     MessageBox.Show(exp.Message);
                                 }
                                 finally
                                 {
                                     Cursor.Current = Cursors.Default;
                                 }

                             }
                             */
                        }
                        catch (Exception exp)
                        {
                            button3.Text = "Включить";
                            timerOSC.Enabled = false;
                            MessageBox.Show(exp.Message);
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                    }
                }
                catch (Exception ew)
                { }
            }


            if (mbSession != null)
            {
                mbSession.RawIO.Write("C2:WF? ALL\n");
                if (mbSession.SendEndEnabled)
                {
                    try
                    {
                        strSynh1 = mbSession.RawIO.Read(0x2000);
                        strSynh2 = mbSession.RawIO.Read(0x3140);

                        strSynh3 = new byte[strSynh1.Length + strSynh2.Length];
                        Array.Copy(strSynh1, strSynh3, strSynh1.Length);
                        Array.Copy(strSynh2, 0, strSynh3, strSynh1.Length, strSynh2.Length);
                        /* mbSession.RawIO.Write("TDIV?\n");
                         if (mbSession.SendEndEnabled)
                         {
                             try
                             {
                                 strSynh3 = mbSession.RawIO.Read(15);
                                 var startPos = 5;
                                 var endPos = 12;
                                 var subset = new byte[endPos - startPos + 1];
                                 Array.Copy(strSynh3, startPos, subset, 0, endPos - startPos + 1);
                                 Tdiv = (System.Text.Encoding.UTF8.GetString(subset).TrimEnd('\0'));
                                 Double.TryParse(Tdiv, out DTdivStep);
                                 DTdivStep = Math.Round((0.025), 5);


                             }
                             catch (Exception exp)
                             {
                                 button3.Text = "Включить";
                                 timerOSC.Enabled = false;
                                 MessageBox.Show(exp.Message);
                             }
                             finally
                             {
                                 Cursor.Current = Cursors.Default;
                             }
                         }
                         */
                    }
                    catch (Exception exp)
                    {
                        button3.Text = "Включить";
                        timerOSC.Enabled = false;
                        MessageBox.Show(exp.Message);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }



        private void timer2_Tick(object sender, EventArgs e)
        {
            int runNull = 0;
            if (str3 != null)
            {
                list.Clear();
                list2.Clear();
                k = 0; int g = 0;
                runNull = ((strSynh3.Length - 559) / 2);
                /* while (strSynh3.ElementAt(runNull) > delta)
                 {
                     runNull--;
                 }

                 /*while((strSynh3.ElementAt(runNull)<delta)&&(g<DeltaY))
                 {
                     runNull--;g++;
                 }
                 */
                // if (g == DeltaY)
                //{
                // g = 0;
                //  runNull = ((strSynh3.Length - 559) / 2);
                while ((runNull < strSynh3.Length) && (strSynh3.ElementAt(runNull) < delta))
                {
                    runNull++; g++;
                }
                // }

                for (var j = 0; j < 4; j++)
                {

                    for (int i = runNull; i < str3.Count() - 1; i++)
                    {
                        double tmp1, tmp2, tmp3;
                        if (str3[i] > 128) tmp1 = (str3[i] - 0xFF) / ys;
                        else tmp1 = ((int)str3[i]) / ys;
                        if (str3[i + 1] > 128) tmp2 = (str3[i + 1] - 0xFF) / ys;
                        else tmp2 = ((int)str3[i + 1]) / ys;
                        if (str3[i - 1] > 128) tmp3 = (str3[i - 1] - 0xFF) / ys;
                        else tmp3 = ((int)str3[i - 1]) / ys;
                        tmp1 = 0.5 * (0.5 * (tmp3 + tmp2) + tmp1);
                        if (tmp1 < 0) str3[i] = Convert.ToByte((int)(255 + tmp1));
                        else str3[i] = Convert.ToByte((int)tmp1);

                        if (j == 3) { list.Add(k, tmp1); k = Math.Round(k + DTdivStep, 4); }
                        // for (var j = 0, m = 4; j < m; j++)
                        //    for (var i = 1, l = a.length - 1; i < l; i++)
                        //      a[i] = 0.5 * (0.5 * (a[i - 1] + a[i + 1]) + a[i]);

                        // if (str[i] > 128) list.Add(k, (str[i] - 0xFF) / ys);
                        //else list.Add(k, ((int)str[i]) / ys);

                    }
                }
                int countList = (str.Count() + str2.Count() - 559) / 2;
                //if((str.Count()+str2.Count()-559)/2 > str.Count()) то первый наф
                //  else {от str.Count()+str2.Count()-559)/2 до str.Count() читаем в лист}
                /*  if ((str2.Count() - str.Count() - 559) / 2 < str2.Count())
                  {
                      for (int i = (str2.Count() - 559- str.Count()) / 2; i < str2.Count(); i++)
                      {
                          if (str2[i] > 128) list.Add(k, (str2[i] - 0xFF) / ys);
                          else list.Add(k, ((int)str2[i]) / ys);
                          k = Math.Round(k + DTdivStep, 4);
                      }

                  }*/
                //
                //
                /*  if (str2 != null)
                  {

                      for (int i = 1; i < str2.Count()-1; i++)
                      {
                          if (str2[i] > 128) list.Add(k, (str2[i] - 0xFF) / ys);
                          else list.Add(k, ((int)str2[i]) / ys);
                          k = Math.Round(k + DTdivStep, 4);

                      }
                      //середина!


                  }  */
            }


            if (strSynh3 != null)
            {
                list3Synh.Clear();
                list4Synh.Clear();
                double k1 = 0;

                for (var j = 0; j < 4; j++)
                    for (int i = runNull; i < strSynh3.Count() - 1; i++)
                    {

                        double tmp1, tmp2, tmp3;
                        if (strSynh3[i] > 128) tmp1 = (strSynh3[i] - 0xFF) / ys;
                        else tmp1 = ((int)strSynh3[i]) / ys;
                        if (strSynh3[i + 1] > 128) tmp2 = (strSynh3[i + 1] - 0xFF) / ys;
                        else tmp2 = ((int)strSynh3[i + 1]) / ys;
                        if (strSynh3[i - 1] > 128) tmp3 = (strSynh3[i - 1] - 0xFF) / ys;
                        else tmp3 = ((int)strSynh3[i - 1]) / ys;
                        tmp1 = 0.5 * (0.5 * (tmp3 + tmp2) + tmp1);

                        if (tmp1 < 0) strSynh3[i] = Convert.ToByte((int)(255 + tmp1));
                        else strSynh3[i] = Convert.ToByte((int)tmp1);

                        //strSynh3[i] = Convert.ToByte(tmp1);

                        if (j == 3) { list3Synh.Add(k1, tmp1); k1 = Math.Round(k1 + DTdivStep, 4); }

                        //list3Synh.Add(k1, 0.5 * (0.5 * (tmp3 + tmp2) + tmp1));
                        // if (strSynh3[i] > 128) list3Synh.Add(k1, (strSynh3[i] - 0xFF) / ys);
                        // else list3Synh.Add(k1, ((int)strSynh3[i]) / ys);

                    }

                //int countListS = (str.Count() + str2.Count() - 559) / 2;
                //if((str.Count()+str2.Count()-559)/2 > str.Count()) то первый наф
                //  else {от str.Count()+str2.Count()-559)/2 до str.Count() читаем в лист}
                /*  if ((str2.Count() - str.Count() - 559) / 2 < str2.Count())
                  {
                      for (int i = (str2.Count() - 559- str.Count()) / 2; i < str2.Count(); i++)
                      {
                          if (str2[i] > 128) list.Add(k, (str2[i] - 0xFF) / ys);
                          else list.Add(k, ((int)str2[i]) / ys);
                          k = Math.Round(k + DTdivStep, 4);
                      }

                  }*/
                //
                //
                /* if (strSynh2 != null)
                 {

                     for (int i = 0; i < strSynh2.Count(); i++)
                     {
                         if (strSynh2[i] > 128) list3Synh.Add(k1, (strSynh2[i] - 0xFF) / ys);
                         else list3Synh.Add(k1, ((int)strSynh2[i]) / ys);
                         k1 = Math.Round(k1 + DTdivStep, 4);

                     }
                     //середина!
                     



                 }
                 */

                /*   runNull = (list3Synh.Count / 2);
                   while (list3Synh.ElementAt(runNull).Y > delta)
                   {
                       runNull--;
                   }
                   list4Synh.AddRange(list3Synh.Skip(runNull));
                   list3Synh.Clear();
                   k1 = 0;
                   for (int i = 0; i < list4Synh.Count(); i++)
                   {
                       list3Synh.Add(k1, list4Synh.ElementAt(i).Y);
                       k1 = Math.Round(k1 + DTdivStep, 4);
                   }

                   list2.AddRange(list.Skip(runNull));
                   list.Clear();
                   k = 0;
                   for (int i = 0; i < list2.Count(); i++)
                   {
                       list.Add(k, list2.ElementAt(i).Y);
                       k = Math.Round(k + DTdivStep, 4);
                   }
                   */
                if (bit)
                {
                    if (!textBox8.ReadOnly)
                    {
                        double t = point.ElementAt(0).X;
                        double a = point.ElementAt(0).Y;
                        int number = (int)(t / DTdivStep);

                        try
                        {
                            int i = number;
                            //if (number - 20 < 0) i = 0;
                            //else i = number  ;
                            while (Math.Abs(list.ElementAt(i).Y) >= DeltaY) { i--; }

                            if ((list.ElementAt(i).X - t) < 10)
                            {
                                number = i;
                            }
                            double testq = list.ElementAt(i).Y;
                        }
                        catch (Exception we) { }

                        // double tt = list.ElementAt(1).X;
                        //double mno = Math.Round(1 / tt, 2);

                        /*
                                            double n = point.ElementAt(0).X;
                                            double i = Math.Round(n - (DeltaY / 2), 4);
                                            double imax = Math.Round(n + (DeltaY / 2), 4);

                                            List<double> testX = new List<double>();
                                            List<double> testY = new List<double>();

                                            foreach (PointPair item in list)
                                            {
                                                if ((item.X > i) && (item.X < imax))
                                                {
                                                    testX.Add(item.X);
                                                    testY.Add(item.Y);
                                                }
                                            }

                                            double NewX
                                            double yMin = testY.Min();
                                            if((testX.ElementAt(testY.IndexOf(yMin)) == i)|| (testX.ElementAt(testY.IndexOf(yMin)) == i))
                                            {

                                            }







                                             = testX.ElementAt(testY.IndexOf(yMin));*/
                        point.Clear();
                        GraphPane pane = zedGraphControl1.GraphPane;
                        point.Add(list.ElementAt(number).X, list.ElementAt(number).Y);
                        if (pane.CurveList.Count > 2)
                            pane.CurveList.RemoveAt(2);
                        // Кривая, состоящая из одной точки. Точка будет отмечена синим кругом
                        LineItem curvePount = pane.AddCurve("",
                            point,
                            Color.Blue,
                            SymbolType.Circle);
                        curvePount.Line.IsVisible = false;

                        // Цвет заполнения круга - колубой
                        curvePount.Symbol.Fill.Color = Color.Blue;

                        // Тип заполнения - сплошная заливка
                        curvePount.Symbol.Fill.Type = FillType.Solid;

                        // Размер круга
                        curvePount.Symbol.Size = 7;

                        //пересчет!!!!
                        if (!textBox6.ReadOnly)
                        {
                            textBox6.Text = "" + (list.ElementAt(number).X - double.Parse(textBox2.Text));
                        }
                        else
                        {
                            textBox8.Text = "" + (list.ElementAt(number).X - double.Parse(textBox1.Text));
                        }
                    }
                    else
                    {
                        double t = point2.ElementAt(0).X;
                        double a = point2.ElementAt(0).Y;
                        int number = (int)(t / DTdivStep);

                        try
                        {
                            int i = number;
                            //if (number - 20 < 0) i = 0;
                            //else i = number  ;
                            while (Math.Abs(list.ElementAt(i).Y) >= DeltaY) { i--; }

                            if ((list.ElementAt(i).X - t) < 10)
                            {
                                number = i;
                            }
                            double testq = list.ElementAt(i).Y;
                        }
                        catch (Exception we) { }




                        // double tt = list.ElementAt(1).X;
                        //double mno = Math.Round(1 / tt, 2);

                        /*
                                            double n = point.ElementAt(0).X;
                                            double i = Math.Round(n - (DeltaY / 2), 4);
                                            double imax = Math.Round(n + (DeltaY / 2), 4);

                                            List<double> testX = new List<double>();
                                            List<double> testY = new List<double>();

                                            foreach (PointPair item in list)
                                            {
                                                if ((item.X > i) && (item.X < imax))
                                                {
                                                    testX.Add(item.X);
                                                    testY.Add(item.Y);
                                                }
                                            }

                                            double NewX
                                            double yMin = testY.Min();
                                            if((testX.ElementAt(testY.IndexOf(yMin)) == i)|| (testX.ElementAt(testY.IndexOf(yMin)) == i))
                                            {

                                            }







                                             = testX.ElementAt(testY.IndexOf(yMin));*/
                        point2.Clear();
                        GraphPane pane = zedGraphControl1.GraphPane;
                        point2.Add(list.ElementAt(number).X, list.ElementAt(number).Y);
                        if (pane.CurveList.Count > 3)
                            pane.CurveList.RemoveAt(3);
                        // Кривая, состоящая из одной точки. Точка будет отмечена синим кругом
                        LineItem curvePount2 = pane.AddCurve("",
                            point2,
                            Color.Green,
                            SymbolType.Circle);
                        curvePount2.Line.IsVisible = false;

                        // Цвет заполнения круга - колубой
                        curvePount2.Symbol.Fill.Color = Color.Green;

                        // Тип заполнения - сплошная заливка
                        curvePount2.Symbol.Fill.Type = FillType.Solid;

                        // Размер круга
                        curvePount2.Symbol.Size = 7;

                        //пересчет!!!!
                        if (!textBox6.ReadOnly)
                        {
                            textBox6.Text = "" + (list.ElementAt(number).X - double.Parse(textBox2.Text));
                        }
                        else
                        {
                            textBox8.Text = "" + (list.ElementAt(number).X - double.Parse(textBox1.Text));
                        }

                    }
                }
                myCurve.Line.IsSmooth = true;
                myCurve.Line.SmoothTension = 0.5F;
                myCurve1.Line.IsSmooth = true;
                myCurve1.Line.SmoothTension = 0.5F;
                zedGraphControl1.AxisChange();
                zedGraphControl1.Refresh();
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 29," + 5 + "\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 29,-" + 5 + "\n");
        }
        bool bittrig = false;
        private void button3_Click(object sender, EventArgs e)
        {
            // MessageBox.Show(System.Text.Encoding.UTF8.GetString(str3).TrimEnd('\0'));
            if (!bittrig) { System.Diagnostics.Process.Start("MP709.exe", "RELE_1=ON"); bittrig = true; }
            else { System.Diagnostics.Process.Start("MP709.exe", "RELE_1=OFF"); bittrig = false; }
            if (ys == 38.6) ys = 1;
            else ys = 38.6;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (!timerOSC.Enabled)
            {
                if (button11.Text.Equals("Продольные (1)"))
                {
                    timer_init = 0;
                    timer_init_Gen_prod.Enabled = true;
                }
                else
                {
                    timer_init = 0;
                    timer_init_Gen_pop.Enabled = true;
                }
                timer2.Enabled = true;
                timerOSC.Enabled = true;
                button3.Text = "Отключить";
            }
            else
            {
                timer2.Enabled = false;
                timerOSC.Enabled = false;
                button3.Text = "Включить";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (button11.Text.Equals("Продольные (1)"))
            {
                bit = false;
                point.Clear();
                point2.Clear();
                textBox6.ReadOnly = true;
                textBox8.ReadOnly = false;
                timer_init = 0;
                timer_init_Gen_prod.Enabled = true;
                button11.Text = "Поперечные (2)";
                this.Text = "График продольных волн";
            }
            else
            {
                bit = false;

                textBox6.ReadOnly = false;
                textBox8.ReadOnly = true;
                timer_init = 0;
                timer_init_Gen_pop.Enabled = true;
                button11.Text = "Продольные (1)";
                this.Text = "График поперечных волн";
            }
        }

        int timer_init = 0;
        private void timer_init_Gen_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (timer_init)
                {
                    case 0: mbSessionGen.RawIO.Write("VOLT:HIGH:CH2 0\n"); break;
                    case 1: mbSessionGen.RawIO.Write("VOLT:LOW:CH2 0\n"); break;
                    case 2: mbSessionGen.RawIO.Write("FUNC PULS\n"); break;
                    case 3: mbSessionGen.RawIO.Write("FREQ 1000\n"); break;
                    case 4: mbSessionGen.RawIO.Write("VOLT:HIGH 5\n"); break;
                    case 5: mbSessionGen.RawIO.Write("VOLT:LOW 0"); break;
                    case 6: mbSessionGen.RawIO.Write("PULS:WIDT 0.000001\n"); break;
                    case 7: mbSessionGen.RawIO.Write("TRIG:SOUR IMM\n"); break;
                    case 8: mbSessionGen.RawIO.Write("OUTP:SYNC ON\n"); break;
                    case 9: mbSessionGen.RawIO.Write("OUTP:CH2 ON\n"); break;
                    case 10: mbSessionGen.RawIO.Write("OUTP ON\n"); timer_init -= 1; timer_init_Gen_prod.Enabled = false; break;
                }
                timer_init++;
            }
            catch (Exception ee)
            {
                timer_init_Gen_prod.Enabled = false;
                MessageBox.Show("Генератор продольных волн не найден");

            }
        }

        private void timer_init_Gen_pop_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (timer_init)
                {
                    case 0: mbSessionGen.RawIO.Write("VOLT:HIGH 0\n"); break;
                    case 1: mbSessionGen.RawIO.Write("VOLT:LOW 0\n"); break;
                    case 2: mbSessionGen.RawIO.Write("FUNC:CH2 PULS\n"); break;
                    case 3: mbSessionGen.RawIO.Write("FREQ:CH2 1000\n"); break;
                    case 4: mbSessionGen.RawIO.Write("VOLT:HIGH:CH2 5\n"); break;
                    case 5: mbSessionGen.RawIO.Write("VOLT:LOW:CH2 0"); break;
                    case 6: mbSessionGen.RawIO.Write("PULS:WIDT:CH2 0.000001\n"); break;
                    case 7: mbSessionGen.RawIO.Write("TRIG:SOUR IMM\n"); break;
                    case 8: mbSessionGen.RawIO.Write("OUTP:SYNC ON\n"); break;
                    case 9: mbSessionGen.RawIO.Write("OUTP:CH2 ON\n"); break;
                    case 10: mbSessionGen.RawIO.Write("OUTP ON\n"); timer_init -= 1; timer_init_Gen_pop.Enabled = false; break;
                }
                timer_init++;
            }
            catch (Exception ee)
            {
                timer_init_Gen_pop.Enabled = false;
                MessageBox.Show("Генератор поперечных волн не найден");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 29," + 0 + "\n");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 38," + 0 + "\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 38,-" + 3 + "\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (timerOSC.Enabled)
                mbSession.RawIO.Write("$$SY_FP 38," + 3 + "\n");
        }

        private void timer_Init_OSC_Tick(object sender, EventArgs e)
        {
            try
            {
                switch (timer_init)
                {
                    case 0: mbSession.RawIO.Write("C1:VDIV 2E-2 V\n"); break;
                    case 1: mbSession.RawIO.Write("C2:VDIV 2 V\n"); break;
                    case 2: mbSession.RawIO.Write("TDIV 2.5US\n"); break;
                    case 3: mbSession.RawIO.Write("TRSE EDGE,SR,CH2,HT,TI,HV,100NS\n"); timer_init -= 1; timer_Init_OSC.Enabled = false; break;
                }
                timer_init++;
            }
            catch (Exception ee)
            {
                timer_Init_OSC.Enabled = false;
                MessageBox.Show("Осцилограф не найден");
            }
        }

        PointPairList point = new PointPairList();
        PointPairList point2 = new PointPairList();

        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!textBox8.ReadOnly)
            {
                // Сюда будут записаны координаты в системе координат графика
                CurveItem curve;
                //curve.Clear();
                // Сюда будет сохранен номер точки кривой, ближайшей к точке клика
                int index;

                GraphPane pane = zedGraphControl1.GraphPane;

                // Максимальное расстояние от точки клика до кривой в пикселях, 
                // при котором еще считается, что клик попал в окрестность кривой.
                GraphPane.Default.NearestTol = 10;

                bool result = pane.FindNearestPoint(e.Location, out curve, out index);
                //curve.Clear();
                if (result)
                {
                    // Максимально расстояние от точки клика до кривой не превысило NearestTol

                    // Добавим точку на график, вблизи которой произошел клик


                    point.Add(curve[index]);

                    // Кривая, состоящая из одной точки. Точка будет отмечена синим кругом
                    LineItem curvePount = pane.AddCurve("",
                        new double[] { curve[index].X },
                        new double[] { curve[index].Y },
                        Color.Blue,
                        SymbolType.Circle);

                    // 
                    curvePount.Line.IsVisible = false;

                    // Цвет заполнения круга - колубой
                    curvePount.Symbol.Fill.Color = Color.Blue;

                    // Тип заполнения - сплошная заливка
                    curvePount.Symbol.Fill.Type = FillType.Solid;

                    // Размер круга
                    curvePount.Symbol.Size = 7;

                    // label1.Text = "" + curve[index].X;
                }
                this.zedGraphControl1.MouseClick -= new System.Windows.Forms.MouseEventHandler(this.zedGraphControl1_MouseClick);
            }
            else
            {
                // Сюда будут записаны координаты в системе координат графика
                CurveItem curve;
                //curve.Clear();
                // Сюда будет сохранен номер точки кривой, ближайшей к точке клика
                int index;

                GraphPane pane2 = zedGraphControl1.GraphPane;

                // Максимальное расстояние от точки клика до кривой в пикселях, 
                // при котором еще считается, что клик попал в окрестность кривой.
                GraphPane.Default.NearestTol = 10;

                bool result = pane2.FindNearestPoint(e.Location, out curve, out index);
                //curve.Clear();
                if (result)
                {
                    // Максимально расстояние от точки клика до кривой не превысило NearestTol

                    // Добавим точку на график, вблизи которой произошел клик


                    point2.Add(curve[index]);
                    if (pane2.CurveList.Count > 3)
                        pane2.CurveList.RemoveAt(3);
                    // Кривая, состоящая из одной точки. Точка будет отмечена синим кругом
                    LineItem curvePount2 = pane2.AddCurve("",
                        new double[] { curve[index].X },
                        new double[] { curve[index].Y },
                        Color.Green,
                        SymbolType.Circle);

                    // 
                    curvePount2.Line.IsVisible = false;

                    // Цвет заполнения круга - колубой
                    curvePount2.Symbol.Fill.Color = Color.Green;

                    // Тип заполнения - сплошная заливка
                    curvePount2.Symbol.Fill.Type = FillType.Solid;

                    // Размер круга
                    curvePount2.Symbol.Size = 7;

                    // label1.Text = "" + curve[index].X;
                }
                this.zedGraphControl1.MouseClick -= new System.Windows.Forms.MouseEventHandler(this.zedGraphControl1_MouseClick);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (point.Count > 0) bit = true;
            else bit = false;

        }

        private void button8_Click(object sender, EventArgs e)
        {
            bit = false;
            if (!textBox8.ReadOnly)
                point.Clear();
            else
                point2.Clear();
            this.zedGraphControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.zedGraphControl1_MouseClick);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (button4.Text == "Изменить")
            {
                textBox1.ReadOnly = false;
                textBox2.ReadOnly = false;
                button4.Text = "Записать";
            }
            else
            {

                double t1, t2;
                if (!double.TryParse(textBox1.Text.Replace(",", "."), out t1)) { MessageBox.Show("Введите число корректно"); textBox1.Focus(); }
                if (!double.TryParse(textBox2.Text.Replace(",", "."), out t2)) { MessageBox.Show("Введите число корректно"); textBox2.Focus(); }

                if (double.TryParse(textBox1.Text.Replace(",", "."), out t1) && double.TryParse(textBox2.Text.Replace(",", "."), out t2))
                {
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    button4.Text = "Изменить";
                    Properties.Settings.Default.TDelayPop = t2;
                    Properties.Settings.Default.TDelayProd = t1;
                    Properties.Settings.Default.Save();

                }

            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            main = this.Owner as INITForm1;
            main.t1 = GetTp();
            main.t2 = GetTs();
            if (main.t1 != 0) main.textBox9.Text = "" + main.t1;
            if (main.t2 != 0) main.textBox8.Text = "" + main.t2;

            this.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the
                // variable will be null indicating this is an interface of a
                // device.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used,
                    // the desired configuration and interface must be selected.

                    // Select config
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface
                    wholeUsbDevice.ClaimInterface(1);
                }

                // open read endpoint
                UsbEndpointReader reader =
                    MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // open write endpoint
                UsbEndpointWriter writer =
                    MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep03);

                // write data, read data
                int bytesWritten;
                // 53 54 41 52 54 42 49 4E                           STARTBIN

                ec = writer.Write(new byte[] { 0x53, 0x54, 0x41, 0x52, 0x54, 0x42, 0x49, 0x4E }, 2000, out bytesWritten); // specify data to send

                if (ec != ErrorCode.None)
                    throw new Exception(UsbDevice.LastErrorString);

                byte[] readBuffer = new byte[0xc];

                int bytesRead;

                // If the device hasn't sent data in the last 100 milliseconds,
                // a timeout error (ec = IoTimedOut) will occur.
                ec = reader.Read(readBuffer, 100, out bytesRead);

                if (bytesRead == 0) throw new Exception("No more bytes!");

                // Write that output to the console.
                Console.WriteLine(BitConverter.ToString(readBuffer, 0, bytesRead));

                byte[] readBuffer2 = new byte[0x1831];

                int bytesRead2;

                // If the device hasn't sent data in the last 100 milliseconds,
                // a timeout error (ec = IoTimedOut) will occur.
                ec = reader.Read(readBuffer2, 600, out bytesRead2);

                if (bytesRead2 == 0) throw new Exception("No more bytes2!");
                list.Clear();
                int z = 0;
                int raz = 6193;// (6193 / 2)+113;
                for (int k = 113; k < raz; k += 2)
                {
                    if (readBuffer2[k] > 128) list.Add(z, (readBuffer2[k] - 0xFF));
                    else list.Add(z, readBuffer2[k]);
                    z++;

                    //                    list.Add(k - 113, readBuffer2[k]);

                }
                timer2.Enabled = true;
                // Write that output to the console.
                Console.WriteLine(BitConverter.ToString(readBuffer2, 0, bytesRead2));



                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the
                        // 'wholeUsbDevice' variable will be null indicating this is
                        // an interface of a device; it does not require or support
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface
                            wholeUsbDevice.ReleaseInterface(1);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }

                // Wait for user input..
                // Console.ReadKey();
            }
        }

        private void zedGraphControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -100.0;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 100.0;

            zedGraphControl1.AxisChange();

            zedGraphControl1.Refresh();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox3.Text, out DeltaY)) { MessageBox.Show("Ошибка ввода."); textBox3.Focus(); }
            else
            {
                Properties.Settings.Default.DeltaY = DeltaY / 0.2;
                Properties.Settings.Default.Save();
            }
        }
    }

}
