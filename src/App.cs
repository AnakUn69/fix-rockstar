using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

class Step
{
    public string Title, Desc, Group;
    public Action Do;
    public Button Btn;
    public Ellipse Dot;
    public TextBlock State;
}

class App
{
    // Palette: neutral near-black ground, one accent (Rockstar-style yellow) used only for the primary action and live state.
    const string Ground = "#0E0E0F", Surface = "#151516", Line = "#232326", Ink = "#ECECEE", Muted = "#8B8B92",
                 Accent = "#FCAF17", Ok = "#5FB37A", Bad = "#E5584F", Idle = "#3A3A3F";

    const string Xaml = @"
<Window xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        Title='Rockstar Launcher Fix' Width='1120' Height='970' MinWidth='820' MinHeight='540'
        Background='#0E0E0F' WindowStartupLocation='CenterScreen'
        FontFamily='Segoe UI Variable Text, Segoe UI'>
  <Window.Resources>
    <Style x:Key='Ghost' TargetType='Button'>
      <Setter Property='Foreground' Value='#ECECEE'/>
      <Setter Property='Background' Value='Transparent'/>
      <Setter Property='BorderBrush' Value='#2C2C30'/>
      <Setter Property='Padding' Value='14,5'/>
      <Setter Property='FontSize' Value='12.5'/>
      <Setter Property='Cursor' Value='Hand'/>
      <Setter Property='Template'>
        <Setter.Value>
          <ControlTemplate TargetType='Button'>
            <Border x:Name='b' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                    BorderThickness='1' CornerRadius='4' Padding='{TemplateBinding Padding}'>
              <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='b' Property='Background' Value='#1F1F22'/></Trigger>
              <Trigger Property='IsEnabled' Value='False'><Setter TargetName='b' Property='Opacity' Value='0.35'/></Trigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
        </Setter.Value>
      </Setter>
    </Style>
    <Style x:Key='Primary' TargetType='Button'>
      <Setter Property='Foreground' Value='#111111'/>
      <Setter Property='Background' Value='#FCAF17'/>
      <Setter Property='Padding' Value='20,9'/>
      <Setter Property='FontSize' Value='13'/>
      <Setter Property='FontWeight' Value='SemiBold'/>
      <Setter Property='Cursor' Value='Hand'/>
      <Setter Property='Template'>
        <Setter.Value>
          <ControlTemplate TargetType='Button'>
            <Border x:Name='b' Background='{TemplateBinding Background}' CornerRadius='4' Padding='{TemplateBinding Padding}'>
              <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
            </Border>
            <ControlTemplate.Triggers>
              <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='b' Property='Background' Value='#FFC040'/></Trigger>
              <Trigger Property='IsEnabled' Value='False'><Setter TargetName='b' Property='Opacity' Value='0.4'/></Trigger>
            </ControlTemplate.Triggers>
          </ControlTemplate>
        </Setter.Value>
      </Setter>
    </Style>
    <Style TargetType='ScrollBar'>
      <Setter Property='Width' Value='8'/>
      <Setter Property='Background' Value='Transparent'/>
      <Setter Property='Template'>
        <Setter.Value>
          <ControlTemplate TargetType='ScrollBar'>
            <Track x:Name='PART_Track' IsDirectionReversed='True'>
              <Track.DecreaseRepeatButton><RepeatButton Command='ScrollBar.PageUpCommand' Opacity='0' IsTabStop='False'/></Track.DecreaseRepeatButton>
              <Track.IncreaseRepeatButton><RepeatButton Command='ScrollBar.PageDownCommand' Opacity='0' IsTabStop='False'/></Track.IncreaseRepeatButton>
              <Track.Thumb>
                <Thumb>
                  <Thumb.Template><ControlTemplate TargetType='Thumb'><Border Background='#2E2E33' CornerRadius='4' Margin='2,0'/></ControlTemplate></Thumb.Template>
                </Thumb>
              </Track.Thumb>
            </Track>
          </ControlTemplate>
        </Setter.Value>
      </Setter>
    </Style>
  </Window.Resources>

  <Grid>
    <Grid.RowDefinitions>
      <RowDefinition Height='Auto'/><RowDefinition Height='*'/><RowDefinition Height='Auto'/>
    </Grid.RowDefinitions>

    <Grid Margin='32,26,32,22'>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/><ColumnDefinition Width='Auto'/>
      </Grid.ColumnDefinitions>
      <Grid Width='24' Height='24' Margin='0,0,14,0' VerticalAlignment='Center'>
        <Polygon Points='12,0 22.4,6 22.4,18 12,24 1.6,18 1.6,6' Fill='#FCAF17'/>
        <Ellipse Width='8' Height='8' Fill='#0E0E0F'/>
      </Grid>
      <StackPanel Grid.Column='1' VerticalAlignment='Center'>
        <TextBlock Text='Launcher Fix' Foreground='#ECECEE' FontSize='19' FontWeight='SemiBold'/>
        <TextBlock Foreground='#8B8B92' FontSize='12.5' Margin='0,2,0,0'
                   Text='Rockstar Games Launcher timeouts and FiveM exit code 506'/>
      </StackPanel>
      <Button x:Name='AllBtn' Grid.Column='2' Style='{StaticResource Primary}' Content='Run recommended fix' VerticalAlignment='Center'/>
    </Grid>

