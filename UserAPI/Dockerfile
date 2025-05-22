FROM mcr.microsoft.com/dotnet/sdk:9.0 AS prepare-restore
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset --version 0.3.2
WORKDIR /src
COPY . .
RUN dotnet subset restore UserAPI/UserAPI.csproj --root-directory /src --output restore_subset/

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY --from=prepare-restore /src/restore_subset .
RUN dotnet restore UserAPI/UserAPI.csproj


COPY . .
RUN dotnet publish UserAPI/UserAPI.csproj -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl &&\
	apt-get clean && \
	rm -rf /var/lib/apt/lists/
COPY --from=build /app/publish .
LABEL version="1.0.0"
LABEL description="Сервис для управления пользователями"
LABEL org.opencontainers.image.authors="alexeeff@internet.ru"
EXPOSE 82
ENTRYPOINT ["dotnet", "UserAPI.dll"]