# Setup Litium Accelerator

## Prerequisites

### From this repo

- [Podman](../Podman/README.md)
- [Docker Compose](../Docker%20Compose/README.md)
- [.NET 6 CLI](../DotNet/README.md)

### From `LitiumAB/Education`

1. Litium is only distributed through NuGet, so first  [configure the Litium NuGet feed](https://docs.litium.com/documentation/get-started/litium-packages) (requires a [Litium Docs account](https://docs.litium.com/system_pages/createlitiumaccount) with partner privileges).

### Required

1. Install [Visual Studio Code](https://code.visualstudio.com/),
   [VSCodium](https://vscodium.com/), or some other .NET-capable IDE

   - Install a C# extension. Recommended:

     - (VS Code) <https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp>
     - (VSCodium) <https://open-vsx.org/extension/muhammad-sammy/csharp>

2. Install [Azure Data Studio](https://github.com/microsoft/azuredatastudio),
   or some other MsSql-capable IDE.

   (It's named "***Azure*** Data Studio", but it's not dependent on Azure cloud
   at all)

### Optional but recommended

1. Install [Git](https://git-scm.com/) to make life easier

## Setup working directory

Such as:

```sh
mkdir -pv ~/code/litium/education
cd ~/code/litium/education
```

## Litium templates

This only installs the template. It doesn't template anything in your current
directory:

```sh
dotnet new --install "Litium.Accelerator.Templates"
```

After you've installed the template, use it. This however does create
some files:

```sh
cd ~/code/litium/education
dotnet new litmvcacc
```

It creates the following files:

```text
./.config/dotnet-tools.json

./Src/Litium.Accelerator.Administration.Extensions/...
./Src/Litium.Accelerator.Elasticsearch/...
./Src/Litium.Accelerator.Email/...
./Src/Litium.Accelerator.FieldTypes/...
./Src/Litium.Accelerator.Mvc/...

./Src/.browserslistrc
./Src/angular.json
./Src/BuildClientProjects.bat
./Src/BuildClientProjects.ps1
./Src/Directory.Build.props
./Src/Directory.Build.targets
./Src/package.json

./Test/Litium.Accelerator.Test/...

./Accelerator.sln
```

## Makefile

We have most of the scripts ready inside a Makefile. Make sure you've `make`
installed:

```sh
sudo apt install make
```

Then copy the [Makefile](./Makefile) into your working directory, next to
`Src`, `Test`, and `Accelerator.sln`.

## docker-compose.yml

Copy the [docker-compose.yml](./docker-compose.yml) into your working directory,
next to `Src`, `Test`, `Accelerator.sln`, and the new `Makefile`.

## Install dev certs

```sh
make install-developer-certificate
```

## Test starting the docker-compose

(Make sure your Podman socket is running first: [../Docker Compose/README.md](../Docker%20Compose/README.md#use-docker-compose-with-podman))

```sh
# This can take a while:
docker-compose pull

# The "--abort-on-container-exit" is always a nice flag to add
docker-compose up --abort-on-container-exit
```

### docker-compose: No such image

If you get the following output:

```console
$ docker-compose up --abort-on-container-exit
[+] Running 1/8
 ⠋ synonymserver Pulling                                                   0.1s
 ⠋ kibana Pulling                                                          0.1s
 ⠿ direct-shipment Pulled                                                  0.1s
 ⠋ direct-payment Pulling                                                  0.1s
 ⠋ redis Pulling                                                           0.1s
 ⠋ mailhog Pulling                                                         0.1s
 ⠋ elasticsearch Pulling                                                   0.1s
 ⠋ sqlserver Pulling                                                       0.1s
Error: No such image: registry.litium.cloud/apps/direct-shipment:latest
```

Then you could try to pull the images manually via Podman. There's a risk that
the Podman credentials doesn't transfer well to docker-compose, but once the
images are downloaded, it should work just fine.

```sh
podman login registry.litium.cloud

podman pull registry.litium.cloud/apps/direct-shipment:latest
podman pull registry.litium.cloud/apps/direct-payment:latest
```

### docker-compose: `AccessDeniedException[/usr/share/elasticsearch/data/nodes]`

If elasticsearch or mssql fails, you may have to do some `chmod`'ing:

```sh
sudo chmod -R 777 data volumes
```

(Yes, very surgical.)
Then try `docker-compose up --abort-on-container-exit` again.

## Configure Litium database

This requires that the `docker-compose` is currently up and running.

Open Azure Data Studio and connect to:

- Connection type: `Microsoft SQL Server`
- Server: `localhost,5434`
- Authentication type: `SQL Login`
- User name: `sa`
- Password: `Pass@word`

Then start a new SQL script and run the following:

```sql
CREATE DATABASE LitiumEducation;
```

The run the following Makefile targets:

```sh
make litium-db-update

make litium-db-user
```

Then set the connection string by editing the
`Src/Litium.Accelerator.Mvc/appsettings.json` file:

```json
{
  "Litium": {
    "Data": {
      "ConnectionString": "Pooling=true;User Id=sa;Password=Pass@word;Database=LitiumEducation;Server=localhost,5434"
```

## Run the accelerator

```sh
make dotnet-run
```

Then navigate to: <https://localhost:10011>

(port 10011 is the `direct-payment` service, that will then redirect you to
<https://bookstore.localtest.me:5001/Litium/UI/login>)

> Even though we've installed the certificates, there's a high chance your web
> browser has it's own cert store which it doesn't trust. For now, just click
> "Yes, continue anyway" in your browser.

Log in with:

- Username: `admin`
- Password: `nimda`

## Deploy "Bookstore"

In the side panel, navigate to "Deployment" > "Accelerator", or visit this
link:

<https://bookstore.localtest.me:5001/Litium/UI/settings/extensions/AcceleratorDeployment/deployment>

In the "Install Accelerator package" form, enter:

- Name: `Bookstore`
- Domain name: `bookstore.localtest.me`

Click <kbd>Import</kbd>, and wait a couple of minutes. It's a slow process,
and no logs are spat out in the process. But be patient.

> During the import, you can visit <https://bookstore.localtest.me:5001/Litium/UI/media>
> and see the `0 Files 0 Images 0 Movies 0 Others` go up as it gets closer to
> finishing the import.
>
> By the end, it should say something like `366 Files 365 Images 1 Movies 0 Others`

Now you can visit <https://bookstore.localtest.me:5001> and see the template
site!

:warning: Please note that no products are visible yet, as that relies on the
Litium Search, that we've not yet set up.

## Dockerfile

To run Litium inside Podman instead of via the .NET CLI,
then you need to copy the [Dockerfile](./Dockerfile) into
`./Src/Litium.Accelerator.Mvc/Dockerfile`, and then run the following:

```sh
# Builds image
make podman

# Runs built image
make podman-run
```
