#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.10-alpine3.16-amd64 AS base
WORKDIR /app
#EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MainApplication/MainApplication.csproj", "MainApplication/"]
COPY ["SmartERP.Infrastructure.Nginx/SmartERP.Infrastructure.Nginx.csproj", "SmartERP.Infrastructure.Nginx/"]
RUN dotnet restore "MainApplication/MainApplication.csproj"
COPY . .
WORKDIR "/src/MainApplication"
RUN dotnet build "MainApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MainApplication.csproj" -c Release -o /app/publish


# Build nginx # Extract from here: https://zawadidone.nl/using-docker-images-from-scratch/
FROM  nginx:alpine as nginx-build
# Build nginx done


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .



FROM final
RUN apk update && apk upgrade
RUN apk add --no-cache tzdata certbot certbot-nginx
RUN addgroup -S tor && adduser -S tor -G tor
# copy nginx begin ---------------------------------------------------------------------------
# Customise static content, and configuration
#COPY --from=nginx-build /etc/passwd /etc/passwd
COPY --from=nginx-build [ \
    "/lib/ld-musl-x86_64.so.1", \
    "/lib/libssl.so.1.1", \
    "/lib/libcrypto.so.1.1", \
    "/lib/libz.so.1", \
    "/lib/ld-musl-x86_64.so.1", \
    "/lib/" \
    ]
COPY --from=nginx-build ["/usr/lib/libpcre2-8.so.0", "/usr/lib/"]
COPY --from=nginx-build ["/usr/sbin/nginx", "/usr/sbin/nginx"]
COPY --from=nginx-build ["/var/log/nginx", "/var/log/nginx"]
COPY --from=nginx-build ["/etc/nginx", "/etc/nginx"]
COPY --from=nginx-build ["/usr/share/nginx/html/index.html", "/usr/share/nginx/html/index.html"]
# Run as non-root -> need create folders and permissions
RUN mkdir -p /var/cache/nginx/ && \
    chown tor /var/cache/nginx/
#COPY default.conf /etc/nginx/conf.d/
#COPY nginx.conf /etc/nginx/
COPY Gateways/nginx/nginx.conf /etc/nginx/
#Change default stop signal from SIGTERM to SIGQUIT
#STOPSIGNAL SIGQUIT
## install nginx done, copy files for using https with nginx
COPY Gateways/nginx/cert.crt /etc/ssl/certs/cert.crt
COPY Gateways/nginx/cert.key /etc/ssl/private/cert.key
# copy nginx done ------------------------------------------------------------------------------

USER tor
ENTRYPOINT ["dotnet", "MainApplication.dll"]
