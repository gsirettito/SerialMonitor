using System;

namespace SerialMonitor {
    public class OnEnterEventArgs : EventArgs {
        public OnEnterEventArgs(string command) {
            this.Command = command;
        }

        public string Command { get; internal set; }
    }
}