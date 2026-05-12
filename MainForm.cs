using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SkasoHandheldSwitcher
{
    public class MainForm : Form
    {
        private readonly Color DiscordBg = Color.FromArgb(54, 57, 63);
        private readonly Color DiscordPanel = Color.FromArgb(47, 49, 54);
        private readonly Color DiscordBorder = Color.FromArgb(32, 34, 37);
        private readonly Color DiscordText = Color.FromArgb(220, 221, 222);
        private readonly Color DiscordMuted = Color.FromArgb(114, 118, 125);
        private readonly Color Green = Color.FromArgb(67, 181, 129);
        private readonly Color Red = Color.FromArgb(240, 71, 71);
        private readonly Color Yellow = Color.FromArgb(250, 166, 26);

        private string installDir = @"C:\ProgramData\Skaso\";
        private string physpanelPath;
        private string taskName = "SkasoPhyspanelStartup";
        private int currentBuild;
        private readonly int requiredBuild = 26100;

        private readonly List<string> xboxFeatureIds = new List<string> { "58989070", "59765208" };

        private readonly List<string> resourceFiles = new List<string>
        {
            "physpanel.exe",
            "ViVeTool.exe",
            "Albacore.ViVe.dll",
            "Newtonsoft.Json.dll",
            "FeatureDictionary.pfs"
        };

        private Panel statusBadge;
        private ToggleSwitch toggleSwitch;
        private RichTextBox logBox;
        private CheckBox chkVive1;
        private CheckBox chkVive2;
        private Label lblRegistryStatus;
        private Label lblTaskStatus;
        private Label lblViveStatus;
        private Label lblStatus;
        private Label buildLabel;
        private bool isToggling = false;

        public MainForm()
        {
            physpanelPath = Path.Combine(installDir, "physpanel.exe");
            InitializeComponent();
            Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            Text = "Skaso Handheld Switcher v1";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(720, 680);
            BackColor = DiscordBg;
            ForeColor = DiscordText;
            Font = new Font("Segoe UI", 10);

            var topBar = new Panel();
            topBar.Size = new Size(ClientSize.Width, 60);
            topBar.Location = new Point(0, 0);
            topBar.BackColor = DiscordPanel;

            var titleLabel = new Label();
            titleLabel.Text = "  Skaso Handheld Switcher v1";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = DiscordText;
            titleLabel.Size = new Size(420, 28);
            titleLabel.Location = new Point(0, 8);

            buildLabel = new Label();
            buildLabel.Text = "  Verificando...";
            buildLabel.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            buildLabel.ForeColor = DiscordMuted;
            buildLabel.Size = new Size(350, 18);
            buildLabel.Location = new Point(2, 36);

            statusBadge = new Panel();
            statusBadge.Size = new Size(110, 26);
            statusBadge.Location = new Point(ClientSize.Width - 125, 17);
            statusBadge.Paint += StatusBadge_Paint;
            statusBadge.BackColor = Color.Transparent;

            topBar.Controls.Add(titleLabel);
            topBar.Controls.Add(buildLabel);
            topBar.Controls.Add(statusBadge);

            var body = new Panel();
            body.Size = new Size(ClientSize.Width, 95);
            body.Location = new Point(0, 62);
            body.BackColor = DiscordPanel;

            toggleSwitch = new ToggleSwitch();
            toggleSwitch.Location = new Point(28, 8);
            toggleSwitch.CheckedChanged += ToggleSwitch_CheckedChanged;
            toggleSwitch.BackColor = DiscordPanel;

            var rightPane = new Panel();
            rightPane.Size = new Size(370, 56);
            rightPane.Location = new Point(340, 19);
            rightPane.BackColor = Color.FromArgb(38, 40, 46);

            lblRegistryStatus = new Label();
            lblRegistryStatus.Text = "Registry: --";
            lblRegistryStatus.Font = new Font("Segoe UI", 9);
            lblRegistryStatus.ForeColor = DiscordText;
            lblRegistryStatus.Size = new Size(350, 18);
            lblRegistryStatus.Location = new Point(10, 3);

            lblTaskStatus = new Label();
            lblTaskStatus.Text = "Tarea: --";
            lblTaskStatus.Font = new Font("Segoe UI", 9);
            lblTaskStatus.ForeColor = DiscordText;
            lblTaskStatus.Size = new Size(350, 18);
            lblTaskStatus.Location = new Point(10, 21);

            lblViveStatus = new Label();
            lblViveStatus.Text = "ViVeTool: --";
            lblViveStatus.Font = new Font("Segoe UI", 9);
            lblViveStatus.ForeColor = DiscordText;
            lblViveStatus.Size = new Size(350, 18);
            lblViveStatus.Location = new Point(10, 39);

            rightPane.Controls.Add(lblRegistryStatus);
            rightPane.Controls.Add(lblTaskStatus);
            rightPane.Controls.Add(lblViveStatus);

            body.Controls.Add(toggleSwitch);
            body.Controls.Add(rightPane);

            var logOuter = new Panel();
            logOuter.Size = new Size(ClientSize.Width - 16, 250);
            logOuter.Location = new Point(8, 165);
            logOuter.BackColor = DiscordPanel;
            logOuter.Paint += (s, e) => {
                using (var pen = new Pen(DiscordBorder, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, logOuter.Width - 1, logOuter.Height - 1);
            };

            var logHdr = new Label();
            logHdr.Text = "  Verificaciones";
            logHdr.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            logHdr.ForeColor = DiscordMuted;
            logHdr.Size = new Size(logOuter.Width - 2, 22);
            logHdr.Location = new Point(1, 1);
            logHdr.BackColor = Color.FromArgb(38, 40, 46);

            logBox = new RichTextBox();
            logBox.Location = new Point(1, 23);
            logBox.Size = new Size(logOuter.Width - 2, logOuter.Height - 24);
            logBox.BackColor = Color.FromArgb(35, 37, 42);
            logBox.ForeColor = DiscordText;
            logBox.Font = new Font("Consolas", 9);
            logBox.ReadOnly = true;
            logBox.BorderStyle = BorderStyle.None;
            logBox.WordWrap = false;

            logOuter.Controls.Add(logHdr);
            logOuter.Controls.Add(logBox);

            var toolsOuter = new Panel();
            toolsOuter.Size = new Size(ClientSize.Width - 16, 160);
            toolsOuter.Location = new Point(8, 425);
            toolsOuter.BackColor = DiscordPanel;
            toolsOuter.Paint += (s, e) => {
                using (var pen = new Pen(DiscordBorder, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, toolsOuter.Width - 1, toolsOuter.Height - 1);
            };

            var toolsHdr = new Label();
            toolsHdr.Text = "  Herramientas";
            toolsHdr.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            toolsHdr.ForeColor = DiscordMuted;
            toolsHdr.Size = new Size(toolsOuter.Width - 2, 22);
            toolsHdr.Location = new Point(1, 1);
            toolsHdr.BackColor = Color.FromArgb(38, 40, 46);

            var viveTitle = new Label();
            viveTitle.Text = "ViVeTool IDs:";
            viveTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            viveTitle.ForeColor = DiscordText;
            viveTitle.Size = new Size(100, 18);
            viveTitle.Location = new Point(14, 28);

            chkVive1 = new CheckBox();
            chkVive1.Text = "58989070";
            chkVive1.ForeColor = DiscordText;
            chkVive1.Font = new Font("Consolas", 9);
            chkVive1.Size = new Size(140, 20);
            chkVive1.Location = new Point(14, 48);
            chkVive1.BackColor = DiscordPanel;

            chkVive2 = new CheckBox();
            chkVive2.Text = "59765208";
            chkVive2.ForeColor = DiscordText;
            chkVive2.Font = new Font("Consolas", 9);
            chkVive2.Size = new Size(140, 20);
            chkVive2.Location = new Point(14, 68);
            chkVive2.BackColor = DiscordPanel;

            var btnEnableVive = new Button();
            btnEnableVive.Text = "Activar IDs";
            btnEnableVive.Size = new Size(95, 30);
            btnEnableVive.Location = new Point(170, 48);
            btnEnableVive.FlatStyle = FlatStyle.Flat;
            btnEnableVive.FlatAppearance.BorderColor = Green;
            btnEnableVive.ForeColor = Green;
            btnEnableVive.BackColor = DiscordPanel;
            btnEnableVive.Cursor = Cursors.Hand;
            btnEnableVive.Click += (s, e) => ApplyViveAction("/enable", "ENABLED", btnEnableVive);

            var btnDisableVive = new Button();
            btnDisableVive.Text = "Desactivar IDs";
            btnDisableVive.Size = new Size(110, 30);
            btnDisableVive.Location = new Point(275, 48);
            btnDisableVive.FlatStyle = FlatStyle.Flat;
            btnDisableVive.FlatAppearance.BorderColor = Yellow;
            btnDisableVive.ForeColor = Yellow;
            btnDisableVive.BackColor = DiscordPanel;
            btnDisableVive.Cursor = Cursors.Hand;
            btnDisableVive.Click += (s, e) => ApplyViveAction("/disable", "DISABLED", btnDisableVive);

            var viveNote = new Label();
            viveNote.Text = "Solo modifica flags ViVeTool, no toca registro ni physpanel";
            viveNote.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            viveNote.ForeColor = DiscordMuted;
            viveNote.Size = new Size(400, 16);
            viveNote.Location = new Point(170, 85);

            var stLabel = new Label();
            stLabel.Text = "Estado actual:";
            stLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            stLabel.ForeColor = DiscordText;
            stLabel.Size = new Size(100, 18);
            stLabel.Location = new Point(420, 28);

            lblStatus = new Label();
            lblStatus.Text = "Selecciona IDs y haz clic en Activar o Desactivar.";
            lblStatus.Font = new Font("Segoe UI", 8);
            lblStatus.ForeColor = DiscordMuted;
            lblStatus.Size = new Size(260, 60);
            lblStatus.Location = new Point(420, 48);

            toolsOuter.Controls.Add(toolsHdr);
            toolsOuter.Controls.Add(viveTitle);
            toolsOuter.Controls.Add(chkVive1);
            toolsOuter.Controls.Add(chkVive2);
            toolsOuter.Controls.Add(btnEnableVive);
            toolsOuter.Controls.Add(btnDisableVive);
            toolsOuter.Controls.Add(viveNote);
            toolsOuter.Controls.Add(stLabel);
            toolsOuter.Controls.Add(lblStatus);

            var linkCanal = new LinkLabel();
            linkCanal.Text = "Creado por Skaso. ¡Gracias por usar la app! Visita mi canal de YouTube";
            linkCanal.LinkColor = Green;
            linkCanal.ActiveLinkColor = Color.White;
            linkCanal.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            linkCanal.Size = new Size(500, 20);
            linkCanal.Location = new Point(12, 605);
            linkCanal.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://www.youtube.com/@Skasorpg") { UseShellExecute = true });

            var linkVive = new LinkLabel();
            linkVive.Text = "Agradecimientos a: ViVeTool (thebookisclosed)";
            linkVive.LinkColor = DiscordMuted;
            linkVive.ActiveLinkColor = Color.White;
            linkVive.Font = new Font("Segoe UI", 8);
            linkVive.Size = new Size(260, 16);
            linkVive.Location = new Point(12, 630);
            linkVive.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/thebookisclosed/ViVe/releases") { UseShellExecute = true });

            var linkPhys = new LinkLabel();
            linkPhys.Text = "y physpanel (riverar)";
            linkPhys.LinkColor = DiscordMuted;
            linkPhys.ActiveLinkColor = Color.White;
            linkPhys.Font = new Font("Segoe UI", 8);
            linkPhys.Size = new Size(200, 16);
            linkPhys.Location = new Point(275, 630);
            linkPhys.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/riverar/physpanel") { UseShellExecute = true });

            Controls.Add(topBar);
            Controls.Add(body);
            Controls.Add(logOuter);
            Controls.Add(toolsOuter);
            Controls.Add(linkCanal);
            Controls.Add(linkVive);
            Controls.Add(linkPhys);
        }

        private GraphicsPath CreateRoundRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float r2 = radius * 2;
            path.AddArc(rect.X, rect.Y, r2, r2, 180, 90);
            path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90);
            path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void StatusBadge_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            bool active = toggleSwitch.Checked;
            Color bg = active ? Green : Color.FromArgb(79, 84, 92);
            string text = active ? "ACTIVO" : "INACTIVO";
            var rect = new RectangleF(0, 0, statusBadge.Width - 1, statusBadge.Height - 1);
            using (var path = CreateRoundRect(rect, 13))
            using (var brush = new SolidBrush(bg))
                e.Graphics.FillPath(brush, path);
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(text, font, brush, rect, sf);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                Log("ERROR: Requiere permisos de Administrador.", Red);
                Log("Cierra y ejecuta como Administrador.", Red);
                lblStatus.Text = "Sin permisos de administrador";
                lblStatus.ForeColor = Red;
                toggleSwitch.Enabled = false;
                return;
            }
            Log("Permisos de administrador confirmados.", Green);

            currentBuild = GetWindowsBuild();
            buildLabel.Text = "  Build " + currentBuild;

            if (currentBuild < requiredBuild)
            {
                buildLabel.ForeColor = Red;
                Log("Build " + currentBuild + " INCOMPATIBLE. Requiere " + requiredBuild + "+", Red);
                Log("(24H2 con KB5083631 o 25H2 Dev Channel)", Red);
                toggleSwitch.Enabled = false;
                return;
            }
            buildLabel.ForeColor = Green;
            Log("Build " + currentBuild + " compatible.", Green);
            UpdateAllStatus();
        }

        private void UpdateAllStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\OEM"))
                {
                    object val = key?.GetValue("DeviceForm", 0);
                    int deviceForm = (val is int i) ? i : 0;
                    bool active = (deviceForm == 0x2e);
                    toggleSwitch.Checked = active;
                    lblRegistryStatus.Text = active
                        ? "Registry:  DeviceForm = 0x2e  (OK)"
                        : "Registry:  DeviceForm = -- (no activo)";
                    lblRegistryStatus.ForeColor = active ? Green : DiscordMuted;
                }
            }
            catch
            {
                lblRegistryStatus.Text = "Registry:  Error al leer";
                lblRegistryStatus.ForeColor = Red;
            }

            UpdateTaskStatus();
            UpdateViveStatus();
            statusBadge.Invalidate();
        }

        private bool TaskExists()
        {
            try
            {
                var p = new Process();
                p.StartInfo.FileName = "schtasks.exe";
                p.StartInfo.Arguments = "/query /TN \"" + taskName + "\" /FO CSV /NH";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();
                p.WaitForExit(5000);
                return p.ExitCode == 0;
            }
            catch { return false; }
        }

        private void UpdateTaskStatus()
        {
            bool exists = TaskExists();
            lblTaskStatus.Text = exists ? "Tarea:  SkasoPhyspanelStartup  (existente)" : "Tarea:  No existe";
            lblTaskStatus.ForeColor = exists ? Green : DiscordMuted;
        }

        private string QueryViveFeature(string id)
        {
            try
            {
                var p = new Process();
                p.StartInfo.FileName = Path.Combine(installDir, "ViVeTool.exe");
                p.StartInfo.Arguments = "/query /id:" + id;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WorkingDirectory = installDir;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(10000);
                return output;
            }
            catch { return ""; }
        }

        private void UpdateViveStatus()
        {
            EnsureViveToolExists();
            
            int enabled = 0;
            foreach (var id in xboxFeatureIds)
            {
                string q = QueryViveFeature(id);
                if (q.Contains("Enabled") || q.Contains("ON"))
                    enabled++;
            }
            lblViveStatus.Text = "ViVeTool:  " + enabled + "/2 IDs activos";
            lblViveStatus.ForeColor = enabled == 2 ? Green : DiscordMuted;
        }

        private async void ToggleSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (isToggling) return;
            isToggling = true;
            toggleSwitch.Enabled = false;
            logBox.Clear();

            try
            {
                if (toggleSwitch.Checked)
                {
                    await Task.Run(() => ActivateHandheldMode());
                    Log("", Color.White);
                    Log("EXITO TOTAL! Modo Xbox activado.", Green);
                    Log("Reinicia la PC para aplicar cambios.", Yellow);
                }
                else
                {
                    await Task.Run(() => DeactivateHandheldMode());
                    Log("", Color.White);
                    Log("Sistema restaurado.", Yellow);
                    Log("Reinicia para volver a resolucion nativa.", Yellow);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message, Red);
                toggleSwitch.Checked = !toggleSwitch.Checked;
            }
            finally
            {
                UpdateAllStatus();
                isToggling = false;
                toggleSwitch.Enabled = true;
            }
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private int GetWindowsBuild()
        {
            string build = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                "CurrentBuild", "0") as string ?? "0";
            int.TryParse(build, out int num);
            return num;
        }

        private void Log(string message, Color color)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => Log(message, color)));
                return;
            }
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = color;
            logBox.AppendText(message + "\n");
            logBox.ScrollToCaret();
        }

        private void ExtractResource(string resourceName, string targetPath)
        {
            string fullName = "SkasoHandheldSwitcher." + resourceName;
            using (var stream = typeof(Program).Assembly.GetManifestResourceStream(fullName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Recurso '" + fullName + "' no encontrado.");
                using (var fs = new FileStream(targetPath, FileMode.Create))
                    stream.CopyTo(fs);
            }
        }

        private void RunSchTasks(string arguments)
        {
            var p = new Process();
            p.StartInfo.FileName = "schtasks.exe";
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                string err = p.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(err) && !err.Contains("does not exist"))
                    throw new Exception("schtasks: " + err);
            }
        }

        private string RunViveTool(string action, string featureId)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(installDir, "ViVeTool.exe");
            p.StartInfo.Arguments = action + " /id:" + featureId;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.WorkingDirectory = installDir;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(15000);
            return output;
        }

        // =============== ACCIONES PRINCIPALES ===============

        private void ActivateHandheldMode()
        {
            Log("--- Activando Modo Xbox ---", Color.Cyan);

            Log("[1/4] Registro ─────────────────", Color.White);
            Log("  [CHECK] Escribiendo DeviceForm = 0x2e...", Color.White);
            using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\OEM"))
                key.SetValue("DeviceForm", 0x2e, RegistryValueKind.DWord);
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\OEM"))
            {
                int val = (int)(key?.GetValue("DeviceForm", 0) ?? 0);
                if (val == 0x2e)
                    Log("  [OK] DeviceForm = 0x2e confirmado", Green);
                else
                    Log("  [FAIL] DeviceForm = " + val + " (esperado 0x2e)", Red);
            }

            Log("[2/4] Recursos ────────────────", Color.White);
            if (!Directory.Exists(installDir))
                Directory.CreateDirectory(installDir);
            foreach (var file in resourceFiles)
            {
                string target = Path.Combine(installDir, file);
                Log("  [CHECK] Extrayendo " + file + "...", Color.White);
                ExtractResource(file, target);
                var fi = new FileInfo(target);
                Log("  [OK] " + file + " (" + fi.Length + " bytes)", Green);
            }

            Log("[3/4] Tarea programada ────────", Color.White);
            try { RunSchTasks("/Delete /TN \"" + taskName + "\" /F"); } catch { }

            string taskXml = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" +
                "<Task version=\"1.4\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">" +
                "<RegistrationInfo><Description>Skaso Fix: Ajuste de pantalla para modo Xbox al inicio.</Description></RegistrationInfo>" +
                "<Triggers><BootTrigger><Enabled>true</Enabled></BootTrigger></Triggers>" +
                "<Principals><Principal id=\"Author\"><RunLevel>HighestAvailable</RunLevel><UserId>SYSTEM</UserId></Principal></Principals>" +
                "<Settings><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries></Settings>" +
                "<Actions Context=\"Author\"><Exec><Command>" + physpanelPath + "</Command><Arguments>set 155 87</Arguments><WorkingDirectory>" + installDir + "</WorkingDirectory></Exec></Actions>" +
                "</Task>";

            string taskFile = Path.Combine(Path.GetTempPath(), "skaso_task.xml");
            File.WriteAllText(taskFile, taskXml);
            Log("  [CHECK] Creando tarea como SYSTEM...", Color.White);
            RunSchTasks("/Create /TN \"" + taskName + "\" /XML \"" + taskFile + "\" /F");
            File.Delete(taskFile);
            if (TaskExists())
                Log("  [OK] Tarea '" + taskName + "' creada y verificada", Green);
            else
                Log("  [FAIL] No se pudo verificar la tarea", Red);

            Log("[4/4] ViVeTool Feature Flags ───", Color.White);
            foreach (var id in xboxFeatureIds)
            {
                Log("  [CHECK] Activando ID " + id + "...", Color.White);
                string output = RunViveTool("/enable", id);
                if (output.Contains("Successfully") || output.Contains("Enabled"))
                    Log("  [OK] ID " + id + " -> ENABLED", Green);
                else
                    Log("  [OK] ID " + id + " -> activado (" + output.Trim() + ")", Green);
            }
        }

        private void DeactivateHandheldMode()
        {
            Log("--- Desactivando Modo Xbox ---", Color.Cyan);

            Log("[1/3] Registro ────────────────", Color.White);
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\OEM", true))
                key?.DeleteValue("DeviceForm", false);
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\OEM"))
            {
                int val = (int)(key?.GetValue("DeviceForm", 0) ?? 0);
                if (val == 0)
                    Log("  [OK] DeviceForm eliminado", Green);
                else
                    Log("  [WARN] DeviceForm aun existe: " + val, Yellow);
            }

            Log("[2/3] Tarea programada ────────", Color.White);
            Log("  [CHECK] Eliminando tarea...", Color.White);
            try { RunSchTasks("/Delete /TN \"" + taskName + "\" /F"); } catch { }
            if (!TaskExists())
                Log("  [OK] Tarea eliminada", Green);
            else
                Log("  [WARN] No se pudo eliminar la tarea", Yellow);

            Log("[3/3] ViVeTool + limpieza ────", Color.White);
            if (Directory.Exists(installDir))
            {
                foreach (var id in xboxFeatureIds)
                {
                    Log("  [CHECK] Desactivando ID " + id + "...", Color.White);
                    RunViveTool("/disable", id);
                    Log("  [OK] ID " + id + " -> DISABLED", Yellow);
                }
                Directory.Delete(installDir, true);
                Log("  [OK] Recursos eliminados", Green);
            }
            else
            {
                Log("  [OK] No hay recursos que limpiar", Green);
            }
        }

        // =============== HERRAMIENTAS: VIvETOOL SOLO ===============

        private void EnsureViveToolExists()
        {
            if (!Directory.Exists(installDir))
                Directory.CreateDirectory(installDir);

            foreach (var file in resourceFiles)
            {
                string target = Path.Combine(installDir, file);
                if (!File.Exists(target))
                {
                    try { ExtractResource(file, target); } catch { }
                }
            }
        }

        private async void ApplyViveAction(string action, string actionName, Button senderBtn)
        {
            senderBtn.Enabled = false;
            Log($"--- Aplicando ViVeTool ({actionName}) ---", Color.Cyan);

            await Task.Run(() =>
            {
                EnsureViveToolExists();

                var idsToToggle = new List<string>();
                if (chkVive1.Checked) idsToToggle.Add(xboxFeatureIds[0]);
                if (chkVive2.Checked) idsToToggle.Add(xboxFeatureIds[1]);

                if (idsToToggle.Count == 0)
                {
                    Log("  No seleccionaste ningun ID.", Yellow);
                    return;
                }

                foreach (var id in idsToToggle)
                {
                    Log("  [CHECK] " + actionName + " ID " + id + "...", Color.White);
                    string output = RunViveTool(action, id);
                    Log("  [OK] ID " + id + " -> " + actionName, Green);
                }
            });

            UpdateViveStatus();
            Log("--- Listo (solo ViVeTool, sin tocar registro ni physpanel) ---", Green);
            senderBtn.Enabled = true;
        }
    }
}