    <Grid Grid.Row='1' Margin='32,0,32,0'>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width='*'/><ColumnDefinition Width='28'/><ColumnDefinition Width='340'/>
      </Grid.ColumnDefinitions>

      <ScrollViewer VerticalScrollBarVisibility='Auto'>
        <StackPanel x:Name='Steps' Margin='0,0,12,16'/>
      </ScrollViewer>

      <Border Grid.Column='2' Background='#151516' CornerRadius='8' Margin='0,0,0,16'>
        <Grid>
          <Grid.RowDefinitions><RowDefinition Height='Auto'/><RowDefinition Height='*'/></Grid.RowDefinitions>
          <Grid Margin='18,14,14,0'>
            <TextBlock Text='Activity' Foreground='#ECECEE' FontSize='13' FontWeight='SemiBold' VerticalAlignment='Center'/>
            <Button x:Name='ClearBtn' Style='{StaticResource Ghost}' Content='Clear' HorizontalAlignment='Right' Padding='10,3' BorderBrush='Transparent'/>
          </Grid>
          <TextBox x:Name='LogBox' Grid.Row='1' Background='Transparent' Foreground='#A5A5AD' BorderThickness='0'
                   FontFamily='Cascadia Mono, Consolas' FontSize='12' IsReadOnly='True' TextWrapping='Wrap'
                   VerticalScrollBarVisibility='Auto' Padding='14,4,10,14' CaretBrush='Transparent'/>
        </Grid>
      </Border>
    </Grid>

    <Border Grid.Row='2' Padding='32,0' Height='38' BorderBrush='#232326' BorderThickness='0,1,0,0'>
      <TextBlock x:Name='Status' Foreground='#8B8B92' VerticalAlignment='Center' FontSize='12' Text='Ready'/>
    </Border>
  </Grid>
</Window>";

    static Window win;
    static TextBox logBox;
    static TextBlock status;
    static Button allBtn, clearBtn;
    static List<Step> steps = new List<Step>();
    static Step[] Recommended;
    static bool busy;

