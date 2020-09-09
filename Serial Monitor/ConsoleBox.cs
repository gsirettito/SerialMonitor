using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SerialMonitor {
    public class ConsoleBox : TextBox {
        private int cindx;
        private List<string> cmds = new List<string>();
        private string prevText;
        private string name = "ConsoleBox";
        private string version = "1.0.0.0";
        public event EventHandler<OnEnterEventArgs> OnEnter;

        public ConsoleBox() {
            this.Background = new SolidColorBrush(Color.FromArgb(255, 12, 12, 12));
            this.Foreground = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
            this.CaretBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.SelectionBrush = new SolidColorBrush(Color.FromArgb(255, 147, 147, 147));
            this.FontFamily = new FontFamily("Consolas");
            this.FontSize = 13.46;
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            base.PreviewKeyDown += ConsoleBox_PreviewKeyDown;
            StartMessage = string.Format("{0} [{1}]", this.name, this.version);
            Write(StartMessage);
        }

        public string Prompt { get; set; }

        public string StartMessage { get; set; }

        public bool WhaitForCommand { get; set; }

        public bool ScrollToEnd { get; set; } = true;

        public void Restart() {
            Clear();
            Write(StartMessage + "\n");
            WritePrompt();
        }

        public void WritePrompt() {
            this.Text += "\n" + Prompt;
            this.CaretIndex = this.Text.Length;
            prevText = this.Text;
            if (ScrollToEnd)
                this.ScrollToEnd();
        }

        public void Write(object obj) {
            this.Text += obj;
            this.CaretIndex = this.Text.Length;
            prevText = this.Text;
            if (ScrollToEnd)
                this.ScrollToEnd();
        }

        public void WriteAnswer(object obj) {
            this.Text = prevText;
            this.Text += "\n" + obj + "\n" + Prompt;
            this.CaretIndex = this.Text.Length;
            prevText = this.Text;
            if (ScrollToEnd)
                this.ScrollToEnd();
        }

        private void SendCommand(string command) {
            if (OnEnter != null) {
                OnEnter(this, new OnEnterEventArgs(command));
                var st = DateTime.Now;
                while (WhaitForCommand && DateTime.Now - st < TimeSpan.FromSeconds(0.5)) ;
                //WritePrompt();
            }
        }

        private void ConsoleBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            int lindx = this.Text.LastIndexOf(Prompt) + Prompt.Length;
            if (e.Key == Key.Enter) {
                string cmd = this.Text.Substring(lindx);
                if (cmds.Count == 0 || cmds[cindx == cmds.Count ? cindx - 1 : cindx] != cmd) {
                    cmds.Add(cmd);
                    cindx = cmds.Count - 1;
                }
                SendCommand(cmd);
                //WritePrompt();
                prevText = this.Text;
            } else if (e.Key == Key.Up) {
                if (cmds.Count > 0) {
                    cindx = cindx < 1 ? 0 : cindx - 1;
                    this.Text = prevText + cmds[cindx];
                }
                e.Handled = true;
                this.CaretIndex = this.Text.Length;
            } else if (e.Key == Key.Down) {
                if (cmds.Count > 0) {
                    cindx = cindx >= cmds.Count - 1 ? cmds.Count - 1 : cindx + 1;
                    this.Text = prevText + cmds[cindx];
                    if (cindx == cmds.Count - 1) cindx += 1;
                }
                e.Handled = true;
                this.CaretIndex = this.Text.Length;
            } else if (e.Key == Key.Back) {
                if (this.SelectionStart - 1 < lindx)
                    e.Handled = true;
            } else if (e.Key == Key.Delete) {
                if (this.SelectionStart < lindx)
                    e.Handled = true;
            } else if (e.Key == Key.Home) {
                this.CaretIndex = lindx;
                e.Handled = true;
            } else if ((e.Key == Key.C || e.Key == Key.X || e.Key == Key.V) && Keyboard.Modifiers == ModifierKeys.Control) {
            } else if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.End) {
                if (this.SelectionStart < lindx)
                    e.Handled = true;
            }
        }
    }
}
