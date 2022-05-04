# Install Docker Compose (without Docker)

We don't want to install Docker, but instead only Docker Compose.

This is easiest done by just downloading the binary.

```sh
sudo curl -SL https://github.com/docker/compose/releases/download/v2.5.0/docker-compose-linux-x86_64 -o /usr/local/bin/docker-compose

sudo chmod +x /usr/local/bin/docker-compose
```

Done.

## Use docker-compose with Podman

1. Start podman socket, e.g via systemd:

   ```sh
   systemctl --user start podman
   ```

   Or manually:

   ```sh
   podman --log-level info system service --time 0
   ```

2. Find the socket it's using, e.g via systemd:

   ```sh
   systemctl --user status podman | grep '\.sock'
   ```

3. Set the environment variable to tell docker-compose to talk to podman:

   ```sh
   export DOCKER_HOST=unix:///run/user/1000/podman/podman.sock
   ```

   The value is the path to the socket prefixed with `unix://` 

4. Try it out:

   ```sh
   cat << EOF > docker-compose.yml
   services:
     nginx:
       image: nginx
       ports:
       - 8123:80
   EOF
   
   docker-compose up -d
   
   curl localhost:8123

   # Cleanup
   docker-compose down
   rm docker-compose.yml
   ```