    static readonly string[] Procs = { "Launcher.exe", "LauncherPatcher.exe", "RockstarService.exe", "SocialClubHelper.exe", "PlayGTAV.exe", "GTA5.exe", "FiveM.exe" };
    static string Local { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); } }
    static string Docs { get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); } }
    static string Hosts { get { return Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts"; } }

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);

    [STAThread]
    static void Main()
    {
        win = (Window)XamlReader.Parse(Xaml);
        // never taller or wider than the screen's usable area
        win.Height = Math.Min(win.Height, SystemParameters.WorkArea.Height - 20);
        win.Width = Math.Min(win.Width, SystemParameters.WorkArea.Width - 20);
        logBox = (TextBox)win.FindName("LogBox");
        status = (TextBlock)win.FindName("Status");
        allBtn = (Button)win.FindName("AllBtn");
        clearBtn = (Button)win.FindName("ClearBtn");
        var panel = (StackPanel)win.FindName("Steps");

        win.SourceInitialized += delegate
        {
            int on = 1; // dark title bar (Windows 10 2004+ / 11)
            DwmSetWindowAttribute(new WindowInteropHelper(win).Handle, 20, ref on, 4);
        };

        BuildSteps();
        string group = null;
        foreach (var s in steps)
        {
            if (s.Group != group) { group = s.Group; panel.Children.Add(Heading(group, panel.Children.Count == 0)); }
            panel.Children.Add(Row(s));
        }

        allBtn.Click += delegate { Exec(RunAll); };
        clearBtn.Click += delegate { logBox.Clear(); };
        Log("Signed in as " + WindowsIdentity.GetCurrent().Name);
        new Application().Run(win);
    }

    static void BuildSteps()
    {
        const string R = "Recommended", O = "Optional";
        Step kill = Add(R, "Close Rockstar processes", "Ends the launcher, Social Club helper, GTA and FiveM so nothing holds stale connections.", Kill);
        Step perm = Add(R, "Repair folder permissions", "Gives your account full control of the Rockstar folders. Fixes \"only works as administrator\".", Perm);
        Step time = Add(R, "Sync Windows clock", "A clock off by minutes breaks HTTPS certificates; the launcher reports it as a timeout.", Time);
        Step net = Add(R, "Reset network stack", "Flushes DNS, resets Winsock and IP. Restart the PC afterwards.", Net);
        Step cache = Add(R, "Clear launcher cache", "Renames the launcher and Social Club cache folders to .bak. You may need to sign in again.", Cache);
        Step svc = Add(R, "Restart Rockstar service", "Restarts Rockstar Game Library Service in case it is stuck.", Svc);
        Step hosts = Add(R, "Clean hosts file", "Removes lines that mention rockstar or socialclub, after asking. Backup: hosts.bak.", HostsFix);
        Step proxy = Add(R, "Disable proxy", "Turns off a leftover user proxy and resets the WinHTTP proxy.", Proxy);
        Step run = Add(R, "Start launcher without admin", "FiveM refuses an elevated launcher, so it is started with your normal rights.", RunLauncher);
        Add(O, "Use Cloudflare and Google DNS", "Sets 1.1.1.1 and 8.8.8.8 on active adapters. Try it if the recommended fix is not enough.", Dns);
        Add(O, "Restore automatic DNS", "Reverts the DNS change above.", DnsReset);
        Recommended = new[] { kill, perm, time, net, cache, svc, hosts, proxy, run };
    }

    static Step Add(string group, string title, string desc, Action a)
    {
        var s = new Step { Group = group, Title = title, Desc = desc };
        s.Do = delegate
        {
            Log(""); Log("> " + title);
            SetState(s, "run");
            try { a(); SetState(s, "ok"); }
            catch (Exception e) { Log("  failed: " + e.Message); SetState(s, "fail"); }
        };
        steps.Add(s);
        return s;
    }

    // ---- UI building ----

    static Brush B(string hex) { return (Brush)new BrushConverter().ConvertFromString(hex); }

    static UIElement Heading(string text, bool first)
    {
        return new TextBlock { Text = text, Foreground = B(Muted), FontSize = 12, FontWeight = FontWeights.SemiBold,
                               Margin = new Thickness(0, first ? 0 : 26, 0, 6) };
    }

    static UIElement Row(Step s)
    {
        s.Dot = new Ellipse { Width = 8, Height = 8, Fill = B(Idle), VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 6, 16, 0) };
        s.State = new TextBlock { Foreground = B(Muted), FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
        s.Btn = new Button { Content = "Run", MinWidth = 64, VerticalAlignment = VerticalAlignment.Center, Style = (Style)win.FindResource("Ghost") };
        s.Btn.Click += delegate { Exec(s.Do); };

        var text = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };
        text.Children.Add(new TextBlock { Text = s.Title, Foreground = B(Ink), FontSize = 14, FontWeight = FontWeights.SemiBold });
        text.Children.Add(new TextBlock { Text = s.Desc, Foreground = B(Muted), FontSize = 12.5, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) });

        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        right.Children.Add(s.State);
        right.Children.Add(s.Btn);

        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.ColumnDefinitions.Add(new ColumnDefinition());
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        g.Children.Add(s.Dot);
        Grid.SetColumn(text, 1); g.Children.Add(text);
        Grid.SetColumn(right, 2); g.Children.Add(right);

        return new Border { Child = g, Padding = new Thickness(0, 13, 0, 13), BorderBrush = B(Line), BorderThickness = new Thickness(0, 0, 0, 1) };
    }

    static void SetState(Step s, string st)
    {
        win.Dispatcher.BeginInvoke(new Action(delegate
        {
            string color = st == "run" ? Accent : st == "ok" ? Ok : Bad;
            string label = st == "run" ? "Running" : st == "ok" ? "Done" : "Failed";
            s.Dot.Fill = B(color);
            s.State.Text = label;
            s.State.Foreground = B(st == "run" ? Muted : color);
        }));
    }

    // ---- execution plumbing ----

    static void Exec(Action work)
    {
        if (busy) return;
        busy = true;
        SetUi(false, "Working...");
        Task.Run(delegate
        {
            try { work(); Log(""); Log("Finished."); }
            catch (Exception e) { Log("ERROR: " + e.Message); }
            busy = false;
            SetUi(true, "Ready");
        });
    }

    static void RunAll()
    {
        foreach (var s in Recommended) s.Do();
        Log(""); Log("Restart your PC if you reset the network.");
    }

    static void SetUi(bool on, string text)
    {
        win.Dispatcher.Invoke(delegate
        {
            foreach (var s in steps) s.Btn.IsEnabled = on;
            allBtn.IsEnabled = on;
            status.Text = text;
        });
    }

    static void Log(string line)
    {
        win.Dispatcher.BeginInvoke(new Action(delegate { logBox.AppendText(line + "\n"); logBox.ScrollToEnd(); }));
    }

    // quiet: print output only when the command fails, and then only the first 3 lines
    static int Cmd(string file, string args, bool quiet = false)
    {
        var enc = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
        var p = Process.Start(new ProcessStartInfo(file, args)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = enc, StandardErrorEncoding = enc
        });
        string o = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (quiet && p.ExitCode == 0) return 0;
        var shown = o.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(l => l.Trim().Length > 0);
        Log(string.Join("\n", (quiet ? shown.Take(3) : shown).Select(l => "  " + l.Trim()).ToArray()));
        return p.ExitCode;
    }

    // ---- steps ----

    static void Kill()
    {
        foreach (var p in Procs) Cmd("taskkill", "/f /im " + p, true);
    }

    static void Time()
    {
        Cmd("net", "start w32time");
        Cmd("w32tm", "/resync");
    }

    static void Net()
    {
        Cmd("ipconfig", "/flushdns");
        Cmd("netsh", "winsock reset");
        Cmd("netsh", "int ip reset");
        Log("  Restart the PC for the reset to take full effect.");
    }

    static void Cache()
    {
        string[] dirs =
        {
            Local + @"\Rockstar Games\Launcher", Local + @"\Rockstar Games\Social Club",
            Docs + @"\Rockstar Games\Launcher", Docs + @"\Rockstar Games\Social Club"
        };
        foreach (var d in dirs)
        {
            if (!Directory.Exists(d)) continue;
            if (Directory.Exists(d + ".bak")) Directory.Delete(d + ".bak", true);
            Directory.Move(d, d + ".bak");
            Log("  renamed " + d);
        }
    }

    static void Svc()
    {
        Cmd("sc", "stop \"Rockstar Game Library Service\"");
        System.Threading.Thread.Sleep(2000);
        Cmd("sc", "start \"Rockstar Game Library Service\"");
    }

    static void Perm()
    {
        string user = WindowsIdentity.GetCurrent().Name;
        string[] dirs =
        {
            Environment.GetEnvironmentVariable("ProgramFiles") + @"\Rockstar Games",
            Environment.GetEnvironmentVariable("ProgramData") + @"\Rockstar Games",
            Local + @"\Rockstar Games", Docs + @"\Rockstar Games"
        };
        foreach (var d in dirs)
        {
            if (!Directory.Exists(d)) continue;
            Cmd("takeown", "/f \"" + d + "\" /r /d y", true);
            Cmd("icacls", "\"" + d + "\" /grant \"" + user + "\":(OI)(CI)F /t /c /q", true);
            Log("  ok: " + d);
        }
    }

    static void HostsFix()
    {
        var lines = File.ReadAllLines(Hosts);
        var bad = lines.Where(l => l.IndexOf("rockstar", StringComparison.OrdinalIgnoreCase) >= 0
                                || l.IndexOf("socialclub", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        if (bad.Length == 0) { Log("  hosts file is clean"); return; }
        foreach (var l in bad) Log("  found: " + l);
        var r = (MessageBoxResult)win.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
            MessageBox.Show(win, "Remove these " + bad.Length + " line(s) from the hosts file?\n\n" + string.Join("\n", bad) +
                            "\n\nA backup is saved as hosts.bak.", "Hosts file", MessageBoxButton.YesNo, MessageBoxImage.Question)));
        if (r != MessageBoxResult.Yes) { Log("  skipped"); return; }
        File.Copy(Hosts, Hosts + ".bak", true);
        File.WriteAllLines(Hosts, lines.Except(bad).ToArray());
        Log("  removed, backup: hosts.bak");
    }

    static void Proxy()
    {
        using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
        {
            var was = k.GetValue("ProxyEnable");
            if (was != null && Convert.ToInt32(was) == 1) Log("  proxy was on, disabling");
            k.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
        }
        Cmd("netsh", "winhttp reset proxy");
        Log("  if you use a VPN, turn it off manually");
    }

    static void Dns()
    {
        Cmd("powershell", "-NoProfile -Command \"Get-NetAdapter | ? Status -eq 'Up' | Set-DnsClientServerAddress -ServerAddresses 1.1.1.1,8.8.8.8\"");
        Cmd("ipconfig", "/flushdns");
    }

    static void DnsReset()
    {
        Cmd("powershell", "-NoProfile -Command \"Get-NetAdapter | ? Status -eq 'Up' | Set-DnsClientServerAddress -ResetServerAddresses\"");
    }

    static void RunLauncher()
    {
        string pf = Environment.GetEnvironmentVariable("ProgramFiles");
        string pf86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        string path = new[] { pf, pf86 }.Where(x => x != null).Select(x => x + @"\Rockstar Games\Launcher\Launcher.exe").FirstOrDefault(File.Exists);
        if (path == null) throw new Exception("Launcher not found, start it manually");
        // explorer.exe starts it with the normal (non-elevated) user token
        Process.Start("explorer.exe", "\"" + path + "\"");
        Log("  started");
    }
}
