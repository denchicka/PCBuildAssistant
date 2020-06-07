# PCBuildAssistant

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CsvHelper;
using System.Globalization;

namespace WindowsFormsApplication6
{
    public partial class Form1 : Form
    {
        // Forma, inicializaciya componentov i dr.
        public Form1()

        {
            InitializeComponent(); // Inicializaciya vsex componentov;
            tabControl1.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem); // Otobrajenie vkladok sleva (vrode);
            comboBox13.Items.AddRange(new string[] { "Intel", "Amd" }); // Dobavlenie Itemov v ComboBox13;
        }

        // Zagryzka formi i ob'ektov ykazanix v nei;
        private void Form1_Load(object sender, EventArgs e)
        {
            // Sozdanie lista CPU
            List<CPU> cpus = CPUs.GetCPUValues(); // Polu4enie spiska CPU;
            foreach (CPU cpu in cpus) // dlya kajdogo CPU v liste CPU poly4it' spisok;
            {
                comboBox1.Items.Add(cpu); // ComboBox1 = CPU;
                comboBox2.Items.Add(cpu); // ComboBox2 = CPU;
            }
            // Sozdanie lista GPU
            List<GPU> cards = GPUs.GetGPUValues(); // dlya kajdogo GPU v liste GPU poly4it' spisok;
            foreach (GPU card in cards)
            {
                comboBox3.Items.Add(card); // ComboBox3 = GPU;
                comboBox4.Items.Add(card); // ComboBox4 = GPU;
            }
            // Sozdanie lista RAM
            List<RAM> rams = RAMs.GetRAMValues(); // Polu4enie spiska RAM;
            foreach (RAM ram in rams) // dlya kajdogo RAM v liste RAM poly4it' spisok;
            {
                comboBox5.Items.Add(ram); // ComboBox5 = RAM;
                comboBox6.Items.Add(ram); // ComboBox6 = RAM;
            }
            // Sozdanie lista SSD
            List<SSD> ssds = SSDs.GetSSDValues(); // Polu4enie spiska SSD;
            foreach (SSD ssd in ssds)
            {
                comboBox7.Items.Add(ssd); // ComboBox7 = SSD;
                comboBox8.Items.Add(ssd); // ComboBox8 = SSD;
            }
            // Sozdanie lista HDD
            List<HDD> hdds = HDDs.GetHDDValues(); // Polu4enie spiska HDD;
            foreach (HDD hdd in hdds)
            {
                comboBox9.Items.Add(hdd); // ComboBox9 = HDD;
                comboBox10.Items.Add(hdd); // ComboBox10 = HDD;
            }

            // Compare with my PC

            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;


        }

        // Otobrajenie vkladok sleva;
        private void tabControl1_DrawItem(Object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Indigo);
                g.FillRectangle(Brushes.Black, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Times New Roman", (float)12.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        // Sozdanie classa dlya vsex CPU;
        public static class CPUs
        {
            public static List<CPU> GetCPUValues()
            {
                string filePath = Path.Combine(Application.StartupPath, "CPU_UserBenchmarks.csv");
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

        // Sozdanie classa dlya vsex GPU;
        public static class GPUs
        {
            public static List<GPU> GetGPUValues()
            {
                string filePath = Path.Combine(Application.StartupPath, "GPU_UserBenchmarks.csv");
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
                string filePath = Path.Combine(Application.StartupPath, "RAM_UserBenchmarks.csv");
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
                string filePath = Path.Combine(Application.StartupPath, "SSD_UserBenchmarks.csv");
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
                string filePath = Path.Combine(Application.StartupPath, "HDD_UserBenchmarks.csv");
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

        // Knopka Compare with my PC;
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
            textBox5.Enabled = true;

            comboBox2.Enabled = false;
            comboBox4.Enabled = false;
            comboBox6.Enabled = false;
            comboBox8.Enabled = false;
            comboBox10.Enabled = false;

        }

        // Knopka Compare with other components;
        private void button88_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;

            comboBox2.Enabled = true;
            comboBox4.Enabled = true;
            comboBox6.Enabled = true;
            comboBox8.Enabled = true;
            comboBox10.Enabled = true;
        }

        // Sobitie dlya pokaza ranzici CPU;
        private void ComboBoxCPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            CPU leftCPU = comboBox1.SelectedItem as CPU;    // Viborka levogo CPU;
            CPU rightCPU = comboBox2.SelectedItem as CPU;   // Viborka pravogo CPU;
            float result = (float)Math.Abs((leftCPU?.Benchmark ?? 0.0f) - (rightCPU?.Benchmark ?? 0.0f));   // result = math.abs = module 4isla (levoe - pravoe);
            textBox6.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
        }

        // Sobitie dlya pokaza raznici GPU;
        private void ComboBoxGPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            GPU leftGPU = comboBox3.SelectedItem as GPU;    // Viborka levogo GPU;
            GPU rightGPU = comboBox4.SelectedItem as GPU;   // Viborka pravogo GPU;
            float result = (float)Math.Abs((leftGPU?.Benchmark ?? 0.0f) - (rightGPU?.Benchmark ?? 0.0f));   // result = math.abs = module 4isla (levoe - pravoe);
            textBox7.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
        }

        // Sobitie dlya pokaza raznici RAM;
        private void ComboBoxRAM_SelectedIndexChanged(object sender, EventArgs e)
        {
            RAM leftRAM = comboBox5.SelectedItem as RAM;    // Viborka levogo RAM;
            RAM rightRAM = comboBox6.SelectedItem as RAM;   // Viborka levogo RAM;
            float result = (float)Math.Abs((leftRAM?.Benchmark ?? 0.0f) - (rightRAM?.Benchmark ?? 0.0f));   // result = math.abs = module 4isla (levoe - pravoe);
            textBox8.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
        }

        // Sobitie dlya pokaza raznici SSD;
        private void ComboBoxSSD_SelectedIndexChanged(object sender, EventArgs e)
        {
            SSD leftSSD = comboBox7.SelectedItem as SSD;    // Viborka levogo SSD;
            SSD rightSSD = comboBox8.SelectedItem as SSD;   // Viborka levogo SSD;
            float result = (float)Math.Abs((leftSSD?.Benchmark ?? 0.0f) - (rightSSD?.Benchmark ?? 0.0f));   // result = math.abs = module 4isla (levoe - pravoe);
            textBox9.Text = $"{result:0.##}" + "%"; // Vivod result + okruglenie;
        }

        // Sobitie dlya pokaza raznici HDD;
        private void ComboBoxHDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            HDD leftHDD = comboBox9.SelectedItem as HDD;    // Viborka levogo HDD;
            HDD rightHDD = comboBox10.SelectedItem as HDD;  // Viborka levogo HDD;
            float result = (float)Math.Abs((leftHDD?.Benchmark ?? 0.0f) - (rightHDD?.Benchmark ?? 0.0f));   // result = math.abs = module 4isla (levoe - pravoe);
            textBox10.Text = $"{result:0.##}" + "%";    // Vivod result + okruglenie;
        }
    }
}
