<h1 align="center">Rockstar Launcher Fix</h1>

<p align="center">
  A single-file Windows utility that repairs the most common Rockstar Games Launcher<br>
  and FiveM connection failures.
</p>

<p align="center">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D4?style=flat-square">
  <img alt="Language" src="https://img.shields.io/badge/language-batch-4D4D4D?style=flat-square">
  <img alt="Dependencies" src="https://img.shields.io/badge/dependencies-none-2EA44F?style=flat-square">
  <img alt="Size" src="https://img.shields.io/badge/size-~130%20lines-lightgrey?style=flat-square">
</p>

<p align="center">
  <a href="#overview">Overview</a> &middot;
  <a href="#quick-start">Quick start</a> &middot;
  <a href="#reference">Reference</a> &middot;
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

The usual fixes for these errors are scattered across forum threads and involve the registry, the hosts file, network commands and folder permissions. `Fix-Rockstar.bat` collects them in one interactive menu. Each step is independent, reversible where possible, and documented below.

## Quick start

1. Download [`Fix-Rockstar.bat`](Fix-Rockstar.bat).
2. Run it and accept the UAC prompt. Administrator rights are required to change system settings.
3. Press `A` to run all recommended steps, or select steps individually.
4. Restart the computer if you ran the network reset (step 3).

No installation and no external downloads are needed. Close the launcher, FiveM and GTA first, or let step 1 do it for you.

```text
==============================================
  ROCKSTAR LAUNCHER FIX / FiveM exit code 506
==============================================

 1  Kill Rockstar / FiveM / GTA processes
 2  Sync Windows clock
 3  Flush DNS + reset network (Winsock, IP)
 4  Clear launcher and Social Club cache
 5  Restart Rockstar service
 6  Start Rockstar Launcher (without admin rights)
 7  Set DNS to 1.1.1.1 / 8.8.8.8        R  Restore automatic DNS
 8  Check hosts file
 9  Check and disable proxy
 P  Fix folder permissions
 A  Everything (P, 1-5, 8, 9), then start launcher
 0  Exit
```

### Which option should I choose?

| Situation | Recommended action |
|---|---|
| Any of the errors above, first attempt | `A`, then restart the PC |
| Launcher works only as administrator | `P`, then `6` |
| Still timing out after `A` and a restart | `7`, then check VPN and antivirus |
| Want to revert the DNS change | `R` |

## Reference

| Key | Action | Reversible |
|:---:|---|:---:|
| `1` | Terminate Rockstar, GTA and FiveM processes | n/a |
| `2` | Resynchronize the system clock | n/a |
| `3` | Flush DNS cache, reset Winsock and IP stack | Partly |
| `4` | Rename launcher and Social Club cache folders to `.bak` | Yes |
| `5` | Restart the Rockstar Game Library Service | n/a |
| `6` | Start the launcher without elevated rights | n/a |
| `7` / `R` | Set DNS to Cloudflare and Google / restore automatic DNS | Yes |
| `8` | Remove Rockstar-related entries from the hosts file | Yes |
| `9` | Disable the user proxy and reset the WinHTTP proxy | Manual |
| `P` | Take ownership of Rockstar folders and grant the user full control | Manual |

### Details

<details>
<summary><strong>1. Kill processes</strong></summary>

Ends `Launcher.exe`, `LauncherPatcher.exe`, `RockstarService.exe`, `SocialClubHelper.exe`, `PlayGTAV.exe`, `GTA5.exe` and `FiveM.exe`.

A half-hung launcher keeps stale connections open and locks files. Starting from a clean state makes every other step more reliable.
</details>

<details>
<summary><strong>2. Sync Windows clock</strong></summary>

Starts the Windows Time service and forces a resynchronization.

Rockstar's servers use HTTPS. If the system clock is off by more than a few minutes, certificate validation fails and the launcher reports it as a timeout.
</details>

<details>
<summary><strong>3. Flush DNS and reset the network stack</strong></summary>

