FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/KiemTraGiuaKy.Web.Host/KiemTraGiuaKy.Web.Host.csproj", "src/KiemTraGiuaKy.Web.Host/"]
COPY ["src/KiemTraGiuaKy.Web.Core/KiemTraGiuaKy.Web.Core.csproj", "src/KiemTraGiuaKy.Web.Core/"]
COPY ["src/KiemTraGiuaKy.Application/KiemTraGiuaKy.Application.csproj", "src/KiemTraGiuaKy.Application/"]
COPY ["src/KiemTraGiuaKy.Core/KiemTraGiuaKy.Core.csproj", "src/KiemTraGiuaKy.Core/"]
COPY ["src/KiemTraGiuaKy.EntityFrameworkCore/KiemTraGiuaKy.EntityFrameworkCore.csproj", "src/KiemTraGiuaKy.EntityFrameworkCore/"]
WORKDIR "/src/src/KiemTraGiuaKy.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/KiemTraGiuaKy.Web.Host", "src/KiemTraGiuaKy.Web.Host"]
COPY ["src/KiemTraGiuaKy.Web.Core", "src/KiemTraGiuaKy.Web.Core"]
COPY ["src/KiemTraGiuaKy.Application", "src/KiemTraGiuaKy.Application"]
COPY ["src/KiemTraGiuaKy.Core", "src/KiemTraGiuaKy.Core"]
COPY ["src/KiemTraGiuaKy.EntityFrameworkCore", "src/KiemTraGiuaKy.EntityFrameworkCore"]
WORKDIR "/src/src/KiemTraGiuaKy.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "KiemTraGiuaKy.Web.Host.dll"]
