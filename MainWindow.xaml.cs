using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace ProgramLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process? _client;

        public MainWindow()
        {
            InitializeComponent();

            launcher.Text = ConfigHandler.Launcher;
            arguments.Text = ConfigHandler.Arguments;
            program.Text = ConfigHandler.Program;
        }

        public void UpdateTextCallback(string message)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate () {
                outLog.AppendText(message + "\n");

                if (outLog.Document.Blocks.Count > 1000) {
                    outLog.Document.Blocks.Remove(outLog.Document.Blocks.FirstBlock);
                }

                outLog.ScrollToEnd();
            });
        }

        private void Btn_Launcher(object sender, RoutedEventArgs e)
        {
            _client = new Process();
            _client.StartInfo.FileName = launcher.Text;
            _client.StartInfo.Arguments = $"{arguments.Text} {program.Text}";
            _client.StartInfo.UseShellExecute = false;
            _client.StartInfo.CreateNoWindow = true;
            _client.StartInfo.RedirectStandardInput = true;
            _client.StartInfo.RedirectStandardOutput = true;
            _client.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            _client.EnableRaisingEvents = true;
            _client.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
                UpdateTextCallback(e.Data ?? "");
            });
            _client.Exited += new EventHandler((sender, e) => {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate () {
                    process_status.Content = "程序未启动";
                });
            });
            _client.Start();

            process_status.Content = $"程序已启动，PID:{_client.Id} {_client.StartInfo.Arguments}";

            _client.StandardInput.AutoFlush = true;
            _client.BeginOutputReadLine();
        }

        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            StopProcess(_client);
        }

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

        public void StopProcess(Process? proc)
        {
            if (_client == null || _client.HasExited) {
                return;
            }

            // 以防父进程已经attach到另一个Console，先调一次FreeConsole
            WinApi.FreeConsole();

            // 一个进程最多只能attach到一个Console，否则失败，返回0
            if (WinApi.AttachConsole((uint)proc!.Id)) {
                // 设置父进程属性，忽略Ctrl-C信号
                WinApi.SetConsoleCtrlHandler(null, true);

                // 发出一个Ctrl-C到共享该控制台的所有进程中
                WinApi.GenerateConsoleCtrlEvent(WinApi.CtrlTypes.CTRL_C_EVENT, 0);

                // 父进程与控制台分离，此时子进程控制台收到Ctrl-C关闭
                WinApi.FreeConsole();

                // 现在父进程没有Console，为它新建一个
                //AllocConsole();

                // 等待子进程退出
                proc.WaitForExit(2000);

                // 恢复父进程处理Ctrl-C信号
                WinApi.SetConsoleCtrlHandler(null, false);

                // C#版的GetLastError()
                var lastError = Marshal.GetLastWin32Error();
            }

            _client.WaitForExit();
            _client.Close();
            _client.Dispose();
        }

#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

        private void Btn_Launcher_Select(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "可执行文件|*.exe";
            fileDialog.Title = "选择启动器程序";
            var selected = fileDialog.ShowDialog();
            if (selected != null && selected.Value) {
                launcher.Text = fileDialog.FileName;
            }
        }

        private void Btn_Program_Select(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "所有|*.*";
            fileDialog.Title = "选择程序";
            var selected = fileDialog.ShowDialog();
            if (selected != null && selected.Value) {
                program.Text = fileDialog.FileName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var messageBox = MessageBox.Show("是否关闭程序？", "确认", MessageBoxButton.YesNo);
            if (messageBox != MessageBoxResult.Yes) {
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopProcess(_client);
        }

        private void ShowMainWindow(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized) {
                Hide();
            }
        }

        private void launcher_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ConfigHandler.Launcher = launcher.Text;
            ConfigHandler.Save();
        }

        private void program_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ConfigHandler.Program = program.Text;
            ConfigHandler.Save();
        }

        private void arguments_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ConfigHandler.Arguments = arguments.Text;
            ConfigHandler.Save();
        }
    }
}