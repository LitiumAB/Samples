
.PHONY: build
build:
	dotnet build

.PHONY: deps
deps:
	dotnet restore

.PHONY: podman
podman: nuget.config
	podman build --pull -f Src/Litium.Accelerator.Mvc/Dockerfile . -t litium-education

.gitignore:
	echo "nuget.config" > .gitignore
	echo "obj/" >> .gitignore
	echo "bin/" >> .gitignore
	echo "node_modules/" >> .gitignore
	echo "/data/" >> .gitignore
	echo "/volumes/" >> .gitignore

nuget.config: .gitignore
	cp "${HOME}/.nuget/NuGet/NuGet.Config" "${PWD}/nuget.config"

.PHONY: dotnet-run
dotnet-run: nuget.config
	ASPNETCORE_URLS='http://+:5000;https://+:5001' \
	ASPNETCORE_Kestrel__Certificates__Default__Password=NaQX7wBk252h6fRNdqtd9WGXgoRN10kx \
	ASPNETCORE_Kestrel__Certificates__Default__Path="${PWD}"/data/https/localhost.pfx \
	Litium__Folder__Shared="${PWD}"/data/accelerator/data/shared \
	Litium__Folder__Local="${PWD}"/data/accelerator/data/local \
		dotnet run -v m --project Src/Litium.Accelerator.Mvc

.PHONY: podman-run
podman-run:
	podman run --rm -it \
		--network "$(shell basename "${PWD}")_default" \
		-p 5000:80 -p 5001:443 \
		-e 'ASPNETCORE_URLS=http://+:80;https://+:443' \
		-e ASPNETCORE_Kestrel__Certificates__Default__Password=NaQX7wBk252h6fRNdqtd9WGXgoRN10kx \
		-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx \
		-v ./data/https:/https:ro \
		litium-education

.PHONY: litium-db-update
# Run database migrations and configure the database
litium-db-update:
	dotnet litium-db update --connection "Pooling=true;User Id=sa;Password=Pass@word;Database=LitiumEducation;Server=localhost,5434"

.PHONY: litium-db-user
# Create a new Litium backoffice admin user in the database with login admin/nimda
litium-db-user:
	dotnet litium-db user --connection "Pooling=true;User Id=sa;Password=Pass@word;Database=LitiumEducation;Server=localhost,5434" --login admin --password nimda

##############################
### CERTIFICATE GENERATION ###
##############################

data:
	mkdir -pv data

data/localhost.config: data
	wget -O data/localhost.config 'https://raw.githubusercontent.com/LitiumAB/Education/main/Developer%20Education/Tasks/Developer%20certificate/Resources/localhost.config'
	echo "DNS.5   = *.dns.podman" >> data/localhost.config

data/generated-localhost.crt: data/localhost.config
	openssl req -x509 -nodes -days 3650 -newkey rsa:4096 -keyout data/generated-localhost.key -out data/generated-localhost.crt -config data/localhost.config -subj '/CN=localhost'

data/generated-localhost.pass: data
	base64 /dev/urandom -w 0 | head -c 32 > data/generated-localhost.pass

data/generated-localhost.pfx: data/generated-localhost.pass data/generated-localhost.crt
	openssl pkcs12 -export -out data/generated-localhost.pfx -inkey data/generated-localhost.key -in data/generated-localhost.crt -passout pass:"$$(cat data/generated-localhost.pass)"

data/https:
	mkdir -pv data/https

data/https/localhost.pfx: data/https data/generated-localhost.pfx
	cp data/generated-localhost.pfx data/https/localhost.pfx

.PHONY: generate-developer-certificate
generate-developer-certificate: data/https/localhost.pfx
	sed -i "s/ASPNETCORE_Kestrel__Certificates__Default__Password=[^ ]*/ASPNETCORE_Kestrel__Certificates__Default__Password=$$(cat data/generated-localhost.pass)/" docker-compose.yml Makefile

.PHONY: install-developer-certificate
install-developer-certificate: data/generated-localhost.pfx data/generated-localhost.pass generate-developer-certificate
	dotnet dev-certs https --clean --import data/generated-localhost.pfx --password "$$(cat data/generated-localhost.pass)"
	dotnet dev-certs https --trust
	
	sudo rm -rf /usr/local/share/ca-certificates/litium-education-localhost.crt
	sudo update-ca-certificates --fresh
	sudo cp data/generated-localhost.crt /usr/local/share/ca-certificates/litium-education-localhost.crt
	sudo update-ca-certificates
