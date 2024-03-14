using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using CsvHelper;

namespace PCBuildAssistant
{
    public partial class Form1 : Form
    {
        private const string _socketAll = "Any";

        //источники данных для комбобоксов
        private BindingSource _bsSockets;
        private BindingSource _bsCpus;

        //данные
        private List<SOCKETintel> _records;





        // Forma, inicializaciya componentov i dr.
        public Form1()

        {
            InitializeComponent(); // Inicializaciya vsex componentov;
            tabControl1.DrawItem += TabControl1_DrawItem; // Otobrajenie vkladok sleva (vrode);
            comboBox11.Items.AddRange(new object[] { "Intel", "Amd" }); // Dobavlenie Itemov v ComboBox13;
            label30.Text = OSvers(); // Operating System Version;
            label32.Text = CPUid(); // CPU name;
            label33.Text = GPUid(); // GPU name;
            label36.Text = DiskLetter() + Diskid(); // HDD name;
            label113.Text = @"All space: " + Disksize() + "\n" + " FreeSpace: " + FreeSpace(); // HDD size;
            label31.Text = Mthrboard() + @" " + Mthrboard1(); // MotherBoard name;
            label35.Text = RAM0() + @" " + RAM1(); // RAM (slot + name);
            tabPage1.ToolTipText = @"This page will help you compare components."; // Vsplivauwaya podskazka dlya tabPage1;
            tabPage2.ToolTipText = @"This page will help you build a PC."; // Vsplivauwaya podskazka dlya tabPage2;
            tabPage3.ToolTipText = @"This page will show you the recommended builds."; // Vsplivauwaya podskazka dlya tabPage3;
            tabPage4.ToolTipText = @"This page will show you information about your PC."; // Vsplivauwaya podskazka dlya tabPage4;
            tabPage5.ToolTipText = @"This page will show you the PC you assembled."; // Vsplivauwaya podskazka dlya tabPage5;
            tabPage6.ToolTipText = @"This page will show you some popular Intel builds."; // Vsplivauwaya podskazka dlya tabPage6;
            tabPage7.ToolTipText = @"This page will show you some popular AMD builds."; // Vsplivauwaya podskazka dlya tabPage7;
            toolTip1.SetToolTip(comboBox1, comboBox1.Text);

            //привязка у комбобокса сокетов
            _bsSockets = new BindingSource();
            _bsSockets.DataSource = typeof(List<String>);
            comboBox12.DataSource = _bsSockets;

            //привязка у комбобокса процессоров
            _bsCpus = new BindingSource();
            _bsCpus.DataSource = typeof(List<SOCKETintel>);
            comboBox13.DataSource = _bsCpus;
            comboBox13.DisplayMember = nameof(SOCKETintel.CPU_Name);

            this.Load += Form1_Load;
        }

        // Zagryzka formi i ob'ektov ykazanix v nei;
        private void Form1_Load(object sender, EventArgs e)
        {
            // Sozdanie lista CPU;

            List<CPU> cpus = CPUs.GetCPUValues(); // Polu4enie spiska CPU;
            foreach (CPU cpu in cpus) // dlya kajdogo CPU v liste CPU poly4it' spisok;
            {
                comboBox1.Items.Add(cpu); // ComboBox1 = CPU;
                comboBox2.Items.Add(cpu); // ComboBox2 = CPU;
            }

            // Sozdanie lista GPU;

            List<GPU> cards = GPUs.GetGPUValues(); // dlya kajdogo GPU v liste GPU poly4it' spisok;
            foreach (GPU card in cards)
            {
                comboBox3.Items.Add(card); // ComboBox3 = GPU;
                comboBox4.Items.Add(card); // ComboBox4 = GPU;
            }

            // Sozdanie lista RAM;

            List<RAM> rams = RAMs.GetRAMValues(); // Polu4enie spiska RAM;
            foreach (RAM ram in rams) // dlya kajdogo RAM v liste RAM poly4it' spisok;
            {
                comboBox5.Items.Add(ram); // ComboBox5 = RAM;
                comboBox6.Items.Add(ram); // ComboBox6 = RAM;
            }

            // Sozdanie lista SSD;

            List<SSD> ssds = SSDs.GetSSDValues(); // Polu4enie spiska SSD;
            foreach (SSD ssd in ssds)
            {
                comboBox7.Items.Add(ssd); // ComboBox7 = SSD;
                comboBox8.Items.Add(ssd); // ComboBox8 = SSD;
            }

            // Sozdanie lista HDD;

            List<HDD> hdds = HDDs.GetHDDValues(); // Polu4enie spiska HDD;
            foreach (HDD hdd in hdds)
            {
                comboBox9.Items.Add(hdd); // ComboBox9 = HDD;
                comboBox10.Items.Add(hdd); // ComboBox10 = HDD;
            }

            //получаем данные
            _records = SOCKETsIntel.GetSOCKETintelValues();

            //заполняем комбобокс сокетов
                _bsSockets.Add(_socketAll);
                _records.Select(r => r.Socketintel)
                        .Distinct()
                        .ToList()
                        .ForEach(s => _bsSockets.Add(s));

            //заполняем комбобокс процессоров
            UpdateComboBoxCpus(_socketAll);

            //подписка на событие выбора в комбобоксе сокетов
            _bsSockets.CurrentChanged += (s, a) => UpdateComboBoxCpus(_bsSockets.Current as String);

        }

        // Sobitie dlya Updata ComboboxCPUs
        private void UpdateComboBoxCpus(string selectedValue)
        {
            _bsCpus.Clear();

            if (selectedValue.Equals(_socketAll))
            {
                //полный список
                _records.ForEach(r => _bsCpus.Add(r));
            }
            else
            {
                //список с выборкой по сокету
                _records.Where(r => r.Socketintel.Equals(selectedValue))
                        .ToList()
                        .ForEach(r => _bsCpus.Add(r));
            }
        }

