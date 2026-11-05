<h1 align="center">Rockstar Launcher Fix</h1>

<p align="center">
  A small Windows app that repairs the most common Rockstar Games Launcher<br>
  and FiveM connection failures, in one window.
</p>

<p align="center">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D4?style=flat-square">
  <img alt="Built with" src="https://img.shields.io/badge/built%20with-C%23%20%2F%20WPF-512BD4?style=flat-square">
  <img alt="Dependencies" src="https://img.shields.io/badge/dependencies-none-2EA44F?style=flat-square">
  <img alt="License" src="https://img.shields.io/badge/license-MIT-lightgrey?style=flat-square">
</p>

<p align="center">
  <a href="#overview">Overview</a> &middot;
  <a href="#quick-start">Quick start</a> &middot;
  <a href="#what-each-step-does">Steps</a> &middot;
  <a href="#building-from-source">Build</a> &middot;
  <a href="#troubleshooting">Troubleshooting</a> &middot;
  <a href="#safety">Safety</a>
</p>

---

## Overview

This tool targets the following errors:

```text
Rockstar Games Launcher timed out loading both its online and offline content
```

```text
FiveM: Exit Code = 506 | Failure Connecting To The Rockstar Games Launcher
```

It also covers the case where **the launcher only works when started as administrator**, even though FiveM refuses to run alongside an elevated launcher.

The usual fixes for these errors are scattered across forum threads and involve the registry, the hosts file, network commands and folder permissions. Fix-Rockstar collects them in one window. Each step can be run on its own, shows whether it succeeded, and is documented below.

## Quick start

1. Download `Fix-Rockstar.exe` from the [Releases](../../releases) page.
2. Run it and accept the UAC prompt. Administrator rights are required to change system settings.
3. Click **Run recommended fix**, or run steps one at a time with their **Run** buttons.
4. Restart the computer if the network reset ran.

Nothing needs to be installed. Close the launcher, FiveM and GTA first, or let the first step do it for you.

