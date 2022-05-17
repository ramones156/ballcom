# ONE TIME ONLY! 
# Configure Powershell.
Om scripts uit te voeren in powershell moet het volgende commando in een powershel administrator prompt gerunned worden.
```
Set-ExecutionPolicy Unrestricted
```
Druk vervolgens op `A` om alles te accepteren.

# Bouwen van dotnet en containers
Om alle containers te maken en bij te werken, voer het build-all script uit (ps1 voor windows, sh voor linux)

# Uitvoeren/starten van de containers
Om alle containers te starten, voer `docker-compose up` uit

# RabbitMQ
Om bij de rabbitMq client te komen, ga naar http://localhost:15672
Log in met User: Rathalos en Wachtwoord: 1234

# phpMyAdmin
Om bij phpMyAdmin te komen, ga naar http://localhost:8080/
Log in met User: 'root' en wachtwoord: 'secret'

# Migration CustomerManagment
- Zorg ervoor dat de MariaDb draaiend is in de container.
- Open een powershell window 
- Navigeer naar de map customer management
- Om te controleren of je entity framework heb, voer de volgende command uit.
```
dotnet ef
```
- als je geen eenhoorn ziet, voer de volgende command uit
```
 dotnet tool install --global dotnet-ef
```
- Als je zelf een migration wil toevoegen, voer uit
```
dotnet migrations add "naam_migration"
```
- Wil je de database updaten met de huidige migrations voer dan uit
```
dotnet ef database update
```