        // Otobrajenie vkladok sleva;
        private void TabControl1_DrawItem(Object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush textBrush;

            // Get the item from the collection.
            TabPage tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            var tabBounds = tabControl1.GetTabRect(e.Index);
            var brush2 = new SolidBrush(Color.FromArgb(255, 68, 68, 70)); // Black color for tabpages;
            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                textBrush = new SolidBrush(Color.White);
                g.FillRectangle(brush2, e.Bounds);
            }
            else
            {
                textBrush = new SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font tabFont = new Font("Times New Roman", (float)12.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat stringFlags = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(tabPage.Text, tabFont, textBrush, tabBounds, new StringFormat(stringFlags));
        }

        // Polu4enie puti do programmi;
        public static string GetPath = (Directory.GetCurrentDirectory() + @"\");

        // Sozdanie classa dlya vsex CPU;
        public static class CPUs
        {
            public static List<CPU> GetCPUValues()
            {
                Ping ping = new Ping();
                PingReply pingReply = null;

                pingReply = ping.Send("8.8.8.8");
                if (pingReply.Status.ToString() == "Success")

                {
                    // Ska4ka CPU_new;
                    WebClient wc = new WebClient();
                    string url = "https://www.userbenchmark.com/resources/download/csv/CPU_UserBenchmarks.csv"; // Sdelat' proverky na podklu4enie k inety;
                    string savePath = GetPath; // AvtoPut';
                    string name = "CPU_UserBenchmarks.csv";
                    wc.DownloadFile(url, savePath + name);
                }

                else
                { 
                    MessageBox.Show(Convert.ToString(pingReply.Address + "\n" + pingReply.Status + "\n" + pingReply.RoundtripTime), @"Adress", MessageBoxButtons.OK);
                }
                string filePath = Path.Combine(GetPath, "CPU_UserBenchmarks.csv"); // Sdelat' Proverky na nazvanie File;
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = new List<CPU>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = new CPU
                        {
                            Type = csv.GetField<string>("Type"),
                            Brand = csv.GetField<string>("Brand"),
                            Model = csv.GetField<string>("Model"),
                            Benchmark = csv.GetField<float>("Benchmark")
                        };
                        records.Add(record);
                    }
                    return records;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a CPU;
        public class CPU
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public float Benchmark { get; set; }

            public override string ToString() => $"| {Brand} | {Model} | {Benchmark}%";
        }

        public static class SOCKETsIntel
        {
            public static List<SOCKETintel> GetSOCKETintelValues()
            {
                string filePath = Path.Combine(GetPath + @"\INTEL_CPU\", "INTEL.csv"); // Sdelat' Proverky na nazvanie File;
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var socksintel = new List<SOCKETintel>();
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var sock = new SOCKETintel
                        {
                            CPU_Name = csv.GetField<string>("CPU_name"),
                            Socketintel = csv.GetField<string>("Socket")
                        };
                        socksintel.Add(sock);
                    }
                    return socksintel;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a CPU;
        public class SOCKETintel
        {
            public string CPU_Name { get; set; }
            public string Socketintel { get; set; }
        }

        // Sozdanie classa dlya vsex GPU;
        public static class GPUs
        {
            public static List<GPU> GetGPUValues()
            {
                
                // Ska4ka GPU_new;
                WebClient wc = new WebClient();
                string url = "https://www.userbenchmark.com/resources/download/csv/GPU_UserBenchmarks.csv"; // Sdelat' proverky na podklu4enie k inety;
                string savePath = GetPath; // AvtoPut';
                string name = "GPU_UserBenchmarks.csv";
                wc.DownloadFile(url, savePath + name);

                string filePath = Path.Combine(GetPath, "GPU_UserBenchmarks.csv"); // Sdelat' Proverky na nazvanie File;
                using (var reader1 = new StreamReader(filePath))
                using (var csv1 = new CsvReader(reader1, CultureInfo.InvariantCulture))
                {
                    var cards = new List<GPU>();
                    csv1.Read();
                    csv1.ReadHeader();
                    while (csv1.Read())
                    {
                        var record1 = new GPU
                        {
                            Type = csv1.GetField<string>("Type"),
                            Brand = csv1.GetField<string>("Brand"),
                            Model = csv1.GetField<string>("Model"),
                            Benchmark = csv1.GetField<float>("Benchmark")
                        };
                        cards.Add(record1);
                    }
                    return cards;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a GPU;
        public class GPU
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public float Benchmark { get; set; }

            public override string ToString() => $"| {Brand} | {Model} | {Benchmark}%";
        }

        // Sozdanie classa dlya vsex RAM;
        public static class RAMs
        {
            public static List<RAM> GetRAMValues()
            {
                // Ska4ka RAM_new;
                WebClient wc = new WebClient();
                string url = "https://www.userbenchmark.com/resources/download/csv/RAM_UserBenchmarks.csv"; // Sdelat' proverky na podklu4enie k inety;
                string save_path = GetPath; // AvtoPut';
                string name = "RAM_UserBenchmarks.csv"; 
                wc.DownloadFile(url, save_path + name);

                string filePath = Path.Combine(GetPath, "RAM_UserBenchmarks.csv"); // Sdelat' Proverky na nazvanie File;
                using (var reader2 = new StreamReader(filePath))
                using (var csv2 = new CsvReader(reader2, CultureInfo.InvariantCulture))
                {
                    var rams = new List<RAM>();
                    csv2.Read();
                    csv2.ReadHeader();
                    while (csv2.Read())
                    {
                        var record2 = new RAM
                        {
                            Type = csv2.GetField<string>("Type"),
                            Brand = csv2.GetField<string>("Brand"),
                            Model = csv2.GetField<string>("Model"),
                            Benchmark = csv2.GetField<float>("Benchmark")
                        };
                        rams.Add(record2);
                    }
                    return rams;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a RAM;
        public class RAM
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public float Benchmark { get; set; }

            public override string ToString() => $"| {Brand} | {Model} | {Benchmark}%";
        }

        // Sozdanie classa dlya vsex SSD;
        public static class SSDs
        {
            public static List<SSD> GetSSDValues()
            {
                // Ska4ka SSD_new;
                WebClient wc = new WebClient();
                string url = "https://www.userbenchmark.com/resources/download/csv/SSD_UserBenchmarks.csv"; // Sdelat' proverky na podklu4enie k inety;
                string save_path = GetPath; // AvtoPut';
                string name = "SSD_UserBenchmarks.csv";
                wc.DownloadFile(url, save_path + name);

                string filePath = Path.Combine(GetPath, "SSD_UserBenchmarks.csv"); // Sdelat' Proverky na nazvanie File;
                using (var reader0 = new StreamReader(filePath))
                using (var csv0 = new CsvReader(reader0, CultureInfo.InvariantCulture))
                {
                    csv0.Configuration.Delimiter = ",";

                    csv0.Read();
                    csv0.ReadHeader();

                    List<SSD> badssds = new List<SSD>(); // Obrabotka bad SSD;
                    csv0.Configuration.BadDataFound = context => badssds.Add(null); // Obrabotka bad SSD;

                    var ssds = new List<SSD>();

                    while (csv0.Read())
                    {
                        var ssd = new SSD
                        {
                            Type = csv0.GetField<string>("Type"),
                            Brand = csv0.GetField<string>("Brand"),
                            Model = csv0.GetField<string>("Model"),
                            Benchmark = csv0.GetField<float>("Benchmark")
                        };
                        ssds.Add(ssd);
                    }
                    return ssds;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a SSD;
        public class SSD
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public float Benchmark { get; set; }

            public override string ToString() => $"| {Brand} | {Model} | {Benchmark}%";
        }

        // Sozdanie classa dlya vsex HDD;
        public static class HDDs
        {
            public static List<HDD> GetHDDValues()
            {
                // Ska4ka HDD_new;
                WebClient wc = new WebClient();
                string url = "https://www.userbenchmark.com/resources/download/csv/HDD_UserBenchmarks.csv"; // Sdelat' proverky na podklu4enie k inety;
                string savePath = GetPath; // AvtoPut';
                string name = "HDD_UserBenchmarks.csv";
                wc.DownloadFile(url, savePath + name);

                string filePath = Path.Combine(GetPath, "HDD_UserBenchmarks.csv");  // Sdelat' Proverky na nazvanie File;
                using (var reader3 = new StreamReader(filePath))
                using (var csv3 = new CsvReader(reader3, CultureInfo.InvariantCulture))
                {
                    csv3.Configuration.Delimiter = ",";

                    csv3.Read();
                    csv3.ReadHeader();

                    List<HDD> badhdds = new List<HDD>(); // Obrabotka bad HDD;
                    csv3.Configuration.BadDataFound = context => badhdds.Add(null); // Obrabotka bad HDD;

                    var hdds = new List<HDD>();

                    while (csv3.Read())
                    {
                        var hdd = new HDD
                        {
                            Type = csv3.GetField<string>("Type"),
                            Brand = csv3.GetField<string>("Brand"),
                            Model = csv3.GetField<string>("Model"),
                            Benchmark = csv3.GetField<float>("Benchmark")

                        };
                        hdds.Add(hdd);
                    }
                    return hdds;
                }
            }
        }

        // Sozdanie classa dlya parsinga Type/Brand/Model/Benchmark'a HDD;
        public class HDD
        {
            public string Type { get; set; }
            public string Brand { get; set; }
            public string Model { get; set; }
            public float Benchmark { get; set; }

            public override string ToString() => $"| {Brand} | {Model} | {Benchmark}%";
        }
        // Sobitie dlya pokaza ranzici CPU;
        private void ComboBoxCPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            CPU leftCPU = comboBox1.SelectedItem as CPU;    // Viborka levogo CPU;
            CPU rightCPU = comboBox2.SelectedItem as CPU;   // Viborka pravogo CPU;

            if (comboBox1.SelectedItem == null)
            {
                leftCPU = new CPU();
                leftCPU.Benchmark = 0;
            }
            if (comboBox2.SelectedItem == null)
            {
                rightCPU = new CPU();
                rightCPU.Benchmark = 0;
            }

            float result;   // result = math.abs = module 4isla (levoe - pravoe);
            result = Math.Abs((leftCPU?.Benchmark ?? 0.0f) - (rightCPU?.Benchmark ?? 0.0f));

            if (leftCPU.Benchmark > rightCPU.Benchmark)

            {
                textBox2.Text = $@"+{result:0.##}" + @"% (1st)"; // Vivod result + okruglenie;
            }
            else if (leftCPU.Benchmark < rightCPU.Benchmark)
            {
                textBox2.Text = $@"+{result:0.##}" + @"% (2nd)"; // Vivod result + okruglenie;
            }
            else
            {
                textBox2.Text = $@"{result:0.##}" + @"%"; // Vivod result + okruglenie;
            }
        }

        // Sobitie dlya pokaza raznici GPU;
        private void ComboBoxGPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            GPU leftGPU = comboBox3.SelectedItem as GPU;    // Viborka levogo GPU;
            GPU rightGPU = comboBox4.SelectedItem as GPU;   // Viborka pravogo GPU;

            if (comboBox3.SelectedItem == null)
            {
                leftGPU = new GPU();
                leftGPU.Benchmark = 0;
            }
            if (comboBox4.SelectedItem == null)
            {
                rightGPU = new GPU();
                rightGPU.Benchmark = 0;
            }

            float result;   // result = math.abs = module 4isla (levoe - pravoe);
            result = Math.Abs((leftGPU?.Benchmark ?? 0.0f) - (rightGPU?.Benchmark ?? 0.0f));

            if (leftGPU.Benchmark > rightGPU.Benchmark)
            {
                textBox4.Text = $@"+{result:0.##}" + @"% (1st)"; // Vivod result + okruglenie;
            }
            else if (leftGPU.Benchmark < rightGPU.Benchmark)
            {
                textBox4.Text = $@"+{result:0.##}" + @"% (2nd)"; // Vivod result + okruglenie;
            }
            else
            {
                textBox4.Text = $@"{result:0.##}" + @"%"; // Vivod result + okruglenie;
            }
        }

        // Sobitie dlya pokaza raznici RAM;
        private void ComboBoxRAM_SelectedIndexChanged(object sender, EventArgs e)
        {
            RAM leftRAM = comboBox5.SelectedItem as RAM;    // Viborka levogo RAM;
            RAM rightRAM = comboBox6.SelectedItem as RAM;   // Viborka levogo RAM;

            if (comboBox5.SelectedItem == null)
            {
                leftRAM = new RAM();
                leftRAM.Benchmark = 0;
            }
            if (comboBox6.SelectedItem == null)
            {
                rightRAM = new RAM();
                rightRAM.Benchmark = 0;
            }

            float result;   // result = math.abs = module 4isla (levoe - pravoe);
            result = Math.Abs((leftRAM?.Benchmark ?? 0.0f) - (rightRAM?.Benchmark ?? 0.0f));

            if (leftRAM.Benchmark > rightRAM.Benchmark)
            {
                textBox6.Text = $@"+{result:0.##}" + @"% (1st)"; // Vivod result + okruglenie;
            }
            else if (leftRAM.Benchmark < rightRAM.Benchmark)
            {
                textBox6.Text = $@"+{result:0.##}" + @"% (2nd)"; // Vivod result + okruglenie;
            }
            else
            {
                textBox6.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
            }
            
        }

        // Sobitie dlya pokaza raznici SSD;
        private void ComboBoxSSD_SelectedIndexChanged(object sender, EventArgs e)
        {
            SSD leftSSD = comboBox7.SelectedItem as SSD;    // Viborka levogo SSD;
            SSD rightSSD = comboBox8.SelectedItem as SSD;   // Viborka levogo SSD;

            if (comboBox7.SelectedItem == null)
            {
                leftSSD = new SSD();
                leftSSD.Benchmark = 0;
            }
            if (comboBox8.SelectedItem == null)
            {
                rightSSD = new SSD();
                rightSSD.Benchmark = 0;
            }

            float result;   // result = math.abs = module 4isla (levoe - pravoe);
            result = Math.Abs((leftSSD?.Benchmark ?? 0.0f) - (rightSSD?.Benchmark ?? 0.0f));

            if (leftSSD.Benchmark > rightSSD.Benchmark)
            {
                textBox8.Text = $@"+{result:0.##}" + @"% (1st)"; // Vivod result + okruglenie;
            }
            else if (leftSSD.Benchmark < rightSSD.Benchmark)
            {
                textBox8.Text = $@"+{result:0.##}" + @"% (2nd)"; // Vivod result + okruglenie;
            }
            else
            {
                textBox8.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
            }
        }

        // Sobitie dlya pokaza raznici HDD;
        private void ComboBoxHDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            HDD leftHDD = comboBox9.SelectedItem as HDD;    // Viborka levogo HDD;
            HDD rightHDD = comboBox10.SelectedItem as HDD;  // Viborka levogo HDD;

            if (comboBox9.SelectedItem == null)
            {
                leftHDD = new HDD();
                leftHDD.Benchmark = 0;
            }
            if (comboBox10.SelectedItem == null)
            {
                rightHDD = new HDD();
                rightHDD.Benchmark = 0;
            }

            float result;   // result = math.abs = module 4isla (levoe - pravoe);
            result = Math.Abs((leftHDD?.Benchmark ?? 0.0f) - (rightHDD?.Benchmark ?? 0.0f));

            if (leftHDD.Benchmark > rightHDD.Benchmark)
            {
                textBox10.Text = $@"+{result:0.##}" + @"% (1st)"; // Vivod result + okruglenie;
            }
            else if (leftHDD.Benchmark < rightHDD.Benchmark)
            {
                textBox10.Text = $@"+{result:0.##}" + @"% (2nd)"; // Vivod result + okruglenie;
            }
            else
            {
                textBox10.Text = $@"{result:0.##}" + @"%";    // Vivod result + okruglenie;
            }
        }

        // Polu4enie name OS;
        static string OSvers()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Caption")).LastOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        // Polu4enie name CPU;
        static string CPUid()
        {
            var cpuname = (from y in new ManagementObjectSearcher("SELECT Name FROM Win32_Processor").Get().Cast<ManagementObject>()
                           select y.GetPropertyValue("Name")).LastOrDefault();
            return cpuname != null ? cpuname.ToString() : "Unknown";
        }

        // Polu4enie name GPU;
        static string GPUid()
        {
            var gpuname = (from z in new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController").Get().Cast<ManagementObject>()
                           select z.GetPropertyValue("Name")).LastOrDefault();
            return gpuname != null ? gpuname.ToString() : "Unknown";
        }

        // Polu4enie bukvi diska;
        static string DiskLetter()
        {
            var diskletter = (from b in new ManagementObjectSearcher("SELECT Name FROM Win32_Volume").Get().Cast<ManagementObject>()
                              select b.GetPropertyValue("Name")).LastOrDefault();
            return diskletter != null ? diskletter.ToString() : "Unknown";
        }

        // Polu4enie name diska;
        static string Diskid()
        {
            var diskname = (from a in new ManagementObjectSearcher("SELECT Caption FROM Win32_DiskDrive").Get().Cast<ManagementObject>()
                            select a.GetPropertyValue("Caption")).LastOrDefault();
            return diskname != null ? diskname.ToString() : "Unknown";
        }

        // Polu4enie space disk vsego;
        static string Disksize()
        {
            var disksize = (from b in new ManagementObjectSearcher("SELECT Size FROM Win32_DiskDrive").Get().Cast<ManagementObject>()
                            select b.GetPropertyValue("Size")).LastOrDefault();
            
            return disksize != null ? disksize.ToString() : "Unknown";
        }

        // Polu4enie free space;
        static string FreeSpace()
        {
            var freespace = (from b in new ManagementObjectSearcher("SELECT FreeSpace FROM Win32_Volume").Get().Cast<ManagementObject>()
                             select b.GetPropertyValue("FreeSpace")).LastOrDefault();
            return freespace != null ? freespace.ToString() : "Unknown";
        }

        // Polu4enie motherBoard proizvoditel;
        static string Mthrboard()
        {
            var mthrbrd = (from b in new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_BaseBoard").Get().Cast<ManagementObject>()
                           select b.GetPropertyValue("Manufacturer")).LastOrDefault();
            return mthrbrd != null ? mthrbrd.ToString() : "Unknown";
        }

        // Polu4enie motherBoard model;
        static string Mthrboard1()
        {
            var mthrbrd1 = (from b in new ManagementObjectSearcher("SELECT Product FROM Win32_BaseBoard").Get().Cast<ManagementObject>()
                            select b.GetPropertyValue("Product")).LastOrDefault();
            return mthrbrd1 != null ? mthrbrd1.ToString() : "Unknown";
        }

        // Polu4enie name RAM;
        static string RAM0()
        {
            var ram = (from b in new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory").Get().Cast<ManagementObject>()
                       select b.GetPropertyValue("Capacity")).LastOrDefault();
            return ram != null ? ram.ToString() : "Unknown";
        }

        // Polu4enie slot RAM;
        static string RAM1()
        {
            var ram = (from b in new ManagementObjectSearcher("SELECT DeviceLocator FROM Win32_PhysicalMemory").Get().Cast<ManagementObject>()
                       select b.GetPropertyValue("DeviceLocator")).LastOrDefault();
            return ram != null ? ram.ToString() : "Unknown";
        }

        // Parsing $;
        String Dollar()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            WebClient wc = new WebClient();
            String Response = wc.DownloadString("http://www.profinance.ru/currency_usd.asp"); // Site dlya parsinga;
            String Rate = System.Text.RegularExpressions.Regex.Match(Response, @"<b><font color=""Red"">([0-9]+\.[0-9]+)</font></b>").Groups[1].Value; // Stro4ka + reg virajeniye dlya parsinga;
            Rate = Rate.Replace(".", ",");
            return Rate;
        }

        // Parsing €;
        String Eur()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            WebClient wc = new WebClient();
            String Response = wc.DownloadString("http://www.profinance.ru/currency_eur.asp?s=USD/EUR"); // Site dlya parsinga;
            String Rate = System.Text.RegularExpressions.Regex.Match(Response, @"<b><font id=""bid"" color=""Red"">([0-9]+\,[0-9]+)</font></b>").Groups[1].Value; // Stro4ka + reg virajeniye dlya parsinga;
            Rate = Rate.Replace(".", ",");
            return Rate;
        }

        public int Totalprice;
        public float Rubli;
        public int Euro;

        // PC for gaming 60 FPS ultra FULL HD (recommended build);
        private void Button10_Click(object sender, EventArgs e)
        {
            // Components;

            textBox77.Text = @"Intel Core i5‑9400F"; // CPU;
            textBox78.Text = @"MSI H310M PRO-VDH PLUS"; // MotherBoard;
            textBox79.Text = @"Crucial Ballistix Sport 3200 MHz 16GB"; // RAM:
            textBox80.Text = @"Nvidia GeForce RTX 2060 6GB"; // VideoCard;
            textBox81.Text = @"Samsung 860 EVO 500 GB"; // SSD;
            textBox82.Text = @"Seagate BarraCuda 1TB"; // HDD;
            textBox89.Text = @"be quiet! System Power 9 600W"; // PSU;
            textBox90.Text = @"NZXT H510"; // CASE;
            textBox91.Text = @"be quiet! Pure Rock BK009"; // CPU Cooler;

            // Price for Components;

            textBox83.Text = @"170$"; // Price CPU;
            textBox84.Text = @"57$"; // Price MotherBoard;
            textBox85.Text = @"145$"; // Price RAM;
            textBox86.Text = @"390$"; // Price VideoCard;
            textBox87.Text = @"162$"; // Price SSD;
            textBox88.Text = @"43$"; // Price HDD;
            textBox92.Text = @"61$"; // Price PSU;
            textBox93.Text = @"70$"; // Price CASE;
            textBox94.Text = @"58$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox83.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox84.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox85.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox86.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox87.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox88.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox92.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox93.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox94.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro =  (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox101.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox102.Text = Convert.ToString(Euro); // Total Price €;
            textBox103.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // Rendering PC (recommended build);
        private void Button11_Click(object sender, EventArgs e)
        {
            // Components;

            textBox77.Text = @"Intel Core i9 9900k"; // CPU;
            textBox78.Text = @"ASUS Prime Z390-A ATX"; // MotherBoard;
            textBox79.Text = @"(2 x 16GB) Corsair Vengeance LPX 3200Mhz"; // RAM:
            textBox80.Text = @"NVIDIA RTX 2070 8GB - MSI Gaming"; // VideoCard;
            textBox81.Text = @"Samsung 970 EVO PLUS 500GB M.2"; // SSD;
            textBox82.Text = @"Seagate Barracuda 2TB"; // HDD;
            textBox89.Text = @"Seasonic Focus Plus Gold 650W ATX 2.4"; // PSU;
            textBox90.Text = @"Corsair Carbide Series 275Q ATX"; // CASE;
            textBox91.Text = @"be quiet! Dark Rock Pro 4"; // CPU Cooler;

            // Price for Components;

            textBox83.Text = @"513$"; // Price CPU;
            textBox84.Text = @"180$"; // Price MotherBoard;
            textBox85.Text = @"142$"; // Price RAM;
            textBox86.Text = @"550$"; // Price VideoCard;
            textBox87.Text = @"120$"; // Price SSD;
            textBox88.Text = @"55$"; // Price HDD;
            textBox92.Text = @"100$"; // Price PSU;
            textBox93.Text = @"64$"; // Price CASE;
            textBox94.Text = @"90$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox83.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox84.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox85.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox86.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox87.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox88.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox92.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox93.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox94.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox101.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox102.Text = Convert.ToString(Euro); // Total Price €;
            textBox103.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // PC for Office (recommended build);
        private void Button12_Click(object sender, EventArgs e)
        {
            // Components;

            textBox77.Text = @"AMD Ryzen 3 3200G Box"; // CPU;
            textBox78.Text = @"Asus TUF B450M-Plus Gaming"; // MotherBoard;
            textBox79.Text = @"Kingston HyperX 2x4GB DDR4-2666 Black"; // RAM:
            textBox80.Text = @"Integrated from CPU"; // VideoCard;
            textBox81.Text = @"Sabrent 256GB Rocket NVMe"; // SSD;
            textBox82.Text = @"Seagate Barracuda 2TB"; // HDD;
            textBox89.Text = @"Thermaltake Smart 500W 80+ White"; // PSU;
            textBox90.Text = @"Silverstone PS08B MicroATX"; // CASE;
            textBox91.Text = @"Box cooler from CPU"; // CPU Cooler;

            // Price for Components;

            textBox83.Text = @"88$"; // Price CPU;
            textBox84.Text = @"186$"; // Price MotherBoard;
            textBox85.Text = @"48$"; // Price RAM;
            textBox86.Text = @"0$"; // Price VideoCard;
            textBox87.Text = @"63$"; // Price SSD;
            textBox88.Text = @"55$"; // Price HDD;
            textBox92.Text = @"45$"; // Price PSU;
            textBox93.Text = @"50$"; // Price CASE;
            textBox94.Text = @"0$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox83.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox84.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox85.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox86.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox87.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox88.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox92.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox93.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox94.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox101.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox102.Text = Convert.ToString(Euro); // Total Price €;
            textBox103.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // Streaming PC (games) (recommended build);
        private void Button30_Click(object sender, EventArgs e)
        {
            // Components;

            textBox77.Text = @"Intel Core i9-9900K"; // CPU;
            textBox78.Text = @"MSI MEG Z390 GODLIKE"; // MotherBoard;
            textBox79.Text = @"G.Skill Trident Z RGB 32 GB 3000Mhz"; // RAM:
            textBox80.Text = @"ASUS ROG Strix GeForce RTX 2080TI 11GB"; // VideoCard;
            textBox81.Text = @"Samsung 970 EVO 1TB"; // SSD;
            textBox82.Text = @"Seagate Barracuda 2TB"; // HDD;
            textBox89.Text = @"EVGA Supernova G3 80+ Gold 850W"; // PSU;
            textBox90.Text = @"Corsair Obsidian 500D RGB-SE"; // CASE;
            textBox91.Text = @"Corsair Hydro Series H100i Pro RGB"; // CPU Cooler;

            // Price for Components;

            textBox83.Text = @"513$"; // Price CPU;
            textBox84.Text = @"500$"; // Price MotherBoard;
            textBox85.Text = @"140$"; // Price RAM;
            textBox86.Text = @"1500$"; // Price VideoCard;
            textBox87.Text = @"190$"; // Price SSD;
            textBox88.Text = @"55$"; // Price HDD;
            textBox92.Text = @"289$"; // Price PSU;
            textBox93.Text = @"100$"; // Price CASE;
            textBox94.Text = @"197$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox83.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox84.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox85.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox86.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox87.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox88.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox92.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox93.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox94.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox101.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox102.Text = Convert.ToString(Euro); // Total Price €;
            textBox103.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // The most Popular build on AMD * Recommended by pcgamehaven.com;
        private void Label104_Click(object sender, EventArgs e)
        {
            textBox44.Text = @"AMD Ryzen 5 3600 Box"; // CPU;
            textBox45.Text = @"MSI B450-A Pro MAX"; // MotherBoard;
            textBox46.Text = @"XFX RX 5700 XT Thicc III Ultra"; // GPU;
            textBox47.Text = @"16GB Corsair Vengeance LPX"; // RAM;
            textBox48.Text = @"WD Blue 500GB SSD"; // SSD;
            textBox49.Text = @"none"; // HDD;
            textBox50.Text = @"Corsair CX650M"; // PSU;
            textBox51.Text = @"NZXT H510"; // Case;
            textBox52.Text = @"Box cooler from CPU"; // CPU Cooler;

            textBox32.Text = @"167$"; // Price CPU;
            textBox36.Text = @"157$"; // Price MotherBoard;
            textBox37.Text = @"410$"; // Price GPU;
            textBox38.Text = @"76$"; // Price RAM;
            textBox39.Text = @"65$"; // Price SSD;
            textBox40.Text = @"0$"; // Price HDD;
            textBox41.Text = @"65$"; // Price PSU
            textBox42.Text = @"70$"; // Price Case;
            textBox43.Text = @"0$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox32.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox36.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox37.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox38.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox39.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox40.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox41.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox42.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox43.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox53.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox54.Text = Convert.ToString(Euro); // Total Price €;
            textBox55.Text = Convert.ToString(Rubli); // Total Price ₽;

        }

        // The most Popular build on Intel * Recommended by pcpartpicker.com;
        private void Label88_Click(object sender, EventArgs e)
        {
            textBox68.Text = @"Intel Core i5-9600K"; // CPU;
            textBox69.Text = @"MSI Z390-A PRO ATX"; // MotherBoard;
            textBox70.Text = @"XFX RX 5700 XT Thicc III Ultra"; // GPU;
            textBox71.Text = @"G.Skill Aegis 16 GB"; // RAM;
            textBox72.Text = @"Team GX1 480 GB"; // SSD;
            textBox73.Text = @"Seagate Barracuda 2 TB"; // HDD;
            textBox74.Text = @"Corsair TXM Gold 550W 80+"; // PSU;
            textBox75.Text = @"Phanteks Eclipse P300"; // Case;
            textBox76.Text = @"Cooler Master Hyper 212 Black Edition"; // CPU Cooler;

            textBox59.Text = @"197$"; // Price CPU;
            textBox60.Text = @"130$"; // Price MotherBoard;
            textBox61.Text = @"410$"; // Price GPU;
            textBox62.Text = @"59$"; // Price RAM;
            textBox63.Text = @"49$"; // Price SSD;
            textBox64.Text = @"55$"; // Price HDD;
            textBox65.Text = @"65$"; // Price PSU
            textBox66.Text = @"70$"; // Price Case;
            textBox67.Text = @"100$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox59.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox60.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox61.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox62.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox63.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox64.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox65.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox66.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox67.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox56.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox57.Text = Convert.ToString(Euro); // Total Price €;
            textBox58.Text = Convert.ToString(Rubli); //Total Price ₽;
        }

        // Recommended AMD * Recommended by 3dnews.ru;
        private void Label29_Click(object sender, EventArgs e)
        {
            textBox44.Text = @"AMD Ryzen 5 1600 Box"; // CPU;
            textBox45.Text = @"ASRock B450M PRO4-F"; // MotherBoard;
            textBox46.Text = @"AMD Radeon RX 570 8GB"; // GPU;
            textBox47.Text = @"16 GB Kingston HyperX Fury 3000Mhz"; // RAM;
            textBox48.Text = @"Kingston A400 240GB"; // SSD;
            textBox49.Text = @"none"; // HDD;
            textBox50.Text = @"be Quiet! System Power 9 500W"; // PSU;
            textBox51.Text = @"DeepCool MATREXX 30"; // Case;
            textBox52.Text = @"PCcooler GI-X2"; // CPU Cooler;
            
            textBox32.Text = @"152$"; // Price CPU;
            textBox36.Text = @"143$"; // Price MotherBoard;
            textBox37.Text = @"180$"; // Price GPU;
            textBox38.Text = @"85$"; // Price RAM;
            textBox39.Text = @"40$"; // Price SSD;
            textBox40.Text = @"0$"; // Price HDD;
            textBox41.Text = @"45$"; // Price PSU
            textBox42.Text = @"32$"; // Price Case;
            textBox43.Text = @"15$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox32.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox36.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox37.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox38.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox39.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox40.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox41.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox42.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox43.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox53.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox54.Text = Convert.ToString(Euro); // Total Price €;
            textBox55.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // Recommended AMD * Recommended by hardwareluxx.ru;
        private void Label37_Click(object sender, EventArgs e)
        {
            textBox44.Text = @"AMD Ryzen 5 3400G Box"; // CPU;
            textBox45.Text = @"Gigabyte Aorus B450 Elite"; // MotherBoard;
            textBox46.Text = @"AMD Radeon RX 580"; // GPU;
            textBox47.Text = @"16 GB Kingston HyperX Fury 3000Mhz"; // RAM;
            textBox48.Text = @"Seagate Barracuda 250 GB"; // SSD;
            textBox49.Text = @"none"; // HDD;
            textBox50.Text = @"Corsair VS550W 550W"; // PSU;
            textBox51.Text = @"be quiet! Pure Base 600"; // Case;
            textBox52.Text = @"Box cooler from CPU"; // CPU Cooler;

            textBox32.Text = @"150$"; // Price CPU;
            textBox36.Text = @"90$"; // Price MotherBoard;
            textBox37.Text = @"170$"; // Price GPU;
            textBox38.Text = @"85$"; // Price RAM;
            textBox39.Text = @"50$"; // Price SSD;
            textBox40.Text = @"0$"; // Price HDD;
            textBox41.Text = @"65$"; // Price PSU
            textBox42.Text = @"89$"; // Price Case;
            textBox43.Text = @"0$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox32.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox36.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox37.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox38.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox39.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox40.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox41.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox42.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox43.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox53.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox54.Text = Convert.ToString(Euro); // Total Price €;
            textBox55.Text = Convert.ToString(Rubli); // Total Price ₽;
        }

        // Recommended Intel * Recommended by games.mail.ru;
        private void Label144_Click(object sender, EventArgs e)
        {
            textBox68.Text = @"Intel Core i5-9400F OEM"; // CPU;
            textBox69.Text = @"ASUS TUF B360M-Plus Gaming"; // MotherBoard;
            textBox70.Text = @"MSI GeForce RTX 2060 Super Ventus"; // GPU;
            textBox71.Text = @"Samsung DDR4 2666Mhz 16 GB"; // RAM;
            textBox72.Text = @"Kingston A400 240Gb"; // SSD;
            textBox73.Text = @"WD Blue 1Tb"; // HDD;
            textBox74.Text = @"DeepCool DA500N 500W"; // PSU;
            textBox75.Text = @"Thermaltake Versa H21"; // Case;
            textBox76.Text = @"AeroCool Air Frost 2"; // CPU Cooler;

            textBox59.Text = @"199$"; // Price CPU;
            textBox60.Text = @"77$"; // Price MotherBoard;
            textBox61.Text = @"340$"; // Price GPU;
            textBox62.Text = @"70$"; // Price RAM;
            textBox63.Text = @"40$"; // Price SSD;
            textBox64.Text = @"45$"; // Price HDD;
            textBox65.Text = @"39$"; // Price PSU
            textBox66.Text = @"91$"; // Price Case;
            textBox67.Text = @"12$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox59.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox60.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox61.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox62.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox63.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox64.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox65.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox66.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox67.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox56.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox57.Text = Convert.ToString(Euro); // Total Price €;
            textBox58.Text = Convert.ToString(Rubli); //Total Price ₽;
        }

        // Recommended Intel * Recommended by hardprice.ru;
        private void Label145_Click(object sender, EventArgs e)
        {
            textBox68.Text = @"Intel Core i5-9400F OEM"; // CPU;
            textBox69.Text = @"Gigabyte B365M"; // MotherBoard;
            textBox70.Text = @"Gigabyte GeForce GTX 1660 SUPER 6GB"; // GPU;
            textBox71.Text = @"Kingston HyperX Fury 16Gb 2400Mhz"; // RAM;
            textBox72.Text = @"Kingston A400 240Gb"; // SSD;
            textBox73.Text = @"WD Blue 1Tb"; // HDD;
            textBox74.Text = @"Thermaltake TR2 Bronze 550W"; // PSU;
            textBox75.Text = @"Aerocool Shard Black"; // Case;
            textBox76.Text = @"DeepCool GAMMAXX400"; // CPU Cooler;

            textBox59.Text = @"199$"; // Price CPU;
            textBox60.Text = @"76$"; // Price MotherBoard;
            textBox61.Text = @"310$"; // Price GPU;
            textBox62.Text = @"74$"; // Price RAM;
            textBox63.Text = @"40$"; // Price SSD;
            textBox64.Text = @"45$"; // Price HDD;
            textBox65.Text = @"45$"; // Price PSU
            textBox66.Text = @"62$"; // Price Case;
            textBox67.Text = @"20$"; // Price CPU Cooler;

            Totalprice = Convert.ToInt32((textBox59.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox60.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox61.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox62.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox63.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox64.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox65.Text).Trim(new Char[] { '$' })) + Convert.ToInt32((textBox66.Text).Trim(new Char[] { '$' })) +
                Convert.ToInt32((textBox67.Text).Trim(new Char[] { '$' })); // Total Price $, without $;

            Euro = (int)(float)(Convert.ToDouble(Totalprice) / Convert.ToDouble(Eur())); // Perevod v €;
            Rubli = (int)(Convert.ToDouble(Totalprice) * Convert.ToDouble(Dollar())); // Perevod v ₽;

            textBox56.Text = Convert.ToString(Totalprice); // Total Price $;
            textBox57.Text = Convert.ToString(Euro); // Total Price €;
            textBox58.Text = Convert.ToString(Rubli); //Total Price ₽;
        }
        // Copy OS;
        private void Button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label30.Text);
        }

        // Copy Motherboard;
        private void Button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label31.Text);
        }

        // Copy CPU;
        private void Button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label32.Text);
        }

        // Copy GPU;
        private void Button5_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label33.Text);
        }

        // Copy RAM;
        private void Button6_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label35.Text);
        }

