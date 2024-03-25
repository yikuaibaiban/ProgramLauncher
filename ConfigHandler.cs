using System.IO;
using System.Text;

namespace ProgramLauncher
{
    /// <summary>
    /// 配置处理器
    /// </summary>
    public static class ConfigHandler
    {
        /// <summary>
        /// 启动器路径
        /// </summary>
        public static string Launcher { get; set; } = "";

        /// <summary>
        /// 启动器参数
        /// </summary>
        public static string Arguments { get; set; } = "";

        /// <summary>
        /// 启动程序
        /// </summary>
        public static string Program { get; set; } = "";

        static ConfigHandler()
        {
            if (!File.Exists("config.txt")) {
                File.Create("config.txt").Close();

                Launcher = "";
                Arguments = "";
                Program = "";
                Save();
                return;
            }

            File.ReadAllLines("config.txt", Encoding.UTF8).ToList().ForEach(line => {
                var kv = line.Split('=');
                switch (kv[0]) {
                    case "launcher":
                        Launcher = kv[1];
                        break;

                    case "arguments":
                        Arguments = kv[1];
                        break;

                    case "program":
                        Program = kv[1];
                        break;
                }
            });
        }

        /// <summary>
        /// 保存
        /// </summary>
        public static void Save()
        {
            var lines = new List<string> {
                $"launcher={Launcher}",
                $"arguments={Arguments}",
                $"program={Program}"
            };
            File.WriteAllLines("config.txt", lines, Encoding.UTF8);
        }
    }
}