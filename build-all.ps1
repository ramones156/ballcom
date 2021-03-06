echo "Starting..."

.\build.ps1 CustomerManagement
.\build.ps1 InventoryManagement
.\build.ps1 NotificationService
.\build.ps1 OrderManagement
.\build.ps1 Orderpicker
.\build.ps1 PaymentService
.\build.ps1 ServiceDesk
.\build.ps1 SupplierManagement
.\build.ps1 TransportManagement
.\build.ps1 EventService

Write-Host "Finished!" -ForegroundColor Blue
echo "Building docker containers with docker-compose"

docker-compose build