        // Copy Storages;
        private void Button7_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label36.Text);
        }

        // Copy DiskSpace;
        private void Button89_Click(object sender, EventArgs e)
        {
            StringBuilder clipboard = new StringBuilder();
            clipboard.Append(label113.Text + "\n");
            Clipboard.SetText(clipboard.ToString());
        }

        // Copy All;
        private void Button9_Click(object sender, EventArgs e)
        {
            StringBuilder clipboard = new StringBuilder();
            clipboard.Append(label30.Text + "\n");
            clipboard.Append(label31.Text + "\n");
            clipboard.Append(label32.Text + "\n");
            clipboard.Append(label33.Text + "\n");
            clipboard.Append(label35.Text + "\n");
            clipboard.Append(label36.Text + "\n");
            clipboard.Append(label113.Text + "\n");
            Clipboard.SetText(clipboard.ToString());
        }

        private void button99_Click(object sender, EventArgs e) // RU 1 vkladka;
        {
            label8.Text = @"ЦП";
            label8.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label9.Text = @"Видеокарта";
            label9.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label10.Text = @"ОЗУ";
            label10.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label11.Text = @"ССД";
            label11.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label12.Text = @"ПЗУ";
            label12.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label13.Text = @"ЦП";
            label13.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label14.Text = @"Видеокарта";
            label14.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label15.Text = @"ОЗУ";
            label15.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label16.Text = @"ССД";
            label16.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label17.Text = @"ПЗУ";
            label17.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label18.Text = @"Первый компонент";
            label18.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label19.Text = @"Второй компонент"; 
            label19.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label20.Text = @"Сравнение";
            label20.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label22.Text = @"Разница";
            label22.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button100_Click(object sender, EventArgs e) // ENG 1 vkladka;
        {
            label8.Text = @"CPU";
            label8.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label9.Text = @"Videocard";
            label9.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label10.Text = @"RAM";
            label10.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label11.Text = @"SSD";
            label11.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label12.Text = @"HDD";
            label12.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label13.Text = @"CPU";
            label13.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label14.Text = @"Videocard";
            label14.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label15.Text = @"RAM";
            label15.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label16.Text = @"SSD";
            label16.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label17.Text = @"HDD";
            label17.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label18.Text = @"1st component";
            label18.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label19.Text = @"2nd component";
            label19.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label20.Text = @"Compare";
            label20.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label22.Text = @"Difference";
            label22.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button102_Click(object sender, EventArgs e) // RU 2 vkladka;
        {
            label38.Text = @"Компоненты";
            label38.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label39.Text = @"ПЗУ";
            label39.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label40.Text = @"ССД";
            label40.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label41.Text = @"ОЗУ";
            label41.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label42.Text = @"Видеокарта";
            label42.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label43.Text = @"INTEL/AMD ЦП";
            label43.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label44.Text = @"Сборщик ПК";
            label44.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label45.Text = @"Мат. плата";
            label45.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label46.Text = @"Сокет";
            label46.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label47.Text = @"Модель";
            label47.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label48.Text = @"Модель";
            label48.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label49.Text = @"Цена";
            label49.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label50.Text = @"Цена";
            label50.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label51.Text = @"Цена";
            label51.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label52.Text = @"Цена";
            label52.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label53.Text = @"Цена";
            label53.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label54.Text = @"Цена";
            label54.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button101_Click(object sender, EventArgs e) // ENG 2 vkladka;
        {
            label38.Text = @"Components";
            label38.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label39.Text = @"HDD";
            label39.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label40.Text = @"SSD";
            label40.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label41.Text = @"RAM";
            label41.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label42.Text = @"Videocard";
            label42.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label43.Text = @"INTEL/AMD CPU";
            label43.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label44.Text = @"PC Builder";
            label44.Font = new Font("Unispace", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label45.Text = @"Motherboard";
            label45.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label46.Text = @"Socket";
            label46.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label47.Text = @"Model";
            label47.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label48.Text = @"Model";
            label48.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label49.Text = @"Price";
            label49.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label50.Text = @"Price";
            label50.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label51.Text = @"Price";
            label51.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label52.Text = @"Price";
            label52.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label53.Text = @"Price";
            label53.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label54.Text = @"Price";
            label54.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button104_Click(object sender, EventArgs e) // RU 3 vkladka;
        {
            label59.Text = @"Рекомендованная сборка";
            label59.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label114.Text = @"Цена";
            label114.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label115.Text = @"Цена";
            label115.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label116.Text = @"Цена";
            label116.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label117.Text = @"Цена";
            label117.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label118.Text = @"Цена";
            label118.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label119.Text = @"Цена";
            label119.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label120.Text = @"Модель видеокарты";
            label120.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label121.Text = @"Модель ЦП";
            label121.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label122.Text = @"Мат. плата";
            label122.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label123.Text = @"Компоненты";
            label123.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label124.Text = @"ПЗУ";
            label124.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label125.Text = @"ССД";
            label125.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label126.Text = @"ОЗУ";
            label126.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label127.Text = @"Цена";
            label127.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label128.Text = @"Цена";
            label128.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label129.Text = @"Цена";
            label129.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label133.Text = @"Куллер ЦП";
            label133.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label134.Text = @"Корпус";
            label134.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label135.Text = @"БП";
            label135.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button10.Text = @"Игровой ПК";
            button10.Font = new Font("Times New Roman", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button11.Text = @"Рендер ПК";
            button11.Font = new Font("Times New Roman", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button12.Text = @"Офисный ПК";
            button12.Font = new Font("Times New Roman", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button30.Text = @"Для стримов (игры)";
            button30.Font = new Font("Times New Roman", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button103_Click(object sender, EventArgs e) // ENG 3 vkladka;
        {
            label59.Text = @"Recommended build";
            label59.Font = new Font("Lucida Bright", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label114.Text = @"Price";
            label114.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label115.Text = @"Price";
            label115.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label116.Text = @"Price";
            label116.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label117.Text = @"Price";
            label117.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label118.Text = @"Price";
            label118.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label119.Text = @"Price";
            label119.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label120.Text = @"GPU model";
            label120.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label121.Text = @"CPU model";
            label121.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label122.Text = @"Motherboard";
            label122.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label123.Text = @"Components";
            label123.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label124.Text = @"HDD";
            label124.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label125.Text = @"SSD";
            label125.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label126.Text = @"RAM";
            label126.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label127.Text = @"Price";
            label127.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label128.Text = @"Price";
            label128.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label129.Text = @"Price";
            label129.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label133.Text = @"CPU cooler";
            label133.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label134.Text = @"Case";
            label134.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label135.Text = @"PSU";
            label135.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button10.Text = @"Gaming PC";
            button10.Font = new Font("Microsoft Uighur", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button11.Text = @"Rendering PC";
            button11.Font = new Font("Microsoft Uighur", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button12.Text = @"PC for office";
            button12.Font = new Font("Microsoft Uighur", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            button30.Text = @"Streaming PC (games)";
            button30.Font = new Font("Microsoft Uighur", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button88_Click(object sender, EventArgs e) // RU 4 vkladka;
        {
            label23.Text = @"Операционная система:";
            label23.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label24.Text = @"Материнская плата:";
            label24.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label25.Text = @"ЦП:";
            label25.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label26.Text = @"Видеокарта:";
            label26.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label27.Text = @"ОЗУ:";
            label27.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label28.Text = @"Диски:";
            label28.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label34.Text = @"Информация об этом компьютере";
            label34.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label34.Location = new Point(260, 10);
            label30.Location = new Point(220, 71);
            label31.Location = new Point(190, 100);
            label32.Location = new Point(45, 125);
            label33.Location = new Point(120, 160);
            label35.Location = new Point(60, 185);
            label36.Location = new Point(70, 215);
            label113.Location = new Point(165, 245);
            label143.Text = @"Место на дисках:";
            label143.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button9.Text = @"Скопировать все";
            button9.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button9.Size = new Size(120, 25);
            button9.Location = new Point(810, 475);
        }

        private void button1_Click(object sender, EventArgs e) // ENG 4 vkladka;
        {
            label23.Text = @"Operating System:";
            label23.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label24.Text = @"Motherboard:";
            label24.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label25.Text = @"CPU:";
            label25.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label26.Text = @"GPU:";
            label26.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label27.Text = @"RAM:";
            label27.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label28.Text = @"Storages:";
            label28.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label34.Text = @"INFO about this PC";
            label34.Font = new Font("Unispace", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label34.Location = new Point(290, 8);
            label34.Location = new Point(260, 10);
            label30.Location = new Point(190, 71);
            label31.Location = new Point(140, 100);
            label32.Location = new Point(60, 125);
            label33.Location = new Point(60, 160);
            label35.Location = new Point(60, 185);
            label36.Location = new Point(110, 215);
            label113.Location = new Point(135, 245);
            label143.Text = @"Disk Space:";
            label143.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button9.Text = @"Copy all";
            button9.Font = new Font("Unispace", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button9.Size = new Size(75, 25);
            button9.Location = new Point(850, 475);
        }

        private void button106_Click(object sender, EventArgs e) // RU 5 vkladka;
        {
            label72.Text = @"Собранный ПК";
            label72.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label74.Text = @"Компоненты";
            label74.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label71.Text = @"ЦП";
            label71.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label73.Text = @"Мат. плата";
            label73.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label77.Text = @"ОЗУ";
            label77.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label70.Text = @"Видеокарта";
            label70.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label76.Text = @"ССД";
            label76.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label75.Text = @"ПЗУ";
            label75.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label132.Text = @"Блок питания";
            label132.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label131.Text = @"Корпус";
            label131.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label130.Text = @"Кулер ЦП";
            label130.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label136.Text = @"Цена";
            label136.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label137.Text = @"Цена";
            label137.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label138.Text = @"Цена";
            label138.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label64.Text = @"Цена";
            label64.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label65.Text = @"Цена";
            label65.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label66.Text = @"Цена";
            label66.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label67.Text = @"Цена";
            label67.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label68.Text = @"Цена";
            label68.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label69.Text = @"Цена";
            label69.Font = new Font("Times New Roman", 14F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button27.Text = @"Сохранить сборку";
            button27.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button27.Size = new Size(125, 25);
            button27.Location = new Point(800, 670);
        }

        private void button105_Click(object sender, EventArgs e) // ENG 5 vkladka;
        {
            label72.Text = @"Builded PC";
            label72.Font = new Font("Unispace", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label74.Text = @"Components";
            label74.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label71.Text = @"CPU model";
            label71.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label73.Text = @"Motherboard";
            label73.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label77.Text = @"RAM";
            label77.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label70.Text = @"GPU model";
            label70.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label76.Text = @"SSD";
            label76.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label75.Text = @"HDD";
            label75.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label132.Text = @"PSU";
            label132.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label131.Text = @"Case";
            label131.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label130.Text = @"CPU cooler";
            label130.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label136.Text = @"Price";
            label136.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label137.Text = @"Price";
            label137.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label138.Text = @"Price";
            label138.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label64.Text = @"Price";
            label64.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label65.Text = @"Price";
            label65.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label66.Text = @"Price";
            label66.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label67.Text = @"Price";
            label67.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label68.Text = @"Price";
            label68.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label69.Text = @"Price";
            label69.Font = new Font("Unispace", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button27.Text = @"Save build";
            button27.Font = new Font("Unispace", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            button27.Size = new Size (85, 20);
            button27.Location = new Point(840, 670);
        }

        private void button108_Click(object sender, EventArgs e) // RU 6 vkladka;
        {
            label78.Text = @"Наиболее популярная сборка на Intel ЦП*";
            label78.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label111.Text = @"ЦП";
            label111.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label110.Text = @"Мат. плата";
            label110.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label87.Text = @"Видеокарта";
            label87.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label86.Text = @"ОЗУ";
            label86.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label85.Text = @"Главное хранилище (ССД)";
            label85.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label84.Text = @"Дополнительное хранилище (ПЗУ)";
            label84.Font = new Font("Times New Roman", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label83.Text = @"БП";
            label83.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label82.Text = @"Корпус";
            label82.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label81.Text = @"Куллер ЦП";
            label81.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label80.Text = @"Модель";
            label80.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label79.Text = @"Цена";
            label79.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label88.Text = @"* Рекомендованно pcpartpicker.com";
            label88.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label144.Text = @"* Рекомендованно games.mail.ru";
            label144.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label145.Text = @"* Рекомендованно hardprice.ru";
            label145.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button107_Click(object sender, EventArgs e) // ENG 6 vkladka;
        {
            label78.Text = @"The most popular build on Intel*";
            label78.Font = new Font("Unispace", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label111.Text = @"CPU";
            label111.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label110.Text = @"Motherboard";
            label110.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label87.Text = @"GPU";
            label87.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label86.Text = @"Memory (RAM)";
            label86.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label85.Text = @"Primary Storage (SSD)";
            label85.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label84.Text = @"Additional Storage (HDD)";
            label84.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label83.Text = @"PSU";
            label83.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label82.Text = @"Case";
            label82.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label81.Text = @"CPU cooler";
            label81.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label80.Text = @"Model";
            label80.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label79.Text = @"Price";
            label79.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label88.Text = @"* Recommended by pcpartpicker.com";
            label88.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label144.Text = @"* Recommended by games.mail.ru";
            label144.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label145.Text = @"* Recommended by hardprice.ru";
            label145.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button110_Click(object sender, EventArgs e)
        {
            label99.Text = @"Наиболее популярная сборка на AMD ЦП*";
            label99.Font = new Font("Times New Roman", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label101.Text = @"ЦП";
            label101.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label100.Text = @"Мат. плата";
            label100.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label98.Text = @"Видеокарта";
            label98.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label97.Text = @"ОЗУ";
            label97.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label96.Text = @"Главное хранилище (ССД)";
            label96.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label95.Text = @"Дополнительное хранилище (ПЗУ)";
            label95.Font = new Font("Times New Roman", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label94.Text = @"БП";
            label94.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label93.Text = @"Корпус";
            label93.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label92.Text = @"Куллер ЦП";
            label92.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label91.Text = @"Модель";
            label91.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label90.Text = @"Цена";
            label90.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label104.Text = @"* Рекомендованно pcgamehaven.com";
            label104.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label37.Text = @"* Рекомендованно hardwareluxx.ru";
            label37.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label29.Text = @"* Рекомендованно 3dnews.ru";
            label29.Font = new Font("Times New Roman", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

        private void button109_Click(object sender, EventArgs e)
        {
            label99.Text = @"The most popular build on AMD*";
            label99.Font = new Font("Unispace", 24F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label101.Text = @"CPU";
            label101.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label100.Text = @"Motherboard";
            label100.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label98.Text = @"GPU";
            label98.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label97.Text = @"Memory (RAM)";
            label97.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label96.Text = @"Primary Storage (SSD)";
            label96.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label95.Text = @"Additional Storage (HDD)";
            label95.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label94.Text = @"PSU";
            label94.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label93.Text = @"Case";
            label93.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label92.Text = @"CPU cooler";
            label92.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label91.Text = @"Model";
            label91.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label90.Text = @"Price";
            label90.Font = new Font("Unispace", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label104.Text = @"* Recommended by pcgamehaven.com";
            label104.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label37.Text = @"* Recommended by hardwareluxx.ru";
            label37.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            label29.Text = @"* Recommended by 3dnews.ru";
            label29.Font = new Font("Unispace", 9.7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        }

    }
}