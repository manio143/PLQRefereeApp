FROM microsoft/dotnet:2.1-aspnetcore-runtime

LABEL maintainer=marian.dziubiak@gmail.com

WORKDIR /var/app
COPY . /var/app

ENV certPassword ###

#Configure SSL
RUN openssl genrsa -des3 -passout pass:${certPassword} -out server.key 2048
RUN openssl rsa -passin pass:${certPassword} -in server.key -out server.key
RUN openssl req -sha256 -new -key server.key -out server.csr -subj '/CN=localhost'
RUN openssl x509 -req -sha256 -days 365 -in server.csr -signkey server.key -out server.crt
RUN openssl pkcs12 -export -out cert.pfx -inkey server.key -in server.crt -certfile server.crt -passout pass:${certPassword}

RUN echo "Europe/Warsaw" > /etc/timezone
RUN unlink /etc/localtime
RUN ln -s /usr/share/zoneinfo/Europe/Warsaw /etc/localtime

EXPOSE 443/tcp

ENTRYPOINT ["dotnet", "PLQRefereeApp.dll"]
