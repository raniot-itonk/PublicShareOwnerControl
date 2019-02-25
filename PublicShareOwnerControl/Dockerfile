FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PublicShareOwnerControl/PublicShareOwnerControl.csproj", "PublicShareOwnerControl/"]
RUN dotnet restore "PublicShareOwnerControl/PublicShareOwnerControl.csproj"
COPY . .
WORKDIR "/src/PublicShareOwnerControl"
RUN dotnet build "PublicShareOwnerControl.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PublicShareOwnerControl.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PublicShareOwnerControl.dll"]