> **Prefer a script?** You can download just [`Fix-Rockstar.bat`](Fix-Rockstar.bat) instead. It is a single plain-text file with the same fixes in a console menu, so you can read exactly what it does before running it. See [Script version](#script-version).

### The window

The window lists every step with a status dot: grey while idle, yellow while running, green when done, red if it failed. A live activity log sits on the right.

- **Recommended** steps are what **Run recommended fix** executes, top to bottom.
- **Optional** steps change settings that most people do not need to touch. They are never run by the recommended fix.

### Which option should I choose?

| Situation | What to do |
|---|---|
| Any of the errors above, first attempt | **Run recommended fix**, then restart the PC |
| Launcher works only as administrator | **Repair folder permissions**, then **Start launcher without admin** |
| Still timing out after the above and a restart | **Use Cloudflare and Google DNS**, then check VPN and antivirus |
| Want to revert the DNS change | **Restore automatic DNS** |

## What each step does

| Step | Group | Reversible |
|---|---|:---:|
| Close Rockstar processes | Recommended | n/a |
| Repair folder permissions | Recommended | Manual |
| Sync Windows clock | Recommended | n/a |
| Reset network stack | Recommended | Partly |
| Clear launcher cache | Recommended | Yes |
| Restart Rockstar service | Recommended | n/a |
| Clean hosts file | Recommended | Yes |
| Disable proxy | Recommended | Manual |
| Start launcher without admin | Recommended | n/a |
| Use Cloudflare and Google DNS | Optional | Yes |
| Restore automatic DNS | Optional | n/a |

<details>
<summary><strong>Close Rockstar processes</strong></summary>

Ends `Launcher.exe`, `LauncherPatcher.exe`, `RockstarService.exe`, `SocialClubHelper.exe`, `PlayGTAV.exe`, `GTA5.exe` and `FiveM.exe`.

A half-hung launcher keeps stale connections open and locks files. Starting from a clean state makes every other step more reliable.
</details>

<details>
<summary><strong>Repair folder permissions</strong></summary>

Takes ownership of the following folders and grants the current user full control:

```text
C:\Program Files\Rockstar Games
C:\ProgramData\Rockstar Games
%LOCALAPPDATA%\Rockstar Games
%USERPROFILE%\Documents\Rockstar Games
```

This is the fix for "it only works when run as administrator". If the launcher ever ran elevated, it may have created files that a standard account cannot write to. Without write access it cannot load its content. Once permissions are repaired, administrator rights are no longer needed.

The rights go to the account that approved the UAC prompt. If you completed it with a different administrator's credentials, that account receives the rights instead.
</details>

<details>
<summary><strong>Sync Windows clock</strong></summary>

Starts the Windows Time service and forces a resynchronization.

Rockstar's servers use HTTPS. If the system clock is off by more than a few minutes, certificate validation fails and the launcher reports it as a timeout.
</details>

<details>
<summary><strong>Reset network stack</strong></summary>

Runs `ipconfig /flushdns`, `netsh winsock reset` and `netsh int ip reset`.

Winsock is the Windows layer that programs use for network access. VPN clients, antivirus software and malware can leave broken components registered in it. The reset restores defaults. No files are removed, but VPN or firewall software may need to be reconfigured. **A restart is required for the reset to take full effect.**
</details>

<details>
<summary><strong>Clear launcher cache</strong></summary>

Renames the following folders to `*.bak`. An existing `.bak` of the same name is replaced.

```text
%LOCALAPPDATA%\Rockstar Games\Launcher
%LOCALAPPDATA%\Rockstar Games\Social Club
%USERPROFILE%\Documents\Rockstar Games\Launcher
%USERPROFILE%\Documents\Rockstar Games\Social Club
```

A corrupted cache or settings file can make the launcher wait indefinitely for content. Both folders are recreated on the next start. You will likely need to sign in again.

To undo: delete the new folder and rename the `.bak` folder back.
</details>

<details>
<summary><strong>Restart Rockstar service</strong></summary>

Stops and starts `Rockstar Game Library Service`. The launcher depends on this background service, and it times out if the service is stuck.
</details>

<details>
<summary><strong>Clean hosts file</strong></summary>

Searches `C:\Windows\System32\drivers\etc\hosts` for lines containing `rockstar` or `socialclub`. Matches are listed in the log and removed only after you confirm in a dialog. The original is saved as `hosts.bak`.

Cracks, some optimization tools and outdated guides add entries that redirect Rockstar's servers.
</details>

<details>
<summary><strong>Disable proxy</strong></summary>

Sets `ProxyEnable` to `0` for the current user and runs `netsh winhttp reset proxy`.

A leftover proxy from a VPN or proxy tool that has since been removed blocks the launcher's connections. The step does not turn off a VPN; do that by hand.
</details>

<details>
<summary><strong>Start launcher without admin</strong></summary>

Opens `Launcher.exe` through `explorer.exe`, so it runs as the normal user even though Fix-Rockstar itself is elevated.

FiveM refuses to connect to an elevated launcher. Starting it directly from an elevated program would hand the administrator token on to the launcher.
</details>

<details>
<summary><strong>Use Cloudflare and Google DNS / Restore automatic DNS</strong></summary>

Sets `1.1.1.1` and `8.8.8.8` as DNS servers on all active network adapters, or puts them back to automatic.

Some ISP resolvers fail to resolve Rockstar's domains. Because this changes network configuration, it is not part of the recommended fix.
</details>

## Building from source

The app is a single C# file (`src/App.cs`) using WPF. It compiles with the C# compiler that ships with Windows, so no SDK or Visual Studio is needed.

```text
build.bat
```

This produces `Fix-Rockstar.exe` next to the script. The manifest in `src/app.manifest` makes Windows ask for administrator rights on start.

### Script version

[`Fix-Rockstar.bat`](Fix-Rockstar.bat) is a plain batch menu with the same fixes and no window. You do not need anything else from this repository: download only that file, run it, and accept the UAC prompt. Use it if you prefer to read and run a script, or if you cannot run an unsigned executable.

Its menu keys: `1` close processes, `2` sync clock, `3` reset network, `4` clear cache, `5` restart service, `6` start launcher without admin, `7` / `R` set / restore DNS, `8` hosts file, `9` proxy, `P` folder permissions, and `A` for everything recommended.

## Troubleshooting

The app addresses the common causes. The following need to be checked manually:

- **VPN.** Disable it completely, not just disconnect.
- **Antivirus and firewall.** Disable temporarily, or add an exclusion for `C:\Program Files\Rockstar Games\`. Avast, Kaspersky and ESET are frequent causes.
- **Controlled folder access.** In Windows Security under Ransomware protection, turn it off or allow the launcher.
- **Interfering software.** Run `msconfig` and perform a clean boot to identify third-party services.
- **Visual C++ Redistributable.** Reinstall the 2015-2022 x64 and x86 packages.
- **Corrupted installation.** Uninstall the launcher, delete `C:\Program Files\Rockstar Games\Launcher` and install a fresh copy from rockstargames.com.

If Windows SmartScreen or your antivirus flags the download, that is because the executable is unsigned and asks for administrator rights. You can read the full source and build it yourself with `build.bat`.

## Safety

- The whole app is one readable source file. Review it before running.
- Nothing is permanently deleted. Cache folders and the hosts file are backed up.
- The hosts file is edited only after you confirm.
- No downloads and no telemetry. The only network activity is Windows' own time synchronization.

## Contributing

Issues and pull requests are welcome. If you add a step, keep the project dependency-free and document why the step helps.

## License

MIT. See [LICENSE](LICENSE).

## Disclaimer

This project is not affiliated with, endorsed by or connected to Rockstar Games, Take-Two Interactive or Cfx.re (FiveM). All product names are trademarks of their respective owners. The app modifies system settings; use it at your own risk.
