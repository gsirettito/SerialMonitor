using SiretT;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SerialMonitor {
    public class USBDeviceInfo {
        public USBDeviceInfo(string _deviceID, string _pnpDeviceID, string _description, string _name) {
            this.DeviceID = _deviceID;
            this.PnpDeviceID = _pnpDeviceID;
            this.Description = _description;
            this.Name = _name;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
    }
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private SerialPort sp;
        private IniFile ini;
        private int cindx;
        private List<string> cmds = new List<string>();
        private bool useTransparency;
        private DateTime startTime;

        public MainWindow() {
            InitializeComponent();
            var local_path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            combaud.Items.Add(110);
            combaud.Items.Add(300);
            combaud.Items.Add(600);
            combaud.Items.Add(1200);
            combaud.Items.Add(2400);
            combaud.Items.Add(4800);
            combaud.Items.Add(9600);
            combaud.Items.Add(19200);
            combaud.Items.Add(38400);
            combaud.Items.Add(57600);
            combaud.Items.Add(115200);
            combaud.Items.Add(230400);
            combaud.Items.Add(460800);
            combaud.Items.Add(921600);

            databits.Items.Add(7);
            databits.Items.Add(8);

            parity.Items.Add(Parity.None);
            parity.Items.Add(Parity.Even);
            parity.Items.Add(Parity.Mark);
            parity.Items.Add(Parity.Odd);
            parity.Items.Add(Parity.Space);

            stopbits.Items.Add(StopBits.None);
            stopbits.Items.Add(StopBits.One);
            stopbits.Items.Add(StopBits.OnePointFive);
            stopbits.Items.Add(StopBits.Two);

            endl.Items.Add("No line ending");
            endl.Items.Add("Both NL & CR");
            endl.Items.Add("Newline");
            endl.Items.Add("Carriage return");

            ini = new IniFile(local_path + "\\config.ini");
            var loc = (Point)ini.GetValue("Main\\Location", new Point(this.Left, this.Top));
            this.Left = loc.X;
            this.Top = loc.Y;
            var size = (Point)ini.GetValue("Main\\Size", new Point(this.Width, this.Height));
            this.Width = size.X;
            this.Height = size.Y;
            this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), (ini.GetValue("Main\\State", (object)"Normal").ToString()));
            autoscroll.IsChecked = (bool)ini.GetValue("Main\\Autoscroll", true);
            endl.SelectedValue = ini.GetValue("Main\\EndingLine", (object)"Both NL & CR").ToString();
            console.Background = new SolidColorBrush(ColorFromHtmlFormat(ini.GetValue("Main\\Background", (object)"#FFFDFDFD").ToString()));
            console.Foreground = new SolidColorBrush(ColorFromHtmlFormat(ini.GetValue("Main\\Foreground", (object)"#FF000000").ToString()));
            console.FontFamily = new FontFamily(ini.GetValue("Main\\FontFamily", (object)"Console").ToString());
            console.FontSize = double.Parse(ini.GetValue("Main\\FontSize", 12.0).ToString());

            var lastp = ini.GetValue("Port\\LastPort", (object)"").ToString();
            ScanPorts();
            foreach (var i in comports.Items) {
                if ((i as COMPort).Name == lastp)
                    comports.SelectedValue = i;
            }
            combaud.SelectedValue = ini.GetValue("Port\\Baudrate", 9600);
            databits.SelectedValue = ini.GetValue("Port\\DataBits", 8);
            parity.SelectedValue = Enum.Parse(typeof(Parity), (ini.GetValue("Port\\Parity", (object)"None").ToString()));
            stopbits.SelectedValue = Enum.Parse(typeof(StopBits), (ini.GetValue("Port\\StopBits", (object)"One").ToString()));
            sendBox.Focus();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            ini.AddOrUpdate("Main\\Location", new Point(this.Left, this.Top));
            ini.AddOrUpdate("Main\\Size", new Point(this.Width, this.Height));
            ini.AddOrUpdate("Main\\State", this.WindowState == WindowState.Minimized ? WindowState.Normal : WindowState);
            ini.AddOrUpdate("Main\\Autoscroll", (bool)autoscroll.IsChecked);
            ini.AddOrUpdate("Main\\EndingLine", endl.SelectedValue.ToString());
            
            ini.AddOrUpdate("Main\\Background", ColorToHtmlFormat((console.Background as SolidColorBrush).Color));
            ini.AddOrUpdate("Main\\Foreground", ColorToHtmlFormat((console.Foreground as SolidColorBrush).Color));
            ini.AddOrUpdate("Main\\FontFamily", console.FontFamily.ToString());
            ini.AddOrUpdate("Main\\FontSize", console.FontSize);

            ini.AddOrUpdate("Port\\LastPort", ((COMPort)comports.SelectedValue).Name);
            ini.AddOrUpdate("Port\\Baudrate", combaud.SelectedValue.ToString());
            ini.AddOrUpdate("Port\\DataBits", databits.SelectedValue.ToString());
            ini.AddOrUpdate("Port\\Parity", parity.SelectedValue.ToString());
            ini.AddOrUpdate("Port\\StopBits", stopbits.SelectedValue.ToString());
            ini.Save();
        }

        private string ColorToHtmlFormat(Color color) {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        private Color ColorFromHtmlFormat(string htmlFormat) {
            var hex = htmlFormat[0] == '#' ? htmlFormat.Substring(1).ToUpper(): htmlFormat;
            var int32 = Convert.ToInt32(hex, 16);
            var color = Color.FromArgb(
                    (byte)((int32 & 0xFF000000) >> 24),
                    (byte)((int32 & 0xFF0000) >> 16),
                    (byte)((int32 & 0xFF00) >> 8),
                    (byte)(int32 & 0xFF));
            return color;
        }

        static List<USBDeviceInfo> GetUSBDevices(string filter = null) {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
                collection = searcher.Get();

            try {
                foreach (var device in collection) {
                    string name = device.GetPropertyValue("Name").ToString();
                    if (filter != null && !name.ToLower().Contains(filter.ToLower())) continue;
                    string description = (string)device.GetPropertyValue("Description");
                    string pnpDeviceID = (string)device.GetPropertyValue("PNPDeviceID");
                    string deviceID = (string)device.GetPropertyValue("DeviceID");
                    devices.Add(new USBDeviceInfo(deviceID, pnpDeviceID, description, name));
                }
            } catch { }

            collection.Dispose();
            return devices;
        }

        private void ScanPorts() {
            var devices = GetUSBDevices("COM");
            comports.Items.Clear();
            foreach (var i in SerialPort.GetPortNames()) {
                var device = devices.Where(d => d.Name.Contains(i)).ToArray();
                var description = "";
                if (device.Length > 0)
                    description = device[0].Description;
                comports.Items.Add(new COMPort() { Name = i, Description = description });
            }
            comports.SelectedIndex = 0;
        }

        private void comports_DropDownOpened(object sender, EventArgs e) {
            ScanPorts();
        }

        internal class COMPort {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private bool SerialDisconnect() {
            if (sp != null && sp.IsOpen) {
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Close();
                sp.Dispose();
                this.Title = "Serial Monitor";
                return true;
            }else if (!sp.IsOpen) {
                this.Title = "Serial Monitor";
                return true;
            }
            return false;
        }

        private bool SerialConnect() {
            if (comports.SelectedValue == null || combaud.SelectedValue == null || comports.SelectedIndex == 0) {
                if (sp != null && sp.IsOpen) sp.Close();
                return false;
            }
            if (sp != null && sp.IsOpen) sp.Close();
            sp = new SerialPort(((COMPort)comports.SelectedValue).Name);
            sp.Parity = (Parity)parity.SelectedValue;
            sp.DataBits = (int)databits.SelectedValue;
            sp.StopBits = (StopBits)stopbits.SelectedValue;
            sp.BaudRate = Convert.ToInt32(combaud.SelectedValue.ToString());
            sp.DataReceived += Sp_DataReceived;
            sp.ErrorReceived += Sp_ErrorReceived;
            try {
                sp.Open();
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.BaseStream.Flush();
                this.Title = "Serial Monitor - " + sp.PortName;
            } catch (Exception ex) {
                comports.SelectedIndex = 0;
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e) {
            if (SerialConnect()) {
                btnConnect.Content = "Disconnect";
                comports.IsEnabled = false;
                combaud.IsEnabled = false;
            } else {
                comports.IsEnabled = true;
                combaud.IsEnabled = true;
                btnConnect.Checked -= ToggleButton_Checked;
                btnConnect.Unchecked -= ToggleButton_Unchecked;
                btnConnect.IsChecked = false;
                btnConnect.Checked += ToggleButton_Checked;
                btnConnect.Unchecked += ToggleButton_Unchecked;
            }
            sendBox.Focus();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e) {
            if (SerialDisconnect()) {
                btnConnect.Content = "Connect";
                comports.IsEnabled = true;
                combaud.IsEnabled = true;
            } else {
                comports.IsEnabled = false;
                combaud.IsEnabled = false;
                btnConnect.Checked -= ToggleButton_Checked;
                btnConnect.Unchecked -= ToggleButton_Unchecked;
                btnConnect.IsChecked = true;
                btnConnect.Checked += ToggleButton_Checked;
                btnConnect.Unchecked += ToggleButton_Unchecked;
            }
            sendBox.Focus();
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            var time = DateTime.Now - startTime;
            timer.Dispatcher.Invoke(new Action(() => timer.Text = time.TotalMilliseconds.ToString()));
            try {
                string read = sp.ReadExisting();
                console.Dispatcher.Invoke(new Action(() => {
                    console.Text += (read);
                    if ((bool)autoscroll.IsChecked)
                        console.ScrollToEnd();
                }));
            } catch (Exception ex) { }
        }

        private void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            MessageBox.Show(e.EventType.ToString());
            sp.DiscardInBuffer();
            sp.DiscardOutBuffer();
        }

        private void SendBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Send();
            } else if (e.Key == Key.Up) {
                if (cmds.Count > 0) {
                    cindx = cindx < 1 ? 0 : cindx - 1;
                    sendBox.Text = cmds[cindx];
                }
                e.Handled = true;
                sendBox.CaretIndex = sendBox.Text.Length;
            } else if (e.Key == Key.Down) {
                if (cmds.Count > 0) {
                    cindx = cindx >= cmds.Count - 1 ? cmds.Count - 1 : cindx + 1;
                    sendBox.Text = cmds[cindx];
                    if (cindx == cmds.Count - 1) cindx += 1;
                }
                e.Handled = true;
                sendBox.CaretIndex = sendBox.Text.Length;
            }
        }

        private void send_Click(object sender, RoutedEventArgs e) {
            Send();
        }

        private void Send() {
            if (sp != null && sp.IsOpen) {
                string cmd = sendBox.Text;
                string write = sendBox.Text;
                switch (endl.SelectedValue.ToString()) {
                    case "Both NL & CR": write += "\r\n"; break;
                    case "Newline": write += "\n"; break;
                    case "Carriage return": write += "\r"; break;
                }
                sp.Write(write);
                startTime = DateTime.Now;
                if (cmds.Count == 0 || cmds[cindx == cmds.Count ? cindx - 1 : cindx] != cmd) {
                    cmds.Add(cmd);
                    cindx = cmds.Count - 1;
                }
                sendBox.Text = "";
            }
        }

        ~MainWindow() {
            if (sp != null && sp.IsOpen) {
                sp.Close();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e) {
            console.Text = "";
            sendBox.Focus();
        }
    }
}
