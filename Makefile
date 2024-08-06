build:
	dotnet build RestApis.sln

run:
	dotnet run --project src/Movies.Api/Movies.Api.csproj

run-identity:
	dotnet run --project src/Identity.Api/Identity.Api.csproj

up:
	docker-compose up -d

down:
	docker-compose down