using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPlotLog.Serial;
using System.IO.Ports;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using System.Windows.Media;

namespace SerialPlotLog
{
    public partial class MainForm : Form
    {
        SerialPortManager _spManager;
        long rt = 0; 
        List<Reading> readings = new List<Reading>();
        List<float> autocorrelation = new List<float>();
        List<float> autocorr = new List<float>();
        int rtrd = 0;
        int rtrds = 0;

        public float Mean(List<float> x)
        {
            float sum = 0;
            for (int i = 0; i < x.Count; i++)
                sum += x[i];
            return sum / x.Count;
        }

        public List<float> Autocorrelation(List<float> o)
        {
            
            float mean = Mean(o);
            for (int t = 0; t < o.Count/2; t++)
            {
                float n = 0; // Numerator
                float d = 0; // Denominator

                for (int i = 0; i < o.Count; i++)
                {
                    float xim = o[i] - mean;
                    n += xim * (o[(i + t) % o.Count] - mean);
                    d += xim * xim;
                }

                float hitung = n / d;

                autocorr.Add(hitung);
            }

            return autocorr;
        }

        List<string> listdata = new List<string>();
        List<float> listfloat = new List<float>();
        

        public class Reading
        {
            public double nilai { get; set; }
            public DateTime Waktu { get; set; }
            public string[] ArrayNilai { get; set; }
            public int rtr { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();

            UserInitialization();

            ChartExample();
        }


        private void UserInitialization()
        {
            _spManager = new SerialPortManager();
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }

        private void ChartExample()
        {

            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Data Serial",
                    Values = new ChartValues<float> {},
                    LineSmoothness = 0,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 4
                }
            };

            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "Data",
                LabelFormatter = rt => rt.ToString()
            });

            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "Intensity",
                LabelFormatter = value => value.ToString()
            });

            cartesianChart1.LegendLocation = LegendLocation.Right;

            cartesianChart2.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "ACF",
                    Values = new ChartValues<float> {},
                    LineSmoothness = 0,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 1,
                    Stroke = Brushes.Green
                }
            };

            cartesianChart2.AxisX.Add(new Axis
            {
                Title = "Data",
                LabelFormatter = rt => rt.ToString()
            });

            cartesianChart2.AxisY.Add(new Axis
            {
                Title = "AC",
                LabelFormatter = value => value.ToString()
            });

            cartesianChart2.LegendLocation = LegendLocation.Right;

            //modifying the series collection will animate and update the chart
            //cartesianChart1.Series.Add(new LineSeries
            //{
            //    Values = new ChartValues<double> { 5, 3, 2, 4, 5 },
            //    LineSmoothness = 0, //straight lines, 1 really smooth lines
            //    PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            //    PointGeometrySize = 50,
            //    PointForeround = System.Windows.Media.Brushes.Gray
            //});

            //modifying any series values will also animate and update the chart
            //cartesianChart1.Series[0].Values.Add(5d);


            cartesianChart1.DataClick += CartesianChart1OnDataClick;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();
        }

        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {

            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 1000; // maximum text length in text box
            if (tbData.TextLength > maxTextLength)
                tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

            // This application is connected to a GPS sending ASCCI characters, so data is converted to text
            //string str = Encoding.ASCII.GetString(e.Data);

            //tbData.AppendText(str);

            string str = e.Nilai;
            tbData.AppendText(str);
            tbData.AppendText("\r\n");
            tbData.ScrollToCaret();
            /*
            var reading = new Reading
            {
                ArrayNilai = e.Nilai
                //rtr = e.Nilai.Count()
            };
            */
            //readings.Add(reading);

            listdata.Add(e.Nilai);
            listfloat.Add(float.Parse(e.Nilai));
            rt += 1;
            rtrd = listdata.Count();
            if (rtrd.Equals(int.Parse(textBox1.Text)))
            {
                _spManager.StopListening();
                listfloat.RemoveAt(0);
                rtrds = listfloat.Count;

            }
            

           
            //cartesianChart1.Series[0].Values = listfloat;
            //cartesianChart1.Series[0].Values.Add(reading.ArrayNilai);



            //if (rtr == 9)
            //{
            //    _spManager.StopListening();
            //}

            // var reading = new Reading
            //{

            // };

            // readings.Add(reading); 

            /*
            string nilai =  
            int i;
            for (i = 0; i < 33; i++)
            {
                if (e.Data[i] != 44)
                {

                }
            }


            byte[] newData = new byte[3];
            Array.Copy(e.Data, 0, newData, 0,3);
            //for (int dataloop = 0; dataloop < 33; dataloop++ )
            //{
            string strg = Encoding.ASCII.GetString(newData);
                cartesianChart1.Series[0].Values.Add(newData);
            //}
            */
            //rt += 1;





        }


        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            listdata.Clear();
            _spManager.StartListening();
            
            //MessageBox.Show(cartesianChart1.Series[0].Values[1].ToString());
            /*
            try
            {
                string pathfile = @"D:\";
                string filename = "Data.csv";
                using (FileStream fs = new FileStream(
                    Path.Combine(pathfile, filename),
                    FileMode.Append,
                    FileAccess.Write))
                {
                    using (StreamWriter s = new StreamWriter(fs))
                    {
                        foreach (var reading in readings)
                        {
                            // format the output
                            s.WriteLine(
                              "{0},{1}",//,{2}",
                              reading.Waktu,
                              //jam,
                              reading.nilai
                            );
                        }
                    }
                    MessageBox.Show("Data has been saved to " + pathfile, "Save File");
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error");
            } */
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            //_spManager.Write();

            string rtrs = rtrds.ToString();
            _spManager.StopListening();
            MessageBox.Show(rtrs);



        }

        public void button3_Click(object sender, EventArgs e)
        {
            cartesianChart1.Series[0].Values.Clear();
            cartesianChart2.Series[0].Values.Clear();

            for (int uu = 0; uu < listfloat.Count; uu++)
            {
                cartesianChart1.Series[0].Values.Add(listfloat[uu]);
            }

            autocorrelation = Autocorrelation(listfloat);
            

            for (int uuu = 0; uuu < listfloat.Count/2; uuu++)
            {
                cartesianChart2.Series[0].Values.Add(autocorrelation[uuu]);
            }

            // cartesianChart1.Series[0].Values.Add(ArrayNilai);

            //Random rnd = new Random();

            //var reading = new Reading
            //{
            //    Waktu = DateTime.Now,
            //    nilai = rnd.Next(10, 100)
            //};
            //readings.Add(reading);
        }

        private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("You clicked (" + chartPoint.X + "," + chartPoint.Y + ")");
        }
    }

}