Runs `ipconfig /flushdns`, `netsh winsock reset` and `netsh int ip reset`.

Winsock is the Windows layer that programs use for network access. VPN clients, antivirus software and malware can leave broken components registered in it. The reset restores defaults. No files are removed, but VPN or firewall software may need to be reconfigured. **A restart is required for the reset to take full effect.**
</details>

<details>
<summary><strong>4. Clear launcher and Social Club cache</strong></summary>

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
<summary><strong>5. Restart the Rockstar service</strong></summary>

Stops and starts `Rockstar Game Library Service`. The launcher depends on this background service, and it times out if the service is stuck.
</details>

<details>
<summary><strong>6. Start the launcher without admin rights</strong></summary>

Opens `Launcher.exe` through `explorer.exe`, so it runs as the normal user even though the script is elevated.

FiveM refuses to connect to an elevated launcher. A plain `start` from an elevated script would pass the administrator token on to the launcher.
</details>

<details>
<summary><strong>7 / R. DNS</strong></summary>

`7` sets Cloudflare (`1.1.1.1`) and Google (`8.8.8.8`) as DNS servers on all active network adapters. `R` restores automatic DNS.

Some ISP resolvers fail to resolve Rockstar's domains. Because this changes network configuration, it is **not** part of `A`.
</details>

<details>
<summary><strong>8. Check the hosts file</strong></summary>

Searches `C:\Windows\System32\drivers\etc\hosts` for lines containing `rockstar` or `socialclub`. Matches are shown and removed only after confirmation. The original is saved as `hosts.bak`.

Cracks, some optimization tools and outdated guides add entries that redirect Rockstar's servers.
</details>

<details>
<summary><strong>9. Check and disable the proxy</strong></summary>

Sets `ProxyEnable` to `0` for the current user and runs `netsh winhttp reset proxy`.

A leftover proxy from a VPN or proxy tool that has since been removed blocks the launcher's connections.
</details>

<details>
<summary><strong>P. Fix folder permissions</strong></summary>

Takes ownership of the following folders and grants the current user full control:

```text
C:\Program Files\Rockstar Games
C:\ProgramData\Rockstar Games
%LOCALAPPDATA%\Rockstar Games
%USERPROFILE%\Documents\Rockstar Games
```

This is the fix for "it only works when run as administrator". If the launcher ever ran elevated, it may have created files that a standard account cannot write to. Without write access it cannot load its content. Once permissions are repaired, administrator rights are no longer needed.

Note: the script grants rights to the account that elevated it. If the UAC prompt was completed with a different administrator's credentials, that account receives the rights instead.
</details>

## Troubleshooting

The script addresses the common causes. The following need to be checked manually:

- **VPN.** Disable it completely, not just disconnect.
- **Antivirus and firewall.** Disable temporarily, or add an exclusion for `C:\Program Files\Rockstar Games\`. Avast, Kaspersky and ESET are frequent causes.
- **Controlled folder access.** In Windows Security under Ransomware protection, turn it off or allow the launcher.
- **Interfering software.** Run `msconfig` and perform a clean boot to identify third-party services.
- **Visual C++ Redistributable.** Reinstall the 2015-2022 x64 and x86 packages.
- **Corrupted installation.** Uninstall the launcher, delete `C:\Program Files\Rockstar Games\Launcher` and install a fresh copy from rockstargames.com.

## Safety

- The entire tool is one readable batch file. Review it before running.
- Nothing is permanently deleted. Cache folders and the hosts file are backed up.
- Destructive-looking changes (hosts edit) require confirmation.
- No downloads, no telemetry. The only network activity is Windows' own time synchronization.

## Contributing

Issues and pull requests are welcome. If you add a step, keep the script dependency-free and document why the step helps.

## Disclaimer

This project is not affiliated with, endorsed by or connected to Rockstar Games, Take-Two Interactive or Cfx.re (FiveM). All product names are trademarks of their respective owners. The script modifies system settings; use it at your own risk.
