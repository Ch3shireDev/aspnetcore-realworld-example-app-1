.PHONY: publish

run:
	dotnet run --project src/WebUI
watch:
	@cd src/WebUI && dotnet watch
fresh:
	dotnet run --project tools/Application.Tools fresh
seed:
	dotnet run --project tools/Application.Tools seed
publish:
	dotnet run --project targets
format:
	dotnet format
test:
	dotnet test -l:"console;verbosity=detailed"
test-watch-app:
	dotnet watch test --project tests/Application.IntegrationTests -l:"console;verbosity=detailed"
test-watch-web:
	dotnet watch test --project tests/WebUI.IntegrationTests
migrations-add:
	dotnet ef migrations add --project src/Infrastructure -s src/WebUI -o Persistence/Migrations $(name)
migrations-remove:
	dotnet ef migrations remove --project src/Infrastructure -s src/WebUI
db-update:
	dotnet ef database update --project src/Infrastructure -s src/WebUI $(name)
