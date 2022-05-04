# Install Podman

This guide will build Podman v4 from source, because the Ubuntu APT registry
is still stuck on v3, and will continue be stuck with v3 until the next version
(Ubuntu 22.10), if not later.

## Install dependencies

```sh
sudo apt install -y \
  build-essential \
  dnsmasq \
  iptables \
  libassuan-dev \
  libbtrfs-dev \
  libc6-dev \
  libdevmapper-dev \
  libglib2.0-dev \
  libgpg-error-dev \
  libgpgme-dev \
  libseccomp-dev \
  libselinux1-dev \
  libsystemd-dev \
  pkg-config \
  slirp4netns \
  runc \
  uidmap
```

## Install Go

```sh
wget https://go.dev/dl/go1.18.1.linux-amd64.tar.gz
sudo rm -rf /usr/local/go && sudo tar -C /usr/local -xzf go1.18.1.linux-amd64.tar.gz
rm go1.18.1.linux-amd64.tar.gz

# If Bash or Dash is your root user's default shell (highly probable):
echo 'export PATH="$PATH":/usr/local/go/bin' | sudo tee -a /etc/profile

# If ZSH is your root user's default shell:
echo 'export PATH="$PATH":/usr/local/go/bin' | sudo tee -a /etc/zsh/zshenv

# Then restart your terminal to let the PATH changes apply, or run the
# following manually:
export PATH="$PATH":/usr/local/go/bin
```

## Set up working directory

You will need a directory to work in. It will not be used or referenced after
Podman has been installed.

Example:

```sh
mkdir ~/podman-from-source
cd ~/podman-from-source
```

## Clone repositories

```sh
git clone https://github.com/containers/conmon.git --branch v2.1.0
git clone https://github.com/containers/dnsname.git --branch v1.3.0
git clone https://github.com/containers/podman.git --branch v4.0.3
```

## Build conmon

Conmon in Ubuntu's APT registry is also majorly outdated, therefore we build
that one from scratch as well.

```sh
cd ~/podman-from-source/conmon
make
sudo make podman
```

## Build dnsname

Dnsname doesn't exist in Ubuntu's APT registry. We have to build it from
scratch.

```sh
cd ~/podman-from-source/dnsname
make
sudo make install PREFIX=/usr
```

## Build podman

```sh
cd ~/podman-from-source/podman

go install github.com/cpuguy83/go-md2man/v2@v2.0.2

make BUILDTAGS="selinux seccomp exclude_graphdriver_devicemapper"
sudo env PATH="$PATH" make install PREFIX=/usr
```

## Install CNI plugins

CNI plugins are more of a reference implementation, and therefore not exported
as packages. However they're really good for desktop usage that they are used
by most container packages, such as containerd and podman.

```sh
mkdir ~/podman-from-source/cni
cd ~/podman-from-source/cni

wget https://github.com/containernetworking/plugins/releases/download/v1.1.1/cni-plugins-linux-amd64-v1.1.1.tgz

mkdir plugins
tar -C plugins -xzvf cni-plugins-linux-amd64-v1.1.1.tgz

sudo mkdir -pv /opt/cni/bin
sudo cp -v plugins/* /opt/cni/bin
```

## Install config files

These are config files from the latest Fedora package of containers-common,
but this is fine. These are not strictly necessary to be correct, they just
need to be *existing* so that Podman doesn't get sad.

```sh
sudo mkdir -pv /etc/containers
sudo wget -O /etc/containers/registries.conf https://src.fedoraproject.org/rpms/containers-common/raw/main/f/registries.conf
sudo wget -O /etc/containers/policy.json https://src.fedoraproject.org/rpms/containers-common/raw/main/f/default-policy.json
```

Copy the [`87-podman-bridge.conflist`](./87-podman-bridge.conflist) from this
repo, and then move it into `/etc/cni/net.d/`

```sh
sudo mkdir -pv /etc/cni/net.d
sudo cp -v ./87-podman-bridge.conflist /etc/cni/net.d/87-podman-bridge.conflist
```

## Try it out

Now you should have Podman available with all necessary plugins.

If the following succeeds, then you're ready for the next step:

```sh
# Set up network with pod running basic Nginx
podman network create test-network
podman run --rm --name my-nginx -d -p 8080:80 --network test-network docker.io/library/nginx

# Makes a HTTP request using DNS aliasing inside the same network.
podman run --rm -it --network test-network docker.io/library/alpine wget -O- http://my-nginx

# Cleanup
podman stop my-nginx
podman network rm test-network
```

## Troubleshooting

### `WARN[0000] "/" is not a shared mount`

Symptom:

```console
$ podman ps
WARN[0000] "/" is not a shared mount, this could cause issues or missing mounts with rootless containers
CONTAINER ID  IMAGE       COMMAND     CREATED     STATUS      PORTS       NAMES
```

Your mount isn't shared, as can bee seen by e.g:

```console
$ findmnt -o PROPAGATION /
PROPAGATION
private,slave
```

This is probably because either:

- You're running inside WSL. Double-check your WSL config to make the mount shared
- You're running inside LXC as a container. Try running as a VM instead.

### `ERRO[0001] failed to move the rootless netns slirp4netns process to the systemd user.slice`

```console
$ podman run --rm --name my-nginx -d -p 8080:80 --network test-network docker.io/library/nginx
ERRO[0001] failed to move the rootless netns slirp4netns process to the systemd user.slice: exec: "dbus-launch": executable file not found in $PATH
```

Try ignore it if possible. It seems like actually a harmless error, where it's
complaining about lacking systemd or dbus-launch, which are more common on
desktop installations, in contrast to WSL or LXC VMs.

If it works even with the error, then treat it as a nuisance, and continue with
your day.
