build:
	dotnet build
clean:
	dotnet clean
restore:
	dotnet restore
watch:
	dotnet watch run --project src/WebUI
start:
	dotnet run --project src/WebUI
test-app:
	dotnet watch test --project tests/Application.IntegrationTests
test-web:
	dotnet watch test --project tests/WebUI.IntegrationTests
migrations:
	dotnet ef migrations add InitialCreate -p src/Infrastructure -s src/WebUI -o Persistence/Migrations
migrate:
	dotnet ef database update -p src/Infrastructure -s src/WebUI
seed:
	dotnet run -p tools/Application.Tools seed