# Setup Litium Search

## Prerequisites

- Follow the [Setup Litium Accelerator](../Litium%20Accelerator/README.md)
  guide first.

## Configure Litium to use Elasticsearch

Set the connection strings by editing the
`Src/Litium.Accelerator.Mvc/appsettings.json` file:

```json
{
  "Litium": {
    "Elasticsearch": {
      "ConnectionString": "http://localhost:9200",
      "Username": null,
      "Password": null,
      "Prefix": "LitiumEducation",
      "Synonym": {
        "Server": "http://localhost:9210",
        "ApiKey": null
      }
    },
```

## Configure Litium for running inside Podman

If you wish to run Litium inside Podman instead, such as via `make podman-run`,
then you need to supply some other values:

```json
{
  "Litium": {
    "Elasticsearch": {
      "ConnectionString": "http://elasticsearch:9200",
      "Username": null,
      "Password": null,
      "Prefix": "LitiumEducation",
      "Synonym": {
        "Server": "http://synonymserver",
        "ApiKey": null
      }
    },
```

This makes use of the DNS aliasing, as described in the [Podman](../Podman/README.md)
guide.
