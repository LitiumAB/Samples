# Litium Accelerator via Podman on GNU/Linux

**Tested in Litium version: 8.3.0**

Inside the [LitiumAB/Education](https://github.com/LitiumAB/Education)
repository there's a guide on setting up Litium Accelerator locally on Windows
via Docker Compose.

This guide shows a modified version of that guide, allowing:

- Using rootless Podman (no `sudo` privileges required)
- Running on GNU/Linux, including via [WSL](https://docs.microsoft.com/en-us/windows/wsl/)

## Disclaimer

- This guide has only been tested on Pop_OS! (an Ubuntu derivative). Following
  this on a different GNU/Linux branch such as Fedora, Arch, or NixOS will
	require you to:

  - Lookup package names and how to install the `apt` equivalent packages for
    your OS.

  - If you don't have `systemd` (such as in WSL), then easiest option is to
    start all the services manually once you need them. We only use `systemd`
    for starting the Podman socket, which can be done manually anyway, but
    you'll have to look that up yourself.

## Instructions

1. [Install Podman](./Tasks/Podman/README.md)

   - Podman v4 is highly recommended as their new network stack is much more
	   stable, and plays much nicer with docker-compose.

   - Podman CNI plugins, including `dnsname` which is really helpful when using
     docker-compose.

2. [Install Docker Compose (without Docker)](./Tasks/Docker Compose/README.md)

3. [Install .NET 6](./Tasks/DotNet/README.md)

4. [Setup Litium Accelerator](./Tasks/Litium Accelerator/README.md)

5. [Setup Litium Search](./Tasks/Litium Search/README.md)

6. Continue with [Litium Developer Education](https://github.com/LitiumAB/Education/tree/main/Developer%20Education)
   docs, "Development tasks" step 5 *("Author page")* and beyond.
